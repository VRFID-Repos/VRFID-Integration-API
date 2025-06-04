import os
import time
import logging
import hashlib
import requests
from watchdog.observers import Observer
from watchdog.events import FileSystemEventHandler
from datetime import datetime
import pytz

# Paths
NEWBOOK_DIR = "C:/Newbook"

# Configure logging
logging.basicConfig(level=logging.INFO, format='%(asctime)s - %(levelname)s - %(message)s')

class NewbookHandler(FileSystemEventHandler):
    def __init__(self):
        self.processed_files = set()  # Track files that have been processed
        self.last_processed_time = {}

    def to_utc(self, date_str, time_str):
        """
        Convert local date-time strings to UTC.
        """
        # Combine date and time strings into a datetime object
        local_time = datetime.strptime(f"{date_str} {time_str}", "%d%m%y %H%M")
        
        # Define your local time zone (e.g., "Australia/Sydney")
        local_tz = pytz.timezone("Australia/Sydney")  # Replace with your actual time zone
        local_dt = local_tz.localize(local_time, is_dst=None)  # Localize datetime to the given time zone
        
        # Convert to UTC
        utc_dt = local_dt.astimezone(pytz.utc)
        return utc_dt.strftime("%d%m%y %H%M")

    def process_dat_file(self, file_path):
        current_time = time.time()

        # Calculate hash of the file content
        file_hash = self.calculate_file_hash(file_path)

        # Prevent processing the same file within a short time frame (e.g., 1 second)
        if file_hash in self.processed_files and (current_time - self.last_processed_time.get(file_hash, 0) < 1):
            logging.info(f"Skipping already processed file: {file_path}")
            return

        # Update the tracking info for the processed file and time
        self.processed_files.add(file_hash)
        self.last_processed_time[file_hash] = current_time

        try:
            with open(file_path, 'r') as dat_file:
                line = dat_file.readline().strip()
                data = line.split('|')
                
                logging.info(f"Data length: {len(data)}")
                logging.info("Data elements: %s", data)

                # Check for 11 fields and process accordingly
                if len(data) == 11:
                    self.process_11_field_data(data)
                elif len(data) == 3:
                    self.process_3_field_data(data)
                else:
                    logging.warning(f"Unexpected data format in {file_path}: {line}")

        except Exception as e:
            logging.error(f"Error processing file {file_path}: {e}")

    def process_11_field_data(self, data):
        """
        Handle data with 11 fields to create credentials via the C# API.
        """
        name = data[6].strip()  # Using Booking Number as Name
        license_plate = data[3].strip()
        pin = data[1].strip()
        group = data[4].strip()

        # Convert activation and deactivation times to UTC
        activation_datetime = self.to_utc(data[7].strip(), data[8].strip())
        deactivation_datetime = self.to_utc(data[9].strip(), data[10].strip())

        # Call the C# API to create the credential
        response = self.create_genetec_credential_via_csharp_api(name, license_plate, activation_datetime, deactivation_datetime, pin, group)
        self.handle_api_response(response, "Genetec credential")

    def process_3_field_data(self, data):
        """
        Handle data with 3 fields to cancel bookings and credentials.
        """
        method_of_deactivation = data[0].strip()
        physical_access_code = data[1].strip()
        action = data[2].strip()

        # Validate the action type
        if action.upper() != "DELETE":
            logging.warning(f"Action type is not DELETE: {action}. Skipping file.")
            return

        # Prepare the payload
        payload = {
            "MethodOfDeactivatingAccessCode": method_of_deactivation,
            "PhysicalAccessCode": physical_access_code,
            "Action": action
        }

        # Call the CancelBookingAndCredentials API
        response = self.cancel_booking_and_credentials(payload)
        self.handle_api_response(response, "Cancel Booking and Credentials")

    def calculate_file_hash(self, file_path):
        """
        Calculate a hash of the file content to track if it has been processed.
        """
        hasher = hashlib.md5()
        with open(file_path, 'rb') as f:
            content = f.read()
            hasher.update(content)
        return hasher.hexdigest()

    def create_genetec_credential_via_csharp_api(self, name, license_plate, activation_datetime, deactivation_datetime, pin, group):
        """
        Sends a POST request to the C# API to create a credential with the given details.
        """
        api_url = "https://anithswork.com/Credential/CreateLicensePlateCredential2"
        payload = {
            "Name": name,
            "LicensePlate": license_plate,
            "ActivationDateTime": activation_datetime,
            "DeactivationDateTime": deactivation_datetime,
            "Pin": pin,
            "Group": group
        }
        try:
            response = requests.post(api_url, json=payload, verify=False)
            return response
        except requests.RequestException as e:
            logging.error(f"Failed to call C# API for creating Genetec credential: {e}")
            return None

    def cancel_booking_and_credentials(self, payload):
        """
        Sends a POST request to the CancelBookingAndCredentials API.
        """
        api_url = "https://anithswork.com/Credential/CancelBookingAndCredentials"
        try:
            response = requests.post(api_url, json=payload, verify=False)
            return response
        except requests.RequestException as e:
            logging.error(f"Failed to call CancelBookingAndCredentials API: {e}")
            return None

    def handle_api_response(self, response, action_description):
        """
        Handles the API response, logging success or failure.
        """
        if response and response.status_code == 200:
            try:
                response_data = response.json()
                if response_data["Rsp"]["Status"] == "Ok":
                    logging.info(f"Successfully completed {action_description} via API.")
                else:
                    logging.error(f"API responded with a non-Ok status for {action_description}.")
            except (KeyError, ValueError) as e:
                logging.error(f"Failed to parse API response for {action_description}: {e}")
        else:
            logging.error(f"Failed to complete {action_description}. Response: {response.text if response else 'No response received.'}")

    def on_modified(self, event):
        if not event.is_directory and event.src_path.endswith('.dat'):
            logging.info(f"Detected modification in file: {event.src_path}")
            self.process_dat_file(event.src_path)

    def on_created(self, event):
        if not event.is_directory and event.src_path.endswith('.dat'):
            logging.info(f"Detected new file creation: {event.src_path}")
            self.process_dat_file(event.src_path)

def start_monitoring():
    event_handler = NewbookHandler()
    observer = Observer()
    observer.schedule(event_handler, path=NEWBOOK_DIR, recursive=False)
    observer.start()
    try:
        while True:
            time.sleep(1)
    except KeyboardInterrupt:
        observer.stop()
    observer.join()

if __name__ == "__main__":
    start_monitoring()

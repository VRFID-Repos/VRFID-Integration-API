using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace App.Entity.Models
{
    public class AccessCodeResponse
    {
        [JsonPropertyName("success")]
        public string Success { get; set; }

        [JsonPropertyName("data")]
        public List<AccessCodeData> Data { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; }

        [JsonPropertyName("timestamp")]
        public string Timestamp { get; set; }

        [JsonPropertyName("timezone")]
        public string Timezone { get; set; }

        [JsonPropertyName("newbook_version")]
        public string NewBookVersion { get; set; }

        [JsonPropertyName("api_key")]
        public string ApiKey { get; set; }
    }

    public class AccessCodeData
    {
        [JsonPropertyName("access_code_id")]
        public string AccessCodeId { get; set; }

        [JsonPropertyName("access_code")]
        public string AccessCode { get; set; }

        [JsonPropertyName("access_code_car_rego")]
        public string AccessCodeCarRego { get; set; }

        [JsonPropertyName("access_code_period_from")]
        public string AccessCodePeriodFrom { get; set; }

        [JsonPropertyName("access_code_period_to")]
        public string AccessCodePeriodTo { get; set; }

        [JsonPropertyName("security_area_id")]
        public string SecurityAreaId { get; set; }

        [JsonPropertyName("security_area_name")]
        public string SecurityAreaName { get; set; }

        [JsonPropertyName("booking_id")]
        public string BookingId { get; set; }

        [JsonPropertyName("booking_name")]
        public string BookingName { get; set; }

        [JsonPropertyName("booking_arrival")]
        public string BookingArrival { get; set; }

        [JsonPropertyName("booking_departure")]
        public string BookingDeparture { get; set; }

        [JsonPropertyName("booking_status")]
        public string BookingStatus { get; set; }

        [JsonPropertyName("guest_id")]
        public string GuestId { get; set; }

        [JsonPropertyName("guest_name")]
        public string GuestName { get; set; }

        [JsonPropertyName("mappings")]
        public List<object> Mappings { get; set; } // Adjust this if mappings have a specific structure
    }


}

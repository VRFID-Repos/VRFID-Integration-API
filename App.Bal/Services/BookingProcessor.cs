using App.Bal.Repositories;
using App.Common.Model;
using App.Entity.Models;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using static GenetecApiHelper;

namespace App.Bal.Services
{
    public class BookingProcessor
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ISMSService _SMSservice;
        private readonly IMailService _Mailservice;
        private readonly IGenetecServices _GenetecServices;
        private readonly IHttpClientFactory _httpClientFactory;

        public BookingProcessor(IServiceProvider serviceProvider, ISMSService SMSservice, IMailService mailservice, IGenetecServices genetecServices, IHttpClientFactory httpClientFactory)
        {
            _serviceProvider = serviceProvider;
            _SMSservice = SMSservice;
            _Mailservice = mailservice;
            _GenetecServices = genetecServices;
            _httpClientFactory = httpClientFactory;
        }

        public async Task ProcessBookingsAsync(IEnumerable<BookingData> bookings, CustomerDTO customer)
        {
            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            // Use dbContext as needed
            foreach (var booking in bookings)
            {
                var AccessCodeList = new List<AccessCodeData>();
                //Process Access Codes
                if (booking.AccessCodes.Count > 0)
                {
                    foreach (var code in booking.AccessCodes)
                    {
                        var accessCode = await SearchAccessCodesAsync(code.Id, customer);
                        AccessCodeList.AddRange(accessCode.Data);
                    }
                    await ProcessAccessCodesAsync(AccessCodeList, customer);
                }
                if (await dbContext.ProcessedBookings.AnyAsync(b => b.BookingId == booking.BookingId.ToString() && b.CustomerId==customer.CUSTOMER_ID))
                    continue;

                dbContext.ProcessedBookings.Add(new ProcessedBooking
                {
                    BookingId = booking.BookingId.ToString(),
                    CustomerId = customer.CUSTOMER_ID,
                    ProcessedAt = DateTime.UtcNow,
                    EmailScheduled = false,
                    EmailSent = false,
                    PassClaimed = false
                    
                });
                await dbContext.SaveChangesAsync();

                // Trigger notification
                await SendNotificationV2(booking.Guests, booking, customer);
                
            }
        }
        public async Task ProcessAccessCodesAsync(IEnumerable<AccessCodeData> accessCodes, CustomerDTO customer)
        {
            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            foreach (var accessCode in accessCodes)
            {
                // Check if the access code has already been processed
                if (await dbContext.ProcessedAccessCodes.AnyAsync(a => a.AccessCodeId == accessCode.AccessCodeId && a.CustomerId == customer.CUSTOMER_ID))
                    continue;
                if (customer.IsGeneticIntegration)
                {
                    // Create cardholder and credentials via API
                    var apiModel = new EntityCredentials2
                    {
                        Name = accessCode.BookingName,
                        Pin = accessCode.AccessCode,
                        LicensePlate = accessCode.AccessCodeCarRego,
                        ActivationDateTime = DateTime.Parse(accessCode.AccessCodePeriodFrom).ToString("ddMMyy HHmm"),
                        DeactivationDateTime = DateTime.Parse(accessCode.AccessCodePeriodTo).ToString("ddMMyy HHmm")
                    };
                    try
                    {
                        GuidResponses responseData = await _GenetecServices.CreateCardholderAndCredentialForNewBook(apiModel, customer);
                        var cardholderGuid = (string)responseData?.CardholderGuid;
                        var licenseCredGuid = (string)responseData?.LicenseCredGuid;
                        var pinCredGuid = (string)responseData?.PinCredGuid;

                        // Store the access code and credentials in the database
                        dbContext.ProcessedAccessCodes.Add(new ProcessedAccessCode
                        {
                            AccessCodeId = accessCode.AccessCodeId,
                            CustomerId = customer.CUSTOMER_ID,
                            ProcessedAt = DateTime.UtcNow,
                            AccessCodeCarRego = accessCode.AccessCodeCarRego,
                            AccessCodePeriodFrom = DateTime.Parse(accessCode.AccessCodePeriodFrom),
                            AccessCodePeriodTo = DateTime.Parse(accessCode.AccessCodePeriodTo),
                            SecurityAreaId = accessCode.SecurityAreaId,
                            SecurityAreaName = accessCode.SecurityAreaName,
                            BookingId = accessCode.BookingId,
                            BookingName = accessCode.BookingName,
                            BookingArrival = string.IsNullOrEmpty(accessCode.BookingArrival) ? (DateTime?)null : DateTime.Parse(accessCode.BookingArrival),
                            BookingDeparture = string.IsNullOrEmpty(accessCode.BookingDeparture) ? (DateTime?)null : DateTime.Parse(accessCode.BookingDeparture),
                            GuestId = accessCode.GuestId,
                            GuestName = accessCode.GuestName,
                            CardholderGuid = cardholderGuid,
                            LicenseCredGuid = licenseCredGuid,
                            PinCredGuid = pinCredGuid
                        });

                        await dbContext.SaveChangesAsync();

                    }
                    catch (Exception ex)
                    {
                        // Log the exception
                        Console.WriteLine($"Exception while creating cardholder and credentials: {ex.Message}");
                    }
                }
                else
                {
                    try
                    {
                        
                        var cardholderGuid = "";
                        var licenseCredGuid = "";
                        var pinCredGuid = "";

                        // Store the access code and credentials in the database
                        dbContext.ProcessedAccessCodes.Add(new ProcessedAccessCode
                        {
                            AccessCodeId = accessCode.AccessCodeId,
                            CustomerId = customer.CUSTOMER_ID,
                            ProcessedAt = DateTime.UtcNow,
                            AccessCodeCarRego = accessCode.AccessCodeCarRego,
                            AccessCodePeriodFrom = DateTime.Parse(accessCode.AccessCodePeriodFrom),
                            AccessCodePeriodTo = DateTime.Parse(accessCode.AccessCodePeriodTo),
                            SecurityAreaId = accessCode.SecurityAreaId,
                            SecurityAreaName = accessCode.SecurityAreaName,
                            BookingId = accessCode.BookingId,
                            BookingName = accessCode.BookingName,
                            BookingArrival = string.IsNullOrEmpty(accessCode.BookingArrival) ? (DateTime?)null : DateTime.Parse(accessCode.BookingArrival),
                            BookingDeparture = string.IsNullOrEmpty(accessCode.BookingDeparture) ? (DateTime?)null : DateTime.Parse(accessCode.BookingDeparture),
                            GuestId = accessCode.GuestId,
                            GuestName = accessCode.GuestName,
                            CardholderGuid = cardholderGuid,
                            LicenseCredGuid = licenseCredGuid,
                            PinCredGuid = pinCredGuid
                        });

                        await dbContext.SaveChangesAsync();

                    }
                    catch (Exception ex)
                    {
                        // Log the exception
                        Console.WriteLine($"Exception while saving Access Codes: {ex.Message}");
                    }
                }
            }
        }


        public async Task<Task> SendNotification(IEnumerable<Guest> guests, BookingData booking, CustomerDTO customer)
        {
            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            // Get Claim page URL:
            PassResponse model = await GenetecApiHelper.GetPassUrlByNameAsync(booking.CategoryName);
            if (String.IsNullOrEmpty(model.Url))
            {
                return Task.CompletedTask;
            }
            // Send SMS for each guest
            foreach (var guest in guests)
            {
                if (model.IsSMS)
                {
                    //SMS
                    if (!string.IsNullOrWhiteSpace(guest.ContactDetails.FirstOrDefault(c => c.Type == "mobile")?.Content))
                    {
                        string messageBody = $"Hello {guest.Firstname}, your can avail Pass from Here. Access it here: {model.Url}";
                        await _SMSservice.SendSmsAsync(guest.ContactDetails.FirstOrDefault(c => c.Type == "mobile")?.Content, messageBody);
                    }
                }
                if (model.IsEmail)
                {
                    //Email
                    if (!string.IsNullOrWhiteSpace(guest.ContactDetails.FirstOrDefault(c => c.Type == "email")?.Content))
                    {
                        await _Mailservice.SendEmailFromVRFIDAsync(guest.ContactDetails.FirstOrDefault(c => c.Type == "email")?.Content, model.Url);
                    }
                }
            }
            var EntitiyBookingData = await dbContext.ProcessedBookings.Where(b => b.BookingId == booking.BookingId.ToString()).FirstOrDefaultAsync();
            if (EntitiyBookingData != null)
            {
                EntitiyBookingData.EmailSent = true;
                dbContext.ProcessedBookings.Update(EntitiyBookingData);
                await dbContext.SaveChangesAsync();
            }
            return Task.CompletedTask;
        }
        private async Task<Task> SendNotificationV2(IEnumerable<Guest> guests, BookingData booking, CustomerDTO customer)
        {
            if (booking == null)
            {
                throw new ArgumentNullException(nameof(booking));
            }

            // Parse the BookingPlaced and BookingArrival dates
            if (DateTime.TryParse(booking.BookingPlaced, out DateTime bookingPlacedDate) &&
                DateTime.TryParse(booking.BookingArrival, out DateTime bookingArrivalDate))
            {
                // Calculate the difference in days
                int daysDifference = (bookingArrivalDate - bookingPlacedDate).Days;

                if (daysDifference <= 3)
                {
                    // Send SMS and Email instantly
                    await SendNotification(guests, booking, customer);
                    return Task.CompletedTask;
                }
                else
                {
                    // Schedule the notification for 3 days before arrival
                    DateTime notificationDate = bookingArrivalDate.AddDays(-3);

                    if (notificationDate > DateTime.Now)
                    {
                        Console.WriteLine($"Scheduling notification for {notificationDate}");
                        ScheduleNotification(notificationDate, guests, booking, customer);
                    }
                    else
                    {
                        // Fallback: Send notification instantly if the notification date is in the past
                        await SendNotification(guests, booking, customer);
                    }
                }
                return Task.CompletedTask;
            }
            else
            {
                throw new InvalidOperationException("Invalid date format in booking data.");
            }
        }
        /// <summary>
        /// Schedules the notification using Hangfire or a similar scheduler.
        /// </summary>
        private void ScheduleNotification(DateTime notificationDate, IEnumerable<Guest> guests, BookingData booking, CustomerDTO customer)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var test = notificationDate - DateTime.Now;
                // Using Hangfire to schedule the task
                BackgroundJob.Schedule(() => SendNotification(guests, booking, customer), notificationDate - DateTime.Now);

                // Log scheduling details
                Console.WriteLine($"Notification scheduled for {notificationDate} for Booking ID: {booking.BookingId}");
                var EntitiyBookingData =  dbContext.ProcessedBookings.Where(b => b.BookingId == booking.BookingId.ToString()).FirstOrDefault();
                if (EntitiyBookingData != null)
                {
                    EntitiyBookingData.EmailScheduled = true;
                    dbContext.ProcessedBookings.Update(EntitiyBookingData);
                    dbContext.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private async Task<AccessCodeResponse> SearchAccessCodesAsync(
                                                        string searchTerm,
                                                        CustomerDTO customer,
                                                        int? fuzzySearchLength = null,
                                                        bool accessCodeMappings = false
                                                        )
        {

            if (string.IsNullOrWhiteSpace(customer.NewBookRegion) || string.IsNullOrWhiteSpace(customer.NewBookApiKey) || string.IsNullOrWhiteSpace(searchTerm))
            {
                throw new ArgumentException("Region, API Key, and Search Term are required parameters.");
            }

            try
            {
                var client = _httpClientFactory.CreateClient();

                // Prepare the request body
                var requestBody = new Dictionary<string, object>
                {
                    { "region", customer.NewBookRegion },
                    { "api_key", customer.NewBookApiKey },
                    { "search_term", searchTerm }
                };

                // Serialize the request body
                var jsonBody = JsonSerializer.Serialize(requestBody, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
                //var apiUrl = apiConfig.Endpoint;
                var apiUrl = $"{customer.NewBookEndpoint}access_codes_search";

                var httpRequest = new HttpRequestMessage(HttpMethod.Post, apiUrl)
                {
                    Content = new StringContent(jsonBody, Encoding.UTF8, "application/json")
                };
                var authToken = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{customer.NewBookUsername}:{customer.NewBookPassword}"));
                httpRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", authToken);
                // Send the request
                var response = await client.SendAsync(httpRequest);

                if (!response.IsSuccessStatusCode)
                {
                    throw new InvalidOperationException("Error occurred while searching access codes.");
                }

                // Deserialize the response
                var responseData = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<AccessCodeResponse>(responseData);

                return result;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error occurred while searching access codes.", ex);
            }
        }
    }

}



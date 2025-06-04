using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace App.Entity.Models
{
    public class EntityPassCreatorModel
    {
    }
   
    public class PassTemplateField
    {
        public string Type { get; set; }
        public string key { get; set; }
        public bool Required { get; set; }
        public string label { get; set; }
    }
    public class PassDetailsResponse
    {
        public string Identifier { get; set; }
        public string Uri { get; set; }
        public string GeneratedId { get; set; }
        public string SearchString { get; set; }
        public bool Voided { get; set; }
        public bool Redeemed { get; set; }
        public string CreatedOn { get; set; }
        public string ModifiedOn { get; set; }
        public string ExpirationDate { get; set; }
        public string UserProvidedId { get; set; }
        public string PassTemplateGuid { get; set; } // Template UID
        public string PassTemplateName { get; set; }
        public string LinkToPassPage { get; set; }
        public string Thumbnail { get; set; }
        public string QrCodeImage { get; set; }
        public string BarcodeValue { get; set; }
        public int NumberOfActive { get; set; }
        public int NumberOfInactive { get; set; }
        public int NumberOfPrinted { get; set; }
        public Dictionary<string, string> PassFieldData { get; set; } // For additional properties
        public object LastUsage { get; set; } // Can adjust type if needed
        public object StoredValue { get; set; } // Can adjust type if needed
        public List<string> EligiblePlaces { get; set; }
        public List<string> PlacesUsedAt { get; set; }
        public string DownloadPin { get; set; }
        public string BundleId { get; set; }
        public Dictionary<string, string> FieldMapping { get; set; } // Mapping of field names to keys
        public string PassData { get; set; }
    }
    public class UpdatePassRequest
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
    }
    // Model to represent the request body
    public class EnableDownloadRequest
    {
        // Optional field for enabling download until a specified time
        public string EnableDownloadUntil { get; set; }
    }
    public class UpdatePassVoidRequest
    {
        public bool Voided { get; set; }
    }
    public class PushNotificationRequest
    {
        [JsonProperty("pushNotificationText")]
        public string PushNotificationText { get; set; }
    }
    public class MultiPushNotificationRequest
    {
        [JsonProperty("listOfPasses")]
        public List<string> ListOfPasses { get; set; }

        [JsonProperty("pushNotificationText")]
        public string PushNotificationText { get; set; }
    }
    public class PassListResponse
    {
        public Query Query { get; set; }
        public int Total { get; set; }
        public List<Result> Results { get; set; }
    }
    public class Query
    {
        public int Start { get; set; }
        public int PageSize { get; set; }
        public string Self { get; set; }
    }
    public class Result
    {
        public string Identifier { get; set; }
        public string Uri { get; set; }
        public string GeneratedId { get; set; }
        public string SearchString { get; set; }
        public bool Voided { get; set; }
        public bool Redeemed { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime ModifiedOn { get; set; }
        public DateTime? ExpirationDate { get; set; }
        public string UserProvidedId { get; set; }
        public string PassTemplateGuid { get; set; }
        public string PassTemplateName { get; set; }
        public string LinkToPassPage { get; set; }
        public string Thumbnail { get; set; }
        public string QrCodeImage { get; set; }
        public string BarcodeValue { get; set; }
        public int NumberOfActive { get; set; }
        public int NumberOfInactive { get; set; }
        public int NumberOfPrinted { get; set; }
        public Dictionary<string, string> PassFieldData { get; set; } // Dynamic keys
        public DateTime? LastUsage { get; set; }
        public object StoredValue { get; set; }
        public List<object> EligiblePlaces { get; set; }
        public List<object> PlacesUsedAt { get; set; }
        public string DownloadPin { get; set; }
        public string BundleId { get; set; }
        public string PassData { get; set; }
    }
    public class PassUrisResponse
    {
        public string iPhoneUri { get; set; }
        public string AndroidUri { get; set; }
    }
    public class PassStatisticsResponse
    {
        public List<TimelineEntry> ActiveRegistrationsTimeline { get; set; }
        public List<TimelineEntry> InactiveRegistrationsTimeline { get; set; }
        public List<TimelineEntry> CreatedPassesTimeline { get; set; }
        public List<TimelineEntry> UpdatedPassesTimeline { get; set; }
        public List<TimelineEntry> SentPushNotificationsTimeline { get; set; }
        public List<TimelineEntry> ReceivedPushNotificationsTimeline { get; set; }
        public PassesPerOperatingSystem PassesPerOperatingSystem { get; set; }
    }

    public class TimelineEntry
    {
        [JsonPropertyName("date")]
        public string Date { get; set; }

        [JsonPropertyName("noOfPasses")]
        public int NoOfPasses { get; set; } // Updated to `int` to match the expected data type
    }

    public class PassesPerOperatingSystem
    {
        [JsonPropertyName("iOS")]
        public int IOS { get; set; }

        [JsonPropertyName("Android")]
        public int Android { get; set; }

        [JsonPropertyName("WindowsPhone")]
        public int WindowsPhone { get; set; }
    }
    public class ActiveHistoryEntry
    {
        [JsonPropertyName("date")]
        public string Date { get; set; }

        [JsonPropertyName("active")]
        public int Active { get; set; }

        [JsonPropertyName("inactive")]
        public int Inactive { get; set; }
    }
    public class MovePassResponse
    {
        [JsonPropertyName("success")]
        public bool Success { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; }

        [JsonPropertyName("errors")]
        public List<string> Errors { get; set; }

        [JsonPropertyName("data")]
        public List<object> Data { get; set; }

        [JsonPropertyName("statusCode")]
        public int StatusCode { get; set; }
    }


}

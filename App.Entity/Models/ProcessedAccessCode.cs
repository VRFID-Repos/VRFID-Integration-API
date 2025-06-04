using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App.Entity.Models
{
    public class ProcessedAccessCode
    {
        public string AccessCodeId { get; set; }
        public int CustomerId { get; set; }
        public DateTime ProcessedAt { get; set; }
        public string AccessCodeCarRego { get; set; }
        public DateTime AccessCodePeriodFrom { get; set; }
        public DateTime AccessCodePeriodTo { get; set; }
        public string SecurityAreaId { get; set; }
        public string SecurityAreaName { get; set; }
        public string BookingId { get; set; }
        public string BookingName { get; set; }
        public DateTime? BookingArrival { get; set; }
        public DateTime? BookingDeparture { get; set; }
        public string? GuestId { get; set; }
        public string? GuestName { get; set; }

        // New columns for storing GUIDs
        public string CardholderGuid { get; set; }
        public string LicenseCredGuid { get; set; }
        public string PinCredGuid { get; set; }
        public string? CustomCredGuid { get; set; }
        public string? PassIdentifier { get; set; }
    }

}

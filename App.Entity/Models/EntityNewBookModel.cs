using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace App.Entity.Models
{
    public class EntityNewBookModel
    {
    }
    public class BookingListRequest
    {

        public string? ListType { get; set; }

        // Optional parameters
        public DateTime? PeriodFrom { get; set; }
        public DateTime? PeriodTo { get; set; }
        public int? RestrictMailOuts { get; set; }
        public List<int> BookingReason { get; set; }
        public int? BookingSource { get; set; }
        public int? BookingMethod { get; set; }
        public int? BookingDemographic { get; set; }
        public bool? AccountBreakdown { get; set; }
    }
    public class BookingResponse
    {
        [JsonPropertyName("success")]
        public string Success { get; set; }

        [JsonPropertyName("data")]
        public List<BookingData> Data { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; }

        [JsonPropertyName("data_total")]
        public int DataTotal { get; set; }

        [JsonPropertyName("data_limit")]
        public int DataLimit { get; set; }

        [JsonPropertyName("data_offset")]
        public int DataOffset { get; set; }

        [JsonPropertyName("data_count")]
        public int DataCount { get; set; }

        [JsonPropertyName("timestamp")]
        public string Timestamp { get; set; }

        [JsonPropertyName("timezone")]
        public string Timezone { get; set; }

        [JsonPropertyName("newbook_version")]
        public string NewbookVersion { get; set; }

        [JsonPropertyName("api_key")]
        public string ApiKey { get; set; }
    }

    public class BookingResponse2
    {
        [JsonPropertyName("success")]
        public string Success { get; set; }

        [JsonPropertyName("data")]
        public List<dynamic> Data { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; }

        [JsonPropertyName("data_total")]
        public int DataTotal { get; set; }

        [JsonPropertyName("data_limit")]
        public int DataLimit { get; set; }

        [JsonPropertyName("data_offset")]
        public int DataOffset { get; set; }

        [JsonPropertyName("data_count")]
        public int DataCount { get; set; }

        [JsonPropertyName("timestamp")]
        public string Timestamp { get; set; }

        [JsonPropertyName("timezone")]
        public string Timezone { get; set; }

        [JsonPropertyName("newbook_version")]
        public string NewbookVersion { get; set; }

        [JsonPropertyName("api_key")]
        public string ApiKey { get; set; }
    }

    public class BookingData
    {
        [JsonPropertyName("booking_id")]
        public int BookingId { get; set; }

        [JsonPropertyName("booking_arrival")]
        public string BookingArrival { get; set; }

        [JsonPropertyName("booking_departure")]
        public string BookingDeparture { get; set; }
        
        [JsonPropertyName("booking_placed")]
        public string BookingPlaced { get; set; }

        [JsonPropertyName("booking_eta")]
        public string BookingEta { get; set; }

        [JsonPropertyName("booking_length")]
        public int BookingLength { get; set; }

        [JsonPropertyName("booking_status")]
        public string BookingStatus { get; set; }

        [JsonPropertyName("site_id")]
        public string SiteID { get; set; }

        [JsonPropertyName("site_name")]
        public string SiteName { get; set; }

        [JsonPropertyName("category_id")]
        public string CategoryID { get; set; }

        [JsonPropertyName("category_name")]
        public string CategoryName { get; set; }

        [JsonPropertyName("tariffs_quoted")]
        public List<TariffQuoted> TariffsQuoted { get; set; }

        [JsonPropertyName("guests")]
        public List<Guest> Guests { get; set; }

        [JsonPropertyName("access_codes")]
        public List<AccessCode> AccessCodes { get; set; }
    }

    public class TariffQuoted
    {
        [JsonPropertyName("tariff_quoted_id")]
        public string TariffQuotedId { get; set; }

        [JsonPropertyName("label")]
        public string Label { get; set; }

        [JsonPropertyName("stay_date")]
        public string StayDate { get; set; }

        [JsonPropertyName("taxes")]
        public List<Tax> Taxes { get; set; }
    }

    public class Tax
    {
        [JsonPropertyName("tax_id")]
        public int TaxId { get; set; }

        [JsonPropertyName("tax_name")]
        public string TaxName { get; set; }

        [JsonPropertyName("tax_inclusive")]
        public bool TaxInclusive { get; set; }

        [JsonPropertyName("tax_amount")]
        public decimal TaxAmount { get; set; }
    }

    public class Guest
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("firstname")]
        public string Firstname { get; set; }

        [JsonPropertyName("lastname")]
        public string Lastname { get; set; }

        [JsonPropertyName("contact_details")]
        public List<ContactDetail> ContactDetails { get; set; }
        
        [JsonPropertyName("equipment")]
        public List<Equipment> Equipments { get; set; }

        [JsonPropertyName("street")]
        public string Street { get; set; }

        [JsonPropertyName("state")]
        public string state { get; set; }

        [JsonPropertyName("city")]
        public string city { get; set; }

        [JsonPropertyName("postcode")]
        public string postcode { get; set; }
        
        [JsonPropertyName("country")]
        public string country { get; set; }

    }

    public class ContactDetail
    {
        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("content")]
        public string Content { get; set; }
    }

    public class AccessCode
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("card_id")]
        public string CardId { get; set; }

        [JsonPropertyName("access_code")]
        public string AccessCodeValue { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("type_id")]
        public string TypeId { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; }

        [JsonPropertyName("car_rego")]
        public string CarRego { get; set; }

        [JsonPropertyName("period_from")]
        public string PeriodFrom { get; set; }

        [JsonPropertyName("period_to")]
        public string PeriodTo { get; set; }

        [JsonPropertyName("area_id")]
        public string AreaId { get; set; }

        [JsonPropertyName("created_when")]
        public string CreatedWhen { get; set; }

        [JsonPropertyName("cancelled_when")]
        public string CancelledWhen { get; set; }

        [JsonPropertyName("cancelled_reason")]
        public string CancelledReason { get; set; }

        [JsonPropertyName("cancelled_by")]
        public string CancelledBy { get; set; }

        [JsonPropertyName("access_code_status")]
        public string AccessCodeStatus { get; set; }

        [JsonPropertyName("area_name")]
        public string AreaName { get; set; }
    }

    public class Equipment
    {
        [JsonPropertyName("equipment_id")]
        public string Id { get; set; }

        [JsonPropertyName("equipment_registration")]
        public string Registration { get; set; }

        [JsonPropertyName("equipment_type_name")]
        public string TypeName { get; set; }

        [JsonPropertyName("equipment_make")]
        public string Make { get; set; }
        
        [JsonPropertyName("equipment_model")]
        public string Model { get; set; }
        
        [JsonPropertyName("equipment_name")]
        public string Name { get; set; }
    }
    public class BookingUpdateRequest
    {
        public string BookingId { get; set; }
        public List<AccessCodeUpdateModel>? AccessCodes { get; set; }
    }

    public class AccessCodeUpdateModel
    {
        public string? CardId { get; set; }
        public string? AccessCodeValue { get; set; }
        public string? Type { get; set; }
        public string? TypeId { get; set; }
        public string? Description { get; set; }
        public string? PeriodFrom { get; set; }
        public string? PeriodTo { get; set; }
        public string? AreaId { get; set; }
       
        //public string? AccessCodeStatus { get; set; }
        //public string? AreaName { get; set; }
    }
    public class BookingEmailStatus
    {
        [JsonPropertyName("EmailSent")]
        public bool EmailSent { get; set; }

        [JsonPropertyName("PassClaimed")]
        public bool PassClaimed { get; set; }

        [JsonPropertyName("EmailScheduled")]
        public bool EmailScheduled { get; set; }
    }

}

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App.Common.Model
{
    public class CustomerDTO
    {
        public int CUSTOMER_ID { get; set; }
        public required string CUSTOMER_FIRSTNAME { get; set; }
        public string? CUSTOMER_LASTNAME { get; set; }
        public string? CUSTOMER_EMAILCUSTOMER_EMAIL { get; set; }
        public string? CUSTOMER_EMAIL { get; set; }
        public int? COUNTRY_ID { get; set; }
        public int? STATE_ID { get; set; }
        public string? ADDRESS_LINE1 { get; set; }
        public string? PHONE_NUMBER { get; set; }
        public string? LOGO_NAME { get; set; }
        public byte[]? LOGO_IMAGE { get; set; }
        public string? WEBSITE_URL { get; set; }
        public string? NOTES { get; set; }
        public int ACTIVE_IND { get; set; }
        public int? CREATED_BY { get; set; }
        public int? UPDATED_BY { get; set; }
        public DateTime? CREATION_DATE { get; set; }
        public DateTime? UPDATE_DATE { get; set; }
        public string? CUST_ID { get; set; }
        public string? ALERT_MESSAGE { get; set; }
        public bool IsNewBookIntegration { get; set; }
        public string? NewBookApiKey { get; set; }
        public string? NewBookUsername { get; set; }
        public string? NewBookRegion { get; set; }
        public string? NewBookPassword { get; set; }
        public string? NewBookEndpoint { get; set; }
        public bool IsGeneticIntegration { get; set; }
        public string? GeneticUsername { get; set; }
        public string? GeneticPassword { get; set; }
        public string? GeneticBaseUrl { get; set; }
        public string? GenetecCustomCardFormatGUID { get; set; }
        public string? GenetecCustomerGroupGUID { get; set; }
        public ICollection<CustomerContact>? CustomerContacts { get; set; }
        public ICollection<Site>? Sites { get; set; }
    }
}

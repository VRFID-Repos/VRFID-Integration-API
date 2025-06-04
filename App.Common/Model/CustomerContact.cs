
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json.Serialization;

namespace App.Common.Model
{
    public class CustomerContact
    {
        public int CUSTOMER_CONTACT_ID { get; set; }
        public required string CONTACT_NAME { get; set; }
        public string? CONTACT_EMAIL { get; set; }
        public string? CONTACT_PHONE_NUMBER { get; set; }
        public string? CONTACT_ROLE { get; set; }
        public int? CREATED_BY { get; set; }
        public int? UPDATED_BY { get; set; }
        public DateTime? CREATION_DATE { get; set; }
        public DateTime? UPDATE_DATE { get; set; }
    }
}

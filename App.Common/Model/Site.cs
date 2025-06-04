using App.Common.Model;
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
    public class Site
    {
        public int SITE_ID { get; set; }
        public required string SITE_NAME { get; set; }
        public int? COUNTRY_ID { get; set; }
        public int? STATE_ID { get; set; }
        public string? SITE_ADDRESS { get; set; }
        public string? SITE_CONTACT { get; set; }
        public string? EMAIL_ID { get; set; }
        public string? PHONE_NUMBER { get; set; }
        public int? CREATED_BY { get; set; }
        public int? UPDATED_BY { get; set; }
        public DateTime? CREATION_DATE { get; set; }
        public DateTime? UPDATE_DATE { get; set; }
    }
}

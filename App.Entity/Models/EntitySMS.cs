using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App.Entity.Models
{
    public class EntitySMS
    {
    }
    public class SendSMSRequest
    {
        public string To { get; set; } // Recipient phone number
        public string Body { get; set; } // SMS text
    }
}

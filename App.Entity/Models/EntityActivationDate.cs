using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App.Entity.Models
{
    public class EntityActivationDate
    {
        public string CardholderId { get; set; } // The GUID or logical ID of the cardholder
        public DateTime ActivationDate { get; set; } // The activation date
        public DateTime? DeactivationDate { get; set; } // The optional deactivation date
    }
    public class CardholderActivationDateRequestModel
    {
        public string Cardholder { get; set; }  // This will hold the GUID of the cardholder
        public DateTime ActivationDate { get; set; }  // The activation date
    }
    public class CardholderDeactivationAtDateRequestModel
    {
        public string Cardholder { get; set; }  // This will hold the GUID of the cardholder
        public DateTime ExpirationDate { get; set; } // This will hold the expiration date
    }
    public class CardholderActivationWithExpirationRequestModel
    {
        public string Cardholder { get; set; }  // This will hold the GUID of the cardholder
        public DateTime ActivationDate { get; set; }  // The activation date
        public DateTime ExpirationDate { get; set; }  // The expiration date
    }


}

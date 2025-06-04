using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App.Entity.Models
{
    public class EntityCardholder
    {
        public string Name { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string MobilePhone { get; set; }
    }
    public class EntityUpdateCardholder
    {
        public string CardholderId { get; set; }
        public string Name { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string MobilePhone { get; set; }
    }
    public class CardholderCredentialsRequestModel
    {
        public string CardholderId { get; set; } // The GUID or logical ID of the cardholder
    }
    public class CardholderGroupAssignmentRequestModel
    {
        public string Group { get; set; } // Group GUID
        public string Member { get; set; } // Cardholder GUID
    }
    public class CardholderPictureRequestModel
    {
        public string Cardholder { get; set; } // Cardholder GUID
        public IFormFile Picture { get; set; } // The picture file to be uploaded
    }
    public class CardholderPictureRequestModel2
    {
        public string Cardholder { get; set; } // Cardholder GUID
    }
    public class CredentialAssignmentRequestModel
    {
        public string Cardholder { get; set; }
        public string Credential { get; set; }
    }
    public class GuidResponses
    {
        public string CardholderGuid { get; set; }
        public string LicenseCredGuid { get; set; }
        public string PinCredGuid { get; set; }
    }

}

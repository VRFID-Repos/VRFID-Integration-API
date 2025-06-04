using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App.Entity.Models
{
    public class EntityCredentials
    {
        public string Name { get; set; }
        public string LicensePlate { get; set; }
    }
    public class EntityCredentials2
    {
        public string Name { get; set; }
        public string Pin { get; set; }
        public string Group { get; set; }
        public string LicensePlate { get; set; }
        public string ActivationDateTime { get; set; }
        public string DeactivationDateTime { get; set; }
    }
    public class EntityCredentials3
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string LicensePlate { get; set; }
        public string ActivationDateTime { get; set; }
        public string DeactivationDateTime { get; set; }
    }
    public class EntityDeleteBooking
    {
        public string MethodOfDeactivatingAccessCode { get; set; }
        public string PhysicalAccessCode { get; set; }
        public string Action { get; set; }
    }
    public class LicensePlateCredentialRequestModel
    {
        [Required]
        public string LicensePlate { get; set; }
    }
    public class ActivateCredentialRequestModel
    {
        public string Credential { get; set; }
    }
    public class DeactivateCredentialRequest
    {
        public string Credential { get; set; } // Credential GUID or logical ID
        public DateTime ExpirationDate { get; set; } // Expiration date for deactivation
    }
    public class ActivateCredentialRequest
    {
        public string Credential { get; set; } // GUID or Logical ID of the credential
        public DateTime ActivationDate { get; set; } // Date of activation
        public DateTime ExpirationDate { get; set; } // Date of expiration
    }

    public class AssignCredentialRequestModel
    {
        public string Cardholder { get; set; }
        public string Credential { get; set; }
    }
    public class CreateCredentialWithPinRequest
    {
        public string Name { get; set; }
        public string CredentialCode { get; set; }
    }
    public class CreateCredentialWithCustomFormatRequest
    {
        public string Name { get; set; } // Credential name
        public string FormatId { get; set; } // Custom card format ID
        public Dictionary<string, string> KeyValues { get; set; } // Dynamic key-value pairs
    }

    public class CreateCredentialRequest
    {
        public string Name { get; set; }
    }
    public class GetCredentialRequest
    {
        public string Credential { get; set; }
    }
    public class CreateCredentialWithDetailsRequest
    {
        public string FirstName { get; set; }
        public string Pin { get; set; }
        public string LicensePlate { get; set; }
        public string CardHolderGroup { get; set; }
        public string FormatId { get; set; }
        public DateTime ActivationDateTime { get; set; }
        public DateTime ExpirationDateTime { get; set; }
    }
    public class ResponseModel
    {
        public ResponseRsp Rsp { get; set; }
    }

    public class ResponseRsp
    {
        public string Status { get; set; }
        public ResponseResult Result { get; set; }
    }

    public class ResponseResult
    {
        public string Guid { get; set; }
    }
    public class CreateDigitalPassCredentialRequest
    {
        public string Name { get; set; } // Credential name
        public string FormatId { get; set; } // FormatID: "718f3362-156f-4718-83b5-4d81dc2e4cf9"
        public string Identifier { get; set; } // The identifier received from Passcreator
    }
    public class CreateWiegandCredentialRequest
    {
        public string Name { get; set; }
        public int BitLength { get; set; }
        public string CustomFormatId { get; set; }
        public string RawData { get; set; }
    }
    public class PostPassCreationTaskModel
    {
        [Required]
        public string Identifier { get; set; } // The identifier received from Passcreator
    }

}

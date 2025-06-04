using App.Common.Model;
using App.Entity.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App.Bal.Services
{
    public interface IGenetecServices
    {
        public Task<ResponseModel> CreateCardholderFromNewBook(EntityCardholder cardholder, CustomerDTO customer);
        public Task<String> SetActivationAndExpirationDatesForNewBook(CardholderActivationWithExpirationRequestModel model, CustomerDTO customer);
        public Task<String> AddCardholderToGroupForNewbook(CardholderGroupRequestModel model, CustomerDTO customer);
        public Task<ResponseModel> CreateLicensePlateCredentialForNewBook(EntityCredentials model, CustomerDTO customer);
        public Task<ResponseModel> CreateCredentialWithPinForNewBook([FromBody] CreateCredentialWithPinRequest request, CustomerDTO customer);
        public Task<String> AssignCredentialToCardholder([FromBody] AssignCredentialRequestModel model, CustomerDTO customer);
        public Task<GuidResponses> CreateCardholderAndCredentialForNewBook([FromBody] EntityCredentials2 model,CustomerDTO customer);
        public Task<GuidResponses> CreateCardholderAndCredentialWithoutNewBookIntegration([FromBody] EntityCredentials3 model, CustomerDTO customer);
        public Task<ResponseModel> CreateDigitalPassCredential([FromBody] CreateDigitalPassCredentialRequest request, CustomerDTO customer);

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App.Common.Messages
{
    public class AppMessages
    {
        public const string MissingCredentials = "API credentials or URL are missing.";
        public const string FailedToFetch = "Failed to fetch online status.";
        public const string InvalidCardHolder = "All cardholder properties (Name, FirstName, LastName, Email, MobilePhone) are required.";
       
    }
}

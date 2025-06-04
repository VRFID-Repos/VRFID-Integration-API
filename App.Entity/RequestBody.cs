using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App.Entity
{
    // RequestBody.cs
    public class RequestBody
    {
        public string UniqueId { get; set; }
        public string FileUrl { get; set; }
        public string IsFileUrlPublic { get; set; }
        public string Metadata { get; set; }
        public string ParentDelimiter { get; set; }
        public string ChildDelimiter { get; set; }
        public string IsPublic { get; set; }
        public string AuthorizedUsers { get; set; }
        public string RedirectUrl { get; set; }
        public string IsVerificationGatewayRequired { get; set; }
        public string SendMetadataToBlockchain { get; set; }
        public string MetadataForBlockchain { get; set; }
        public string IsParent { get; set; }
        public string ParentId { get; set; }
    }

}

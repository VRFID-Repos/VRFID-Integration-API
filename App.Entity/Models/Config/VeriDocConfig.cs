using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App.Entity.Models.Config
{
    public class VeriDocConfig
    {
        public const string Path = "VeriDocConfig";

        public string ApiKey { get; set; } = string.Empty;
        public string SecretKey { get; set; } = string.Empty;
        public string GetQRURL { get; set; } = string.Empty;
        public string SubmitDocURL { get; set; } = string.Empty;
        public string GetBlockChainStatusURL { get; set; } = string.Empty;
        
    }

    public class PdfSubmitApiResult
    {
        [JsonProperty("filehash")]
        public string? Filehash { get; set; }


        [JsonProperty("returncode")]
        public string? ReturnCode { get; set; }


        [JsonProperty("uniqueid")]
        public string? UniqueId { get; set; }

        [JsonProperty("message")]
        public string? Message { get; set; }
    }


    public class GetQRApiResult
    {
        [JsonProperty("qr")]
        public string? BlockChainURL { get; set; }


        [JsonProperty("qrimage")]
        public string? QRCode { get; set; }


        [JsonProperty("uniqueId")]
        public string? UniqueId { get; set; }

        [JsonProperty("returncode")]
        public string? ReturnCode { get; set; }
        
        [JsonProperty("message")]
        public string? Message { get; set; }
    }

    public class BlockchainStatus
    {
        [JsonProperty("guid")]
        public string Guid { get; set; } = string.Empty;

        [JsonProperty("transactionid")]
        public string TransectionId { get; set; } = string.Empty;

        [JsonProperty("blockchainstatus")]
        public string Status { get; set; } = string.Empty;

        [JsonProperty("returncode")]
        public string ReturnCode { get; set; } = string.Empty;

        [JsonProperty("blockchainurl")]
        public string BlockchainUrl { get; set; } = string.Empty;

        [JsonProperty("parentqrcode")]
        public ParentQrcode ParentQrcode { get; set; } = new ParentQrcode();
    }

    public class ParentQrcode
    {
        [JsonProperty("verifyurl")]
        public string VerifyLink { get; set; } = string.Empty;

        [JsonProperty("blockchainurl")]
        public string BlockchainUrl { get; set; } = string.Empty;

        [JsonProperty("transactionid")]
        public string TransectionId { get; set; } = string.Empty;

        [JsonProperty("blockchainstatus")]
        public string Status { get; set; } = string.Empty;
    }
}

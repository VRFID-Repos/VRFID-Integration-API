using System;

namespace App.Entity.Dto
{
    public class VerifyDto
    {
        public required string Email { get; set; }
        public string Password { get; set; }
        public long Timestamp { get; set; }

        public string? TokenId { get; set; } 
    }
}

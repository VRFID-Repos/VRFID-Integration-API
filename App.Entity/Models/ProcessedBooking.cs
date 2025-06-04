using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App.Entity.Models
{
    public class ProcessedBooking
    {
        [Key]
        [Required]
        public string BookingId { get; set; } // Primary Key

        [Required]
        public int CustomerId { get; set; } // Foreign Key linking to customers

        [Required]
        public DateTime ProcessedAt { get; set; } // Timestamp when booking was processed

        [Required]
        public bool EmailSent { get; set; }

        [Required]
        public bool PassClaimed { get; set; }

        [Required]
        public bool EmailScheduled { get; set; }
    }
}

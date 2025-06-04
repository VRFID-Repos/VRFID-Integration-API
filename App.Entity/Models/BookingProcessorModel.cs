using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App.Entity.Models
{
    public class Customer
    {
        public int CustomerId { get; set; }
        public string Name { get; set; }
        public NewBookApiConfig ApiConfiguration { get; set; }
    }

    public class NewBookApiConfig
    {
        public string Endpoint { get; set; }
        public string ApiKey { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Region { get; set; }
    }

    public class Booking
    {
        public string BookingId { get; set; }
        public DateTime BookingArrival { get; set; }
        public DateTime BookingDeparture { get; set; }
        public List<Guest> Guests { get; set; }
        public string SiteName { get; set; }
    }

}

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App.Entity.Models.Admin
{
    public class Country
    {
        [Key]
        public int Id { get; set; }

        [StringLength(50)]
        public string? ISO { get; set; }

        [StringLength(200)]
        public string? Name { get; set; }

        [StringLength(200)]
        public string? NickName { get; set; }

        [StringLength(50)]
        public string? ISO3 { get; set; }
        public short? NumCode { get; set; }
        public int? PhoneCode { get; set; }

        [NotMapped]
        public string? PhoneNumber { get; set; }
    }
}

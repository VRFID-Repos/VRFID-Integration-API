using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App.Entity.Models
{
    public class EntityCardHolderGroup
    {
        public string Name { get; set; }
        public string Email { get; set; }
    }
    public class EntityCardHolderGroupLevel
    {
        public string Group { get; set; }
        public string Level { get; set; }
    }
    public class CardholderGroupRequestModel
    {
        public string Group { get; set; }
        public string Cardholder { get; set; }
    }

}

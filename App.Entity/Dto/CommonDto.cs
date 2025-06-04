using App.Entity.Models;
using App.Entity.Models.Admin;

namespace App.Entity.Dto
{
    public class CommonDto
    {
        public CommonDto()
        {
            Countries = new List<Country>();
        }

        public List<Country> Countries { get; set; }
        public AppUser? AppUser { get; set; }

    }
}

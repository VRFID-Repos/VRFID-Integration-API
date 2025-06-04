using App.Entity.Models;
using App.Entity.Models.Admin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App.Entity.Dto
{
    public class AdminDto : CommonDto
    {
        public AdminDto()
        {
            users = new List<AppUser>();
        }
        public List<AppUser> users;
    }
}

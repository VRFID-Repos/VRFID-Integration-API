using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App.Entity.Models
{
    public class PagingRequestModel
    {
        public int Page { get; set; }
        public int PageSize { get; set; }
    }
}

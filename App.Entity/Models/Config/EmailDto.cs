using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App.Entity.Models.Config
{
    public class EmailDto
    {
        public string? FullName { get; set; }
        public string? VerifyLink { get; set; }
        public string? Email { get; set; }
        public string? InstructorName { get; set; }
        public string? CourseName { get; set; }
        public string? Password { get; set; }
    }
}

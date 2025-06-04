using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace App.Common.Support
{
    public partial class PasswordUtil
    {
        public static bool ValidatePassword(string password)
        {
            Regex regex = MyRegex();
            return regex.IsMatch(password);
        }

        [GeneratedRegex("^(?=.*?[A-Z])(?=.*?[a-z])(?=.*?[0-9])(?=.*?[#?!@$%^&*-]).{8,}$", RegexOptions.Compiled)]
        private static partial Regex MyRegex();
    }
}

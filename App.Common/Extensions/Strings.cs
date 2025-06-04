using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App.Common.Extensions
{
    public class Strings
    {
        public const long MaxLength = 8;
        public const long MinLength = 1;

        public static long GenerateRandom(int length)
        {
            if (length < MinLength || length > MaxLength)
            {
                return 0;
            }
            string min = "1".PadRight(length, '0');
            string max = "".PadRight(length, '9');
            Random random = new();
            return random.NextInt64(long.Parse(min), long.Parse(max));
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App.Bal.Services
{
    public interface ISMSService
    {
        Task<string> SendSmsAsync(string to, string body);
    }
}

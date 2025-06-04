using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App.Common.Support
{
    public class LoggerHelper
    {
        public static void LogError(Exception e)
        {
            string path = Path.Combine(Directory.GetCurrentDirectory(), "Logs");
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            path = Path.Combine(Directory.GetCurrentDirectory(), "Logs", "log.txt");

            string datetime = DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss tt");
            string message1 = $"{datetime}\t {e.Message}";
            string message2 = $"\t\t\t {e.StackTrace}";
            string message = $"{message1}{Environment.NewLine}{message2}{Environment.NewLine}";
            File.AppendAllText(path, message);
        }
    }
}

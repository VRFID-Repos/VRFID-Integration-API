using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Session;
using Microsoft.AspNetCore.Http;

namespace App.Common.Extensions
{
    public static class SessionExtensions
    {
        //public static void SetObject<T>(this ISessionStore session, string key, T value)
        //{
        //    session.SetObject(key, JsonConvert.SerializeObject(value));
        //}

        //public static T GetObject<T>(this ISession session, string key)
        //{
        //    var value = session.GetObject(key);
        //    return value == null ? default : JsonConvert.DeserializeObject<T>(value);
        //}
    }
}

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CWC.Common
{
    public class JsonHelper
    {
        public static T DeserializeObject<T>(string json)
        {
            return JsonConvert.DeserializeObject<T>(json);
        }

        public static object DeserializeObject(string json)
        {
            return JsonConvert.DeserializeObject(json);
        }

        public static object DeserializeObject(string json, Type type)
        {
            return JsonConvert.DeserializeObject(json, type);
        }

        public static string SerializeObject(object obj, string dateTimeFormat)
        {
            return JsonConvert.SerializeObject(obj, Formatting.Indented, new JsonConverter[]
            {
                new IsoDateTimeConverter
                {
                    DateTimeFormat = dateTimeFormat
                }
            });
        }

        public static string SerializeObject(object obj)
        {
            return JsonHelper.SerializeObject(obj, "yyyy-MM-dd HH:mm:ss");
        }
    }
}
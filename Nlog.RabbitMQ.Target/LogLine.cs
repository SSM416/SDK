using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Nlog.RabbitMQ.Target
{
    public class LogLine
    {
        [JsonProperty("@source")]
        public Uri Source
        {
            get;
            set;
        }

        [JsonProperty("@timestamp")]
        public string TimeStampISO8601
        {
            get;
            set;
        }

        [JsonProperty("@message")]
        public string Message
        {
            get;
            set;
        }

        [JsonProperty("@fields")]
        public IDictionary<string, object> Fields
        {
            get;
            set;
        }

        [JsonProperty("@tags")]
        public HashSet<string> Tags
        {
            get;
            set;
        }

        [JsonProperty("@type")]
        public string Type
        {
            get;
            set;
        }

        [JsonProperty("level")]
        public string Level
        {
            get;
            set;
        }
    }
}
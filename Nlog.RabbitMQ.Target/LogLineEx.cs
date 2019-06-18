using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Nlog.RabbitMQ.Target
{
    public static class LogLineEx
    {
        public static void EnsureADT(this LogLine line)
        {
            bool flag = line.Fields == null;
            if (flag)
            {
                line.Fields = new Dictionary<string, object>();
            }
            bool flag2 = line.Tags == null;
            if (flag2)
            {
                line.Tags = new HashSet<string>();
            }
        }

        public static void AddField(this LogLine line, string key, string name, object value)
        {
            bool flag = line.Fields == null;
            if (flag)
            {
                line.Fields = new Dictionary<string, object>();
            }
            bool flag2 = line.Fields.ContainsKey(key);
            if (flag2)
            {
                line.Fields.Remove(key);
            }
            bool flag3 = string.IsNullOrEmpty(name) || value == null;
            if (!flag3)
            {
                line.AddField(name, value);
            }
        }

        public static void AddField(this LogLine line, string name, object value)
        {
            bool flag = string.IsNullOrEmpty(name) || value == null;
            if (!flag)
            {
                bool flag2 = line.Fields == null;
                if (flag2)
                {
                    line.Fields = new Dictionary<string, object>();
                }
                line.Fields[name] = value;
            }
        }

        public static void AddTag(this LogLine line, string tag)
        {
            bool flag = tag == null;
            if (!flag)
            {
                bool flag2 = line.Tags == null;
                if (flag2)
                {
                    line.Tags = new HashSet<string>
                    {
                        tag
                    };
                }
                else
                {
                    line.Tags.Add(tag);
                }
            }
        }
    }
}
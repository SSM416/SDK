using Newtonsoft.Json;
using NLog;
using NLog.Layouts;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;

namespace Nlog.RabbitMQ.Target
{
    public static class MessageFormatter
    {
        private static readonly DateTime _epoch = new DateTime(1970, 1, 1, 0, 0, 0, 0);

        private static string _hostName;

        private static string HostName
        {
            get
            {
                return _hostName = (_hostName ?? Dns.GetHostName());
            }
        }

        public static string GetMessageInner(bool useJSON, Layout layout, LogEventInfo info, IList<Field> fields)
        {
            return GetMessageInner(useJSON, false, layout, info, fields);
        }

        public static string GetMessageInner(bool useJSON, bool useLayoutAsMessage, Layout layout, LogEventInfo logEvent, IList<Field> fields)
        {
            bool flag = !useJSON;
            string result;
            if (flag)
            {
                result = layout.Render(logEvent);
            }
            else
            {
                LogLine logLine = new LogLine
                {
                    TimeStampISO8601 = logEvent.TimeStamp.ToUniversalTime().ToString("o", CultureInfo.InvariantCulture),
                    Message = useLayoutAsMessage ? layout.Render(logEvent) : logEvent.FormattedMessage,
                    Level = logEvent.Level.Name,
                    Type = "amqp",
                    Source = new Uri(string.Format("nlog://{0}/{1}", HostName, logEvent.LoggerName))
                };
                logLine.AddField("exception", logEvent.Exception);
                bool hasProperties = logEvent.HasProperties;
                if (hasProperties)
                {
                    bool flag2 = logEvent.Properties.ContainsKey("fields");
                    if (flag2)
                    {
                        foreach (KeyValuePair<string, object> current in ((IEnumerable<KeyValuePair<string, object>>)logEvent.Properties["fields"]))
                        {
                            logLine.AddField(current.Key, current.Value);
                        }
                    }
                    bool flag3 = logEvent.Properties.ContainsKey("tags");
                    if (flag3)
                    {
                        foreach (string current2 in ((IEnumerable<string>)logEvent.Properties["tags"]))
                        {
                            logLine.AddTag(current2);
                        }
                    }
                    foreach (KeyValuePair<object, object> current3 in logEvent.Properties)
                    {
                        string text = current3.Key as string;
                        bool flag4 = text == null || text == "tags" || text == "fields";
                        if (!flag4)
                        {
                            logLine.AddField(text, current3.Value);
                        }
                    }
                }
                bool flag5 = fields != null;
                if (flag5)
                {
                    foreach (Field current4 in fields)
                    {
                        logLine.AddField(current4.Key, current4.Name, current4.Layout.Render(logEvent));
                    }
                }
                logLine.EnsureADT();
                result = JsonConvert.SerializeObject(logLine);
            }
            return result;
        }

        public static long GetEpochTimeStamp(LogEventInfo @event)
        {
            return Convert.ToInt64((@event.TimeStamp - _epoch).TotalSeconds);
        }
    }
}
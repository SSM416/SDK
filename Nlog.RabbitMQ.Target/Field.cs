using NLog.Config;
using NLog.Layouts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Nlog.RabbitMQ.Target
{
    [NLogConfigurationItem, ThreadAgnostic]
    public class Field
    {
        public string Name
        {
            get;
            set;
        }

        [RequiredParameter]
        public string Key
        {
            get;
            set;
        }

        [RequiredParameter]
        public Layout Layout
        {
            get;
            set;
        }

        public Field() : this(null, null, null)
        {
        }

        public Field(string key, string name, Layout layout)
        {
            this.Key = key;
            bool flag = string.IsNullOrEmpty(name);
            if (flag)
            {
                name = key;
            }
            this.Name = name;
            this.Layout = layout;
        }
    }
}
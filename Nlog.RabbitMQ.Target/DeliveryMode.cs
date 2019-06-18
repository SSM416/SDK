using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Nlog.RabbitMQ.Target
{
    public enum DeliveryMode
    {
        NonPersistent = 1,
        Persistent
    }
}
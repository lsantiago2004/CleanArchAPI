using System;
using System.Collections.Generic;
using System.Text;

namespace Sheev.Common.Models
{
    public class ServiceBusSetting
    {
        #region Properties
        public string ConnectionString { get; set; }
        public List<Topic> Topics { get; set; }
        public List<Subscription> Subscriptions { get; set; }
        #endregion

        #region Constructor(s)
        public ServiceBusSetting()
        {
            Topics = new List<Topic>();
            Subscriptions = new List<Subscription>();
        }
        #endregion
    }

    public class Topic
    {
        public string Name { get; set; }
    }

    public class Subscription
    {
        public string Name { get; set; }
    }
}

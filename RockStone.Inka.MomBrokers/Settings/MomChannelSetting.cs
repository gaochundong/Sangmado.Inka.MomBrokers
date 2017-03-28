using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace RockStone.Inka.MomBrokers
{
    public class MomChannelSetting
    {
        public MomChannelSetting()
        {
        }

        public string ClientServiceName { get; set; }

        // exclusive = delete when declaring *connection* closes. No other
        // connection can even access this. Only queues can be exclusive.

        // autodelete = delete when the *last* downstream thing(i.e.consumers for 
        // a queue, or queues for an exchange) goes away. Note that this isn't 
        // bound to channels or connections at all really.

        // exclusive is more frequently useful than autodelete.

        /// <summary>
        /// Convenience class providing compile-time names for standard exchange types.
        /// (direct, fanout, headers, topic)
        /// </summary>
        public string ExchangeType { get; set; }
        /// <summary>
        /// Exchanges can be durable or transient. Durable exchanges survive broker 
        /// restart whereas transient exchanges do not (they have to be redeclared 
        /// when broker comes back online). Not all scenarios and use cases require 
        /// exchanges to be durable.
        /// </summary>
        public bool ExchangeDurable { get; set; }
        /// <summary>
        /// Exchange is deleted when all queues have finished using it.
        /// </summary>
        public bool ExchangeAutoDelete { get; set; }
        public Dictionary<string, object> ExchangeArguments { get; set; }

        /// <summary>
        /// Durable queues are persisted to disk and thus survive broker restarts. 
        /// Queues that are not durable are called transient. 
        /// Not all scenarios and use cases mandate queues to be durable.
        /// Durability of a queue does not make messages that are routed to that queue durable.
        /// If broker is taken down and then brought back up, 
        /// durable queue will be re-declared during broker startup, 
        /// however, only persistent messages will be recovered.
        /// </summary>
        public bool QueueDurable { get; set; }
        /// <summary>
        /// Used by only one connection and the queue will be deleted when that connection closes.
        /// </summary>
        public bool QueueExclusive { get; set; }
        /// <summary>
        /// Queue is deleted when last consumer unsubscribes.
        /// </summary>
        public bool QueueAutoDelete { get; set; }
        public bool QueueNoAck { get; set; }
        public Dictionary<string, object> QueueArguments { get; set; }

        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture,
                "ClientServiceName[{0}], ExchangeType[{1}], ExchangeDurable[{2}], ExchangeAutoDelete[{3}], ExchangeArguments[{4}], "
                + "QueueDurable[{5}], QueueExclusive[{6}], QueueAutoDelete[{7}], QueueNoAck[{8}], QueueArguments[{9}]",
                ClientServiceName, ExchangeType, ExchangeDurable, ExchangeAutoDelete,
                ExchangeArguments == null ? "" :
                    string.Join(",", ExchangeArguments.Select(kv => string.Format("[{0}:{1}]", kv.Key, kv.Value))),
                QueueDurable, QueueExclusive, QueueAutoDelete, QueueNoAck,
                QueueArguments == null ? "" :
                    string.Join(",", QueueArguments.Select(kv => string.Format("[{0}:{1}]", kv.Key, kv.Value))));
        }
    }
}

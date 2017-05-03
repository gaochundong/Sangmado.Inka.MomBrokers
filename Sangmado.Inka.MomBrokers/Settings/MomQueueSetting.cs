using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Sangmado.Inka.MomBrokers
{
    public class MomQueueSetting
    {
        public MomQueueSetting()
        {
        }

        public string QueueName { get; set; }

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

        public List<string> QueueBindRoutingKeys { get; set; }

        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture,
                "QueueName[{0}], QueueDurable[{1}], QueueExclusive[{2}], QueueAutoDelete[{3}], QueueNoAck[{4}], QueueArguments[{5}], QueueBindRoutingKeys[{6}]",
                QueueName, QueueDurable, QueueExclusive, QueueAutoDelete, QueueNoAck,
                QueueArguments == null ? string.Empty :
                    string.Join(",", QueueArguments.Select(kv => string.Format("[{0}:{1}]", kv.Key, kv.Value))),
                QueueBindRoutingKeys == null ? string.Empty :
                    string.Join(";", QueueBindRoutingKeys.ToArray()));
        }
    }
}

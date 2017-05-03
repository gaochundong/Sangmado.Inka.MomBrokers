using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Sangmado.Inka.MomBrokers
{
    public class MomExchangeSetting
    {
        public MomExchangeSetting()
        {
        }

        public string ExchangeName { get; set; }

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

        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture,
                "ExchangeName[{0}], ExchangeType[{1}], ExchangeDurable[{2}], ExchangeAutoDelete[{3}], ExchangeArguments[{4}]",
                ExchangeName, ExchangeType, ExchangeDurable, ExchangeAutoDelete,
                ExchangeArguments == null ? string.Empty :
                    string.Join(",", ExchangeArguments.Select(kv => string.Format("[{0}:{1}]", kv.Key, kv.Value))));
        }
    }
}

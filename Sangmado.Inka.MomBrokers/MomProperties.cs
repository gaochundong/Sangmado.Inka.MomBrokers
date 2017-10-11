using System.Collections.Generic;
using RabbitMQ.Client;

namespace Sangmado.Inka.MomBrokers
{
    internal class MomProperties : IMomProperties
    {
        public MomProperties(IBasicProperties basicProperties)
        {
            BasicProperties = basicProperties;
        }

        public IBasicProperties BasicProperties { get; }

        public IDictionary<string, object> Headers
        {
            get => BasicProperties.Headers;
            set => BasicProperties.Headers = value;
        }
    }
}
using System.Collections.Generic;
using RabbitMQ.Client;

namespace Sangmado.Inka.MomBrokers
{
    internal class MomMomProperties : IMomProperties
    {
        public MomMomProperties(IBasicProperties basicProperties)
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
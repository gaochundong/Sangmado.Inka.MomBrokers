using System.Collections.Generic;
using RabbitMQ.Client;

namespace Sangmado.Inka.MomBrokers
{
    public interface IMomBasicProperties
    {
        IDictionary<string, object> Headers { get; set; }
        bool IsHeadersPresent();

        bool Persistent { get; set; }
        bool IsPersistentPresent();

        void FulfillBasicProperties(IBasicProperties basicProperties);
    }
}
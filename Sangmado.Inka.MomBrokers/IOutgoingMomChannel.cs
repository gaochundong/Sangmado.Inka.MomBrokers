
using RabbitMQ.Client;

namespace Sangmado.Inka.MomBrokers
{
    public interface IOutgoingMomChannel : IMomChannel
    {
        void Publish(byte[] message);
        void Publish(byte[] message, string routingKey);
        void Publish(byte[] message, string routingKey, bool mandatory);
        void Publish(byte[] message, string routingKey, IMomProperties basicProperties);
        IMomProperties BuildMomProperties();
    }
}

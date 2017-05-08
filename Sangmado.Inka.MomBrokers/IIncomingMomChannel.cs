using System;

namespace Sangmado.Inka.MomBrokers
{
    public interface IIncomingMomChannel : IMomChannel
    {
        bool IsConsuming { get; }

        void StartConsume();
        void StopConsume();

        event EventHandler<MessageReceivedEventArgs> Received;

        void Ack(ulong deliveryTag);
        void Ack(ulong deliveryTag, bool multiple);
        void Nack(ulong deliveryTag, bool multiple, bool requeue);
        void Reject(ulong deliveryTag, bool requeue);
        void Recover(bool requeue);
    }
}

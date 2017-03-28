using System;

namespace RockStone.Inka.MomBrokers
{
    public interface IIncomingMomChannel : IMomChannel
    {
        bool IsConsuming { get; }

        void StartConsume();
        void StopConsume();

        event EventHandler<MessageReceivedEventArgs> Received;

        void Ack(ulong deliveryTag);
    }
}

using System;

namespace Sangmado.Inka.MomBrokers
{
    public interface IMomChannel
    {
        MomExchangeSetting ExchangeSetting { get; }
        MomQueueSetting QueueSetting { get; }

        bool IsConnected { get; }

        event EventHandler<MomChannelConnectedEventArgs> Connected;
        event EventHandler<MomChannelDisconnectedEventArgs> Disconnected;

        void Connect();
        void Disconnect();
    }
}

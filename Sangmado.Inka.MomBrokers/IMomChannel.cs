using System;

namespace Sangmado.Inka.MomBrokers
{
    public interface IMomChannel
    {
        MomChannelAddress Address { get; }
        MomChannelSetting Setting { get; }

        bool IsConnected { get; }

        event EventHandler<MomChannelConnectedEventArgs> Connected;
        event EventHandler<MomChannelDisconnectedEventArgs> Disconnected;

        void Connect();
        void Disconnect();
    }
}

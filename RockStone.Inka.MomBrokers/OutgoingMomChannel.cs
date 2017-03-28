using System.Threading;
using RockStone.Inka.Extensions;
using RockStone.Inka.Logging;

namespace RockStone.Inka.MomBrokers
{
    public class OutgoingMomChannel : ReconnectableMomChannel, IOutgoingMomChannel
    {
        private ILog _log = Logger.Get<OutgoingMomChannel>();

        public OutgoingMomChannel(MomHostSetting host, MomChannelAddress address, MomChannelSetting setting)
          : base(host, address, setting)
        {
        }

        public void Publish(byte[] message)
        {
            Guard.ArgumentNotNull(message, "message");

            if (!IsConnected)
            {
                throw new MomChannelNotConnectedException(
                    string.Format("The channel hasn't been connected, Host[{0}], Address[{1}].", this.Host, this.Address));
            }

            lock (_pipelining)
            {
#if VERBOSE
                _log.DebugFormat("Publish, IsChannelOpen[{0}], ExchangeName[{1}], RoutingKey[{2}], MessageLength[{3}], on Thread[{4}].",
                    this.Channel == null ? false : this.Channel.IsOpen,
                    this.Address.ExchangeName,
                    this.Address.RoutingKey,
                    message.Length,
                    Thread.CurrentThread.GetDescription());
#endif

                this.Channel.BasicPublish(this.Address.ExchangeName, this.Address.RoutingKey, false, null, message);
            }
        }

        public void Publish(byte[] message, long token)
        {
            Guard.ArgumentNotNull(message, "message");

            if (!IsConnected)
            {
                throw new MomChannelNotConnectedException(
                    string.Format("The channel hasn't been connected, Host[{0}], Address[{1}].", this.Host, this.Address));
            }

            lock (_pipelining)
            {
                string routingKey = this.Address.GetRoutingKey(token);

#if VERBOSE
                _log.DebugFormat("Publish, IsChannelOpen[{0}], ExchangeName[{1}], RoutingKey[{2}], MessageLength[{3}], on Thread[{4}].",
                    this.Channel == null ? false : this.Channel.IsOpen,
                    this.Address.ExchangeName,
                    routingKey,
                    message.Length,
                    Thread.CurrentThread.GetDescription());
#endif

                this.Channel.BasicPublish(this.Address.ExchangeName, routingKey, false, null, message);
            }
        }
    }
}

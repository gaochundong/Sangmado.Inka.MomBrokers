using System;
using System.Threading;
using Sangmado.Inka.Logging;

namespace Sangmado.Inka.MomBrokers
{
    public class OutgoingMomChannel : ReconnectableMomChannel, IOutgoingMomChannel
    {
        private ILog _log = Logger.Get<OutgoingMomChannel>();

        public OutgoingMomChannel(MomHostSetting host, MomExchangeSetting exchange, MomQueueSetting queue)
          : base(host, exchange, queue)
        {
        }

        public void Publish(byte[] message)
        {
            Publish(message, null);
        }

        public void Publish(byte[] message, string routingKey)
        {
            if (message == null)
                throw new ArgumentNullException("message");

            if (!IsConnected)
            {
                throw new MomChannelNotConnectedException(
                    string.Format("The channel hasn't been connected, HostSetting[{0}], ExchangeSetting[{1}].",
                        this.HostSetting, this.ExchangeSetting));
            }

            lock (_pipelining)
            {
#if VERBOSE
                _log.DebugFormat("Publish, IsChannelOpen[{0}], ExchangeName[{1}], RoutingKey[{2}], MessageLength[{3}], on Thread[{4}].",
                    this.Channel == null ? false : this.Channel.IsOpen,
                    this.ExchangeSetting.ExchangeName,
                    routingKey,
                    message.Length,
                    Thread.CurrentThread.GetDescription());
#endif

                this.Channel.BasicPublish(this.ExchangeSetting.ExchangeName, routingKey, false, null, message);
            }
        }
    }
}

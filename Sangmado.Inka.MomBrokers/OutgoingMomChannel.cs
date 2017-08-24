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

        public OutgoingMomChannel(MomHostSetting host, MomExchangeSetting exchange, MomQueueSetting queue, TimeSpan retryPeriod)
            : base(host, exchange, queue, retryPeriod)
        {
        }

        public void Publish(byte[] message)
        {
            Publish(message, string.Empty);
        }

        public void Publish(byte[] message, string routingKey)
        {
            // This flag tells the server how to react if the message cannot be routed to a queue.
            // If this flag is set, the server will return an unroutable message with a Return method. 
            // If this flag is zero, the server silently drops the message.
            bool mandatory = false;
            Publish(message, routingKey, mandatory);
        }

        public void Publish(byte[] message, string routingKey, bool mandatory)
        {
            if (message == null)
                throw new ArgumentNullException("message");
            if (routingKey == null)
                throw new ArgumentNullException("routingKey");

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
                try
                {
                    this.Channel.BasicPublish(this.ExchangeSetting.ExchangeName, routingKey, mandatory, null, message);
                }
                catch (Exception ex)
                {
                    _log.Error(ex.Message, ex);
                    AbnormalDisconnect();
                    throw;
                }
            }
        }
    }
}

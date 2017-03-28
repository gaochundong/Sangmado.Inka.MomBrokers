using System;
using System.Globalization;

namespace Sangmado.Inka.MomBrokers
{
    public class MessageReceivedEventArgs : EventArgs
    {
        private IIncomingMomChannel _channel;

        public MessageReceivedEventArgs()
        {
        }

        public MessageReceivedEventArgs(IIncomingMomChannel channel)
            : this()
        {
            _channel = channel;
        }

        public string ConsumerTag { get; set; }
        public ulong DeliveryTag { get; set; }
        public string ExchangeName { get; set; }
        public string RoutingKey { get; set; }
        public byte[] Body { get; set; }

        public void Ack()
        {
            if (_channel != null)
            {
                _channel.Ack(this.DeliveryTag);
            }
        }

        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture,
                "ConsumerTag[{0}], DeliveryTag[{1}], ExchangeName[{2}], RoutingKey[{3}], BodyLength[{4}]",
                ConsumerTag, DeliveryTag, ExchangeName, RoutingKey, Body == null ? 0 : Body.Length);
        }
    }
}

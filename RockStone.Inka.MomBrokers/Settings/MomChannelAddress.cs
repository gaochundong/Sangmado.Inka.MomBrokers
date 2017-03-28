using System.Globalization;

namespace RockStone.Inka.MomBrokers
{
    public class MomChannelAddress
    {
        public MomChannelAddress()
        {
        }

        public MomChannelAddress(string exchangeName, string routingKey)
            : this()
        {
            this.ExchangeName = exchangeName;
            this.RoutingKey = routingKey;
        }

        public MomChannelAddress(string exchangeName, string routingKey, string queueName)
            : this()
        {
            this.ExchangeName = exchangeName;
            this.RoutingKey = routingKey;
            this.QueueName = queueName;
        }

        public string ExchangeName { get; set; }
        public string QueueName { get; set; }
        public string RoutingKey { get; set; }

        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture,
                "ExchangeName[{0}], QueueName[{1}], RoutingKey[{2}]",
                ExchangeName, QueueName, RoutingKey);
        }

        public MomChannelAddress Clone()
        {
            return new MomChannelAddress()
            {
                ExchangeName = this.ExchangeName,
                QueueName = this.QueueName,
                RoutingKey = this.RoutingKey,
            };
        }

        public virtual string GetRoutingKey(long token)
        {
            return this.RoutingKey;
        }
    }
}

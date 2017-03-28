using System.Globalization;

namespace RockStone.Inka.MomBrokers
{
    public class MomChannelClusteredAddress : MomChannelAddress
    {
        public MomChannelClusteredAddress()
            : base()
        {
            IsClustered = false;
            ClusterNodeCount = 1;
        }

        public MomChannelClusteredAddress(string exchangeName, string routingKey)
            : base(exchangeName, routingKey)
        {
            IsClustered = false;
            ClusterNodeCount = 1;
        }

        public MomChannelClusteredAddress(string exchangeName, string routingKey, string queueName)
            : base(exchangeName, routingKey, queueName)
        {
            IsClustered = false;
            ClusterNodeCount = 1;
        }

        public bool IsClustered { get; set; }
        public int ClusterNodeCount { get; set; }
        public string ClusterNodePattern { get; set; }

        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture,
                "ExchangeName[{0}], QueueName[{1}], RoutingKey[{2}], IsClustered[{3}], ClusterNodeCount[{4}], ClusterNodePattern[{5}]",
                ExchangeName, QueueName, RoutingKey, IsClustered, ClusterNodeCount, ClusterNodePattern);
        }

        public new MomChannelClusteredAddress Clone()
        {
            return new MomChannelClusteredAddress()
            {
                ExchangeName = this.ExchangeName,
                QueueName = this.QueueName,
                RoutingKey = this.RoutingKey,

                IsClustered = this.IsClustered,
                ClusterNodeCount = this.ClusterNodeCount,
                ClusterNodePattern = this.ClusterNodePattern,
            };
        }

        public override string GetRoutingKey(long token)
        {
            string routingKey = this.RoutingKey;

            if (this.IsClustered)
            {
                routingKey = string.Format(CultureInfo.InvariantCulture,
                    this.ClusterNodePattern,
                    this.RoutingKey,
                    token % this.ClusterNodeCount);
            }

            return routingKey;
        }
    }
}

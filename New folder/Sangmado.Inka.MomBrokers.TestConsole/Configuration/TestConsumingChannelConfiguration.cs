using System.Linq;
using Sangmado.Inka.Configuration;

namespace Sangmado.Inka.MomBrokers.TestConsole
{
    public static class TestConsumingChannelConfiguration
    {
        private static readonly IAppSetting _appSetting = AppSetting.Singleton();

        public static MomHostSetting GetHostSetting()
        {
            var host = new MomHostSetting()
            {
                AutoClose = false,
                RequestedConnectionTimeout = 60000,
                RequestedHeartbeat = 30,

                ClientServiceName = _appSetting.GetItem<string>(@"TestConsumingChannelHostClientServiceName"),

                HostName = _appSetting.GetItem<string>(@"TestConsumingChannelHostName"),
                Port = _appSetting.GetItem<int>(@"TestConsumingChannelHostPort"),
                VirtualHost = _appSetting.GetItem<string>(@"TestConsumingChannelHostVirtualHost"),
                UserName = _appSetting.GetItem<string>(@"TestConsumingChannelHostUserName"),
                Password = _appSetting.GetItem<string>(@"TestConsumingChannelHostPassword"),
            };

            return host;
        }

        public static MomExchangeSetting GetExchangeSetting()
        {
            var address = new MomExchangeSetting()
            {
                ExchangeName = _appSetting.GetItem<string>(@"TestConsumingChannelExchangeName"),
                ExchangeType = _appSetting.GetItem<string>(@"TestConsumingChannelExchangeType"),
                ExchangeDurable = _appSetting.GetItem<bool>(@"TestConsumingChannelExchangeDurable"),
                ExchangeAutoDelete = _appSetting.GetItem<bool>(@"TestConsumingChannelExchangeAutoDelete"),
            };

            return address;
        }

        public static MomQueueSetting GetQueueSetting()
        {
            var setting = new MomQueueSetting()
            {
                QueueName = _appSetting.GetItem<string>(@"TestConsumingChannelQueueName"),
                QueueDurable = _appSetting.GetItem<bool>(@"TestConsumingChannelQueueDurable"),
                QueueExclusive = _appSetting.GetItem<bool>(@"TestConsumingChannelQueueExclusive"),
                QueueAutoDelete = _appSetting.GetItem<bool>(@"TestConsumingChannelQueueAutoDelete"),
                QueueNoAck = _appSetting.GetItem<bool>(@"TestConsumingChannelQueueNoAck"),
            };

            string bindRoutingKeys = _appSetting.GetItem<string>(@"TestConsumingChannelQueueBindRoutingKeys");
            if (!string.IsNullOrWhiteSpace(bindRoutingKeys))
            {
                setting.QueueBindRoutingKeys = bindRoutingKeys.Split(new char[] { ';', '|' }).Select(k => k.Trim()).ToList();
            }

            return setting;
        }
    }
}

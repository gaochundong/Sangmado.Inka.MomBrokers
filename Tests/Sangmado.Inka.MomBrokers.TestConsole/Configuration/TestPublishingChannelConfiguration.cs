using System.Linq;
using Sangmado.Inka.Configuration;

namespace Sangmado.Inka.MomBrokers.TestConsole
{
    public static class TestPublishingChannelConfiguration
    {
        private static readonly IAppSetting _appSetting = AppSetting.Singleton();

        public static MomHostSetting GetHostSetting()
        {
            var host = new MomHostSetting()
            {
                AutoClose = false,
                RequestedConnectionTimeout = 60000,
                RequestedHeartbeat = 30,

                ClientServiceName = _appSetting.GetItem<string>(@"TestPublishingChannelHostClientServiceName"),

                HostName = _appSetting.GetItem<string>(@"TestPublishingChannelHostName"),
                Port = _appSetting.GetItem<int>(@"TestPublishingChannelHostPort"),
                VirtualHost = _appSetting.GetItem<string>(@"TestPublishingChannelHostVirtualHost"),
                UserName = _appSetting.GetItem<string>(@"TestPublishingChannelHostUserName"),
                Password = _appSetting.GetItem<string>(@"TestPublishingChannelHostPassword"),
            };

            return host;
        }

        public static MomExchangeSetting GetExchangeSetting()
        {
            var address = new MomExchangeSetting()
            {
                ExchangeName = _appSetting.GetItem<string>(@"TestPublishingChannelExchangeName"),
                ExchangeType = _appSetting.GetItem<string>(@"TestPublishingChannelExchangeType"),
                ExchangeDurable = _appSetting.GetItem<bool>(@"TestPublishingChannelExchangeDurable"),
                ExchangeAutoDelete = _appSetting.GetItem<bool>(@"TestPublishingChannelExchangeAutoDelete"),
            };

            return address;
        }

        public static MomQueueSetting GetQueueSetting()
        {
            var setting = new MomQueueSetting()
            {
                QueueName = _appSetting.GetItem<string>(@"TestPublishingChannelQueueName"),
                QueueDurable = _appSetting.GetItem<bool>(@"TestPublishingChannelQueueDurable"),
                QueueExclusive = _appSetting.GetItem<bool>(@"TestPublishingChannelQueueExclusive"),
                QueueAutoDelete = _appSetting.GetItem<bool>(@"TestPublishingChannelQueueAutoDelete"),
                QueueNoAck = _appSetting.GetItem<bool>(@"TestPublishingChannelQueueNoAck"),
            };

            string bindRoutingKeys = _appSetting.GetItem<string>(@"TestPublishingChannelQueueBindRoutingKeys");
            if (!string.IsNullOrWhiteSpace(bindRoutingKeys))
            {
                setting.QueueBindRoutingKeys = bindRoutingKeys.Split(new char[] { ';', '|' }).Select(k => k.Trim()).ToList();
            }

            return setting;
        }
    }
}

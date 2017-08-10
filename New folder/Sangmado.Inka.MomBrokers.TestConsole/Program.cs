using System;
using System.Text;
using Sangmado.Inka.Logging;
using Sangmado.Inka.Logging.NLogIntegration;

namespace Sangmado.Inka.MomBrokers.TestConsole
{
    class Program
    {
        static ILog _log;

        static Program()
        {
            CompositeLogger.Use();
            _log = Logger.Get<Program>();
        }

        static void Main(string[] args)
        {
            var publishingChannel = new OutgoingMomChannel(
                TestPublishingChannelConfiguration.GetHostSetting(),
                TestPublishingChannelConfiguration.GetExchangeSetting(),
                TestPublishingChannelConfiguration.GetQueueSetting(),
                TimeSpan.FromSeconds(15));

            var consumingChannel = new IncomingMomChannel(
                TestConsumingChannelConfiguration.GetHostSetting(),
                TestConsumingChannelConfiguration.GetExchangeSetting(),
                TestConsumingChannelConfiguration.GetQueueSetting(),
                TimeSpan.FromSeconds(15));

            Console.WriteLine("Service starting.");

            publishingChannel.Connect();
            consumingChannel.Connect();

            consumingChannel.Received += OnReceived;
            consumingChannel.StartConsume();

            Console.WriteLine("Service started.");
            while (true)
            {
                try
                {
                    string text = Console.ReadLine().ToLowerInvariant();
                    if (text == "quit" || text == "exit")
                    {
                        break;
                    }
                    else if (text == "reconnect")
                    {
                        Console.WriteLine("Channel disconnecting.");

                        publishingChannel.Disconnect();
                        consumingChannel.Disconnect();

                        Console.WriteLine("Channel disconnected.");

                        Console.WriteLine("Channel connecting.");

                        publishingChannel.Connect();
                        consumingChannel.Connect();
                        consumingChannel.StartConsume();

                        Console.WriteLine("Channel connected.");
                    }
                    else
                    {
                        publishingChannel.Publish(Encoding.UTF8.GetBytes(text));
                    }
                }
                catch (Exception ex)
                {
                    _log.Error(ex.Message, ex);
                }
            }

            Console.WriteLine("Service stopping.");

            publishingChannel.Disconnect();
            consumingChannel.Disconnect();

            Console.WriteLine("Service stopped.");

            Console.ReadKey();
        }

        private static void OnReceived(object sender, MessageReceivedEventArgs e)
        {
            try
            {
                _log.DebugFormat("OnReceived, ExchangeName[{0}], ConsumerTag[{1}], DeliveryTag[{2}], RoutingKey[{3}], Content[{4}].",
                    e.ExchangeName, e.ConsumerTag, e.DeliveryTag, e.RoutingKey,
                    Encoding.UTF8.GetString(e.Body));
            }
            finally
            {
                e.Ack();
            }
        }
    }
}

using System;
using System.Threading;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Sangmado.Inka.Logging;

namespace Sangmado.Inka.MomBrokers
{
    public class IncomingMomChannel : ReconnectableMomChannel, IIncomingMomChannel
    {
        private ILog _log = Logger.Get<IncomingMomChannel>();

        private EventingBasicConsumer _consumer = null;
        private string _consumerTag;
        private readonly object _controlLocker = new object();
        private Action _recoverConsume = null;

        public IncomingMomChannel(MomHostSetting host, MomExchangeSetting exchange, MomQueueSetting queue)
            : base(host, exchange, queue)
        {
            this.Connected += OnConnected;
        }

        public bool IsConsuming
        {
            get
            {
                var consumer = _consumer;
                if (consumer != null)
                    return consumer.IsRunning;
                return false;
            }
        }

        public void StartConsume()
        {
            if (!IsConnected)
            {
                throw new MomChannelNotConnectedException("The channel hasn't been connected.");
            }

            lock (_controlLocker)
            {
                if (_consumer != null) return;

                _consumer = new EventingBasicConsumer(this.Channel);
                _consumerTag = this.Channel.BasicConsume(this.QueueSetting.QueueName, this.QueueSetting.QueueNoAck, _consumer);
                _consumer.Received += OnReceived;
                _consumer.Shutdown += OnShutdown;

                _recoverConsume = RecoverConsume;

                _log.DebugFormat("StartConsume, start to consume [{0}] on consumer tag [{1}] with setting [{2}].",
                    this.QueueSetting.QueueName, _consumerTag, this.QueueSetting);
            }
        }

        public void StopConsume()
        {
            lock (_controlLocker)
            {
                _log.DebugFormat("StopConsume, stop to consume [{0}] on consumer tag [{1}] with setting [{2}].",
                    this.QueueSetting.QueueName, _consumerTag, this.QueueSetting);

                try
                {
                    if (_consumer != null)
                    {
                        _consumer.Received -= OnReceived;
                        _consumer.Shutdown -= OnShutdown;

                        if (this.Channel != null && !string.IsNullOrEmpty(_consumerTag))
                        {
                            this.Channel.BasicCancel(_consumerTag);
                        }
                    }
                }
                catch { }
                finally
                {
                    _consumer = null;
                    _consumerTag = null;
                    _recoverConsume = null;
                }
            }
        }

        private void OnConnected(object sender, MomChannelConnectedEventArgs e)
        {
            if (_recoverConsume != null)
                _recoverConsume();
        }

        private void RecoverConsume()
        {
            lock (_controlLocker)
            {
                if (_consumer != null)
                {
                    _log.DebugFormat("RecoverConsume, recover consumer [{0}] on [{1}].", this.QueueSetting, _consumerTag);
                    _consumerTag = this.Channel.BasicConsume(this.QueueSetting.QueueName, this.QueueSetting.QueueNoAck, _consumerTag, _consumer);
                }
            }
        }

        private void OnShutdown(object sender, ShutdownEventArgs e)
        {
            _log.DebugFormat("OnShutdown, consumer [{0}] on [{1}] shutdown due to [{2}].",
                this.QueueSetting, _consumerTag, e);
            StopConsume();
        }

        private void OnReceived(object sender, BasicDeliverEventArgs e)
        {
#if VERBOSE
            _log.DebugFormat("OnReceived, received message, " +
                "ConsumerTag[{0}], DeliveryTag[{1}], Exchange[{2}], RoutingKey[{3}], BodyLength[{4}], on Thread[{5}].",
                e.ConsumerTag, e.DeliveryTag, e.Exchange, e.RoutingKey,
                e.Body == null ? "" : e.Body.Length.ToString(),
                Thread.CurrentThread.GetDescription());
#endif

            if (Received != null)
            {
                var message = new MessageReceivedEventArgs(this)
                {
                    ConsumerTag = e.ConsumerTag,
                    DeliveryTag = e.DeliveryTag,
                    ExchangeName = e.Exchange,
                    RoutingKey = e.RoutingKey,
                    Body = e.Body,
                };
                Received(this, message);
            }
            else
            {
                _log.DebugFormat("OnReceived, message discarded due to handler not found, " +
                    "ConsumerTag[{0}], DeliveryTag[{1}], Exchange[{2}], RoutingKey[{3}], BodyLength[{4}].",
                    e.ConsumerTag, e.DeliveryTag, e.Exchange, e.RoutingKey, e.Body == null ? "" : e.Body.Length.ToString());
            }
        }

        public event EventHandler<MessageReceivedEventArgs> Received;

        public void Ack(ulong deliveryTag)
        {
            if (this.QueueSetting.QueueNoAck)
                return;

#if VERBOSE
            _log.DebugFormat("Ack, ack DeliveryTag[{0}] on ConsumerTag[{1}], on Thread[{2}].",
                deliveryTag, _consumerTag, Thread.CurrentThread.GetDescription());
#endif

            lock (_pipelining)
            {
                this.Channel.BasicAck(deliveryTag, true);
            }
        }
    }
}

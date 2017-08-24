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
        private Action _recoverConsume = null;

        public IncomingMomChannel(MomHostSetting host, MomExchangeSetting exchange, MomQueueSetting queue)
            : base(host, exchange, queue)
        {
            this.Connected += OnConnected;
        }

        public IncomingMomChannel(MomHostSetting host, MomExchangeSetting exchange, MomQueueSetting queue, TimeSpan retryPeriod)
            : base(host, exchange, queue, retryPeriod)
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
            lock (_pipelining)
            {
                if (!IsConnected)
                {
                    throw new MomChannelNotConnectedException("The channel hasn't been connected.");
                }

                if (_consumer != null) return;

                _consumer = new EventingBasicConsumer(this.Channel);
                _consumer.Registered += OnRegistered;
                _consumer.Unregistered += OnUnregistered;
                _consumer.Received += OnReceived;
                _consumer.Shutdown += OnShutdown;
                _consumerTag = this.Channel.BasicConsume(this.QueueSetting.QueueName, this.QueueSetting.QueueNoAck, _consumer);

                _recoverConsume = RecoverConsume;

                _log.WarnFormat("StartConsume, start to consume [{0}] on consumer tag [{1}] with setting [{2}].",
                    this.QueueSetting.QueueName, _consumerTag, this.QueueSetting);
            }
        }

        public void StartConsume(string consumerTag)
        {
            lock (_pipelining)
            {
                if (string.IsNullOrWhiteSpace(consumerTag))
                    throw new ArgumentNullException("consumerTag");

                if (!IsConnected)
                {
                    throw new MomChannelNotConnectedException("The channel hasn't been connected.");
                }

                if (_consumer != null) return;

                _consumer = new EventingBasicConsumer(this.Channel);
                _consumer.Registered += OnRegistered;
                _consumer.Unregistered += OnUnregistered;
                _consumer.Received += OnReceived;
                _consumer.Shutdown += OnShutdown;
                _consumerTag = this.Channel.BasicConsume(this.QueueSetting.QueueName, this.QueueSetting.QueueNoAck, consumerTag, _consumer);

                _recoverConsume = RecoverConsume;

                _log.WarnFormat("StartConsume, start to consume [{0}] on consumer tag [{1}] with setting [{2}].",
                    this.QueueSetting.QueueName, _consumerTag, this.QueueSetting);
            }
        }

        public void StopConsume()
        {
            lock (_pipelining)
            {
                _log.WarnFormat("StopConsume, stop to consume [{0}] on consumer tag [{1}] with setting [{2}].",
                    this.QueueSetting.QueueName, _consumerTag, this.QueueSetting);

                try
                {
                    if (_consumer != null)
                    {
                        _consumer.Registered -= OnRegistered;
                        _consumer.Unregistered -= OnUnregistered;
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

        private void RecoverConsume()
        {
            lock (_pipelining)
            {
                if (_consumer != null)
                {
                    _log.WarnFormat("RecoverConsume, recover consumer [{0}] on [{1}].", this.QueueSetting, _consumerTag);
                    _consumerTag = this.Channel.BasicConsume(this.QueueSetting.QueueName, this.QueueSetting.QueueNoAck, _consumerTag, _consumer);
                    this.Channel.BasicRecover(true);
                }
            }
        }

        private void OnConnected(object sender, MomChannelConnectedEventArgs e)
        {
            if (_recoverConsume != null)
                _recoverConsume();
        }

        private void OnRegistered(object sender, ConsumerEventArgs e)
        {
            _log.DebugFormat("OnRegistered, consumer [{0}] on [{1}] registered to [{2}].",
                this.QueueSetting, _consumerTag, e.ConsumerTag);
        }

        private void OnUnregistered(object sender, ConsumerEventArgs e)
        {
            _log.DebugFormat("OnUnregistered, consumer [{0}] on [{1}] unregistered to [{2}].",
                this.QueueSetting, _consumerTag, e.ConsumerTag);
        }

        private void OnShutdown(object sender, ShutdownEventArgs e)
        {
            _log.ErrorFormat("OnShutdown, consumer [{0}] on [{1}] shutdown due to [{2}].",
                this.QueueSetting, _consumerTag, e);
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

        public uint ConsumerCount()
        {
            // Returns the number of consumers on a queue. 
            // This method assumes the queue exists. If it doesn't, will be closed with an exception.
            lock (_pipelining)
            {
                return this.Channel.ConsumerCount(this.QueueSetting.QueueName);
            }
        }

        public uint MessageCount()
        {
            // Returns the number of messages in a queue ready to be delivered to consumers.
            // This method assumes the queue exists. If it doesn't, will be closed with an exception.
            lock (_pipelining)
            {
                return this.Channel.MessageCount(this.QueueSetting.QueueName);
            }
        }

        public event EventHandler<MessageReceivedEventArgs> Received;

        public void Ack(ulong deliveryTag)
        {
            Ack(deliveryTag, false);
        }

        public void Ack(ulong deliveryTag, bool multiple)
        {
            if (this.QueueSetting.QueueNoAck)
                return;

#if VERBOSE
            _log.DebugFormat("Ack, ack DeliveryTag[{0}] on ConsumerTag[{1}] with Multiple[{2}], on Thread[{3}].",
                deliveryTag, _consumerTag, multiple, Thread.CurrentThread.GetDescription());
#endif

            lock (_pipelining)
            {
                try
                {
                    this.Channel.BasicAck(deliveryTag, multiple);
                }
                catch (Exception ex)
                {
                    _log.Error(ex.Message, ex);
                    AbnormalDisconnect();
                    throw;
                }
            }
        }

        public void Nack(ulong deliveryTag, bool multiple, bool requeue)
        {
            if (this.QueueSetting.QueueNoAck)
                return;

#if VERBOSE
            _log.DebugFormat("Nack, nack DeliveryTag[{0}] on ConsumerTag[{1}] with Multiple[{2}] and Requeue[{3}], on Thread[{4}].",
                deliveryTag, _consumerTag, multiple, requeue, Thread.CurrentThread.GetDescription());
#endif

            lock (_pipelining)
            {
                try
                {
                    this.Channel.BasicNack(deliveryTag, multiple, requeue);
                }
                catch (Exception ex)
                {
                    _log.Error(ex.Message, ex);
                    AbnormalDisconnect();
                    throw;
                }
            }
        }

        public void Reject(ulong deliveryTag, bool requeue)
        {
            if (this.QueueSetting.QueueNoAck)
                return;

#if VERBOSE
            _log.DebugFormat("Reject, reject DeliveryTag[{0}] on ConsumerTag[{1}] with Requeue[{2}], on Thread[{3}].",
                deliveryTag, _consumerTag, requeue, Thread.CurrentThread.GetDescription());
#endif

            lock (_pipelining)
            {
                try
                {
                    this.Channel.BasicReject(deliveryTag, requeue);
                }
                catch (Exception ex)
                {
                    _log.Error(ex.Message, ex);
                    AbnormalDisconnect();
                    throw;
                }
            }
        }

        public void Recover(bool requeue)
        {
            if (this.QueueSetting.QueueNoAck)
                return;

#if VERBOSE
            _log.DebugFormat("Recover, recover on ConsumerTag[{0}] with Requeue[{1}], on Thread[{2}].",
                _consumerTag, requeue, Thread.CurrentThread.GetDescription());
#endif

            lock (_pipelining)
            {
                try
                {
                    this.Channel.BasicRecover(requeue);
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

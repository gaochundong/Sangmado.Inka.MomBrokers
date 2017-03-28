using System;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;
using RockStone.Inka.Extensions;
using RockStone.Inka.Logging;

namespace RockStone.Inka.MomBrokers
{
    public abstract class MomChannel : IMomChannel
    {
        private ILog _log = Logger.Get<MomChannel>();

        private IConnection _connection = null;
        private IModel _channel = null;

        // In general, IModel instances should not be used by more than one thread simultaneously:
        // application code should maintain a clear notion of thread ownership for IModel instances.
        // If more than one thread needs to access a particular IModel instances, 
        // the application should enforce mutual exclusion itself.
        protected readonly object _pipelining = new object();

        protected MomChannel(MomHostSetting host, MomChannelAddress address, MomChannelSetting setting)
        {
            Guard.ArgumentNotNull(host, "host");
            Guard.ArgumentNotNull(address, "address");
            Guard.ArgumentNotNull(setting, "setting");

            Host = host;
            Address = address;
            Setting = setting;
        }

        public MomHostSetting Host { get; protected set; }
        public MomChannelAddress Address { get; protected set; }
        public MomChannelSetting Setting { get; protected set; }

        protected IModel Channel { get { return _channel; } }

        #region Connect

        public void Connect()
        {
            if (IsConnected) return;

            try
            {
                _log.DebugFormat("Connect, begin to connect to message bus.");

                var connectionFactory = BuildConnectionFactory();
                _connection = connectionFactory.CreateConnection();
                _connection.AutoClose = this.Host.AutoClose;
                _connection.CallbackException += OnConnectionCallbackException;
                _connection.ConnectionShutdown += OnConnectionShutdown;

                BindChannel();

                _log.DebugFormat("Connect, connect to message bus successfully.");

                OnConnected();
            }
            catch (Exception ex)
            {
                _log.Error(string.Format("Connect, connect to message bus failed due to [{0}].", ex.Message), ex);

                throw;
            }
        }

        public void Disconnect()
        {
            if (!IsConnected) return;

            _log.DebugFormat("Disconnect, begin to disconnect to message bus.");

            try
            {
                if (_channel != null)
                {
                    _channel.CallbackException -= OnChannelCallbackException;
                    _channel.ModelShutdown -= OnChannelShutdown;
                    _channel.Close();
                }
            }
            catch (AlreadyClosedException) { }
            catch (Exception ex)
            {
                _log.Error(string.Format("Disconnect, disconnect to message bus channel failed due to [{0}].", ex.Message), ex);
            }
            finally
            {
                _channel = null;
            }

            try
            {
                if (_connection != null)
                {
                    _connection.CallbackException -= OnConnectionCallbackException;
                    _connection.ConnectionShutdown -= OnConnectionShutdown;
                    _connection.Close();
                }
            }
            catch (AlreadyClosedException) { }
            catch (Exception ex)
            {
                _log.Error(string.Format("Disconnect, disconnect to message bus connection failed due to [{0}].", ex.Message), ex);
            }
            finally
            {
                _connection = null;
            }

            _log.DebugFormat("Disconnect, disconnect to message bus successfully.");

            OnDisconnected();
        }

        private ConnectionFactory BuildConnectionFactory()
        {
            var factory = new ConnectionFactory()
            {
                Protocol = Protocols.DefaultProtocol,
                HostName = this.Host.HostName,
                Port = this.Host.Port,
                VirtualHost = this.Host.VirtualHost,
                UserName = this.Host.UserName,
                Password = this.Host.Password,
                RequestedConnectionTimeout = this.Host.RequestedConnectionTimeout,
                RequestedHeartbeat = this.Host.RequestedHeartbeat,
            };

            factory.ClientProperties.Add("Application Name", this.Setting.ClientServiceName);
            factory.ClientProperties.Add("Application Connected Time (UTC)", DateTime.UtcNow.ToString("o"));

            _log.DebugFormat("BuildConnectionFactory, Protocol[{0}], HostName[{1}], Port[{2}], VirtualHost[{3}], UserName[{4}], "
                + "RequestedConnectionTimeout[{5}], RequestedHeartbeat[{6}], RequestedFrameMax[{7}], RequestedChannelMax[{8}].",
                factory.Protocol,
                factory.HostName,
                factory.Port,
                factory.VirtualHost,
                factory.UserName,
                factory.RequestedConnectionTimeout,
                factory.RequestedHeartbeat,
                factory.RequestedFrameMax,
                factory.RequestedChannelMax);

            return factory;
        }

        private void BindChannel()
        {
            // Brokers provide four exchange types: Direct, Fanout, Topic and Headers.
            // Durability (exchanges survive broker restart)
            // Auto-delete (exchange is deleted when all queues have finished using it)
            // Arguments (these are broker-dependent)
            string exchangeType = this.Setting.ExchangeType;
            bool exchangeDurable = this.Setting.ExchangeDurable;
            bool exchangeAutoDelete = this.Setting.ExchangeAutoDelete;
            var exchangeArguments = this.Setting.ExchangeArguments;

            // Durable (the queue will survive a broker restart)
            // Exclusive (used by only one connection and the queue will be deleted when that connection closes)
            // Auto-delete (queue is deleted when last consumer unsubscribed)
            // Arguments (some brokers use it to implement additional features like message TTL)
            bool queueDurable = this.Setting.QueueDurable;
            bool queueExclusive = this.Setting.QueueExclusive;
            bool queueAutoDelete = this.Setting.QueueAutoDelete;
            var queueArguments = this.Setting.QueueArguments;

            _log.DebugFormat("BindChannel, binding Address[{0}] with Setting[{1}].", this.Address, this.Setting);

            _channel = _connection.CreateModel();
            _channel.CallbackException += OnChannelCallbackException;
            _channel.ModelShutdown += OnChannelShutdown;

            if (!string.IsNullOrEmpty(this.Address.ExchangeName))
            {
                _channel.ExchangeDeclare(this.Address.ExchangeName, exchangeType, exchangeDurable, exchangeAutoDelete, exchangeArguments);
            }

            if (!string.IsNullOrEmpty(this.Address.QueueName))
            {
                var queueStatus = _channel.QueueDeclare(this.Address.QueueName, queueDurable, queueExclusive, queueAutoDelete, queueArguments);
                _channel.QueueBind(this.Address.QueueName, this.Address.ExchangeName, this.Address.RoutingKey);
                _channel.BasicQos(0, 1, false);

                _log.DebugFormat("BindChannel, QueueName[{0}], ConsumerCount[{1}], MessageCount[{2}].",
                    queueStatus.QueueName, queueStatus.ConsumerCount, queueStatus.MessageCount);
            }

            _log.DebugFormat("BindChannel, bound Address[{0}] with Setting[{1}].", this.Address, this.Setting);
        }

        protected virtual void OnChannelShutdown(object sender, ShutdownEventArgs e)
        {
            _log.ErrorFormat("OnChannelShutdown, channel is shutdown due to [{0}].",
                e == null ? "" :
                    string.Format("ClassId[{0}], MethodId[{1}], ReplyCode[{2}], ReplyText[{3}], Cause[{4}]",
                        e.ClassId, e.MethodId, e.ReplyCode, e.ReplyText, e.Cause));
            Disconnect();
        }

        protected virtual void OnChannelCallbackException(object sender, CallbackExceptionEventArgs e)
        {
            _log.ErrorFormat("OnChannelCallbackException, [{0}].",
                e == null ? "" :
                    string.Format("Exception[{0}], Detail[{1}]",
                        e.Exception, e.Detail == null ? "" : string.Join(",", e.Detail.Values)));
        }

        protected virtual void OnConnectionShutdown(object sender, ShutdownEventArgs e)
        {
            _log.ErrorFormat("OnConnectionShutdown, connection is shutdown due to [{0}].",
                e == null ? "" :
                    string.Format("ClassId[{0}], MethodId[{1}], ReplyCode[{2}], ReplyText[{3}], Cause[{4}]",
                        e.ClassId, e.MethodId, e.ReplyCode, e.ReplyText, e.Cause));
            Disconnect();
        }

        protected virtual void OnConnectionCallbackException(object sender, CallbackExceptionEventArgs e)
        {
            _log.ErrorFormat("OnConnectionCallbackException, [{0}].",
                e == null ? "" :
                    string.Format("Exception[{0}], Detail[{1}]",
                        e.Exception, e.Detail == null ? "" : string.Join(",", e.Detail.Values)));
        }

        #endregion

        #region Connected

        public bool IsConnected { get; private set; }

        public event EventHandler<MomChannelConnectedEventArgs> Connected;
        public event EventHandler<MomChannelDisconnectedEventArgs> Disconnected;

        protected virtual void OnConnected()
        {
            IsConnected = true;
            RaiseConnectedEvent();
        }

        protected virtual void OnDisconnected()
        {
            IsConnected = false;
            RaiseDisconnectedEvent();
        }

        protected void RaiseConnectedEvent()
        {
            if (Connected != null)
            {
                Connected(this, new MomChannelConnectedEventArgs(this.Address));
            }
        }

        protected void RaiseDisconnectedEvent()
        {
            if (Disconnected != null)
            {
                Disconnected(this, new MomChannelDisconnectedEventArgs(this.Address));
            }
        }

        #endregion
    }
}

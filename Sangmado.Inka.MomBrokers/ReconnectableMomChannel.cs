using System;
using System.Threading;
using System.Threading.Tasks;
using RabbitMQ.Client;
using Sangmado.Inka.Logging;

namespace Sangmado.Inka.MomBrokers
{
    public abstract class ReconnectableMomChannel : MomChannel
    {
        private ILog _log = Logger.Get<ReconnectableMomChannel>();

        private System.Threading.Timer _retryTimer = null;
        private readonly object _retryLock = new object();

        protected ReconnectableMomChannel(MomHostSetting host, MomExchangeSetting exchange, MomQueueSetting queue)
            : this(host, exchange, queue, TimeSpan.FromSeconds(60))
        {
        }

        protected ReconnectableMomChannel(MomHostSetting host, MomExchangeSetting exchange, MomQueueSetting queue, TimeSpan retryPeriod)
            : base(host, exchange, queue)
        {
            this.RetryPeriod = retryPeriod;
        }

        public TimeSpan RetryPeriod { get; set; }

        protected override void OnConnected()
        {
            base.OnConnected();
            CloseRetryTimer();
        }

        protected override void OnDisconnected()
        {
            base.OnDisconnected();
            CloseRetryTimer();
        }

        protected override void OnChannelShutdown(object sender, ShutdownEventArgs e)
        {
            base.OnChannelShutdown(sender, e);
            SetupRetryTimer();
        }

        private void SetupRetryTimer()
        {
            lock (_retryLock)
            {
                _log.DebugFormat("SetupRetryTimer, setup timer when retry to connect ExchangeSetting[{0}], QueueSetting[{1}].",
                    this.ExchangeSetting, this.QueueSetting);

                if (_retryTimer == null)
                {

                    _retryTimer = new System.Threading.Timer(
                      (s) =>
                      {
                          Task.Factory.StartNew(() =>
                          {
                              try
                              {
                                  this.Connect();
                              }
                              catch (Exception ex)
                              {
                                  _log.Error(ex.Message, ex);
                              }
                          },
                          TaskCreationOptions.PreferFairness);
                      },
                      null, TimeSpan.FromSeconds(0), this.RetryPeriod);
                }
            }
        }

        private void CloseRetryTimer()
        {
            lock (_retryLock)
            {
                _log.DebugFormat("CloseRetryTimer, close timer after retry to connect ExchangeSetting[{0}], QueueSetting[{1}].",
                    this.ExchangeSetting, this.QueueSetting);

                if (_retryTimer != null)
                {
                    _retryTimer.Change(Timeout.Infinite, Timeout.Infinite);
                    _retryTimer.Dispose();
                    _retryTimer = null;
                }
            }
        }
    }
}

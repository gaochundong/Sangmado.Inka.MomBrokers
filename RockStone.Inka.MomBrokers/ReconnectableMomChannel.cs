using System;
using System.Threading;
using System.Threading.Tasks;
using RabbitMQ.Client;
using RockStone.Inka.Logging;

namespace RockStone.Inka.MomBrokers
{
    public abstract class ReconnectableMomChannel : MomChannel
    {
        private ILog _log = Logger.Get<ReconnectableMomChannel>();

        private System.Threading.Timer _retryTimer = null;
        private readonly object _retryLock = new object();

        protected ReconnectableMomChannel(MomHostSetting host, MomChannelAddress address, MomChannelSetting setting)
            : this(host, address, setting, TimeSpan.FromSeconds(60))
        {
        }

        protected ReconnectableMomChannel(MomHostSetting host, MomChannelAddress address, MomChannelSetting setting, TimeSpan retryPeriod)
            : base(host, address, setting)
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
                _log.DebugFormat("SetupRetryTimer, setup timer when retry to connect [{0}].", this.Address);

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
                _log.DebugFormat("CloseRetryTimer, close timer after retry to connect [{0}].", this.Address);

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

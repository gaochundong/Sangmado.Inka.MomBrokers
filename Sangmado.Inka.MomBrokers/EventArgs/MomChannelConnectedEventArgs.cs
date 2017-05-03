using System;
using System.Globalization;

namespace Sangmado.Inka.MomBrokers
{
    public class MomChannelConnectedEventArgs : EventArgs
    {
        public MomChannelConnectedEventArgs()
        {
        }

        public MomChannelConnectedEventArgs(MomExchangeSetting exchange, MomQueueSetting queue)
          : this()
        {
            this.ExchangeSetting = exchange;
            this.QueueSetting = queue;
        }

        public MomExchangeSetting ExchangeSetting { get; set; }
        public MomQueueSetting QueueSetting { get; set; }

        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture,
                "ExchangeSetting[{0}], QueueSetting[{1}]", this.ExchangeSetting, this.QueueSetting);
        }
    }
}

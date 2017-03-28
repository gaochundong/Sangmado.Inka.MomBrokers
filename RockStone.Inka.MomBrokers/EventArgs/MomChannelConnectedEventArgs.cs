using System;
using System.Globalization;

namespace RockStone.Inka.MomBrokers
{
    public class MomChannelConnectedEventArgs : EventArgs
    {
        public MomChannelConnectedEventArgs()
        {
        }

        public MomChannelConnectedEventArgs(MomChannelAddress address)
          : this()
        {
            this.Address = address;
        }

        public MomChannelAddress Address { get; set; }

        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "{0}", this.Address);
        }
    }
}

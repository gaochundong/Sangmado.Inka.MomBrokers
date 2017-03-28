using System;
using System.Globalization;

namespace RockStone.Inka.MomBrokers
{
    public class MomChannelDisconnectedEventArgs : EventArgs
    {
        public MomChannelDisconnectedEventArgs()
        {
        }

        public MomChannelDisconnectedEventArgs(MomChannelAddress address)
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

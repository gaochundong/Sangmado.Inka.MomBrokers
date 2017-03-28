using System;

namespace RockStone.Inka.MomBrokers
{
    [Serializable]
    public class MomChannelNotConnectedException : Exception
    {
        public MomChannelNotConnectedException()
          : base()
        {
        }

        public MomChannelNotConnectedException(string message)
          : base(message)
        {
        }

        public MomChannelNotConnectedException(string message, Exception innerException)
          : base(message, innerException)
        {
        }
    }
}

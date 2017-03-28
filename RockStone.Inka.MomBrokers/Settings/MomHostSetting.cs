using System.Globalization;

namespace RockStone.Inka.MomBrokers
{
    public class MomHostSetting
    {
        public MomHostSetting()
        {
            this.AutoClose = false;
            this.RequestedConnectionTimeout = 60000;
            this.RequestedHeartbeat = 30;
        }

        public string HostName { get; set; }
        public int Port { get; set; }
        public string VirtualHost { get; set; }

        public string UserName { get; set; }
        public string Password { get; set; }

        public bool AutoClose { get; set; }
        public int RequestedConnectionTimeout { get; set; }
        public ushort RequestedHeartbeat { get; set; }

        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture,
                "HostName[{0}], Port[{1}], VirtualHost[{2}], UserName[{3}]",
                HostName, Port, VirtualHost, UserName);
        }
    }
}

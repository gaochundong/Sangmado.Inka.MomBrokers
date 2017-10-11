using System.Collections.Generic;
using System.Linq;
using System.Text;
using RabbitMQ.Client;

namespace Sangmado.Inka.MomBrokers
{
    public class MomBasicProperties : IMomBasicProperties
    {
        private IDictionary<string, object> _headers;
        private bool _isHeadersPresent;

        private bool _persistent;
        private bool _isPersistentPresent;

        public MomBasicProperties()
        {
        }

        public IDictionary<string, object> Headers
        {
            get
            {
                return _headers;
            }
            set
            {
                _isHeadersPresent = true;
                _headers = value;
            }
        }

        public bool IsHeadersPresent()
        {
            return _isHeadersPresent;
        }

        public bool Persistent
        {
            get
            {
                return _persistent;
            }
            set
            {
                _isPersistentPresent = true;
                _persistent = value;
            }
        }

        public bool IsPersistentPresent()
        {
            return _isPersistentPresent;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();

            sb.Append("Headers=");
            sb.Append(_isHeadersPresent ? (_headers == null ? "(null)" : string.Join(",", _headers.Select(p => string.Format("{0}|{1}", p.Key, p.Value)))) : "_");

            sb.Append(", ");

            sb.Append("Persistent=");
            sb.Append(_isPersistentPresent ? _persistent.ToString() : "_");

            return sb.ToString();
        }

        public void FulfillBasicProperties(IBasicProperties basicProperties)
        {
            if (IsHeadersPresent())
            {
                if (basicProperties.IsHeadersPresent())
                {
                    if (basicProperties.Headers == null)
                    {
                        basicProperties.Headers = this.Headers;
                    }
                    else
                    {
                        foreach (var item in this.Headers)
                        {
                            basicProperties.Headers[item.Key] = item.Value;
                        }
                    }
                }
                else
                {
                    basicProperties.Headers = this.Headers;
                }
            }

            if (IsPersistentPresent())
            {
                basicProperties.Persistent = this.Persistent;
            }
        }
    }
}
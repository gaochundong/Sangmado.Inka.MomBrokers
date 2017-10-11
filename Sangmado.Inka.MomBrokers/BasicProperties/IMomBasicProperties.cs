using System.Collections.Generic;

namespace Sangmado.Inka.MomBrokers
{
    public interface IMomBasicProperties
    {
        IDictionary<string, object> Headers { get; set; }
    }
}
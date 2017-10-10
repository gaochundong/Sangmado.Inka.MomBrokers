using System.Collections.Generic;

namespace Sangmado.Inka.MomBrokers
{
    public interface IMomProperties
    {
        IDictionary<string, object> Headers { get; set; }
    }
}
using EL.Http;
using Newtonsoft.Json;

namespace EL.InfluxDB.IntegrationTests
{
    public class RequestSerializer : IRequestSerializer
    {
        public string SerializeBody(object body)
        {
            return JsonConvert.SerializeObject(body, Formatting.Indented);
        }
    }
}
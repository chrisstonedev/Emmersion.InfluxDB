using System;
using EL.Http;
using Newtonsoft.Json;

namespace EL.InfluxDB
{
    public interface ISender
    {
        void SendPayload(string payload);
    }

    public class Sender : ISender
    {
        readonly InfluxSettings settings;
        readonly IHttpClient httpClient;

        public Sender(InfluxSettings settings, IHttpClient httpClient)
        {
            this.settings = settings;
            this.httpClient = httpClient;
        }

        public void SendPayload(string payload)
        {
            var request = new HttpRequest()
            {
                Url = $"{settings.InfluxHostname}:{settings.InfluxPort }/write?db={settings.InfluxDbName}",
                Method = HttpMethod.POST,
                Body = payload
            };

            var response = httpClient.Execute(request);

            if (response.StatusCode != 204)
            {
                throw new Exception($"Failed to write metrics (influxUrl: {request.Url}, response: {JsonConvert.SerializeObject(response)})");
            }
        }
    }
}
using System;
using EL.Http;

namespace EL.InfluxDB
{
    public interface ISender
    {
        void SendPayload(string payload);
    }

    public class Sender : ISender
    {
        private readonly IHttpClient httpClient;
        private readonly IRequestSerializer requestSerializer;
        private readonly InfluxSettings settings;

        public Sender(InfluxSettings settings, IHttpClient httpClient, IRequestSerializer requestSerializer)
        {
            this.settings = settings;
            this.httpClient = httpClient;
            this.requestSerializer = requestSerializer;
        }

        public void SendPayload(string payload)
        {
            var request = new HttpRequest {Url = $"{settings.InfluxHostname}:{settings.InfluxPort}/write?db={settings.InfluxDbName}", Method = HttpMethod.POST, Body = payload};

            var response = httpClient.Execute(request);

            if (response.StatusCode != 204)
            {
                throw new Exception($"Failed to write metrics (influxUrl: {request.Url}, response: {requestSerializer.SerializeBody(response)})");
            }
        }
    }
}
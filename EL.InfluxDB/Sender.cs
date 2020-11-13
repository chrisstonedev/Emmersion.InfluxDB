using System;
using System.Net.Http;
using System.Text;

namespace EL.InfluxDB
{
    internal interface ISender
    {
        void SendPayload(string payload);
    }

    internal class Sender : ISender
    {
        private readonly IHttpClient httpClient;
        private readonly IInfluxSettings settings;

        public Sender(IHttpClient httpClient, IInfluxSettings settings)
        {
            this.settings = settings;
            this.httpClient = httpClient;
        }

        public void SendPayload(string payload)
        {
            var content = new StringContent(payload, Encoding.UTF8);
            var message = new HttpRequestMessage(HttpMethod.Post, $"{settings.Hostname}:{settings.Port}/write?db={settings.DbName}") {Content = content};

            (int statusCode, string reponseBody) response;
            try
            {
                response = httpClient.Execute(message);
            }
            catch (Exception e)
            {
                throw new AggregateException("Could not send points", e);
            }

            if (response.statusCode != 204)
            {
                throw new Exception($"Failed to write metrics (influxUrl: {message.RequestUri}, response: {response.reponseBody})");
            }
        }
    }
}
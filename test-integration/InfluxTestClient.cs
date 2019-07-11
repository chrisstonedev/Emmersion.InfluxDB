using System;
using EL.Http;

namespace EL.InfluxDB.IntegrationTests
{
    public class InfluxTestClient
    {
        readonly InfluxSettings settings = new IntegrationSettings();
        readonly IHttpClient httpClient = new HttpClient();

        public string Query(string query)
        {
            var uri =$"{settings.InfluxHostname}:{settings.InfluxPort}/query?db={settings.InfluxDbName}&q={query}";

            var request = new HttpRequest
            {
                Url = uri,
                Method = HttpMethod.GET,
            };
            var response = httpClient.Execute(request);

            if (response.StatusCode != 200)
            {
                throw new Exception($"Influx error executing request: {response.Body}");
            }

            return response.Body;
        }

        public void Command(string command)
        {
            Query(command);
        }
    }
}
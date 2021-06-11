using System.Net.Http;

namespace Emmersion.InfluxDB
{
    internal interface IHttpClient
    {
        (int statusCode, string responseBody) Execute(HttpRequestMessage request);
    }

    internal class HttpClient : IHttpClient
    {
        private readonly System.Net.Http.HttpClient client;

        public HttpClient()
        {
            client = new System.Net.Http.HttpClient();
        }

        public (int statusCode, string responseBody) Execute(HttpRequestMessage request)
        {
            var response = client.SendAsync(request).Result;
            return ((int) response.StatusCode, response.Content.ReadAsStringAsync().Result);
        }
    }
}
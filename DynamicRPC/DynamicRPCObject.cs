using System.Net.Http.Headers;
using System.Net;
using System.Dynamic;
using Newtonsoft.Json.Linq;

namespace DynamicRPC
{
    public class DynamicRPCObject : DynamicObject
    {
        private static HttpClient? sharedClient;
        private HttpClient client;
        private int id = int.MinValue;

        private string endpoint;
        private static HttpClient CreateHttpClient()
        {
            HttpClientHandler handler = new()
            {
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
            };

            HttpClient client = new(handler);
            client.DefaultRequestHeaders.Connection.Add("Keep-Alive");
            client.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));

            return client;
        }


        public DynamicRPCObject(string endpoint, HttpClient? client = null)
        {
            if (client == null)
                sharedClient = CreateHttpClient();

            this.client = client ?? sharedClient!;

            this.endpoint = endpoint;
        }


        public override bool TryInvokeMember(InvokeMemberBinder binder, object?[]? args, out object? result)
        {
            dynamic payload = new JObject();
            payload.jsonrpc = "2.0";
            payload.id = id++;
            payload.method = binder.Name.Replace("__", "/").Replace('_', '.').Replace('/', '_');
            payload.@params = JToken.FromObject(args ?? []);

            string payloadString = payload.ToString();

            result = Task.Run(async () => {
                var result = await client.PostAsync(endpoint, new StringContent(payloadString));
                if (!result.IsSuccessStatusCode) throw new Exception($"The server responded with an error status code ('{result.StatusCode}: {result.ReasonPhrase}')");

                string content = await result.Content.ReadAsStringAsync();
                return JObject.Parse(content);
            });

            return true;
        }
    }
}

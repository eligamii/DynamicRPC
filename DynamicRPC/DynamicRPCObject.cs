using System.Net.Http.Headers;
using System.Net;
using System.Dynamic;
using Newtonsoft.Json.Linq;
using System.Reflection;

namespace DynamicRPC
{
    public class DynamicRPCObject : DynamicObject
    {
        private static HttpClient? sharedClient;
        private readonly HttpClient? client;
        private int id = int.MinValue;

        private string className = string.Empty;
        private DynamicRPCObject? parent = null;

        public string Endpoint { get; set; }
       


        public DynamicRPCObject(string endpoint, HttpClient? client = null)
        {
            if (client == null)
                sharedClient = CreateHttpClient();

            this.client = client ?? sharedClient!;

            Endpoint = endpoint;
        }

        private DynamicRPCObject(string className, DynamicRPCObject parent, int lastId, string endpoint)
        {
            this.className = className;
            this.parent = parent;
            this.id = lastId;
            Endpoint = endpoint;
        }


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

        public override bool TryGetMember(GetMemberBinder binder, out object? result)
        {
            if (binder.Name.Equals("endpoint", StringComparison.InvariantCultureIgnoreCase))
                result = Endpoint;
            else
                result = new DynamicRPCObject(binder.Name, this, id, Endpoint);

            return true;
            
        }

        public override bool TrySetMember(SetMemberBinder binder, object? value)
        {
            if (binder.Name.Equals("endpoint", StringComparison.InvariantCultureIgnoreCase))
            {
                Endpoint = (string)value!;
                return true;
            }
            else
            {  
                return false;
            }
        }

        private bool Invoke(string methodName, object?[]? args, out object? result)
        {
            dynamic payload = new JObject();
            payload.jsonrpc = "2.0";
            payload.id = id++;
            payload.method = methodName;
            payload.@params = JToken.FromObject(args ?? Array.Empty<object>());

            string payloadString = payload.ToString();

            result = Task.Run(async () =>
            {
                var result = await client!.PostAsync(Endpoint, new StringContent(payloadString, System.Text.Encoding.UTF8, "application/json"));
                if (!result.IsSuccessStatusCode) throw new Exception($"The server responded with an error status code ('{result.StatusCode}: {result.ReasonPhrase}')");

                string content = await result.Content.ReadAsStringAsync();
                return JObject.Parse(content);
            });

            return true;
        }

        public override bool TryInvokeMember(InvokeMemberBinder binder, object?[]? args, out object? result)
        {
            if (parent == null)
            {
                return Invoke(binder.Name, args ?? Array.Empty<object>(), out result);

            }
            else
            {
                return parent.Invoke($"{className}.{binder.Name}", args ?? Array.Empty<object>(), out result);
            }
        }
    }
}

namespace MainProg
{
    public static class Program
    {
        public static async Task<int> Main(string[] args)
        {
            dynamic rpc = new DynamicRPC.DynamicRPCObject("http://127.0.0.1:6801/jsonrpc");

            string[] urls = ["http://example.org/file"];
            await rpc.aria2.addUri("token:mysecret123", urls);

            return 0;
        }
    }
}
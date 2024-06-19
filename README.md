# DynamicRPC

A very easy-to-use dynamic client for using JSON RPC APIs in .NET



### Exemple usage with Aria2

```csharp
// Create the dynamic RPC Client object using 'dynamic' as type
dynamic client = new DynamicRPCObject("http://localhost:6800/jsonrpc");

// Call a method (aria2.github.io/manual/en/html/aria2c.html#aria2.addUri)
dynamic download = await client.aria2.addUri("token:$$secret$$", ["https://ash-speed.hetzner.com/100MB.bin"]);

// Use the response as a dynamic object (using JSON.NET)
string gid = download.result;

// Call a second method (aria2.github.io/manual/en/html/aria2c.html#aria2.tellStatus)
await client.aria2.tellStatus("token:$$secret$$", gid, ["status"]);

// Print the result (here, "waiting" will be printed)
Console.WriteLine(client.result.status);
```



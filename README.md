# DynamicRPC

Easily work dynamically with JSON RPC calls in C#



## Exemple request using a local Aria2c JSONRPC client

```csharp
// Create a DynamicRPCObject the aria2c jsonrpc endpoint
dynamic client = new DynamicRPCObject("http://localhost:8888/jsonrpc");

// Do a request asynchronously 
// Here the '_'s replace the dots in the method name (here 'aria2_addUri' that would be equal to aria2.addUri) 
// To use underscores in the method name, you should use double underscores ('__') instead
// The response is a JObject from Newtonsoft.JSON
dynamic download = await client.aria2_addUri("token:$$secret$$", ["https://ash-speed.hetzner.com/100MB.bin"]);

// Access to the properties of the JObject dynamically, then perform another request
string gid = download.result;
dynamic status = await client.aria2_tellStatus("token:$$secret$$", [gid, ["status"]]);

Console.WriteLine(status.result);
```







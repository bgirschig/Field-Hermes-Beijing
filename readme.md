https://pastebin.com/0HYnXKLC

## Protocol buffers
The communications over the network use protocol buffers to serialize/deserialize data.
to rebuild the protobuf classes:

- make sure you have [protoc](https://github.com/protocolbuffers/protobuf) installed on your system:
- Run:
```powershell
protoc.exe --csharp_out=./Assets/Scripts/protobufs ./Assets/Scripts/protobufs/*.proto
```
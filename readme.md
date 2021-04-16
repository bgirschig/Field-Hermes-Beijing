## Stars
We used to have a dynamic star field, that would "follow" the camera (stars out of view would be
moved back 'to the other side'). This was to limit the amount of stars needed.

This is impractical with portals, because we need two star fields (main cam & portal cam), which means:
- Twice the draw calls (2 for render, 2 for post process mask)
- 2 update loops
- more complex teleporting logic.

After some tests, it looks like a static star field does the job fine, and:
- Does not require updates
- Is trivial to use with portals (just make the field large enough, so that star density is not
noticeably different through the portal)

## Protocol buffers
The communications over the network use protocol buffers to serialize/deserialize data.
to rebuild the protobuf classes:

- make sure you have [protoc](https://github.com/protocolbuffers/protobuf) installed on your system:
- Run:
```powershell
protoc.exe --csharp_out=./Assets/Scripts/protobufs ./Assets/Scripts/protobufs/*.proto
```

## Thanks
All hail:
- [ronja's tutorials](https://www.ronja-tutorials.com/)
- [RenderDoc](https://renderdoc.org/)
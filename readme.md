# Field Hermès Beijing
A journey through an imaginary city between Paris and Beijing, in which objects take over.
For more information, head to the [Project page](https://bastiengirschig.com/item/377)

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

## Mesh optimisation
- recalculate normals outside
    - didn't work ? manual normals cleanup (flip inside-out normals)
- Use the MatCombiner plugin
- fix broken UVs (some sketchup models break when loaded in blender)
- save as fbx

Priorities (sorted)
- [x] FRA_tourEiffel
- [x] FRA_sculpture
- [x] FRA_grandPalais
- [x] FRA_louvre
- [x] FRA_sacreCoeur
- [] FRA_decorA

## Layers
Layers are used to prevent bad interactions between different "worlds" (groups of objects, "seen" by portals)

Eg. prevent the light that is illuminating the scene on the other side of the portal from illuminating this side of the portal

This also helps performance, since each camera only renders 'useful' objects

To reduce the amount of layers needed, the same layers are used to mask the postprocessing effect.
This means we have to leave all objects that should not be affected by postprocessing on the "default" layer, but since those
objects are usually quite simple (unlit primitives), it seems ok to leave them there (they won't interact with lights)

In case the above compromise does not work anymore, we could duplicate the layers like so:
- group1-filter
- group1-nofilter
- group2-filter
- group2-nofilter
- group3-filter
- group3-nofilter
...
or use a custom layer system, that allows multiple layers on a single gameObject:
- An object in group 1, with postprocessing applied:
    - layer1: group1
    - layer2: filter
- An object in group 3, bypassing postprocessing
    - layer1: group3
    - layer2: nofilter
This may be hard, since it still needs to play nice with built in components (eg. Camera & Lights culling mask)

## Shadows
We are using a point light very close to the ground (sometimes below the objects that cast its shadow), which
should create infinite shadows.

We can't do that so we cheat, with a floor plane slightly rotated to match the horizon:

![shadow hack illustration](projectDoc/shadow.png)

(Angle greatly exagerated for demonstration)

## Thanks
All hail:
- [ronja's tutorials](https://www.ronja-tutorials.com/)
- [RenderDoc](https://renderdoc.org/)

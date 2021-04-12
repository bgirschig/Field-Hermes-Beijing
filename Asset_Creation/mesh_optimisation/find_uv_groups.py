# Find uv groups (islands)

import bpy
import bmesh

currentDirection = 1

class HashedUV(object):
    def __init__(self, uv):
        self.uv = uv
    def __eq__(self, other):
        return self.uv.x == other.uv.x and self.uv.y == other.uv.y
    def __hash__(self):
        return hash(f"{self.uv.x} | {self.uv.y}")

def main(context):
    global currentDirection

    obj = context.active_object
    me = obj.data
    bm = bmesh.from_edit_mesh(me)

    uv_layer = bm.loops.layers.uv.verify()

    # adjust uv coordinates
    faces = set(bm.faces[:])
    
    sample_x = bm.faces[0].loops[0][uv_layer].uv.x
    if sample_x > 1:
        currentDirection = -1
    elif sample_x < 0:
        currentDirection = 1
    
    uv_groups = []

    for face in bm.faces:
        faceUVs = set()
        for loop in face.loops:
            loop_uv = loop[uv_layer]
            faceUVs.add(HashedUV(loop_uv.uv))

        selected_group = None
        # Iterate with a custom (reverse) loop so that we can delete groups while iterating
        for idx in range(len(uv_groups) - 1, -1, -1):
            if len(set.intersection(faceUVs, uv_groups[idx])):
                if selected_group:
                    selected_group.update(uv_groups[idx])
                    del uv_groups[idx]
                else:
                    selected_group = uv_groups[idx]
                    selected_group.update(faceUVs)
        if not selected_group:
            # create a new group 
            uv_groups.append(faceUVs)
    
    print(f"Found {len(uv_groups)} uv groups (islands)")

    bmesh.update_edit_mesh(me)


class UvOperator(bpy.types.Operator):
    """UV Operator description"""
    bl_idname = "uv.simple_operator"
    bl_label = "Simple UV Operator"

    @classmethod
    def poll(cls, context):
        obj = context.active_object
        return obj and obj.type == 'MESH' and obj.mode == 'EDIT'

    def execute(self, context):
        main(context)
        return {'FINISHED'}


def register():
    bpy.utils.register_class(UvOperator)


def unregister():
    bpy.utils.unregister_class(UvOperator)


if __name__ == "__main__":
    register()
    bpy.ops.uv.simple_operator()
import bpy
import bmesh
import math
from mathutils import Vector

def condense_uvs(obj):
    """ Transform the UV map to make it fit in the smallest area possible
        eg. if a uv face is at (1.5, 16.3) move it to (0.5, 0.3)
    """
    # keep ref to active object to be able to restore it
    prev_active = bpy.context.view_layer.objects.active
    
    me = obj.data
    bm = bmesh.from_edit_mesh(me)

    uv_layer = bm.loops.layers.uv.verify()
    
    uv_groups = []
    for face in bm.faces:
        # figure out the face bounds
        minX = math.inf
        minY = math.inf
        maxX = -math.inf
        maxY = -math.inf
        for loop in face.loops:
            uv = loop[uv_layer].uv
            if (uv.x < minX): minX = uv.x
            if (uv.x > maxX): maxX = uv.x
            if (uv.y < minY): minY = uv.y
            if (uv.y > maxY): maxY = uv.y
        
        # Compute the offset to move as close to the origin as possible
        offsetX = minX // 1
        offsetY = minY // 1
        
        # Sometimes the offset face takes more space on one side of the texture space that the other
        # in that case, move it to the other side (by adding 1 to the offset)
        if ((maxX - offsetX) - 1 > 1 - (minX - offsetX)):
            offsetX += 1
        if ((maxY - offsetY) - 1 > 1 - (minY - offsetY)):
            offsetY += 1

        # apply the offset
        offset = Vector((offsetX, offsetY))
        for loop in face.loops:
            loop[uv_layer].uv -= offset

    # Apply our modifications
    bmesh.update_edit_mesh(me)
    
    # clean up uvs by merging the ones that end up in the same position
    bpy.ops.uv.select_all()
    bpy.ops.uv.remove_doubles()
    
    # restore the active object to the state it had before the call to this function
    bpy.context.view_layer.objects.active = prev_active

def bake_texrures(obj):
    me = obj.data
    bm = bmesh.from_edit_mesh(me)
    uv_layer = bm.loops.layers.uv.verify()

    # Create a map of all the textures and data about them:
    # - actual refetence to the texture
    # - a list of faces that are using this texture
    # - the UV bounds (min and max) for the uv coordinates seen in that texture
    textures_map = {}
    for face in bm.faces:
        texture = get_texture_for_face(obj, face)

        # Special case for solid colors: move all uv coordinates to a single position so that
        # they only need a single pixel in the final packed texture 
        if isinstance(texture, SolidColorTexture):
            for loop in face.loops:
                loop[uv_layer].uv.x = 0.5
                loop[uv_layer].uv.y = 0.5

        if not texture in textures_map:
            textures_map[texture] = {
                "texture": texture,
                "faces": set(),
                "uv_min": Vector((math.inf,math.inf)),
                "uv_max": Vector((-math.inf,-math.inf)),
            }
        texture_item = textures_map[texture]
        texture_item["faces"].add(face)
        
        # update the uv bounds for this texture
        uv_min = texture_item["uv_min"]
        uv_max = texture_item["uv_max"]
        for loop in face.loops:
            uv = loop[uv_layer].uv
            if (uv.x < uv_min.x): uv_min.x = uv.x
            if (uv.x > uv_max.x): uv_max.x = uv.x
            if (uv.y < uv_min.y): uv_min.y = uv.y
            if (uv.y > uv_max.y): uv_max.y = uv.y
    
    # Now that we have the final UV bounds for each face, we can compute the size each one
    # should take in the baked output
    total_pixel_count = 0
    textures = list(textures_map.values())
    for texture in textures:
        # Compute the output size
        texture['uv_size'] = texture["uv_max"] - texture["uv_min"]
        texture['baked_pixel_size'] = texture['uv_size'] * Vector(texture["texture"].size)
        
        # Make sure the baked texture is at least 1x1 px
        # Without this, SolidColor textures would have a baked size of 0x0
        texture['baked_pixel_size'].x = max(1, texture['baked_pixel_size'].x)
        texture['baked_pixel_size'].y = max(1, texture['baked_pixel_size'].y)
        
        # Compute some extra data (for sorting problematic textures, etc...)
        texture['baked_pixel_count'] = texture['baked_pixel_size'].x * texture['baked_pixel_size'].y
        total_pixel_count += texture['baked_pixel_count']
    
    # show a summary of bake-preprocess data, sorted by size
    textures.sort(key=lambda t: t["baked_pixel_count"])
    for texture in textures:
        size = f"{texture['baked_pixel_size'].x :.0f} x {texture['baked_pixel_size'].y :.0f} px"    
        print(f"{texture['texture'].name :<30} {size:<20}{len(texture['faces'])} faces")
    
    total_pixel_surface = math.sqrt(total_pixel_count)
    print(f"found {len(textures)} textures taking a total of {total_pixel_surface} square pixels")

    # TODO: Create individual textures, tiled as needed, based on bake-preprocess data 
    # TODO: Pack textures
    # TODO: Modify UVs to match packed positions

    bmesh.update_edit_mesh(me)

def get_texture_for_face(obj, face):
    textures = get_textures_for_face(obj, face)
    textureCount = len(textures)
    if textureCount > 1:
        raise BaseException("More than one texture found. Can't decide which one to use for baking")
    if textureCount < 1:
        # TODO: Create a 1x1 texture with the face base color / vertex color to handle this case
        mat = obj.material_slots[face.material_index].material
        return SolidColorTexture(tuple(mat.diffuse_color))
    else:
        texture = textures[0]
    return texture

def get_textures_for_face(obj, face):
    mat = obj.material_slots[face.material_index].material
    output = []
    for node in mat.node_tree.nodes:
        if node.type == "TEX_IMAGE":
            output.append(node.image)
    return output

class SolidColorTexture:
    def __init__(self, color=(0,0,0)):
        self.color = color
        self.name = f"Solid Color {color[0]*255 :.0f}-{color[1]*255 :.0f}-{color[2]*255 :.0f}"
        self.size = (0,0) # ??
    def __eq__(self, other):
        return self.color == other.color
    def __hash__(self):
        return hash(self.color)

def main(context):
    print("---")
    condense_uvs(context.active_object)
    bake_texrures(context.active_object)


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
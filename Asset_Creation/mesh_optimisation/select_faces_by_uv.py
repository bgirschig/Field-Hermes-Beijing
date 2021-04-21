""" Select faces depending on properties of their UVs """
import bpy
import bmesh
import math
from mathutils import Vector

class Operator(bpy.types.Operator):
    bl_idname = "select.by_uv_size"
    bl_label = "Select faces depending on properties of their UVs"

    @classmethod
    def poll(cls, context):
        obj = context.active_object
        print(obj.type, obj.mode, obj and obj.type == 'MESH' and obj.mode == 'EDIT')
        return obj and obj.type == 'MESH' and obj.mode == 'EDIT'

    def execute(self, context):
        run(context.active_object, density_treshold=1)
        return {'FINISHED'}

def run(obj, density_treshold):
    me = obj.data
    bm = bmesh.from_edit_mesh(me)
    face_areas = get_face_areas(bm)
    max_density = 0
    selected_count = 0
    face_count = 0
    for face in face_areas:
        # face.select = False
        face_area, uv_area = face_areas[face]
        texture = get_texture_for_face(obj, face)
        pixel_area = math.sqrt(uv_area * texture.size[0] * texture.size[1])

        if (face_area == 0):
            continue
        
        texel_density = pixel_area / face_area
        if (face.select):
            print("selected face's texel_density", texel_density)

        if (texel_density > max_density):
            max_density = texel_density
            max_density_face = face
        face_count += 1
        if (texel_density > density_treshold):
            face.select = True
            selected_count += 1

    print(f"selected {selected_count} / {face_count} faces")
    print("max_density", max_density)
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

# Thanks https://blender.stackexchange.com/questions/150797/how-to-get-the-value-of-face-area-using-python !
def get_face_areas(bm):
    # Ensure faces access
    bm.faces.ensure_lookup_table()

    # Triangulate it so that we can calculate tri areas
    triangle_loops = bm.calc_loop_triangles()

    # Initialize face entries
    areas = {face: (0.0, 0.0) for face in bm.faces}

    # Get the uv map
    uv_layer = bm.loops.layers.uv['UVMap']

    # enumerate the loops
    for loop in triangle_loops:
        # Get the face
        face = loop[0].face
        # Get current areas
        face_area, uv_area = areas[face]
        # Add tri surface area
        face_area += triangle_area( *(l.vert.co for l in loop) )
        # Add corresponding uv surface area
        uv_area += triangle_area( *(Vector( (*l[uv_layer].uv, 0) ) for l in loop) )
        # Set the result in the dictionary
        areas[face] = (face_area, uv_area)

    return areas

def distance_to_line(point, lineA, lineB):
    x1, y1 = lineA
    x2, y2 = lineB
    x0, y0 = point
    return abs((x2-x1)(y1-y0) - (x1-x0)(y2-y1)) / math.sqrt(math.pow(x2-x1, 2) + math.pow(y2-y1, 2))

def triangle_area(p0, p1, p2):
    return (p1 - p0).cross( p2 - p0 ).length / 2.0

def get_face_uv_size(face, uv_layer):
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
    return max(abs(maxX-minX), abs(maxY-minY))

def register():
    bpy.utils.register_class(Operator)

def unregister():
    bpy.utils.unregister_class(Operator)

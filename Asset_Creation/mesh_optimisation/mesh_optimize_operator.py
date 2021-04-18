import bpy
import bmesh
import math

from uv_condenser import condense_uvs
from bake_textures import bake_textures

class MeshOptimizerOperator(bpy.types.Operator):
    bl_idname = "object.optimize"
    bl_label = "Optimize a mesh for unity"

    @classmethod
    def poll(cls, context):
        obj = context.active_object
        return obj and obj.type == 'MESH' and obj.mode == 'EDIT'

    def execute(self, context):
        condense_uvs(context.active_object)
        bake_textures(context.active_object)
        return {'FINISHED'}

def register():
    bpy.utils.register_class(MeshOptimizerOperator)

def unregister():
    bpy.utils.unregister_class(MeshOptimizerOperator)

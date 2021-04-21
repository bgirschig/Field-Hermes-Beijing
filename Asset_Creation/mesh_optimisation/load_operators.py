# Run this inside blender to load the module

import bpy
import os
import importlib
import sys

dir = os.path.dirname(bpy.data.filepath)
if not dir in sys.path:
  sys.path.append(dir )

import bake_textures
import find_uv_groups
import mesh_optimize_operator
import texture_utils
import uv_condenser
import select_faces_by_uv
importlib.reload(bake_textures)
importlib.reload(find_uv_groups)
importlib.reload(mesh_optimize_operator)
importlib.reload(texture_utils)
importlib.reload(uv_condenser)
importlib.reload(select_faces_by_uv)

select_faces_by_uv.register()
mesh_optimize_operator.register()
print("---")
# bpy.ops.select.by_uv_size()
select_faces_by_uv.run(bpy.context.active_object, 100)

filename = os.path.join(os.path.dirname(bpy.data.filepath), "mesh_optimize_operator.py")
exec(compile(open(filename).read(), filename, 'exec'))
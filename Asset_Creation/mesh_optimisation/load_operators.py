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
importlib.reload(bake_textures)
importlib.reload(find_uv_groups)
importlib.reload(mesh_optimize_operator)
importlib.reload(texture_utils)
importlib.reload(uv_condenser)

mesh_optimize_operator.register()
print("---")
bpy.ops.object.optimize()

filename = os.path.join(os.path.dirname(bpy.data.filepath), "mesh_optimize_operator.py")
exec(compile(open(filename).read(), filename, 'exec'))
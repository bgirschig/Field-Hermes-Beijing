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
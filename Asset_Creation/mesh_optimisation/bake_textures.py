from texture_utils import SolidColorTexture, get_texture_for_face
from mathutils import Vector
import bmesh
import math

def bake_textures(obj):
  # Bake texture: Create a single texture that contains all the texture this object needs
  # UVs will be updated to match new positions

  # This should only be run on a mesh whose UVs have been 'condensed'
  # TODO: run UV condenser here

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
      texture['baked_pixel_size'].x = math.ceil(max(1, texture['baked_pixel_size'].x))
      texture['baked_pixel_size'].y = math.ceil(max(1, texture['baked_pixel_size'].y))
      
      # Compute some extra data (for sorting problematic textures, etc...)
      texture['baked_pixel_count'] = texture['baked_pixel_size'].x * texture['baked_pixel_size'].y
      total_pixel_count += texture['baked_pixel_count']
  
  # show a summary of bake-preprocess data, sorted by size
  textures.sort(key=lambda t: t["baked_pixel_count"])
  for texture in textures:
      size = f"{texture['baked_pixel_size'].x :.0f} x {texture['baked_pixel_size'].y :.0f} px"    
      print(f"{texture['texture'].name :<30} {size:<20}{len(texture['faces'])} faces")
  
  total_pixel_surface = math.ceil(math.sqrt(total_pixel_count))
  print(f"found {len(textures)} textures taking a total of {total_pixel_surface} square pixels")

  # image = bpy.data.images.new("MyImage", width=200, height=200)
  # image.filepath_raw = "/tmp/temp.png"
  # image.file_format = 'PNG'
  # image.save()

  # TODO: Create individual textures, tiled as needed, based on bake-preprocess data 
  # TODO: Pack textures
  # TODO: Modify UVs to match packed positions

  bmesh.update_edit_mesh(me)
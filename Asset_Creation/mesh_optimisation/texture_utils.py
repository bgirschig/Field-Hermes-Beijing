class SolidColorTexture:
  def __init__(self, color=(0,0,0)):
    self.color = color
    self.name = f"Solid Color {color[0]*255 :.0f}-{color[1]*255 :.0f}-{color[2]*255 :.0f}"
    self.size = (0,0) # ??
  def __eq__(self, other):
    return self.color == other.color
  def __hash__(self):
    return hash(self.color)

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
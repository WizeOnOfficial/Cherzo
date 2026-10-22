"""
chemkit — shared Blender helpers for the Chemnoble asset pipeline.

Usage inside Blender:
    import sys; sys.path.insert(0, r"<project>/ArtSource/_tools")
    import importlib, chemkit; importlib.reload(chemkit)

Conventions (see docs/assets/art-style-guide.md):
  * 1 Blender unit = 1 metre. Z up, asset FRONT faces Blender -Y
    (becomes Unity +Z / transform.forward after export).
  * All colour comes from ONE palette texture (T_Palette.png, 8x8 swatches
    of 16px = 128x128). Each face's UVs are collapsed onto a swatch centre.
  * Two materials only: M_Palette (lit) and M_Palette_Emit (screens, LEDs,
    toxic liquid, emergency lights).
  * Colliders are child meshes named *_COL (simplified, no material use).
  * Sockets / camera targets are empties named Socket_* / CamFocus_*.
"""
import bpy, bmesh, os, math
from mathutils import Vector

PROJECT = os.path.abspath(os.path.join(os.path.dirname(__file__), "..", ".."))
PALETTE_PATH = os.path.join(PROJECT, "Assets", "Art", "Textures", "T_Palette.png")

GRID = 8          # 8x8 swatches
CELL = 16         # px per swatch -> 128x128 texture (safe with Unity default mipmaps/bilinear)

# family -> 4 shades (0 = lightest ... 3 = darkest)
FAMILIES = [
    # rows 0-3 (top half of the texture)
    ("white",    ["F2F0EB", "D9D7D2", "B8B6B2", "8E8D8A"]),  # facility panels / walls
    ("metal",    ["A7AEB4", "7E868D", "575E65", "33383D"]),  # cool steel / machinery
    ("char",     ["4A4A4A", "2E2E2E", "1C1C1C", "0E0E0E"]),  # rubber, plastic, gaps
    ("orange",   ["FF9A3C", "F07818", "C2560C", "8A3A08"]),  # brand / hazard accent
    ("yellow",   ["FFE066", "F2C618", "C99A0C", "8A6608"]),  # safety yellow, stripes
    ("red",      ["FF6B5E", "E0332B", "A81E1A", "6B1210"]),  # emergency, alarm, denied
    ("green",    ["8CFFB0", "33E07A", "1A9E52", "0E5E30"]),  # LEDs, granted, terminal
    ("cyan",     ["8FE3FF", "3BB5E8", "1F7AAE", "124A6B"]),  # screens, UI glow
    # rows 4-7 (bottom half)
    ("uniform",  ["A9B8C8", "7F93A8", "566B80", "37475A"]),  # MC shirt blue-grey
    ("navy",     ["4B5A80", "33405F", "232D45", "151B2B"]),  # trousers, dark accents
    ("wood",     ["D9A066", "B07A45", "7F552E", "4F331B"]),  # office desks, crates
    ("skin",     ["F2C9A5", "D9A380", "B07A5A", "7A513A"]),  # hands / characters
    ("toxic",    ["E8FF7A", "B8F23A", "7FC21A", "4A7A0E"]),  # THE leaked liquid (emissive)
    ("brass",    ["E8B86A", "C28E3E", "8F6224", "5A3C14"]),  # fuse caps, terminals
    ("concrete", ["C9C3B8", "A39D93", "7A756D", "524E48"]),  # floors, raw walls
    ("glass",    ["B8E0DC", "86BDB8", "5A8F8A", "3A5F5C"]),  # glass, lab tint
]

def _hex(h):
    return tuple(int(h[i:i+2], 16) / 255.0 for i in (0, 2, 4))

def swatch_cell(name):
    """'orange_1' -> (col,row) where row counts from the TOP of the image."""
    fam, shade = name.rsplit("_", 1)
    shade = int(shade)
    for idx, (f, _) in enumerate(FAMILIES):
        if f == fam:
            col = idx % GRID
            row = (idx // GRID) * 4 + shade
            return col, row
    raise KeyError(f"unknown swatch {name}")

def swatch_uv(name):
    col, row = swatch_cell(name)
    return ((col + 0.5) / GRID, 1.0 - (row + 0.5) / GRID)

def swatch_rgb(name):
    fam, shade = name.rsplit("_", 1)
    for f, shades in FAMILIES:
        if f == fam:
            return _hex(shades[int(shade)])
    raise KeyError(name)

# ------------------------------------------------------------------ palette
def build_palette_image(save=True):
    """Write T_Palette.png from FAMILIES, then (re)load the 'T_Palette' datablock from
    disk so every material keeps its link. Never resizes an image in place (that
    silently produced an all-black PNG in Blender 5)."""
    size = GRID * CELL
    tmp = bpy.data.images.new("_T_Palette_build", size, size, alpha=False)
    px = [0.0] * (size * size * 4)
    for idx, (fam, shades) in enumerate(FAMILIES):
        for s, hx in enumerate(shades):
            col = idx % GRID
            row_top = (idx // GRID) * 4 + s
            r, g, b = _hex(hx)
            for yy in range(CELL):
                for xx in range(CELL):
                    x = col * CELL + xx
                    y = size - 1 - (row_top * CELL + yy)   # blender origin bottom-left
                    i = (y * size + x) * 4
                    px[i:i+4] = [r, g, b, 1.0]
    tmp.pixels.foreach_set(px)
    tmp.update()
    if save:
        os.makedirs(os.path.dirname(PALETTE_PATH), exist_ok=True)
        tmp.filepath_raw = PALETTE_PATH
        tmp.file_format = "PNG"
        tmp.save()
    bpy.data.images.remove(tmp)
    img = bpy.data.images.get("T_Palette")
    if img is None:
        img = bpy.data.images.load(PALETTE_PATH)
        img.name = "T_Palette"
    else:
        img.filepath = PALETTE_PATH
        img.source = "FILE"
        img.reload()
    assert_palette_ok(img)
    return img

def assert_palette_ok(img=None):
    """Fail loudly if the palette image is missing, wrong size or not colourful."""
    img = img or bpy.data.images.get("T_Palette")
    if img is None:
        raise RuntimeError("T_Palette image missing")
    w, h = img.size
    if (w, h) != (GRID * CELL, GRID * CELL):
        raise RuntimeError(f"T_Palette has size {w}x{h}, expected {GRID*CELL}")
    px = img.pixels[:]
    colours = {tuple(round(c, 2) for c in px[i:i+3]) for i in range(0, len(px), 4 * CELL)}
    if len(colours) < 32:
        raise RuntimeError(f"T_Palette looks broken ({len(colours)} colours) - not exporting")

def _load_palette():
    img = bpy.data.images.get("T_Palette")
    if img is None:
        if os.path.exists(PALETTE_PATH):
            img = bpy.data.images.load(PALETTE_PATH)
            img.name = "T_Palette"
        else:
            img = build_palette_image()
    return img

def _make_mat(name, emissive):
    mat = bpy.data.materials.get(name)
    if mat:
        for n in mat.node_tree.nodes:          # repair a lost image link
            if n.bl_idname == "ShaderNodeTexImage" and n.image is None:
                n.image = _load_palette()
        return mat
    mat = bpy.data.materials.new(name)
    try: mat.use_nodes = True
    except Exception: pass
    nt = mat.node_tree
    bsdf = nt.nodes.get("Principled BSDF")
    tex = nt.nodes.new("ShaderNodeTexImage")
    tex.image = _load_palette()
    tex.interpolation = "Closest"
    tex.location = (-400, 200)
    nt.links.new(tex.outputs["Color"], bsdf.inputs["Base Color"])
    bsdf.inputs["Roughness"].default_value = 0.8
    if "Specular IOR Level" in bsdf.inputs:
        bsdf.inputs["Specular IOR Level"].default_value = 0.2
    if emissive:
        nt.links.new(tex.outputs["Color"], bsdf.inputs["Emission Color"])
        bsdf.inputs["Emission Strength"].default_value = 2.0
    return mat

def materials():
    return _make_mat("M_Palette", False), _make_mat("M_Palette_Emit", True)

# ------------------------------------------------------------------ scene
_WORLD = {}   # object name -> world-space origin (we never rotate parents)

def new_scene(name):
    for o in list(bpy.data.objects):
        bpy.data.objects.remove(o, do_unlink=True)
    for m in list(bpy.data.meshes):
        if m.users == 0: bpy.data.meshes.remove(m)
    bpy.context.scene.name = name
    bpy.context.scene.unit_settings.system = "METRIC"
    bpy.context.scene.unit_settings.scale_length = 1.0
    _WORLD.clear()
    materials()

def _link(obj, parent):
    bpy.context.scene.collection.objects.link(obj)
    if parent is not None:
        obj.parent = parent
        obj.location = Vector(obj.location) - _WORLD[parent.name]
    return obj

def empty(name, world_loc=(0, 0, 0), parent=None, kind="PLAIN_AXES", size=0.05):
    o = bpy.data.objects.new(name, None)
    o.empty_display_type = kind
    o.empty_display_size = size
    o.location = world_loc
    _WORLD[name] = Vector(world_loc)
    return _link(o, parent)

def _finish_mesh(name, bm, origin, parent, swatch, emissive):
    me = bpy.data.meshes.new(name)
    bm.to_mesh(me); bm.free()
    me.uv_layers.new(name="UVMap")
    obj = bpy.data.objects.new(name, me)
    obj.location = origin
    _WORLD[name] = Vector(origin)
    _link(obj, parent)
    lit, emit = materials()
    me.materials.append(lit)
    me.materials.append(emit)
    for p in me.polygons:
        p.material_index = 1 if emissive else 0
        p.use_smooth = False
    if swatch:
        paint(obj, swatch)
    return obj

def box(name, size, center, swatch="white_1", parent=None, origin=None, emissive=False):
    """Axis-aligned box. size=(x,y,z) metres, center in WORLD space.
    origin (world) = object pivot; defaults to the box centre."""
    origin = Vector(origin if origin is not None else center)
    c = Vector(center) - origin
    hx, hy, hz = (s / 2 for s in size)
    bm = bmesh.new()
    vs = [bm.verts.new(c + Vector((x * hx, y * hy, z * hz)))
          for x in (-1, 1) for y in (-1, 1) for z in (-1, 1)]
    faces = [(0, 1, 3, 2), (4, 6, 7, 5), (0, 4, 5, 1), (2, 3, 7, 6), (0, 2, 6, 4), (1, 5, 7, 3)]
    for f in faces:
        bm.faces.new([vs[i] for i in f])
    bmesh.ops.recalc_face_normals(bm, faces=bm.faces)
    return _finish_mesh(name, bm, origin, parent, swatch, emissive)

def cylinder(name, radius, depth, center, axis="Z", segments=8, swatch="metal_1",
             parent=None, origin=None, emissive=False):
    origin = Vector(origin if origin is not None else center)
    bm = bmesh.new()
    bmesh.ops.create_cone(bm, cap_ends=True, cap_tris=False, segments=segments,
                          radius1=radius, radius2=radius, depth=depth)
    rot = {"Z": None, "X": ("Y", math.pi / 2), "Y": ("X", math.pi / 2)}[axis]
    if rot:
        from mathutils import Matrix
        bmesh.ops.rotate(bm, verts=bm.verts, cent=(0, 0, 0), matrix=Matrix.Rotation(rot[1], 3, rot[0]))
    bmesh.ops.translate(bm, verts=bm.verts, vec=Vector(center) - origin)
    return _finish_mesh(name, bm, origin, parent, swatch, emissive)

def prism(name, outline_xz, depth, y_front, swatch="char_1", parent=None, origin=None, emissive=False):
    """Extrude a 2D outline (list of (x,z) world coords, CCW seen from front)
    along +Y from y_front by `depth`. Good for symbols/labels on a front face."""
    pts = [Vector((x, y_front, z)) for x, z in outline_xz]
    cen = sum(pts, Vector()) / len(pts)
    origin = Vector(origin if origin is not None else cen)
    bm = bmesh.new()
    front = [bm.verts.new(p - origin) for p in pts]
    back = [bm.verts.new(p - origin + Vector((0, depth, 0))) for p in pts]
    bm.faces.new(front)
    bm.faces.new(list(reversed(back)))
    n = len(pts)
    for i in range(n):
        j = (i + 1) % n
        bm.faces.new([front[i], back[i], back[j], front[j]])
    bmesh.ops.recalc_face_normals(bm, faces=bm.faces)
    return _finish_mesh(name, bm, origin, parent, swatch, emissive)

def paint(obj, swatch, face_filter=None, emissive=None):
    """Collapse UVs of faces onto a palette swatch. face_filter(poly)->bool."""
    me = obj.data
    u, v = swatch_uv(swatch)
    uv = me.uv_layers.active.data
    for p in me.polygons:
        if face_filter and not face_filter(p):
            continue
        for li in p.loop_indices:
            uv[li].uv = (u, v)
        if emissive is not None:
            p.material_index = 1 if emissive else 0

def facing(direction, tol=0.9):
    d = Vector(direction).normalized()
    return lambda p: p.normal.dot(d) > tol

def collider(name, size, center, parent):
    o = box(name, size, center, swatch="char_3", parent=parent)
    o.data.materials.clear()
    o.display_type = "WIRE"
    o.hide_render = True
    return o

def boolean_cut(target, cutter):
    mod = target.modifiers.new("cut", "BOOLEAN")
    mod.operation = "DIFFERENCE"
    mod.object = cutter
    mod.solver = "EXACT"
    dg = bpy.context.evaluated_depsgraph_get()
    ev = target.evaluated_get(dg)
    me = bpy.data.meshes.new_from_object(ev)
    old = target.data
    target.modifiers.clear()
    target.data = me
    bpy.data.meshes.remove(old)
    bpy.data.objects.remove(cutter, do_unlink=True)
    return target

# ------------------------------------------------------------------ stats / export
def tri_count(root):
    total = 0
    for o in [root] + list(root.children_recursive):
        if o.type == "MESH" and not o.name.endswith("_COL"):
            total += sum(len(p.vertices) - 2 for p in o.data.polygons)
    return total

def select_hierarchy(root):
    bpy.ops.object.select_all(action="DESELECT")
    for o in [root] + list(root.children_recursive):
        o.hide_set(False)
        o.select_set(True)
    bpy.context.view_layer.objects.active = root

def export_unity_fbx(root, rel_path, include_colliders=False):
    """Export the hierarchy so Unity shows it upright with ALL transforms at 0/0/0
    and coloured, with no Unity-side script or import setting needed.

    Trick: the FBX exporter converts Z-up -> Y-up by putting Rx(-90) on the root.
    We pre-bake Rx(-90) into every mesh and child position and give the root Rx(+90),
    so the exporter's conversion cancels out: root = identity, children = identity,
    geometry already Y-up. The palette texture is embedded so Unity's default
    material import picks it up. Everything is reverted after export.
    rel_path relative to project root, e.g. 'Assets/Art/Models/Interactables/SM_X.fbx'.
    *_COL helper meshes stay in the .blend but are NOT exported by default (team adds
    Box Colliders in Unity; no import script is used)."""
    from mathutils import Matrix
    materials()                                  # also repairs lost palette links
    for m in materials():
        if not any(n.bl_idname == "ShaderNodeTexImage" and n.image for n in m.node_tree.nodes):
            raise RuntimeError(f"{m.name} has no palette image; colours would be lost")
    assert_palette_ok()
    path = os.path.join(PROJECT, rel_path)
    os.makedirs(os.path.dirname(path), exist_ok=True)
    rx_m = Matrix.Rotation(-math.pi / 2, 4, "X")
    rx_p = Matrix.Rotation(math.pi / 2, 4, "X")
    nodes = list(root.children_recursive)
    for o in nodes:
        if any(abs(a) > 1e-6 for a in o.rotation_euler) or any(abs(sc - 1) > 1e-6 for sc in o.scale):
            raise RuntimeError(f"{o.name}: children must be unrotated/unscaled for export")
    meshes = {o.data for o in nodes + [root] if o.type == "MESH"}
    old_rot = root.rotation_euler.copy()
    try:
        for me in meshes:
            me.transform(rx_m)
        for o in nodes:
            o.location = (rx_m @ Vector(o.location).to_4d()).to_3d()
        root.rotation_euler = (old_rot.x + math.pi / 2, old_rot.y, old_rot.z)
        bpy.context.view_layer.update()
        select_hierarchy(root)
        if not include_colliders:
            for o in nodes:
                if o.name.endswith("_COL"):
                    o.select_set(False)
        bpy.ops.export_scene.fbx(
            filepath=path, use_selection=True, object_types={"MESH", "EMPTY"},
            apply_unit_scale=True, apply_scale_options="FBX_SCALE_ALL",
            axis_forward="-Z", axis_up="Y", bake_space_transform=False,
            use_mesh_modifiers=True, mesh_smooth_type="FACE", use_tspace=False,
            add_leaf_bones=False, bake_anim=False,
            path_mode="COPY", embed_textures=True,
        )
    finally:
        for me in meshes:
            me.transform(rx_p)
        for o in nodes:
            o.location = (rx_p @ Vector(o.location).to_4d()).to_3d()
        root.rotation_euler = old_rot
        bpy.context.view_layer.update()
    return path

def save_blend(rel_path):
    path = os.path.join(PROJECT, rel_path)
    os.makedirs(os.path.dirname(path), exist_ok=True)
    bpy.ops.wm.save_as_mainfile(filepath=path, relative_remap=True)
    return path

# ------------------------------------------------------------------ compound
def compound(name, parts, origin, parent=None, emissive=()):
    """One mesh made of several primitives, each with its own swatch.
    parts: list of tuples
        ("box", (sx,sy,sz), center, swatch)
        ("cyl", (radius, depth, axis, segments), center, swatch)
    centers are WORLD coords. `emissive` = swatch names rendered emissive."""
    from mathutils import Matrix
    origin = Vector(origin)
    bm = bmesh.new()
    face_swatch = []
    for kind, dims, center, sw in parts:
        before = len(bm.faces)
        c = Vector(center) - origin
        if kind == "box":
            hx, hy, hz = (s / 2 for s in dims)
            vs = [bm.verts.new(c + Vector((x * hx, y * hy, z * hz)))
                  for x in (-1, 1) for y in (-1, 1) for z in (-1, 1)]
            for f in [(0, 1, 3, 2), (4, 6, 7, 5), (0, 4, 5, 1), (2, 3, 7, 6), (0, 2, 6, 4), (1, 5, 7, 3)]:
                bm.faces.new([vs[i] for i in f])
        elif kind == "cyl":
            r, d, axis, seg = dims
            tmp = bmesh.new()
            bmesh.ops.create_cone(tmp, cap_ends=True, cap_tris=False, segments=seg,
                                  radius1=r, radius2=r, depth=d)
            if axis == "X":
                bmesh.ops.rotate(tmp, verts=tmp.verts, cent=(0, 0, 0), matrix=Matrix.Rotation(math.pi / 2, 3, "Y"))
            elif axis == "Y":
                bmesh.ops.rotate(tmp, verts=tmp.verts, cent=(0, 0, 0), matrix=Matrix.Rotation(math.pi / 2, 3, "X"))
            m = {}
            for v in tmp.verts:
                m[v] = bm.verts.new(v.co + c)
            for f in tmp.faces:
                bm.faces.new([m[v] for v in f.verts])
            tmp.free()
        face_swatch += [sw] * (len(bm.faces) - before)
    bmesh.ops.recalc_face_normals(bm, faces=bm.faces)
    obj = _finish_mesh(name, bm, origin, parent, None, False)
    uv = obj.data.uv_layers.active.data
    for p, sw in zip(obj.data.polygons, face_swatch):
        u, v = swatch_uv(sw)
        for li in p.loop_indices:
            uv[li].uv = (u, v)
        p.material_index = 1 if sw in emissive else 0
    return obj

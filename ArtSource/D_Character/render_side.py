import bpy,os
from mathutils import Vector
out=os.path.dirname(os.path.abspath(__file__))
bpy.ops.wm.open_mainfile(filepath=os.path.join(out,'D_Mantis_Base.blend'))
scene=bpy.context.scene;cam=scene.camera;cam.location=(6,.065,.7);cam.rotation_euler=(Vector((0,.065,.22))-cam.location).to_track_quat('-Z','Y').to_euler();cam.data.ortho_scale=1.25;scene.render.filepath=os.path.join(out,'03_side.png');bpy.ops.render.render(write_still=True)

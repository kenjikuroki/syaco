import bpy, os
from mathutils import Vector
OUT=os.path.dirname(os.path.abspath(__file__))
bpy.ops.wm.open_mainfile(filepath=os.path.join(OUT,'Mantis_A6.blend'))
scene=bpy.context.scene; arm=bpy.data.objects['MantisRig_A6']; arm.hide_set(False)
for frame in range(1,43):
 scene.frame_set(frame); bpy.context.view_layer.update()
 for side in ['L','R']:
  for i in range(3):
   lo=arm.pose.bones['leg_lower_%s_%d'%(side,i)]; toe=arm.pose.bones['foot_%s_%d'%(side,i)]
   m=toe.matrix.copy(); m.translation=lo.tail; toe.matrix=m; toe.keyframe_insert('location',frame=frame)
scene.frame_set(1); bpy.ops.object.select_all(action='DESELECT'); arm.select_set(True)
for ob in bpy.data.objects:
 if ob.type=='MESH' and ob.get('customization_slot'): ob.select_set(True)
bpy.context.view_layer.objects.active=arm
bpy.ops.export_scene.fbx(filepath=os.path.join(OUT,'Mantis_A6.fbx'),use_selection=True,object_types={'MESH','ARMATURE'},add_leaf_bones=False,axis_forward='-Z',axis_up='Y',bake_anim=True,bake_anim_use_all_actions=False,bake_anim_use_nla_strips=False,bake_anim_simplify_factor=0,path_mode='COPY',embed_textures=True)
arm.hide_set(True); bpy.ops.wm.save_as_mainfile(filepath=os.path.join(OUT,'Mantis_A6.blend'))
scene.frame_set(18); scene.camera.location=(.75,-1,.65); scene.camera.rotation_euler=(Vector((0,.015,.13))-scene.camera.location).to_track_quat('-Z','Y').to_euler(); scene.render.filepath=os.path.join(OUT,'punch.png'); bpy.ops.render.render(write_still=True)
print('FOOT_CONTINUITY_FIXED')

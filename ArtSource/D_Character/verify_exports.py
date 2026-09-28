import bpy, os, json, math, bmesh
from mathutils import Vector
OUT=os.path.dirname(os.path.abspath(__file__))
report={}
for ext in ['fbx','glb']:
 bpy.ops.wm.read_factory_settings(use_empty=True)
 path=os.path.join(OUT,'D_Mantis_Base.'+ext)
 if ext=='fbx':bpy.ops.import_scene.fbx(filepath=path)
 else:bpy.ops.import_scene.gltf(filepath=path)
 rigs=[o for o in bpy.context.scene.objects if o.type=='ARMATURE'];helpers={b.custom_shape for r in rigs for b in r.pose.bones if b.custom_shape};meshes=[o for o in bpy.context.scene.objects if o.type=='MESH' and o not in helpers]
 assert len(meshes)==6,(ext,'six meshes required',len(meshes));assert len(rigs)==1
 assert len(rigs[0].data.bones)>=29
 unweighted=sum(not v.groups for o in meshes for v in o.data.vertices)
 assert unweighted==0,(ext,'unweighted',unweighted)
 assert all(o.data.uv_layers and o.modifiers for o in meshes)
 points=[o.matrix_world@Vector(v) for o in meshes for v in o.bound_box]
 dims=[max(p[i] for p in points)-min(p[i] for p in points) for i in range(3)]
 assert .5<max(dims)<2,(ext,'unexpected scale',dims)
 materials={m.name for o in meshes for m in o.data.materials};assert len(materials)==1
 assert any(i.packed_file or os.path.isfile(bpy.path.abspath(i.filepath)) for i in bpy.data.images if i.type=='IMAGE'),(ext,'texture missing')
 # Exercise a folded arm joint to ensure its skinned vertices actually deform.
 rig=rigs[0];rig.animation_data_clear();b=rig.pose.bones['propodus_R'];b.rotation_mode='XYZ';b.rotation_euler=(0,0,0)
 ob=next(o for o in meshes if o.name.startswith('03_PunchArm_R'));bpy.context.view_layer.update();dg=bpy.context.evaluated_depsgraph_get();before=[v.co.copy() for v in ob.evaluated_get(dg).data.vertices]
 b.rotation_euler.x=.3;bpy.context.view_layer.update();dg=bpy.context.evaluated_depsgraph_get();after=[v.co.copy() for v in ob.evaluated_get(dg).data.vertices]
 assert any((a-bb).length>1e-5 for a,bb in zip(before,after)),(ext,'arm skin does not deform')
 ob=next(o for o in meshes if o.name.startswith('04_Abdomen'));pb=rig.pose.bones['pleopod_R_02'];pb.rotation_mode='XYZ';pb.rotation_euler=(0,0,0);bpy.context.view_layer.update();dg=bpy.context.evaluated_depsgraph_get();before=[v.co.copy() for v in ob.evaluated_get(dg).data.vertices]
 pb.rotation_euler.x=.3;bpy.context.view_layer.update();dg=bpy.context.evaluated_depsgraph_get();after=[v.co.copy() for v in ob.evaluated_get(dg).data.vertices]
 assert any((a-bb).length>1e-5 for a,bb in zip(before,after)),(ext,'pleopod skin does not deform')
 report[ext]={'meshes':len(meshes),'bones':len(rig.data.bones),'materials':len(materials),'dimensions_m':dims,'unweighted_vertices':unweighted,'texture_resolved':True,'arm_deformation':True}
 report[ext]['pleopod_deformation']=True
json.dump(report,open(os.path.join(OUT,'export_verification.json'),'w'),indent=2)
# Organize the editable blend for handoff without modifying the character geometry.
bpy.ops.wm.open_mainfile(filepath=os.path.join(OUT,'D_Mantis_Base.blend'))
scene=bpy.context.scene;char=bpy.data.collections.new('CHARACTER - export these');studio=bpy.data.collections.new('STUDIO - preview only');scene.collection.children.link(char);scene.collection.children.link(studio)
for ob in list(scene.objects):
 target=char if ob.type=='ARMATURE' or ob.name[:3] in ['01_','02_','03_','04_','05_','06_'] else studio
 for c in list(ob.users_collection):c.objects.unlink(ob)
 target.objects.link(ob)
 ob.select_set(False)
rig=next(o for o in char.objects if o.type=='ARMATURE');rig.select_set(True);bpy.context.view_layer.objects.active=rig
for screen in bpy.data.screens:
 for area in screen.areas:
  if area.type=='VIEW_3D':area.spaces.active.region_3d.view_perspective='CAMERA'
bpy.ops.wm.save_as_mainfile(filepath=os.path.join(OUT,'D_Mantis_Base.blend'))
print('D_EXPORT_VALIDATION_PASS',report)



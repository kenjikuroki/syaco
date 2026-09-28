import bpy,os,json
OUT=os.path.dirname(os.path.abspath(__file__))
exec(compile(open(os.path.join(OUT,'outward_normals.py'),encoding='utf-8').read(),'outward_normals.py','exec'))
bpy.ops.wm.open_mainfile(filepath=os.path.join(OUT,'Mantis_A6.blend'))
arm=bpy.data.objects['MantisRig_A6'];arm.hide_set(False)
meshes=[ob for ob in bpy.data.objects if ob.type=='MESH' and ob.get('customization_slot')]
assert len(meshes)==6
report=[]
for ob in meshes:
    counts=(len(ob.data.vertices),len(ob.data.polygons),len(ob.vertex_groups))
    report.append(fix_outward_normals(ob.data))
    assert counts==(len(ob.data.vertices),len(ob.data.polygons),len(ob.vertex_groups))
bpy.context.scene.frame_set(1);bpy.ops.object.select_all(action='DESELECT');arm.select_set(True)
for ob in meshes:ob.select_set(True)
bpy.context.view_layer.objects.active=arm
bpy.ops.export_scene.fbx(filepath=os.path.join(OUT,'Mantis_A6.fbx'),use_selection=True,object_types={'MESH','ARMATURE'},add_leaf_bones=False,axis_forward='-Z',axis_up='Y',bake_anim=True,bake_anim_use_all_actions=False,bake_anim_use_nla_strips=False,bake_anim_simplify_factor=0,path_mode='COPY',embed_textures=True)
arm.hide_set(True);bpy.ops.wm.save_as_mainfile(filepath=os.path.join(OUT,'Mantis_A6.blend'))
json.dump(report,open(os.path.join(OUT,'normals_verification.json'),'w'),indent=2)
print(json.dumps(report,indent=2))

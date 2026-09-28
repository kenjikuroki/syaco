import bpy, os, math, json, bmesh
from mathutils import Vector
OUT=os.path.dirname(os.path.abspath(__file__))
import shutil
shutil.copyfile(os.path.join(OUT,'base_albedo.png'),os.path.join(OUT,'mantis_albedo.png'))
source=open(os.path.join(OUT,'source_base_v5.py'),encoding='utf-8-sig').read()
source=source[:source.index("bpy.ops.export_scene.fbx")]
source=source.replace("plate('Head_carapace',-1.02,.78,1.53,.49,1.13", "plate('Head_carapace',-1.02,.65,1.42,.40,1.13")
# Restore the abdomen/tail dimensions instead of the previous 18% compaction.
source=source.replace("return Vector((p.x*.82,-.29+(p.y+.29)*.82,1+(p.z-1)*.92))", "return p.copy()")
source=source.replace("(.24,.155,.165),5,eye", "(.265,.175,.215),5,eye")
source=source.replace("s*(1.06+i*.04)", "s*(.78+i*.025)").replace('s*1.42,y+.39,.69','s*1.02,y+.25,.59').replace('length=.85;', 'length=.60;')
source=source.replace("wrist+Vector((0,-.14,-.22)),wrist+Vector((0,.19,.12)),.235,.21,2", "wrist+Vector((0,-.30,-.36)),wrist+Vector((0,.30,.22)),.20,.145,1")
source=source.replace("wrist+Vector((0,.19,.12)),(.19,.15,.16)", "wrist+Vector((0,.30,.22)),(.16,.115,.12)")
source=source.replace("wrist+Vector((-s*.09,-.34,-.23))", "wrist+Vector((-s*.09,-.40,-.30))")
source=source.replace("MantisShrimp_v5", "Mantis_A6").replace('MantisSkeleton_v5','MantisSkeleton_A6').replace('MantisRig_v5','MantisRig_A6')
# Smasher appendage: distal striking body folds ventrally beneath the merus.
# Retain deform bone names and the six customization slots.
source=source.replace("elbow=Vector((s*.40,-1.89,.50)); wrist=Vector((s*.48,-1.40,.60))", "elbow=Vector((s*.40,-1.89,.76)); wrist=Vector((s*.43,-1.12,.39))")
source=source.replace("elbow+Vector((s*.09,0,0)),wrist,.13,.12,11", "elbow,wrist,.17,.12,11")
source=source.replace("wrist+Vector((0,-.30,-.36)),wrist+Vector((0,.30,.22)),.20,.145,1", "wrist+Vector((0,-.56,.08)),wrist+Vector((0,.055,-.035)),.155,.12,1")
source=source.replace("wrist+Vector((0,.30,.22)),(.16,.115,.12)", "wrist+Vector((0,.055,-.035)),(.165,.13,.13)")
source=source.replace("wrist+Vector((-s*.08,.05,.02)),wrist+Vector((-s*.09,-.40,-.30))", "wrist+Vector((-s*.08,.01,-.025)),wrist+Vector((-s*.09,-.62,.18))")
source=source.replace("(2.75-2*math.pi)*extension", "-2.50*extension")
source=source.replace("-.14*extension", "0*extension")
exec(compile(source,__file__,'exec'))
exec(compile(open(os.path.join(OUT,'outward_normals.py'),encoding='utf-8').read(),'outward_normals.py','exec'))
fix_outward_normals(model.data)
# Shared painted atlas, cooler slate shell and muted warm edge wear.
image.unpack(method='USE_LOCAL') if image.packed_file else None
pixels=list(image.pixels[:]); w,h=image.size
for y in range(h):
 for x in range(w):
  tile=(x//256)+(y//256)*4; k=(y*w+x)*4
  r,g,b=pixels[k:k+3]
  if tile in (0,1,8,9,11): pixels[k:k+3]=[r*.95,g*1.10,min(1,b*1.30)]
  elif tile==5: pixels[k:k+3]=[min(1,r*2.9),min(1,g*2.15),min(1,b*1.75)]
  elif tile in (7,10): pixels[k:k+3]=[min(1,r*1.50),g*1.04,b*.85]
image.pixels[:]=pixels; image.filepath_raw=os.path.join(OUT,'mantis_albedo.png'); image.file_format='PNG'; image.save(); image.pack()
# Split by deform bone ownership; shared armature and atlas, no overlapping duplicate faces.
def slot(bone):
 if bone.startswith(('merus_L','propodus_L','dactyl_L')): return '02_PunchArm_L'
 if bone.startswith(('merus_R','propodus_R','dactyl_R')): return '03_PunchArm_R'
 if bone.startswith(('leg_','foot_')): return '06_WalkingLegs'
 if bone=='tail': return '05_Tail'
 if bone.startswith('abdomen_'): return '04_Abdomen'
 return '01_Cephalothorax'
slots=['01_Cephalothorax','02_PunchArm_L','03_PunchArm_R','04_Abdomen','05_Tail','06_WalkingLegs']
owners={v.index:slot(model.vertex_groups[max(v.groups,key=lambda g:g.weight).group].name) for v in model.data.vertices}
meshes=[]
for name in slots:
 ob=model.copy(); ob.data=model.data.copy(); bpy.context.collection.objects.link(ob); ob.name=name
 bm=bmesh.new(); bm.from_mesh(ob.data); bm.verts.ensure_lookup_table()
 bmesh.ops.delete(bm,geom=[v for v in bm.verts if owners[v.index]!=name],context='VERTS'); bm.to_mesh(ob.data); bm.free(); ob.data.update()
 ob['customization_slot']=name; ob['rig_contract']='A6_v1'; meshes.append(ob)
bpy.data.objects.remove(model,do_unlink=True)
scene.frame_set(1); bpy.ops.object.select_all(action='DESELECT'); arm.select_set(True)
for ob in meshes: ob.select_set(True)
bpy.context.view_layer.objects.active=arm
bpy.ops.export_scene.fbx(filepath=os.path.join(OUT,'Mantis_A6.fbx'),use_selection=True,object_types={'MESH','ARMATURE'},add_leaf_bones=False,axis_forward='-Z',axis_up='Y',bake_anim=True,bake_anim_use_all_actions=False,bake_anim_use_nla_strips=False,bake_anim_simplify_factor=0,path_mode='COPY',embed_textures=True)
# Studio is preview only, excluded from FBX.
def aim(ob,p): ob.rotation_euler=(Vector(p)-ob.location).to_track_quat('-Z','Y').to_euler()
bpy.ops.mesh.primitive_plane_add(size=200,location=(0,0,-.005)); floor=bpy.context.object; floor.name='PreviewFloor'
fm=bpy.data.materials.new('Preview floor'); fm.diffuse_color=(.065,.085,.095,1); floor.data.materials.append(fm)
for name,loc,power,size in [('Key',(.4,-.8,1.4),45,.8),('Fill',(-.8,-.3,.6),22,.9),('Rim',(.2,1,.9),45,.6)]:
 data=bpy.data.lights.new(name,'AREA'); ob=bpy.data.objects.new(name,data); scene.collection.objects.link(ob); ob.location=loc; data.energy=power; data.size=size; aim(ob,(0,0,.12))
scene.world.use_nodes=True; scene.world.node_tree.nodes['Background'].inputs[0].default_value=(.18,.22,.26,1); scene.world.node_tree.nodes['Background'].inputs[1].default_value=.4
camdata=bpy.data.cameras.new('PreviewCamera'); cam=bpy.data.objects.new('PreviewCamera',camdata); scene.collection.objects.link(cam); scene.camera=cam; camdata.type='ORTHO'; camdata.ortho_scale=.68
scene.render.engine='CYCLES'; scene.cycles.samples=24; scene.cycles.use_denoising=True
scene.render.resolution_x=1200; scene.render.resolution_y=1000; scene.render.resolution_percentage=100
scene.view_settings.view_transform='AgX'
arm.hide_set(True)
for ob in meshes: ob.select_set(False)
meshes[0].select_set(True); bpy.context.view_layer.objects.active=meshes[0]
cam.location=(.65,-1,.62); aim(cam,(0,.015,.13)); scene.frame_set(1)
bpy.ops.wm.save_as_mainfile(filepath=os.path.join(OUT,'Mantis_A6.blend'))
for name,frame,loc in [('front',1,(.65,-1,.62)),('rear',1,(.65,.95,.65)),('punch',18,(.75,-1,.65))]:
 scene.frame_set(frame); cam.location=loc; aim(cam,(0,.015,.13)); scene.render.filepath=os.path.join(OUT,name+'.png'); bpy.ops.render.render(write_still=True)
stats={'slots':{ob.name:len(ob.data.polygons) for ob in meshes},'triangles':sum(len(ob.data.polygons) for ob in meshes),'bones':len(arm.data.bones),'animation_frames':42,'texture':'1024x1024','version':'A6_v1'}
assert len(meshes)==6 and all(len(ob.data.polygons)>0 for ob in meshes)
json.dump(stats,open(os.path.join(OUT,'stats.json'),'w'),indent=2)
print('A6_COMPLETE',stats)

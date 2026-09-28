import bpy, os, math, json, bmesh
from mathutils import Vector, Quaternion, Matrix
OUT=os.path.dirname(os.path.abspath(__file__))
bpy.ops.wm.open_mainfile(filepath=os.path.join(OUT,'D_Mantis_Base.blend'))
arm=bpy.data.objects['D_Mantis_Rig'];scene=bpy.context.scene
arm.animation_data_clear()
for pb in arm.pose.bones:pb.matrix_basis=Matrix.Identity(4)
bpy.context.view_layer.objects.active=arm
bpy.ops.object.mode_set(mode='EDIT')
for side,s in [('L',-1),('R',1)]:
 b=arm.data.edit_bones.new('strike_'+side);b.head=Vector((s*.30,-1.73,.37))*.2;b.tail=b.head+Vector((0,-.012,0));b.parent=arm.data.edit_bones['dactyl_'+side];b.use_deform=False
bpy.ops.object.mode_set(mode='OBJECT')
scene.frame_start=1;scene.frame_end=42;scene.render.fps=30
def rotate(name,angle):
 pb=arm.pose.bones[name];q=pb.bone.matrix_local.to_quaternion();pb.rotation_quaternion=q.inverted()@Quaternion((1,0,0),angle)@q
def interp(f,keys):
 for (a,x),(b,y) in zip(keys,keys[1:]):
  if f<=b:
   t=max(0,(f-a)/(b-a));t=t*t*(3-2*t);return x+(y-x)*t
 return keys[-1][1]
def feet():
 for side in ['L','R']:
  for i in range(3):
   up=arm.pose.bones['leg_upper_%s_%d'%(side,i)];lo=arm.pose.bones['leg_lower_%s_%d'%(side,i)]
   h=up.head.copy();target=lo.bone.tail_local.copy();pole=up.bone.tail_local.copy()
   axis=target-h;distance=axis.length;axis.normalize();a=up.bone.length;b=lo.bone.length
   distance=min(distance,a+b-.00001);target=h+axis*distance
   along=(a*a-b*b+distance*distance)/(2*distance)
   bend=pole-h-axis*along;bend-=axis*bend.dot(axis);bend.normalize()
   knee=h+axis*along+bend*math.sqrt(max(0,a*a-along*along))
   for pb,start,end in [(up,h,knee),(lo,knee,target)]:
    q=(pb.bone.tail_local-pb.bone.head_local).rotation_difference(end-start)@pb.bone.matrix_local.to_quaternion()
    pb.matrix=Matrix.Translation(start)@q.to_matrix().to_4x4();bpy.context.view_layer.update()
stats=[]
for frame in range(1,43):
 scene.frame_set(frame)
 for pb in arm.pose.bones:
  pb.rotation_mode='QUATERNION';pb.rotation_quaternion=(1,0,0,0);pb.location=(0,0,0);pb.scale=(1,1,1)
 lift=interp(frame,[(1,0),(5,.05),(13,1),(16,1),(18,.83),(22,.72),(34,0),(42,0)])
 extension=interp(frame,[(1,0),(16,0),(18,1),(20,.94),(25,.18),(30,0),(42,0)])
 for name,angle in [('abdomen_02',-.055),('abdomen_01',-.07),('abdomen_00',-.09),('thorax',-.23)]:rotate(name,angle*lift)
 for side in ['L','R']:
  rotate('merus_'+side,-.04*lift+.10*extension)
  rotate('propodus_'+side,-2.85*extension-.04*lift)
  rotate('dactyl_'+side,1.40*extension)
  rotate('eye_'+side,.20*lift)
 bpy.context.view_layer.update();feet();bpy.context.view_layer.update()
 for pb in arm.pose.bones:
  for prop in ['location','rotation_quaternion','scale']:pb.keyframe_insert(prop,frame=frame,group=pb.name)
 stats.append({'frame':frame,'strike':list(arm.pose.bones['strike_L'].head),'eye':list(arm.pose.bones['eye_L'].head)})
arm.animation_data.action.name='D_Underhand_RearUp_Punch'
scene.frame_set(1)
modules=[bpy.data.objects[n] for n in ['01_Cephalothorax','02_PunchArm_L','03_PunchArm_R','04_Abdomen','05_Tail','06_WalkingLegs']]
bpy.ops.object.select_all(action='DESELECT');arm.select_set(True)
for ob in modules:ob.select_set(True)
bpy.context.view_layer.objects.active=arm
bpy.ops.export_scene.fbx(filepath=os.path.join(OUT,'D_Mantis_Game.fbx'),use_selection=True,object_types={'MESH','ARMATURE'},add_leaf_bones=False,axis_forward='-Z',axis_up='Y',bake_anim=True,bake_anim_use_all_actions=False,bake_anim_use_nla_strips=False,bake_anim_simplify_factor=0,path_mode='COPY',embed_textures=True)
bpy.ops.wm.save_as_mainfile(filepath=os.path.join(OUT,'D_Mantis_Game.blend'))
json.dump(stats,open(os.path.join(OUT,'game_pose_checks.json'),'w'),indent=2)
# Rebuild the existing crown donor on this exact rest skeleton and new head mesh.
head=modules[0];verts=[];faces=[];n=12
for rad,zbase,tip in [(.064,.286,False),(.073,.328,True),(.063,.328,True),(.056,.286,False)]:
 for i in range(n):
  a=math.tau*i/n;verts.append((rad*math.cos(a),-.205+rad*math.sin(a),zbase+(.044 if tip and i%2==0 else 0)))
for ring in range(4):
 for i in range(n):
  j=(i+1)%n;nr=(ring+1)%4;faces.append((ring*n+i,ring*n+j,nr*n+j,nr*n+i))
me=bpy.data.meshes.new('D_Crown');me.from_pydata(verts,[],faces);me.update();bm=bmesh.new();bm.from_mesh(me);bmesh.ops.recalc_face_normals(bm,faces=bm.faces);bm.to_mesh(me);bm.free()
crown=bpy.data.objects.new('Crown',me);scene.collection.objects.link(crown);gold=bpy.data.materials.new('CrownGold');gold.diffuse_color=(.85,.49,.065,1);crown.data.materials.append(gold);crown.vertex_groups.new(name='thorax').add(list(range(len(verts))),1,'REPLACE')
bpy.ops.object.select_all(action='DESELECT');head.select_set(True);crown.select_set(True);bpy.context.view_layer.objects.active=head;bpy.ops.object.join();head.name='Crown_Cephalothorax'
bpy.ops.object.select_all(action='DESELECT');head.select_set(True);arm.select_set(True);bpy.context.view_layer.objects.active=arm
bpy.ops.export_scene.fbx(filepath=os.path.join(OUT,'D_CrownHead.fbx'),use_selection=True,object_types={'MESH','ARMATURE'},add_leaf_bones=False,axis_forward='-Z',axis_up='Y',bake_anim=False,path_mode='AUTO')
print('D_GAME_EXPORTED')

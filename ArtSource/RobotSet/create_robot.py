import bpy, math, os, json, bmesh
from mathutils import Vector, Matrix
OUT=os.path.dirname(os.path.abspath(__file__))
bpy.ops.wm.open_mainfile(filepath=os.path.join(OUT,'../D_Character/D_Mantis_Game.blend'))
scene=bpy.context.scene; arm=bpy.data.objects['D_Mantis_Rig'];scene.frame_set(1)
names=['01_Cephalothorax','02_PunchArm_L','03_PunchArm_R','04_Abdomen','05_Tail','06_WalkingLegs']
mods=[bpy.data.objects[n] for n in names]
materials=[]
for name,col,metal,rough,glow in [
 ('Robot_Ivory',(.82,.78,.65),.42,.29,0),('Robot_Petrol',(.045,.12,.135),.65,.28,0),
 ('Robot_Graphite',(.018,.026,.035),.62,.38,0),('Robot_Red',(.58,.035,.026),.4,.28,0),
 ('Robot_Brass',(.52,.32,.10),.78,.27,0),('Robot_Cyan',(.015,.58,.85),.3,.23,2.4)]:
 m=bpy.data.materials.new(name);m.diffuse_color=(*col,1);m.use_nodes=True;p=m.node_tree.nodes.get('Principled BSDF');p.inputs['Base Color'].default_value=(*col,1);p.inputs['Metallic'].default_value=metal;p.inputs['Roughness'].default_value=rough
 if glow:p.inputs['Emission Color'].default_value=(*col,1);p.inputs['Emission Strength'].default_value=glow
 materials.append(m)
# Reuse the exact articulated silhouette and weights. Armor receives a dedicated six-color palette.
for ob in mods:
 uv=ob.data.uv_layers.active
 tiles=[]
 for f in ob.data.polygons:
  coords=[uv.data[i].uv for i in f.loop_indices];u=sum(v.x for v in coords)/len(coords);v=sum(v.y for v in coords)/len(coords);tiles.append(min(3,int(u*4))+min(3,int(v*4))*4)
 ob.data.materials.clear()
 for m in materials:ob.data.materials.append(m)
 for f,t in zip(ob.data.polygons,tiles):
  f.material_index={0:1,1:1,2:0,3:2,4:0,5:1,6:1,7:3,8:0,9:3,10:4,11:2,12:2,13:2,14:1,15:0}[t]
  f.use_smooth=True
parts={n:[] for n in names}
def slot(bn):
 return 1 if bn.endswith('_L') and bn.split('_')[0] in ['merus','propodus','dactyl'] else 2 if bn.endswith('_R') and bn.split('_')[0] in ['merus','propodus','dactyl'] else 3 if bn.startswith(('abdomen','pleopod')) else 4 if bn=='tail' else 5 if bn.startswith('leg_') else 0
def finish(ob,bn,mat):
 bpy.context.view_layer.objects.active=ob;bpy.ops.object.transform_apply(location=True,rotation=True,scale=True)
 ob.data.materials.append(materials[mat]);ob.vertex_groups.new(name=bn).add(list(range(len(ob.data.vertices))),1,'REPLACE')
 parts[names[slot(bn)]].append(ob)
 return ob
def mesh(name,vs,faces,bn,mat,bevel=.008):
 me=bpy.data.meshes.new(name);me.from_pydata([Vector(v)*.2 for v in vs],[],faces);me.update();ob=bpy.data.objects.new(name,me);scene.collection.objects.link(ob)
 bm=bmesh.new();bm.from_mesh(me);bmesh.ops.recalc_face_normals(bm,faces=list(bm.faces));bm.to_mesh(me);bm.free()
 finish(ob,bn,mat)
 if bevel:
  m=ob.modifiers.new('Machined edges','BEVEL');m.width=bevel*.2;m.segments=1;bpy.context.view_layer.objects.active=ob;bpy.ops.object.modifier_apply(modifier=m.name)
 return ob
def panel(name,outline,bn,mat,depth=.026):
 n=len(outline);vs=outline+[(x,y,z-depth) for x,y,z in outline];fs=[tuple(range(n)),tuple(reversed(range(n,2*n)))]+[(i,(i+1)%n,(i+1)%n+n,i+n) for i in range(n)]
 return mesh(name,vs,fs,bn,mat)
def cyl(name,center,r,depth,bn,mat,axis=(1,0,0),count=12):
 bpy.ops.mesh.primitive_cylinder_add(vertices=count,radius=r*.2,depth=depth*.2,location=Vector(center)*.2);ob=bpy.context.object;ob.name=name;ob.rotation_euler=Vector(axis).to_track_quat('Z','Y').to_euler();return finish(ob,bn,mat)
def ring(name,c,r,bn,mat,axis=(1,0,0)):
 bpy.ops.mesh.primitive_torus_add(major_segments=16,minor_segments=4,location=Vector(c)*.2,major_radius=r*.2,minor_radius=.012*.2);ob=bpy.context.object;ob.name=name;ob.rotation_euler=Vector(axis).to_track_quat('Z','Y').to_euler();return finish(ob,bn,mat)
def beam(name,a,b,r,bn,mat):
 a,b=Vector(a),Vector(b);return cyl(name,(a+b)/2,r,(b-a).length,bn,mat,b-a,8)
# Curved, chamfered white saddles over green dorsal armor, separated by black hinge gaps.
for i,(y,z,w) in enumerate([(-.34,1.02,.51),(.08,.94,.50),(.50,.84,.48),(.91,.74,.45),(1.31,.63,.41),(1.69,.51,.36)]):
 bn='abdomen_%02d'%i;h=.26-i*.013
 for s in [-1,1]:
  outline=[(s*w*.96,y-.24,z+h*.10),(s*w*.84,y-.22,z+h*.73),(s*w*.36,y-.19,z+h*1.09),(s*w*.26,y+.17,z+h*1.03),(s*w*.78,y+.25,z+h*.66),(s*w*1.04,y+.21,z+h*.05)]
  panel('Ivory abdominal saddle',outline,bn,0)
  cyl('Brass lateral hinge',(s*(w+.013),y+.03,z-.02),.084,.065,bn,4)
  cyl('Joint recess',(s*(w+.050),y+.03,z-.02),.059,.012,bn,2)
  cyl('Blue core',(s*(w+.059),y+.03,z-.02),.027,.014,bn,5)
  # Small red identification marks.
  panel('Red warning tab',[(s*w*.79,y-.13,z+h*.80),(s*w*.86,y-.13,z+h*.64),(s*w*.86,y-.075,z+h*.64),(s*w*.79,y-.075,z+h*.80)],bn,3,.01)
# Head cheek armor, dorsal white brow and cyan V visor.
for s,label in [(-1,'L'),(1,'R')]:
 panel('Helmet cheek',[(s*.08,-1.56,1.43),(s*.28,-1.43,1.52),(s*.48,-1.05,1.48),(s*.56,-.60,1.30),(s*.42,-.63,1.10),(s*.22,-1.35,1.18)],'thorax',0)
 panel('Ivory face visor',[(0,-1.81,1.49),(s*.21,-1.73,1.50),(s*.26,-1.76,1.28),(s*.11,-1.87,1.05),(0,-1.87,1.00)],'thorax',0,.025)
 beam('Visor light',(s*.018,-1.891,1.16),(s*.17,-1.82,1.32),.019,'thorax',5)
 beam('Red collar',(s*.24,-.76,1.50),(s*.45,-.70,1.44),.040,'thorax',3)
 for k in range(3):beam('Helmet vent',(s*.42,-1.13+k*.07,1.49),(s*.49,-1.09+k*.07,1.43),.009,'thorax',2)
 # Optical lens sits on the front of the existing articulated eye barrel.
 c=(s*.39,-1.925,1.84)
 cyl('Optic housing',c,.124,.050,'eye_'+label,4,(0,-1,0),16)
 cyl('Optic glass',(c[0],c[1]-.028,c[2]),.10,.017,'eye_'+label,2,(0,-1,0),16)
 ring('Optic luminous ring',(c[0],c[1]-.042,c[2]),.078,'eye_'+label,5,(0,-1,0))
 beam('Lens glint',(c[0]-.047,c[1]-.045,c[2]+.018),(c[0]+.035,c[1]-.045,c[2]+.053),.008,'eye_'+label,5)
 # Raptorial joints and shaped dorsal gauntlet plates preserve the underhand fold.
 cyl('Shoulder pivot',(s*.47,-1.05,1.04),.115,.055,'merus_'+label,4)
 cyl('Shoulder cap',(s*.505,-1.05,1.04),.071,.022,'merus_'+label,2)
 wrist=Vector((s*.31,-1.21,.77));tip=Vector((s*.30,-1.82,.28));bn='dactyl_'+label
 # Faceted gauntlet around the long closed striking club, no pincers.
 centers=[wrist,wrist.lerp(tip,.23),wrist.lerp(tip,.58)+Vector((0,-.045,0)),wrist.lerp(tip,.85),tip]
 widths=[.075,.192,.205,.143,.035];depths=[.073,.145,.173,.127,.035];vs=[]
 axis=(tip-wrist).normalized();a=Vector((1,0,0));b=axis.cross(a).normalized()
 for c,w,d in zip(centers,widths,depths):
  for j in range(8):vs.append(c+a*math.cos(j*math.tau/8)*w+b*math.sin(j*math.tau/8)*d)
 fs=[]
 for k in range(4):
  for j in range(8):fs.append((k*8+j,k*8+(j+1)%8,(k+1)*8+(j+1)%8,(k+1)*8+j))
 ob=mesh('Closed armored punch club',vs,fs,bn,0)
 ob.data.materials.append(materials[3])
 # Red terminal striking surface follows the dactyl as one closed club.
 for f in ob.data.polygons:
  if f.center.z<.2*.43:f.material_index=1
 c=wrist.lerp(tip,.36);c.x+=s*.19
 cyl('Gauntlet joint',c,.088,.039,bn,2);ring('Gauntlet reactor',c+Vector((s*.023,0,0)),.064,bn,5)
 for i in range(3):
  up='leg_upper_%s_%d'%(label,i);lo='leg_lower_%s_%d'%(label,i)
  for bone_name in [up,lo]:
   bo=arm.data.bones[bone_name];a=bo.head_local/.2;b=bo.tail_local/.2
   beam('Piston steel',a.lerp(b,.2),a.lerp(b,.75),.037,bone_name,4)
   cyl('Leg hinge',a,.069,.090,bone_name,2)
   cyl('Leg rivet',a+Vector((s*.05,0,0)),.032,.015,bone_name,4)
# Angular tail fins with inset cyan strips, mounted on the existing tail bone.
for s in [-1,1]:
 for i in range(2):
  a=Vector((s*(.13+i*.12),1.97,.42));b=Vector((s*(.42+i*.4),2.89-i*.23,.19));d=b-a;side=Vector((d.y,-d.x,0)).normalized()
  outline=[a-side*.045,a+d*.23-side*.15,a+d*.75-side*.13,b,a+d*.75+side*.14,a+d*.23+side*.15]
  panel('Tail thruster fin',outline,'tail',1)
  beam('Tail luminous blade',a+d*.20+Vector((0,0,.015)),a+d*.77+Vector((0,0,.015)),.018,'tail',5)
  panel('Red fin tip',[a+d*.74-side*.13,b,a+d*.74+side*.14],'tail',3,.030)
# Merge each slot with its additions; preserve exact rig transforms and bind matrices.
for ob in mods:
 bpy.ops.object.select_all(action='DESELECT');ob.select_set(True)
 for add in parts[ob.name]:add.select_set(True)
 bpy.context.view_layer.objects.active=ob;bpy.ops.object.join()
 ob.data.calc_loop_triangles()
 # UVs for newly constructed armor, using a non-overlapping packed layout.
 bpy.ops.object.mode_set(mode='EDIT');bpy.ops.mesh.select_all(action='SELECT');bpy.ops.uv.smart_project(island_margin=.015);bpy.ops.object.mode_set(mode='OBJECT')
 bpy.ops.object.transform_apply(location=False,rotation=False,scale=False)
# Export only character and six replacement slots.
bpy.ops.object.select_all(action='DESELECT');arm.select_set(True)
for ob in mods:ob.select_set(True)
bpy.context.view_layer.objects.active=arm
bpy.ops.export_scene.fbx(filepath=os.path.join(OUT,'Robot_Set.fbx'),use_selection=True,object_types={'MESH','ARMATURE'},add_leaf_bones=False,axis_forward='-Z',axis_up='Y',bake_anim=False)
bpy.ops.export_scene.gltf(filepath=os.path.join(OUT,'Robot_Set.glb'),use_selection=True,export_format='GLB',export_animations=False)
scene.render.engine='CYCLES';scene.cycles.samples=24;scene.render.resolution_x=1280;scene.render.resolution_y=960;scene.render.resolution_percentage=100
cam=scene.camera;cam.data.ortho_scale=1.25
cam.location=(1.15,-1.60,.93);cam.rotation_euler=(Vector((0,.025,.21))-cam.location).to_track_quat('-Z','Y').to_euler()
bpy.ops.wm.save_as_mainfile(filepath=os.path.join(OUT,'Robot_Set.blend'))
for name,loc,target,scale in [('preview',(1.15,-1.60,.93),(0,.025,.21),1.25),('side',(1.8,.05,.70),(0,.05,.21),1.20),('front',(0,-2,.50),(0,-.10,.23),.78)]:
 cam.location=loc;cam.rotation_euler=(Vector(target)-cam.location).to_track_quat('-Z','Y').to_euler();cam.data.ortho_scale=scale;scene.render.filepath=os.path.join(OUT,name+'.png');bpy.ops.render.render(write_still=True)
json.dump({'triangles':sum(len(ob.data.loop_triangles) for ob in mods),'slots':names,'unweighted':sum(sum(not v.groups for v in ob.data.vertices) for ob in mods)},open(os.path.join(OUT,'stats.json'),'w'),indent=2)
print('ROBOT_COMPLETE')

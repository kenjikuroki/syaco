import bpy, math, os, json, bmesh
from mathutils import Vector
OUT=os.path.dirname(os.path.abspath(__file__))
bpy.ops.wm.open_mainfile(filepath=os.path.join(OUT,'../D_Character/D_Mantis_Game.blend'))
scene=bpy.context.scene;arm=bpy.data.objects['D_Mantis_Rig'];scene.frame_set(1)
names=['01_Cephalothorax','02_PunchArm_L','03_PunchArm_R','04_Abdomen','05_Tail','06_WalkingLegs'];mods=[bpy.data.objects[n] for n in names];parts={n:[] for n in names}
colors=[(.83,.78,.65),(.08,.31,.13),(.018,.05,.10),(.62,.014,.025),(.63,.52,.26),(.015,.22,.24)]
materials=[]
for name,col in zip(['Cream','Green','Navy','Red','Gold','Teal'],colors):
 m=bpy.data.materials.new('Boxer_'+name);m.diffuse_color=(*col,1);m.use_nodes=True;p=m.node_tree.nodes.get('Principled BSDF');p.inputs['Base Color'].default_value=(*col,1);p.inputs['Roughness'].default_value=.32 if name=='Red' else .48;materials.append(m)
for ob in mods:
 uv=ob.data.uv_layers.active;tiles=[]
 for f in ob.data.polygons:
  coords=[uv.data[i].uv for i in f.loop_indices];u=sum(v.x for v in coords)/len(coords);v=sum(v.y for v in coords)/len(coords);tiles.append(min(3,int(u*4))+min(3,int(v*4))*4)
 ob.data.materials.clear()
 for m in materials:ob.data.materials.append(m)
 for f,t in zip(ob.data.polygons,tiles):
  f.material_index={0:1,1:1,2:0,3:1,4:1,5:1,6:2,7:3,8:1,9:3,10:0,11:5,12:2,13:1,14:2,15:0}[t]
  if t==5:f.material_index=3 if f.center.z<.368 else 1
  f.use_smooth=True
# Replace the old eye surfaces to get a clean horizontal color band.
head=mods[0];uv=head.data.uv_layers.active;eye_faces=[]
for f in head.data.polygons:
 c=[uv.data[i].uv for i in f.loop_indices];u=sum(v.x for v in c)/len(c);v=sum(v.y for v in c)/len(c)
 if min(3,int(u*4))+min(3,int(v*4))*4==5:eye_faces.append(f.index)
bm=bmesh.new();bm.from_mesh(head.data);bm.faces.ensure_lookup_table();bmesh.ops.delete(bm,geom=[bm.faces[i] for i in eye_faces],context='FACES');bm.to_mesh(head.data);bm.free()
# Helpers create fully weighted parts on the established punch/leg skeleton.
def slot(bn):
 return 1 if bn in ['merus_L','propodus_L','dactyl_L'] else 2 if bn in ['merus_R','propodus_R','dactyl_R'] else 3 if bn.startswith(('abdomen','pleopod')) else 4 if bn=='tail' else 5 if bn.startswith('leg_') else 0
def finish(ob,bn,mat):
 bpy.context.view_layer.objects.active=ob;bpy.ops.object.transform_apply(location=True,rotation=True,scale=True);ob.data.materials.append(materials[mat]);ob.vertex_groups.new(name=bn).add(list(range(len(ob.data.vertices))),1,'REPLACE');parts[names[slot(bn)]].append(ob)
 for f in ob.data.polygons:f.use_smooth=True
 return ob
def mesh(name,vs,fs,bn,mat):
 me=bpy.data.meshes.new(name);me.from_pydata([Vector(v)*.2 for v in vs],[],fs);me.update();bm=bmesh.new();bm.from_mesh(me);bmesh.ops.recalc_face_normals(bm,faces=list(bm.faces));bm.to_mesh(me);bm.free();ob=bpy.data.objects.new(name,me);scene.collection.objects.link(ob);return finish(ob,bn,mat)
def ell(name,c,scale,bn,mat):
 bpy.ops.mesh.primitive_uv_sphere_add(segments=16,ring_count=10,radius=1,location=Vector(c)*.2);ob=bpy.context.object;ob.name=name;ob.scale=Vector(scale)*.2;return finish(ob,bn,mat)
def cylinder(name,c,r,depth,bn,mat,axis):
 bpy.ops.mesh.primitive_cylinder_add(vertices=20,radius=r*.2,depth=depth*.2,location=Vector(c)*.2);ob=bpy.context.object;ob.name=name;ob.rotation_euler=Vector(axis).to_track_quat('Z','Y').to_euler();return finish(ob,bn,mat)
def tube(name,points,r,bn,mat):
 vs=[];fs=[];points=[Vector(p) for p in points]
 for i,p in enumerate(points):
  d=(points[min(i+1,len(points)-1)]-points[max(0,i-1)]).normalized();a=d.cross(Vector((0,0,1)))
  if a.length<.001:a=Vector((1,0,0))
  a.normalize();b=d.cross(a).normalized()
  for j in range(8):vs.append(p+r*(a*math.cos(j*math.tau/8)+b*math.sin(j*math.tau/8)))
 for i in range(len(points)-1):
  for j in range(8):fs.append((i*8+j,i*8+(j+1)%8,(i+1)*8+(j+1)%8,(i+1)*8+j))
 return mesh(name,vs,fs,bn,mat)
def ribbon(name,points,widths,bn,mat):
 vs=[]
 for p,w in zip(points,widths):vs.extend([Vector(p)+Vector((0,0,w/2)),Vector(p)-Vector((0,0,w/2))])
 ob=mesh(name,vs,[(i*2,i*2+1,i*2+3,i*2+2) for i in range(len(points)-1)],bn,mat);m=ob.modifiers.new('Cloth thickness','SOLIDIFY');m.thickness=.008*.2;bpy.context.view_layer.objects.active=ob;bpy.ops.object.modifier_apply(modifier=m.name);return ob
for s,label in [(-1,'L'),(1,'R')]:
 ob=ell('Boxer compound eye',(s*.39,-1.80,1.84),(.16,.125,.18),'eye_'+label,1);ob.data.materials.append(materials[3]);ob.data.update()
 for f in ob.data.polygons:f.material_index=1 if f.center.z<1.84*.2 else 0
 tube('Eye cream band',[(s*.39+.16*math.cos(j*math.tau/32),-1.80+.126*math.sin(j*math.tau/32),1.84) for j in range(33)],.012,'eye_'+label,0)
# Red headband wraps the brow below the eye stalks, with cream piping and trailing ties.
for band,z0,z1,mat in [('Headband',1.23,1.45,3),('Upper piping',1.435,1.472,0),('Lower piping',1.205,1.245,0)]:
 vs=[];fs=[];n=40
 for z in [z0,z1]:
  for j in range(n):
   a=j*math.tau/n;vs.append((.345*math.cos(a),-1.49+.335*math.sin(a),z+.045*math.sin(a)))
 for j in range(n):fs.append((j,(j+1)%n,(j+1)%n+n,j+n))
 ob=mesh(band,vs,fs,'thorax',mat);m=ob.modifiers.new('Band thickness','SOLIDIFY');m.thickness=.018*.2;bpy.context.view_layer.objects.active=ob;bpy.ops.object.modifier_apply(modifier=m.name)
for s in [-1,1]:
 ribbon('Headband tail',[(s*.20,-1.19,1.36),(s*.43,-.91,1.43),(s*.71,-.65,1.36),(s*.96,-.48,1.27)],[.13,.14,.10,.01],'thorax',3)
ell('Headband knot',(0,-1.17,1.36),(.11,.08,.075),'thorax',3)
# Oversized closed gloves with side thumbs, seam and contrasting wrist cuffs.
for s,label in [(-1,'L'),(1,'R')]:
 bn='dactyl_'+label;wrist=Vector((s*.31,-1.21,.77));tip=Vector((s*.30,-1.82,.28));axis=(tip-wrist).normalized();c=wrist+axis*.105
 cylinder('Cream cuff',c,.162,.19,bn,0,axis)
 cylinder('Navy wrist wrap',c,.169,.125,bn,2,axis)
 ell('Padded boxing glove',(s*.34,-1.67,.43),(.275,.255,.31),bn,3)
 ell('Tucked glove thumb',(s*.12,-1.65,.53),(.09,.15,.15),bn,3)
 tube('Glove palm seam',[(s*.15,-1.846,.56),(s*.105,-1.85,.48),(s*.15,-1.87,.39)],.009,bn,2)
 # Tiny cream stitched detail on the cuff.
 for j in range(3):tube('Cuff stitch',[(s*.47,-1.32-j*.028,.72),(s*.46,-1.32-j*.028,.75)],.004,bn,0)
# Segment-weighted trunks allow the abdomen to bend, with a ribbed waistband.
for i,(y,z,w) in enumerate([(.50,.84,.48),(.91,.74,.45),(1.31,.63,.41)]):
 bn='abdomen_%02d'%(i+2);n=32;vs=[];fs=[];length=.46
 for dy in [-length/2,length/2]:
  for j in range(n):
   a=j*math.tau/n;vs.append(((w+.035)*math.cos(a),y+dy,z+(.27-i*.014)*math.sin(a)))
 for j in range(n):fs.append((j,(j+1)%n,(j+1)%n+n,j+n))
 ob=mesh('Boxer trunks panel',vs,fs,bn,3);ob.data.materials.append(materials[0]);ob.data.materials.append(materials[2])
 # Double side stripes, each panel follows its own abdominal joint.
 for j,f in enumerate(ob.data.polygons):
  if j%16 in [0,2,13,15]:f.material_index=1
  elif j%16 in [1,14]:f.material_index=2
 if i==0:
  for dy,width,mat in [(-.22,.13,0),(.22,.035,0)]:
   vs2=[]
   for yy in [y+dy-width/2,y+dy+width/2]:
    for j in range(n):a=j*math.tau/n;vs2.append(((w+.047)*math.cos(a),yy,z+.285*math.sin(a)))
   mesh('Elastic waistband',vs2,fs,bn,mat)
  for j in range(24):
   a=j*math.tau/24;tube('Waistband rib',[( (w+.05)*math.cos(a),y-.278,z+.288*math.sin(a)),((w+.05)*math.cos(a),y-.164,z+.288*math.sin(a))],.003,bn,4)
# Navy tail feathers with cream borders and red tips.
for s in [-1,1]:
 for i in range(2):
  a=Vector((s*(.13+i*.12),1.97,.40));b=Vector((s*(.42+i*.40),2.89-i*.23,.18));d=b-a
  # Three nested curved surfaces, all rigidly weighted to tail.
  for inset,mat,raiseZ in [(1.0,3,0),(.88,0,.012),(.72,2,.023)]:
   vs=[];fs=[]
   for k in range(9):
    t=k/8;ww=math.sin(math.pi*t)*.245*inset;center=a+d*t*inset+Vector((0,0,raiseZ));vs.extend([center+Vector((-ww,0,0)),center+Vector((0,0,.035*math.sin(math.pi*t))),center+Vector((ww,0,0))])
   for k in range(8):
    for j in range(2):fs.append((k*3+j,k*3+j+1,(k+1)*3+j+1,(k+1)*3+j))
   ob=mesh('Boxer tail trim',vs,fs,'tail',mat);sol=ob.modifiers.new('Tail fabric thickness','SOLIDIFY');sol.thickness=.006*.2;bpy.context.view_layer.objects.active=ob;bpy.ops.object.modifier_apply(modifier=sol.name)
# Six sneakers follow lower leg bones, with soles, toe caps and straps.
for s,label in [(-1,'L'),(1,'R')]:
 for i in range(3):
  bn='leg_lower_%s_%d'%(label,i);foot=arm.data.bones[bn].tail_local/.2;c=foot+Vector((0,-.055,.025))
  ell('Navy sneaker sole',c+Vector((0,-.025,-.016)),(.105,.18,.04),bn,2)
  ell('Cream midsole',c+Vector((0,-.025,.018)),(.103,.176,.036),bn,0)
  ell('Red sneaker upper',c+Vector((0,-.004,.074)),(.096,.145,.085),bn,3)
  ell('Cream toe cap',c+Vector((0,-.108,.062)),(.098,.076,.058),bn,0)
  cylinder('Navy ankle cuff',foot+Vector((0,.017,.14)),.08,.105,bn,2,(0,0,1))
  for j in range(2):tube('Shoe lace',[c+Vector((-.055,-.056+j*.037,.14)),c+Vector((.055,-.056+j*.037,.14))],.009,bn,0)
# Export six interchangeable meshes sharing the existing skeleton and action.
for ob in mods:
 bpy.ops.object.select_all(action='DESELECT');ob.select_set(True)
 for add in parts[ob.name]:add.select_set(True)
 bpy.context.view_layer.objects.active=ob;bpy.ops.object.join();bpy.ops.object.mode_set(mode='EDIT');bpy.ops.mesh.select_all(action='SELECT');bpy.ops.uv.smart_project(island_margin=.015);bpy.ops.object.mode_set(mode='OBJECT');ob.data.calc_loop_triangles()
bpy.ops.object.select_all(action='DESELECT');arm.select_set(True)
for ob in mods:ob.select_set(True)
bpy.context.view_layer.objects.active=arm
bpy.ops.export_scene.fbx(filepath=os.path.join(OUT,'Boxer_Set.fbx'),use_selection=True,object_types={'MESH','ARMATURE'},add_leaf_bones=False,axis_forward='-Z',axis_up='Y',bake_anim=False)
bpy.ops.export_scene.gltf(filepath=os.path.join(OUT,'Boxer_Set.glb'),use_selection=True,export_format='GLB',export_animations=False)
scene.view_settings.exposure=-.45;scene.render.engine='CYCLES';scene.cycles.samples=24;scene.render.resolution_x=1280;scene.render.resolution_y=960;scene.render.resolution_percentage=100
cam=scene.camera
for name,loc,target,scale in [('preview',(1.15,-1.6,.93),(0,.025,.21),1.25),('front',(0,-2,.5),(0,-.1,.23),.83),('side',(1.8,.05,.70),(0,.05,.21),1.20)]:
 cam.location=loc;cam.rotation_euler=(Vector(target)-cam.location).to_track_quat('-Z','Y').to_euler();cam.data.ortho_scale=scale;scene.render.filepath=os.path.join(OUT,name+'.png')
 if name=='preview':bpy.ops.wm.save_as_mainfile(filepath=os.path.join(OUT,'Boxer_Set.blend'))
 bpy.ops.render.render(write_still=True)
json.dump({'triangles':sum(len(o.data.loop_triangles) for o in mods),'unweighted':sum(sum(not v.groups for v in o.data.vertices) for o in mods),'slots':names},open(os.path.join(OUT,'stats.json'),'w'),indent=2)

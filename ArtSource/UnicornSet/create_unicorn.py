import bpy,math,os,json,bmesh
from mathutils import Vector
OUT=os.path.dirname(os.path.abspath(__file__))
bpy.ops.wm.open_mainfile(filepath=os.path.join(OUT,'../D_Character/D_Mantis_Game.blend'))
scene=bpy.context.scene;scene.frame_set(1);arm=bpy.data.objects['D_Mantis_Rig']
names=['01_Cephalothorax','02_PunchArm_L','03_PunchArm_R','04_Abdomen','05_Tail','06_WalkingLegs'];mods=[bpy.data.objects[n] for n in names];parts={n:[] for n in names}
materials=[]
for name,c,metal in [('Pearl',(.88,.82,.76),.12),('Gold',(.72,.40,.12),.65),('Pink',(.80,.24,.49),.08),('Lilac',(.43,.28,.74),.08),('Sky',(.24,.59,.83),.08)]:
 m=bpy.data.materials.new('Unicorn_'+name);m.diffuse_color=(*c,1);m.use_nodes=True;p=m.node_tree.nodes.get('Principled BSDF');p.inputs['Base Color'].default_value=(*c,1);p.inputs['Metallic'].default_value=metal;p.inputs['Roughness'].default_value=.35;materials.append(m)
for ob in mods:
 uv=ob.data.uv_layers.active;tiles=[]
 for f in ob.data.polygons:
  c=[uv.data[i].uv for i in f.loop_indices];tiles.append(int(sum(v.x for v in c)/len(c)*4)+int(sum(v.y for v in c)/len(c)*4)*4)
 ob.data.materials.clear()
 for m in materials:ob.data.materials.append(m)
 for f,t in zip(ob.data.polygons,tiles):f.material_index=1 if t in [2,9,15] else 2 if t==5 else 3 if t in [6,14] else 0;f.use_smooth=True
def slot(b):return 1 if b in ['merus_L','propodus_L','dactyl_L'] else 2 if b in ['merus_R','propodus_R','dactyl_R'] else 3 if b.startswith(('abdomen','pleopod')) else 4 if b=='tail' else 5 if b.startswith('leg_') else 0
def finish(ob,b,mat):
 bpy.context.view_layer.objects.active=ob;bpy.ops.object.transform_apply(location=True,rotation=True,scale=True);ob.data.materials.append(materials[mat]);ob.vertex_groups.new(name=b).add(list(range(len(ob.data.vertices))),1,'REPLACE');parts[names[slot(b)]].append(ob)
 for f in ob.data.polygons:f.use_smooth=True
 return ob
def mesh(name,vs,fs,b,mat):
 me=bpy.data.meshes.new(name);me.from_pydata([Vector(v)*.2 for v in vs],[],fs);me.update();bm=bmesh.new();bm.from_mesh(me);bmesh.ops.recalc_face_normals(bm,faces=list(bm.faces));bm.to_mesh(me);bm.free();o=bpy.data.objects.new(name,me);scene.collection.objects.link(o);return finish(o,b,mat)
def strand(name,points,widths,b,mat,flat=.45):
 points=[Vector(p) for p in points]
 if name in ['Pastel mane','Unicorn flowing tail']:
  ps=[];ws=[]
  for k in range(len(points)-1):
   p0,p1,p2,p3=points[max(0,k-1)],points[k],points[k+1],points[min(len(points)-1,k+2)]
   for j in range(5):
    t=j/5;ps.append(.5*((2*p1)+(-p0+p2)*t+(2*p0-5*p1+4*p2-p3)*t*t+(-p0+3*p1-3*p2+p3)*t*t*t));ws.append(widths[k]*(1-t)+widths[k+1]*t)
  ps.append(points[-1]);ws.append(widths[-1]);points,widths=ps,ws
 vs=[];fs=[];n=8
 for i,p in enumerate(points):
  d=(points[min(i+1,len(points)-1)]-points[max(0,i-1)]).normalized();a=d.cross(Vector((0,0,1)))
  if a.length<.01:a=Vector((1,0,0))
  a.normalize();c=d.cross(a).normalized()
  for j in range(n):vs.append(p+widths[i]*(a*math.cos(j*math.tau/n)+c*math.sin(j*math.tau/n)*flat))
 for i in range(len(points)-1):
  for j in range(n):fs.append((i*n+j,i*n+(j+1)%n,(i+1)*n+(j+1)%n,(i+1)*n+j))
 fs.extend([tuple(reversed(range(n))),tuple(range((len(points)-1)*n,len(points)*n))]);return mesh(name,vs,fs,b,mat)
def ell(name,p,scale,b,mat):
 bpy.ops.mesh.primitive_uv_sphere_add(segments=12,ring_count=8,radius=1,location=Vector(p)*.2);o=bpy.context.object;o.name=name;o.scale=Vector(scale)*.2;return finish(o,b,mat)
# Tapered gold horn and spiral ridge mounted on the head, below the independent eyes.
a=Vector((0,-1.42,1.48));z=Vector((0,-.19,.79))
strand('Golden unicorn horn',[a,a+z*.25,a+z*.65,a+z],[.105,.082,.045,.002],'thorax',1,1)
spiral=[];width=[]
for i in range(65):
 t=i/64;r=.105*(1-t);ang=t*math.tau*4;spiral.append(a+z*t+Vector((r*math.cos(ang),r*math.sin(ang),0)));width.append(.014*(1-t)+.002)
strand('Spiral horn relief',spiral,width,'thorax',1,1)
# Swept, closed mane locks, avoiding alpha cards on mobile.
for s in [-1,1]:
 for j in range(6):
  start=Vector((s*(.12+j*.035),-1.38,1.45-j*.025))
  ps=[start,start+Vector((s*.18,.08,.08)),start+Vector((s*.32,.32,-.02)),start+Vector((s*.37,.57,-.12)),start+Vector((s*.24,.69,-.15))]
  strand('Pastel mane',ps,[.035,.085,.09,.055,.002],'thorax',2+j%3)
 # Small framing face feathers and eye jewels.
 strand('Pearl face lock',[(s*.09,-1.67,1.50),(s*.21,-1.73,1.29),(s*.19,-1.78,1.03)],[.06,.105,.003],'thorax',0)
 ell('Eye opal',(s*.39,-1.912,1.85),(.105,.025,.115),'eye_'+('L' if s<0 else 'R'),4)
# Two swept feather wings attached to abdomen_00, kept outside the punching arm corridor.
for s in [-1,1]:
 base=Vector((s*.28,-.30,1.17));tip=Vector((s*1.12,.40,2.02))
 strand('Wing leading arch',[base,base.lerp(tip,.35)+Vector((0,-.10,.16)),tip],[.09,.15,.015],'abdomen_00',0)
 for i in range(9):
  t=i/8;root=base.lerp(tip,t*.83);end=root+Vector((s*(.16+.20*t),.55-.17*t,.12-.22*(1-t)))
  strand('Layered primary feather',[root,root.lerp(end,.30)+Vector((0,0,.06)),root.lerp(end,.73),end],[.045,.085,.058,.001],'abdomen_00',0 if i%3 else 3)
 for i in range(6):
  root=base+Vector((s*i*.07,.10, .06+i*.055));end=root+Vector((s*.12,.33,.02))
  strand('Wing cover feather',[root,root.lerp(end,.45),end],[.04,.065,.001],'abdomen_00',0)
# Gold abdominal edging, following each separate bending segment.
for i,(y,z,w) in enumerate([(-.34,1.02,.51),(.08,.94,.50),(.50,.84,.48),(.91,.74,.45),(1.31,.63,.41),(1.69,.51,.36)]):
 h=.26-i*.013;ps=[]
 for j in range(17):
  t=j*math.pi/16;ps.append((w*math.cos(t),y+.22,z+h*math.sin(t)))
 strand('Gold shell rim',ps,[.019]*17,'abdomen_%02d'%i,1,1)
# Flowing multicolor tail locks fan out behind the original fan, attached to the tail bone.
for j in range(11):
 s=(j-5)/5;root=Vector((s*.20,2.02,.43))
 ps=[root,root+Vector((s*.25,.35,.03)),root+Vector((s*.51,.80,-.13)),root+Vector((s*.65,1.15,-.25)),root+Vector((s*.47,1.42,-.20))]
 strand('Unicorn flowing tail',ps,[.06,.13,.12,.07,.002],'tail',2+j%3)
# Closed pearl punch gauntlets, gold striking tips and a feather cuff on each arm.
for s,label in [(-1,'L'),(1,'R')]:
 bn='dactyl_'+label;a=Vector((s*.31,-1.21,.77));b=Vector((s*.30,-1.82,.28))
 strand('Pearl punch gauntlet',[a,a.lerp(b,.25),a.lerp(b,.65),b],[.075,.19,.16,.04],bn,0,.8)
 strand('Gold striking heel',[a.lerp(b,.65),a.lerp(b,.86),b+Vector((0,-.04,-.01))],[.19,.15,.045],bn,1,.9)
 for j in range(5):
  r=a+Vector((s*(j-2)*.045,-.07,0));e=r+Vector((s*.10,-.22,-.25))
  strand('Wrist feather',[r,r.lerp(e,.5),e],[.025,.06,.001],bn,0 if j%2 else 3)
 for i in range(3):
  bn='leg_lower_%s_%d'%(label,i);foot=arm.data.bones[bn].tail_local/.2
  strand('Golden hoof',[foot+Vector((0,0,.16)),foot+Vector((0,-.04,.05)),foot+Vector((0,-.12,.025))],[.055,.09,.012],bn,1,.8)
  for j in range(4):
   r=foot+Vector(((j-1.5)*.033,0,.27));e=r+Vector(((j-1.5)*.026,-.06,-.17))
   strand('Ankle plumage',[r,r.lerp(e,.45),e],[.015,.045,.001],bn,0 if j%2 else 2+j%3)
for ob in mods:
 bpy.ops.object.select_all(action='DESELECT');ob.select_set(True)
 for add in parts[ob.name]:add.select_set(True)
 bpy.context.view_layer.objects.active=ob;bpy.ops.object.join();bpy.ops.object.mode_set(mode='EDIT');bpy.ops.mesh.select_all(action='SELECT');bpy.ops.uv.smart_project(island_margin=.015);bpy.ops.object.mode_set(mode='OBJECT');ob.data.calc_loop_triangles()
bpy.ops.object.select_all(action='DESELECT');arm.select_set(True)
for o in mods:o.select_set(True)
bpy.context.view_layer.objects.active=arm
bpy.ops.export_scene.fbx(filepath=os.path.join(OUT,'Unicorn_Set.fbx'),use_selection=True,object_types={'MESH','ARMATURE'},add_leaf_bones=False,axis_forward='-Z',axis_up='Y',bake_anim=False)
bpy.ops.export_scene.gltf(filepath=os.path.join(OUT,'Unicorn_Set.glb'),use_selection=True,export_format='GLB',export_animations=False)
scene.render.engine='CYCLES';scene.cycles.samples=24;scene.render.resolution_x=1280;scene.render.resolution_y=960;scene.render.resolution_percentage=100;scene.view_settings.exposure=-.4
cam=scene.camera
for name,loc,target,scale in [('preview',(1.2,-1.65,1.05),(0,.11,.24),1.45),('front',(0,-2,.55),(0,-.02,.27),.98),('side',(1.8,.12,.78),(0,.12,.25),1.45)]:
 cam.location=loc;cam.rotation_euler=(Vector(target)-cam.location).to_track_quat('-Z','Y').to_euler();cam.data.ortho_scale=scale;scene.render.filepath=os.path.join(OUT,name+'.png')
 if name=='preview':bpy.ops.wm.save_as_mainfile(filepath=os.path.join(OUT,'Unicorn_Set.blend'))
 bpy.ops.render.render(write_still=True)
json.dump({'triangles':sum(len(o.data.loop_triangles) for o in mods),'unweighted':sum(sum(not v.groups for v in o.data.vertices) for o in mods),'slots':names},open(os.path.join(OUT,'stats.json'),'w'),indent=2)

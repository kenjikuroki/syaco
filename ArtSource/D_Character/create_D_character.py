import bpy, math, os, random, json, bmesh
from mathutils import Vector, Quaternion
from array import array
OUT=os.path.dirname(os.path.abspath(__file__));S=.2;random.seed(47)
bpy.ops.object.select_all(action='SELECT');bpy.ops.object.delete(use_global=False)
# D design with user-requested anatomical correction to antennae and pleopods.
palette=[(.20,.32,.18),(.22,.30,.19),(.72,.64,.39),(.19,.075,.055),(.72,.25,.10),(.26,.35,.24),(.025,.20,.36),(.69,.13,.065),(.39,.43,.27),(.58,.11,.055),(.66,.52,.27),(.04,.33,.34),(.055,.085,.07),(.17,.25,.16),(.07,.20,.27),(.84,.75,.48)]
size=1024;tile=256;px=array('f',[0])*(size*size*4)
def blend(a,b,t):return tuple(a[i]*(1-t)+b[i]*t for i in range(3))
for y in range(size):
 for x in range(size):
  idx=(x//tile)+(y//tile)*4;u=(x%tile)/255;v=(y%tile)/255;c=palette[idx]
  noise=(math.sin(u*47+math.sin(v*33))*math.sin(v*57+u*13))*.024
  if idx in (0,1,13):
   # Unequal cream islands over green armor; broad enough to read in game.
   q=math.sin(u*15+math.cos(v*12)*1.5)+math.cos(v*19+u*6)
   if idx==0 and q>1.05:c=palette[2]
   elif q<-.8:c=blend(c,(.08,.17,.14),.55)
   elif q>.8:c=blend(c,(.36,.40,.22),.5)
  if idx==4:
   q=math.sin(u*19+v*8)+math.cos(v*17-u*9)
   if q>.45:c=palette[15]
   elif q<-.8:c=blend(c,(.37,.12,.065),.5)
  if idx==5:
   c=palette[11] if u<.23 or u>.81 else palette[5]
   if .30<u<.51:c=palette[4]
   if .51<u<.56:c=palette[2]
   if .47<v<.51:c=blend(c,palette[12],.65)
  if idx in (6,14):
   c=palette[7] if abs(math.sin(u*math.pi*2))<.23 or v>.84 else palette[idx]
   if .31<v<.34:c=blend(c,palette[11],.45)
  if idx==8:
   c=palette[9] if v>.70 else palette[8]
   if .45<v<.58:c=palette[2]
  if idx==11 and v<.28:c=palette[2]
  variation=.95+.075*math.sin(v*math.pi)+noise
  k=(y*size+x)*4;px[k:k+4]=array('f',[max(0,min(1,z*variation)) for z in c]+[1])
image=bpy.data.images.new('D_Character_Albedo_1024',width=size,height=size);image.pixels.foreach_set(px);image.filepath_raw=os.path.join(OUT,'D_Character_Albedo.png');image.file_format='PNG';image.save();image.pack()
mat=bpy.data.materials.new('D_Character_SharedAtlas');mat.use_nodes=True;p=mat.node_tree.nodes.get('Principled BSDF');p.inputs['Roughness'].default_value=.48;p.inputs['Coat Weight'].default_value=.08;p.inputs['Specular IOR Level'].default_value=.32
tex=mat.node_tree.nodes.new('ShaderNodeTexImage');tex.image=image;mat.node_tree.links.new(tex.outputs['Color'],p.inputs['Base Color'])
parts=[];bones=[]
def bone(name,a,b,parent=None):bones.append((name,Vector(a)*S,Vector(b)*S,parent));return name
def uv(t,u,v):return ((t%4+.03+.94*u)/4,(t//4+.03+.94*v)/4)
def mesh(name,vs,fs,faceuv,bn,t=0):
 me=bpy.data.meshes.new(name);me.from_pydata([Vector(v)*S for v in vs],[],fs);me.update();ob=bpy.data.objects.new(name,me);bpy.context.collection.objects.link(ob);me.materials.append(mat)
 layer=me.uv_layers.new(name='AtlasUV')
 for f,coords in zip(me.polygons,faceuv):
  f.use_smooth=True
  for li,coord in zip(f.loop_indices,coords):layer.data[li].uv=uv(t,*coord)
 vg=ob.vertex_groups.new(name=bn);vg.add(list(range(len(vs))),1,'REPLACE')
 bm=bmesh.new();bm.from_mesh(me);bmesh.ops.recalc_face_normals(bm,faces=list(bm.faces));bm.to_mesh(me);bm.free();me.update()
 # Smooth by angle: mark edges above 55 degrees sharp, keeping dorsal planes readable.
 bm=bmesh.new();bm.from_mesh(me)
 for e in bm.edges:
  if len(e.link_faces)==2:e.smooth=e.calc_face_angle(0)<math.radians(55)
 bm.to_mesh(me);bm.free();parts.append(ob);return ob
# Swept anatomical volumes, with tapered profiles and continuous curved contours.
def sweep(name,points,widths,depths,bn,t=0,sides=12):
 points=[Vector(p) for p in points];vs=[];fs=[];uvs=[]
 for k,c in enumerate(points):
  d=(points[min(k+1,len(points)-1)]-points[max(k-1,0)]).normalized();a=Vector((1,0,0));a=(a-d*a.dot(d)).normalized();b=d.cross(a).normalized()
  for j in range(sides):
   angle=j*2*math.pi/sides;vs.append(c+a*math.cos(angle)*widths[k]+b*math.sin(angle)*depths[k])
 for k in range(len(points)-1):
  for j in range(sides):
   fs.append((k*sides+j,k*sides+(j+1)%sides,(k+1)*sides+(j+1)%sides,(k+1)*sides+j));uvs.append([(j/sides,k/(len(points)-1)),((j+1)/sides,k/(len(points)-1)),((j+1)/sides,(k+1)/(len(points)-1)),(j/sides,(k+1)/(len(points)-1))])
 fs += [tuple(reversed(range(sides))),tuple(range((len(points)-1)*sides,len(points)*sides))];uvs += [[(.5,.02)]*sides,[(.5,.98)]*sides]
 return mesh(name,vs,fs,uvs,bn,t)
def rod(name,a,b,r,bn,t=2):
 a,b=Vector(a),Vector(b);return sweep(name,[a,a.lerp(b,.15),a.lerp(b,.8),b],[r*.65,r,r*.56,r*.08],[r*.55,r*.75,r*.4,r*.07],bn,t,8)
def ell(name,center,axes,bn,t=0):
 c=Vector(center);pts=[];ws=[];ds=[]
 for k in range(9):
  a=-math.pi/2+math.pi*k/8;pts.append(c+Vector((0,axes[1]*math.sin(a),0)));ws.append(max(.001,axes[0]*math.cos(a)));ds.append(max(.001,axes[2]*math.cos(a)))
 return sweep(name,pts,ws,ds,bn,t,16)
bone('root',(0,0,0),(0,0,.35));bone('abdomen_02',(0,.72,.75),(0,.30,.86),'root')
for name,a,b,parent in [('abdomen_01',(0,.30,.86),(0,-.10,.99),'abdomen_02'),('abdomen_00',(0,-.10,.99),(0,-.47,1.09),'abdomen_01'),('thorax',(0,-.47,1.09),(0,-1.43,1.15),'abdomen_00'),('abdomen_03',(0,.72,.75),(0,1.13,.67),'root'),('abdomen_04',(0,1.13,.67),(0,1.51,.56),'abdomen_03'),('abdomen_05',(0,1.51,.56),(0,1.90,.44),'abdomen_04'),('tail',(0,1.90,.44),(0,2.45,.23),'abdomen_05')]:bone(name,a,b,parent)
# Armor shields use a dorsal cross section and a narrow posterior rim, not cylinders.
def shield(name,y,w,length,z,h,bn,t):
 section=[(1,-.25),(.98,.13),(.82,.66),(.40,.96),(0,1.02),(-.40,.96),(-.82,.66),(-.98,.13),(-1,-.25),(-.67,-.55),(0,-.63),(.67,-.55)]
 profiles=[(-.51,.87,.92),(-.35,.97,1.0),(.12,1,1),(.39,1.03,.94),(.47,1.025,.86),(.50,.96,.77)]
 if name.startswith("Cephalothorax"):profiles=[(-.51,.36,.52),(-.30,.70,.80),(.12,1,1),(.39,1.03,.94),(.47,1.025,.86),(.50,.96,.77)]
 vs=[];fs=[];uvs=[]
 for dy,fw,fh in profiles:
  for x,zz in section:vs.append((x*w*fw,y+dy*length,z+zz*h*fh-dy*length*.24))
 for k in range(5):
  for j in range(12):
   fs.append((k*12+j,k*12+(j+1)%12,(k+1)*12+(j+1)%12,(k+1)*12+j));uvs.append([(j/12,k/5),((j+1)/12,k/5),((j+1)/12,(k+1)/5),(j/12,(k+1)/5)])
 fs += [tuple(reversed(range(12))),tuple(range(60,72))];uvs += [[(.5,.01)]*12,[(.5,.99)]*12]
 ob=mesh(name,vs,fs,uvs,bn,t)
 for poly in ob.data.polygons:
  if 36<=poly.index<48 and poly.index%12<9:
   for li in poly.loop_indices:ob.data.uv_layers.active.data[li].uv=uv(2,.45,.45)
 return ob
shield('Cephalothorax_dorsal',-1.08,.52,1.27,1.16,.31,'thorax',0)
sweep('Thoracic_ventral_keel',[(0,-1.72,.92),(0,-1.4,.85),(0,-.8,.87),(0,-.47,.89)],[.14,.32,.37,.28],[.1,.16,.18,.13],'thorax',10)
for i,(y,z,w) in enumerate([(-.34,1.02,.51),(.08,.94,.50),(.50,.84,.48),(.91,.74,.45),(1.31,.63,.41),(1.69,.51,.36)]):
 bn='abdomen_%02d'%i;shield('Abdominal_shield_%02d'%i,y,w,.57,z,.26-i*.013,bn,1)
 sweep('Ventral_segment_%02d'%i,[(0,y-.22,z-.13),(0,y,z-.14),(0,y+.25,z-.12)],[w*.65,w*.72,w*.66],[.11,.13,.10],bn,3)
 for s in [-1,1]:
  sweep('Pleura_%02d_%s'%(i,s),[(s*w*.78,y-.16,z-.04),(s*(w+.06),y+.04,z-.10),(s*(w+.10),y+.29,z-.23)],[.065,.12,.015],[.065,.055,.012],bn,0,8)
# Five pairs of ventral swimmerets, distinct from the three walking leg pairs.
# Broad paired blades and compact gill fronds avoid a hairy or spiky silhouette.
for i,(y,z,w) in enumerate([(-.34,1.02,.51),(.08,.94,.50),(.50,.84,.48),(.91,.74,.45),(1.31,.63,.41)]):
 for s,label in [(-1,'L'),(1,'R')]:
  base=Vector((s*w*.52,y,z-.20));end=base+Vector((s*.07,.17,-.24))
  bn=bone('pleopod_%s_%02d'%(label,i),base,end,'abdomen_%02d'%i)
  sweep('Pleopod_stem_'+label+str(i),[base,base.lerp(end,.45),end],[.035,.045,.022],[.035,.04,.02],bn,3,8)
  for j in [-1,1]:
   a=base.lerp(end,.30)+Vector((s*j*.045,0,0));b=end+Vector((s*j*.045,.075,-.045))
   sweep('Pleopod_blade_%s_%d_%d'%(label,i,j),[a,a.lerp(b,.35),a.lerp(b,.72),b],[.025,.075,.065,.008],[.015,.025,.02,.005],bn,13,8)
  for j in range(4):
   a=base.lerp(end,.35+j*.13)+Vector((s*.035,0,0));b=a+Vector((s*.08,.09+j*.008,-.03))
   sweep('Gill_frond_%s_%d_%d'%(label,i,j),[a,a.lerp(b,.5),b],[.012,.027,.005],[.015,.035,.006],bn,3,6)
# Tail fans: closed, broad blue lobes with red margins encoded into shared atlas.
shield('Telson_base',2.00,.31,.49,.39,.16,'tail',1)
for s in [-1,1]:
 for i in range(2):
  start=Vector((s*(.13+i*.12),1.97,.37));end=Vector((s*(.42+i*.40),2.89-i*.23,.15));d=end-start
  sweep('Uropod_%s_%s'%(s,i),[start,start+d*.19,start+d*.48,start+d*.75,end],[.055,.17,.24,.21,.025],[.025,.035,.045,.025,.008],'tail',6 if i==0 else 14,12)
  rod('Uropod_ridge',start+Vector((0,0,.055)),end+Vector((0,0,.01)),.024,'tail',2)
sweep('Telson_center',[(0,2.13,.38),(0,2.37,.31),(0,2.65,.20),(0,2.84,.16)],[.12,.24,.21,.04],[.04,.06,.035,.01],'tail',6)
# Six walking legs, substantial taper and red terminal sections.
for s,label in [(-1,'L'),(1,'R')]:
 for i in range(3):
  hip=Vector((s*.34,-.63+i*.36,.88-i*.055));knee=Vector((s*(.66+i*.055),-.93+i*.46,.46-i*.025));foot=Vector((s*(.91+i*.06),-1.13+i*.55,.04))
  up=bone('leg_upper_%s_%d'%(label,i),hip,knee,'thorax' if i==0 else 'abdomen_00');lo=bone('leg_lower_%s_%d'%(label,i),knee,foot,up)
  sweep('Coxa_%s_%d'%(label,i),[hip,hip.lerp(knee,.35),hip.lerp(knee,.78),knee],[.05,.085,.063,.04],[.06,.083,.055,.04],up,8,10)
  sweep('Tibia_%s_%d'%(label,i),[knee,knee.lerp(foot,.23),knee.lerp(foot,.69),foot],[.045,.060,.039,.012],[.043,.046,.027,.01],lo,8,10)
  rod('Tarsal_claw',foot,foot+Vector((s*.035,-.14,-.018)),.025,lo,9)
 # Broad antennal scales under the eyes: blue inner faces and warm-red rims.
 sweep('Antennal_scale_'+label,[(s*.19,-1.65,1.12),(s*.43,-1.61,1.00),(s*.64,-1.53,.81),(s*.68,-1.65,.74)],[.025,.12,.13,.014],[.026,.045,.036,.006],'thorax',14,10)
 # Long independent eye stalks, with barrel-shaped eyes instead of cute spherical pupils.
 en=bone('eye_'+label,(s*.18,-1.57,1.25),(s*.37,-1.76,1.78),'thorax')
 sweep('Eyestalk_'+label,[(s*.18,-1.57,1.25),(s*.23,-1.62,1.45),(s*.34,-1.73,1.70),(s*.37,-1.76,1.78)],[.055,.065,.066,.075],[.045,.05,.056,.062],en,11,12)
 eye=ell('Compound_eye_'+label,(s*.39,-1.80,1.84),(.16,.125,.18),en,5)
 # Two curved, segmented antennules. Their tips stay thin and distinct in silhouette.
 for j in range(2):
  ps=[(s*.14,-1.68,1.29),(s*.23,-1.87,1.36),(s*(.43+j*.08),-2.06,1.47),(s*(.70+j*.13),-2.20,1.48-j*.08),(s*(.95+j*.14),-2.27,1.40-j*.13)]
  sweep('Antennule_%s_%d'%(label,j),ps,[.022,.024,.014,.008,.0015],[.02,.02,.011,.006,.001],'thorax',9,6)
 # Folded raptorial limb: three linked anatomical volumes. No opposing claw/prongs.
 shoulder=Vector((s*.36,-1.00,1.08));hinge=Vector((s*.34,-1.72,.66));wrist=Vector((s*.31,-1.21,.77));tip=Vector((s*.30,-1.82,.28))
 mer=bone('merus_'+label,shoulder,hinge,'thorax');pro=bone('propodus_'+label,hinge,wrist,mer);dac=bone('dactyl_'+label,wrist,tip,pro)
 sweep('Merus_'+label,[shoulder,shoulder.lerp(hinge,.26)+Vector((0,.02,.04)),shoulder.lerp(hinge,.64),hinge],[.09,.19,.17,.08],[.09,.18,.15,.075],mer,13,14)
 sweep('Folded_link_'+label,[hinge,hinge.lerp(wrist,.28)-Vector((0,0,.03)),hinge.lerp(wrist,.72),wrist],[.08,.115,.10,.065],[.07,.095,.084,.055],pro,10,12)
 sweep('Striking_club_'+label,[wrist,wrist.lerp(tip,.20),wrist.lerp(tip,.49)+Vector((0,-.04,0)),wrist.lerp(tip,.78),tip],[.06,.17,.185,.14,.035],[.065,.13,.16,.12,.025],dac,4,16)
 # Rounded striking heel integrated into the club silhouette, not a separate fist ball.
 # Small tucked mouth appendages, kept inside the chest silhouette.
 for i in range(3):
  a=Vector((s*(.10+i*.045),-1.66,1.12-i*.105));b=a+Vector((s*.05,-.18,-.09));c=b+Vector((-s*.08,-.05,-.09))
  sweep('Maxilliped_%s_%d'%(label,i),[a,a.lerp(b,.6),b,c],[.026,.045,.035,.006],[.025,.04,.03,.006],'thorax',10,8)
sweep('Rostral_face',[(0,-1.59,1.5),(0,-1.74,1.34),(0,-1.78,1.15),(0,-1.76,1.0)],[.08,.14,.10,.025],[.035,.06,.035,.015],'thorax',11,10)
# One armature and six skinned modules. Rigid plate weighting retains clean articulation.
armdata=bpy.data.armatures.new('D_Mantis_Skeleton');arm=bpy.data.objects.new('D_Mantis_Rig',armdata);bpy.context.collection.objects.link(arm)
bpy.ops.object.select_all(action='DESELECT');arm.select_set(True);bpy.context.view_layer.objects.active=arm;bpy.ops.object.mode_set(mode='EDIT')
for name,h,t,parent in bones:
 b=armdata.edit_bones.new(name);b.head=h;b.tail=t
 if parent:b.parent=armdata.edit_bones[parent]
# Attachment bones make future equipment positions explicit without permanent accessories.
for name,h,t,parent in [('socket_head',(0,-1.03,1.52),(0,-1.03,1.66),'thorax'),('socket_back',(0,.45,1.14),(0,.45,1.28),'abdomen_02'),('socket_glove_L',(-.31,-1.58,.47),(-.31,-1.68,.47),'dactyl_L'),('socket_glove_R',(.31,-1.58,.47),(.31,-1.68,.47),'dactyl_R')]:
 b=armdata.edit_bones.new(name);b.head=Vector(h)*S;b.tail=Vector(t)*S;b.parent=armdata.edit_bones[parent];b.use_deform=False
bpy.ops.object.mode_set(mode='OBJECT');arm.show_in_front=True
slots={k:[] for k in ['01_Cephalothorax','02_PunchArm_L','03_PunchArm_R','04_Abdomen','05_Tail','06_WalkingLegs']}
for ob in parts:
 bn=ob.vertex_groups[0].name
 slot='02_PunchArm_L' if bn in ['merus_L','propodus_L','dactyl_L'] else '03_PunchArm_R' if bn in ['merus_R','propodus_R','dactyl_R'] else '04_Abdomen' if bn.startswith(('abdomen','pleopod')) else '05_Tail' if bn=='tail' else '06_WalkingLegs' if bn.startswith('leg_') else '01_Cephalothorax'
 slots[slot].append(ob)
modules=[]
for name,objects in slots.items():
 bpy.ops.object.select_all(action='DESELECT')
 for ob in objects:ob.select_set(True)
 bpy.context.view_layer.objects.active=objects[0];bpy.ops.object.join();ob=bpy.context.object;ob.name=name
 bpy.ops.object.transform_apply(location=True,rotation=True,scale=True)
 # Minimal baked bevels on sharp edges, preserving the mobile triangle budget.
 bevel=ob.modifiers.new('Small shell bevel','BEVEL');bevel.width=.0009;bevel.segments=1;bevel.limit_method='ANGLE';bevel.angle_limit=math.radians(55)
 bpy.ops.object.modifier_apply(modifier=bevel.name)
 for f in ob.data.polygons:f.use_smooth=True
 bm=bmesh.new();bm.from_mesh(ob.data);bmesh.ops.recalc_face_normals(bm,faces=list(bm.faces))
 for e in bm.edges:
  if len(e.link_faces)==2:e.smooth=e.calc_face_angle(0)<math.radians(55)
 bm.to_mesh(ob.data);bm.free()
 skin=ob.modifiers.new('D skeleton deform','ARMATURE');skin.object=arm;ob.parent=arm;ob['customization_slot']=name;modules.append(ob)
# A small inspection action tests eyes and tail without claiming production combat clips.
scene=bpy.context.scene;scene.render.fps=30;scene.frame_start=1;scene.frame_end=60
for frame,amount in [(1,0),(16,.13),(31,0),(46,-.10),(60,0)]:
 for name in ['eye_L','eye_R','tail']:
  pb=arm.pose.bones[name];pb.rotation_mode='XYZ';pb.rotation_euler=(0,amount*(.25 if name=='tail' else 1),amount*.2);pb.keyframe_insert('rotation_euler',frame=frame,group=name)
arm.animation_data.action.name='D_Idle_Inspection';scene.frame_set(1)
for i in range(5):
 for label in ['L','R']:
  pb=arm.pose.bones['pleopod_%s_%02d'%(label,i)];pb.rotation_mode='XYZ'
  for frame in range(1,62,3):
   pb.rotation_euler.x=.16*math.sin((frame-1)*math.tau/60-i*.7)
   pb.keyframe_insert('rotation_euler',frame=frame,group=pb.name)
scene.frame_set(1)
# Export character only, studio excluded.
bpy.ops.object.select_all(action='DESELECT');arm.select_set(True)
for ob in modules:ob.select_set(True)
bpy.context.view_layer.objects.active=arm
bpy.ops.export_scene.fbx(filepath=os.path.join(OUT,'D_Mantis_Base.fbx'),use_selection=True,object_types={'MESH','ARMATURE'},add_leaf_bones=False,axis_forward='-Z',axis_up='Y',bake_anim=True,bake_anim_use_all_actions=False,bake_anim_use_nla_strips=False,path_mode='COPY',embed_textures=True)
bpy.ops.export_scene.gltf(filepath=os.path.join(OUT,'D_Mantis_Base.glb'),export_format='GLB',use_selection=True,export_animations=True)
# Neutral studio previews, no gear and no battle effects.
bpy.ops.mesh.primitive_plane_add(size=2000,location=(0,0,-.002));floor=bpy.context.object;floor.name='Preview_ground';fm=bpy.data.materials.new('Studio gray');fm.diffuse_color=(.22,.24,.23,1);floor.data.materials.append(fm)
def aim(o,p):o.rotation_euler=(Vector(p)-o.location).to_track_quat('-Z','Y').to_euler()
for name,loc,power,size,col in [('Key',(1,-1.4,2),115,1.3,(1,.93,.81)),('Fill',(-1,-.5,1),65,1.1,(.74,.87,1)),('Rim',(.3,1.7,1.6),125,1,(1,.89,.72))]:
 d=bpy.data.lights.new(name,'AREA');o=bpy.data.objects.new(name,d);scene.collection.objects.link(o);o.location=loc;d.energy=power;d.size=size;d.color=col;aim(o,(0,.05,.20))
scene.world.use_nodes=True;scene.world.node_tree.nodes['Background'].inputs[0].default_value=(.23,.26,.28,1);scene.world.node_tree.nodes['Background'].inputs[1].default_value=.35
camdata=bpy.data.cameras.new('Review camera');cam=bpy.data.objects.new('Review camera',camdata);scene.collection.objects.link(cam);scene.camera=cam;camdata.type='ORTHO';camdata.clip_end=1000;camdata.ortho_scale=1.25
scene.render.engine='CYCLES';scene.cycles.samples=40;scene.cycles.use_denoising=True;scene.render.resolution_x=1500;scene.render.resolution_y=1100;scene.render.resolution_percentage=100;scene.view_settings.view_transform='AgX';scene.unit_settings.system='METRIC'
cam.location=(1.1,-1.65,.95);aim(cam,(0,.055,.21));bpy.ops.wm.save_as_mainfile(filepath=os.path.join(OUT,'D_Mantis_Base.blend'))
for name,loc,target,scale in [('01_three_quarter',(1.1,-1.65,.95),(0,.055,.21),1.23),('02_front',(0,-2,.35),(0,-.1,.22),.68),('03_side',(6,.065,.7),(0,.065,.22),1.25),('04_rear',(.9,1.7,.8),(0,.1,.18),1.20)]:
 cam.location=loc;aim(cam,target);camdata.ortho_scale=scale;scene.render.filepath=os.path.join(OUT,name+'.png');bpy.ops.render.render(write_still=True)
checks={};triangles=0
for ob in modules:
 ob.data.calc_loop_triangles();triangles+=len(ob.data.loop_triangles);checks[ob.name]={'vertices':len(ob.data.vertices),'triangles':len(ob.data.loop_triangles),'uv':bool(ob.data.uv_layers),'unweighted':sum(not v.groups for v in ob.data.vertices)}
json.dump({'modules':checks,'total_triangles':triangles,'bones':len(arm.data.bones),'materials':1,'texture':'1024x1024','scale':'meters, forward -Y in Blender, FBX -Z forward Y up','game_integrated':False},open(os.path.join(OUT,'validation.json'),'w'),indent=2)
print('D_MODEL_COMPLETE',triangles)




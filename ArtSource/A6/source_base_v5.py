import bpy, math, os, json
from mathutils import Vector, Quaternion, Matrix

OUT=os.path.dirname(os.path.abspath(__file__))
bpy.ops.object.select_all(action='SELECT'); bpy.ops.object.delete(use_global=False)
parts=[]; specs=[]; leg_specs=[]
mat=bpy.data.materials.new('Mantis_PaintedShell'); mat.use_nodes=True
image=bpy.data.images.load(os.path.join(OUT,'mantis_albedo.png')); image.pack()
tex=mat.node_tree.nodes.new('ShaderNodeTexImage'); tex.image=image
shader=mat.node_tree.nodes.get('Principled BSDF'); shader.inputs['Roughness'].default_value=.72
shader.inputs['Specular IOR Level'].default_value=.15
mat.node_tree.links.new(tex.outputs['Color'],shader.inputs['Base Color'])

def bone(n,h,t,p=None): specs.append((n,Vector(h),Vector(t),p)); return n
bone('root',(0,1.4,.15),(0,1.4,.55))
# Mid-abdomen is the anchor. Anterior and posterior chains branch from it.
ys=[-.05,.43,.91,1.39,1.87,2.35]
zs=[1.00,.95,.90,.83,.75,.67]
bone('abdomen_03',(0,1.64,.83),(0,1.14,.83),'root')
for i in [2,1,0]: bone('abdomen_%02d'%i,(0,ys[i]+.25,zs[i]),(0,ys[i]-.25,zs[i]),'abdomen_%02d'%(i+1))
for i in [4,5]: bone('abdomen_%02d'%i,(0,ys[i]-.25,zs[i]),(0,ys[i]+.25,zs[i]),'abdomen_%02d'%(i-1))
bone('thorax',(0,-.29,1),(0,-1.43,1.08),'abdomen_00')
bone('tail',(0,2.59,.63),(0,3.22,.48),'abdomen_05')

def atlas_uv(u,v,tile):
    # Pad each island against bilinear/mip bleed.
    return ((tile%4+.025+.95*u)/4,(tile//4+.025+.95*v)/4)

def finish(ob,name,tile,bn='thorax',uv_coords=None,smooth=False):
    ob.name=name; ob.data.materials.clear(); ob.data.materials.append(mat)
    uv=ob.data.uv_layers.active or ob.data.uv_layers.new(); uv.name='AtlasUV'
    for p in ob.data.polygons:
        p.use_smooth=smooth
        for li in p.loop_indices:
            if uv_coords:
                u,v=uv_coords(ob.data.vertices[ob.data.loops[li].vertex_index].co,p,li)
            elif ob.data.uv_layers.active:
                u,v=uv.data[li].uv
            else: u,v=.5,.5
            uv.data[li].uv=atlas_uv(max(0,min(1,u)),max(0,min(1,v)),tile)
    vg=ob.vertex_groups.new(name=bn); vg.add(list(range(len(ob.data.vertices))),1,'REPLACE')
    parts.append(ob); return ob

def mesh(name,vs,fs,tile,bn='thorax',uvs=None):
    me=bpy.data.meshes.new(name); me.from_pydata(vs,[],fs); me.update()
    ob=bpy.data.objects.new(name,me); bpy.context.collection.objects.link(ob)
    if uvs:
        uv=me.uv_layers.new(name='AtlasUV')
        for poly,face_uv in zip(me.polygons,uvs):
            for li,xy in zip(poly.loop_indices,face_uv): uv.data[li].uv=xy
    return finish(ob,name,tile,bn)

def ell(name,loc,scale,tile,bn='thorax',segments=12,rings=6,smooth=False):
    bpy.ops.mesh.primitive_uv_sphere_add(segments=segments,ring_count=rings,radius=1,location=loc)
    ob=bpy.context.object; ob.scale=scale
    bpy.ops.object.transform_apply(location=False,rotation=False,scale=True)
    return finish(ob,name,tile,bn,smooth=smooth)

def rod(name,a,b,r1,r2,tile,bn='thorax',sides=6):
    a,b=Vector(a),Vector(b); d=b-a
    bpy.ops.mesh.primitive_cone_add(vertices=sides,radius1=r1,radius2=r2,depth=d.length,location=(a+b)/2)
    ob=bpy.context.object; ob.rotation_euler=d.to_track_quat('Z','Y').to_euler()
    bpy.ops.object.transform_apply(location=False,rotation=True,scale=True)
    return finish(ob,name,tile,bn)

def plate(name,y,w,length,height,z,bn,tile=0,head=False):
    # Sculpted cross section: high central ridge, broad shoulders, lowered flanges.
    section=[(1,-.12),(.99,.20),(.85,.64),(.52,.91),(0,1.04),(-.52,.91),(-.85,.64),(-.99,.20),(-1,-.12),(-.70,-.45),(0,-.51),(.70,-.45)]
    profiles=[(-.50,.62,.62),(-.32,.91,.89),(.18,1.0,1.0),(.43,1.06,.96),(.51,1.045,.91),(.52,.94,.79)]
    if head: profiles=[(-.5,.58,.61),(-.28,.91,.93),(.16,1,1),(.43,1.05,.93),(.51,1.04,.88),(.52,.94,.76)]
    vs=[]; n=len(section)
    for dy,fw,fh in profiles:
        for x,zz in section: vs.append((x*w*fw,y+dy*length,z+zz*height*fh))
    fs=[]; uvs=[]
    for k in range(len(profiles)-1):
        for j in range(n):
            fs.append((k*n+j,k*n+(j+1)%n,(k+1)*n+(j+1)%n,(k+1)*n+j))
            uvs.append([(j/n,k/(len(profiles)-1)),((j+1)/n,k/(len(profiles)-1)),((j+1)/n,(k+1)/(len(profiles)-1)),(j/n,(k+1)/(len(profiles)-1))])
    fs.extend([tuple(reversed(range(n))),tuple(range((len(profiles)-1)*n,len(profiles)*n))])
    uvs.extend([[(.5,.03)]*n,[(.5,.97)]*n])
    ob=mesh(name,vs,fs,tile,bn,uvs)
    # The raised posterior lip is a narrow band of actual shell geometry.
    lip=ob.data.uv_layers.active
    for p in ob.data.polygons:
        if 3*n<=p.index<4*n and p.index%n<9:
            for li in p.loop_indices:
                old=lip.data[li].uv; u=(old.x*4-tile%4-.025)/.95
                lip.data[li].uv=atlas_uv(u,.50,10)
    return ob

def leaf(name,a,b,width,tile,bn='thorax',thick=.09):
    a,b=Vector(a),Vector(b); d=b-a
    across=Vector((d.y,-d.x,0)).normalized()*width
    vs=[a-across*.15,a+across*.15,a+d*.60+across,b+across*.40,b-across*.40,a+d*.60-across,a+d*.52+Vector((0,0,thick)),a+d*.5-Vector((0,0,.025))]
    fs=[(i,(i+1)%6,6) for i in range(6)]+[((i+1)%6,i,7) for i in range(6)]
    uvs=[]
    coords=[(.45,0),(.55,0),(1,.6),(.7,1),(.3,1),(0,.6),(.5,.5),(.5,.5)]
    for f in fs: uvs.append([coords[i] for i in f])
    return mesh(name,vs,fs,tile,bn,uvs)

def pod(name,a,b,width,depth,tile,bn,tip=.5):
    # Tapered curved armor, not a sphere attached to a cylinder.
    a,b=Vector(a),Vector(b); d=b-a; q=d.to_track_quat('Y','Z')
    vs=[]; fs=[]; uv=[]; sides=8
    profiles=[(0,.43),(.18,.85),(.50,1),(.80,.85),(1,tip)]
    for t,f in profiles:
        center=a+d*t
        for j in range(sides):
            ang=2*math.pi*j/sides
            vs.append(center+q@Vector((math.cos(ang)*width*f,0,math.sin(ang)*depth*f)))
    for k in range(4):
        for j in range(sides):
            fs.append((k*sides+j,k*sides+(j+1)%sides,(k+1)*sides+(j+1)%sides,(k+1)*sides+j))
            uv.append([(j/sides,k/4),((j+1)/sides,k/4),((j+1)/sides,(k+1)/4),(j/sides,(k+1)/4)])
    fs += [tuple(reversed(range(sides))),tuple(range(4*sides,5*sides))]
    uv += [[(.5,.05)]*sides,[(.5,.95)]*sides]
    return mesh(name,vs,fs,tile,bn,uv)

# Armor and pleura overlap; abdomen remains visibly narrower than thorax.
plate('Head_carapace',-1.02,.78,1.53,.49,1.13,'thorax',0,True)
ell('Thorax_undershell',(0,-.78,.98),(.53,.91,.26),3,segments=10,rings=4)
for i in range(6):
    bn='abdomen_%02d'%i; w=.62-i*.031
    plate('Abdominal_shield_%02d'%i,ys[i],w,.59,.34-i*.018,zs[i],bn,8 if i%2 else 0)
    for s in [-1,1]:
        leaf('Pleural_shield',(s*w*.70,ys[i]-.18,zs[i]-.02),(s*(w+.12),ys[i]+.31,zs[i]-.20),.18,1,bn,.07)
        # Longitudinal shell carina highlights the hard rim without extra material.
        rod('Carina',(s*w*.74,ys[i]-.10,zs[i]+.23-i*.012),(s*w*.81,ys[i]+.22,zs[i]+.19-i*.010),.018,.010,10,bn,5)

leaf('Central_telson',(0,2.54,.64),(0,3.40,.43),.49,8,'tail',.14)
for s in [-1,1]:
    leaf('Inner_uropod',(s*.22,2.55,.60),(s*.71,3.37,.36),.28,9,'tail',.08)
    leaf('Outer_uropod',(s*.30,2.49,.60),(s*1.03,3.09,.32),.24,1,'tail',.08)
    rod('Tail_carina',(s*.13,2.68,.74),(s*.26,3.22,.54),.028,.014,10,'tail',5)
    rod('Uropod_ridge',(s*.27,2.60,.67),(s*.68,3.25,.43),.023,.011,10,'tail',5)
    for j in range(3):
        rod('Telson_tooth',(s*(.10+j*.13),3.29-j*.025,.46),(s*(.12+j*.16),3.46-j*.025,.40),.032,.002,10,'tail',5)

arm_landmarks={}
for s,label in [(-1,'L'),(1,'R')]:
    eye=bone('eye_'+label,(s*.22,-1.56,1.26),(s*.40,-1.83,1.72),'thorax')
    pod('Eyestalk_'+label,(s*.23,-1.55,1.25),(s*.40,-1.84,1.72),.090,.085,1,eye)
    # Transverse barrel eyes and dark equatorial bands, no cartoon pupils.
    ob=ell('Compound_eye_'+label,(s*.43,-1.89,1.78),(.24,.155,.165),5,eye,14,8,True)
    ob.rotation_euler[1]=s*.22
    rod('Eye_collar',(s*.36,-1.77,1.59),(s*.41,-1.83,1.69),.108,.11,10,eye,8)
    leaf('Antennal_scale',(s*.22,-1.57,1.11),(s*.70,-2.21,.98),.19,9,'thorax',.05)
    for j in range(2):
        a=(s*.13,-1.67,1.18); b=(s*(.33+j*.13),-2.31,1.20-j*.10); c=(s*(.54+j*.43),-3.01+j*.09,1.00-j*.15)
        rod('Antennule',a,b,.028,.017,4,sides=5); rod('Antennule_tip',b,c,.017,.004,10,sides=5)
    shoulder=Vector((s*.42,-1.03,1.08)); elbow=Vector((s*.40,-1.89,.50)); wrist=Vector((s*.48,-1.40,.60))
    upper=bone('merus_'+label,shoulder,elbow,'thorax')
    fore=bone('propodus_'+label,elbow,wrist,upper)
    club=bone('dactyl_'+label,wrist,wrist+Vector((0,-.25,-.12)),fore)
    arm_landmarks[label]={'shoulder':list(shoulder),'elbow':list(elbow),'wrist':list(wrist)}
    pod('Merus_shell_'+label,shoulder,elbow,.23,.20,1,upper,.62)
    pod('Merus_outer_plate_'+label,shoulder+Vector((s*.15,0,.015)),elbow+Vector((s*.09,.02,0)),.145,.14,8,upper,.50)
    ell('Carpal_joint_'+label,elbow,(.15,.14,.14),3,upper,10,5)
    # A narrow propodus folds BACK along the merus, with a clear gap between them.
    pod('Folded_propodus_'+label,elbow+Vector((s*.09,0,0)),wrist,.13,.12,11,fore,.75)
    pod('Dactyl_club_'+label,wrist+Vector((0,-.14,-.22)),wrist+Vector((0,.19,.12)),.235,.21,2,club,.60)
    ell('Impact_heel_'+label,wrist+Vector((0,.19,.12)),(.19,.15,.16),7,club,10,5)
    # Folded pointed finger along the inside of the club, not a lobster pincer.
    pod('Dactyl_fold_'+label,wrist+Vector((-s*.08,.05,.02)),wrist+Vector((-s*.09,-.34,-.23)),.072,.065,10,club,.02)
    for j in range(3):
        a=elbow.lerp(wrist,.32+j*.14)+Vector((-s*.075,0,.045))
        rod('Propodal_ridge',a,a+Vector((-s*.045,-.09,.015)),.030,.002,10,fore,5)
    # Three walking-leg pairs. Their two-bone IK is baked, with fixed foot targets.
    for i in range(3):
        y=-.49+i*.31; hip=Vector((s*.40,y,.90)); foot=Vector((s*(1.06+i*.04),y+.10,.06))
        pole=Vector((s*1.42,y+.39,.69)); mid=(hip+foot)*.5
        axis=(foot-hip).normalized(); direction=(pole-mid)-axis*(pole-mid).dot(axis); direction.normalize()
        length=.85; knee=mid+direction*math.sqrt(length*length-(foot-hip).length_squared/4)
        up=bone('leg_upper_%s_%d'%(label,i),hip,knee,'thorax')
        lo=bone('leg_lower_%s_%d'%(label,i),knee,foot,up)
        leg_specs.append((up,lo,hip,knee,foot,pole,length))
        pod('Walking_coxa',hip,knee,.085,.07,1,up,.50)
        rod('Walking_tibia',knee,foot,.068,.018,4,lo)
        toe=bone('foot_%s_%d'%(label,i),foot,foot+Vector((0,-.11,0)),'root')
        rod('Foot_tip',foot,foot+Vector((0,-.11,0)),.023,.003,10,toe,5)
    for i in range(5):
        y=ys[i]+.06; bn='abdomen_%02d'%i
        leaf('Swimmeret',(s*.24,y,zs[i]-.17),(s*.42,y+.29,zs[i]-.39),.12,13,bn,.025)
ell('Mouth_shield',(0,-1.66,1.09),(.23,.18,.18),1,segments=10,rings=5)
for s in [-1,1]:
    pod('Maxilliped',(s*.15,-1.64,.96),(s*.22,-1.95,.78),.060,.065,4,'thorax',.25)
    rod('Mandible',(s*.15,-1.8,.91),(s*.06,-1.99,.85),.056,.012,10)

# Rest-space proportion edit: bigger cephalothorax, compact abdomen/tail.
# Transform vertices and corresponding bones together so rig pivots still match.
face_names=('Eyestalk','Compound_eye','Eye_collar','Antennal_scale','Antennule','Mouth_shield','Maxilliped','Mandible')
def compact_rear(p):
    return Vector((p.x*.82,-.29+(p.y+.29)*.82,1+(p.z-1)*.92))
def is_rear(bn): return bn.startswith('abdomen_') or bn=='tail'
for ob in parts:
    bn=ob.vertex_groups[0].name
    if is_rear(bn):
        inv=ob.matrix_world.inverted()
        for v in ob.data.vertices: v.co=inv@compact_rear(ob.matrix_world@v.co)
    elif ob.name.startswith(face_names):
        ob.location+=Vector((0,-.14,.06))
specs=[(n,compact_rear(h),compact_rear(t),p) if is_rear(n) or n=='root' else
       (n,h+Vector((0,-.14,.06)),t+Vector((0,-.14,.06)),p) if n.startswith('eye_') else
       (n,h,t,p) for n,h,t,p in specs]

# Bake construction coordinates into one skinned mesh and one armature.
bpy.ops.object.select_all(action='DESELECT')
for ob in parts: ob.select_set(True)
bpy.context.view_layer.objects.active=parts[0]; bpy.ops.object.join(); model=bpy.context.object; model.name='MantisShrimp_v5'
bpy.ops.object.transform_apply(location=True,rotation=True,scale=True)
arm_data=bpy.data.armatures.new('MantisSkeleton_v5'); arm=bpy.data.objects.new('MantisRig_v5',arm_data); bpy.context.collection.objects.link(arm)
bpy.context.view_layer.objects.active=arm; model.select_set(False); arm.select_set(True); bpy.ops.object.mode_set(mode='EDIT')
for n,h,t,p in specs:
    b=arm_data.edit_bones.new(n); b.head=h*.1; b.tail=t*.1
    if p: b.parent=arm_data.edit_bones[p]
bpy.ops.object.mode_set(mode='OBJECT')
for v in model.data.vertices: v.co*=.1
mod=model.modifiers.new('Mantis_skin','ARMATURE'); mod.object=arm; model.parent=arm
bpy.context.view_layer.objects.active=model
tri=model.modifiers.new('Triangles','TRIANGULATE'); bpy.ops.object.modifier_move_up(modifier=tri.name); bpy.ops.object.modifier_apply(modifier=tri.name)
arm.show_in_front=True
scene=bpy.context.scene; scene.render.fps=30; scene.frame_start=1; scene.frame_end=42
scene.unit_settings.system='METRIC'

def local_world_rotation(pb,axis,angle):
    q=pb.bone.matrix_local.to_quaternion()
    pb.rotation_mode='QUATERNION'; pb.rotation_quaternion=q.inverted()@Quaternion(axis,angle)@q

def interp(frame,keys):
    if frame<=keys[0][0]: return keys[0][1]
    for (a,x),(b,y) in zip(keys,keys[1:]):
        if frame<=b:
            t=(frame-a)/(b-a); t=t*t*(3-2*t); return x+(y-x)*t
    return keys[-1][1]

def pose_legs():
    for upn,lon,h,k,f,pole,length in leg_specs:
        up=arm.pose.bones[upn]; lo=arm.pose.bones[lon]
        # Hip follows thorax; endpoint stays on the floor. Solve in armature space.
        parent=up.parent
        hip=(parent.matrix@parent.bone.matrix_local.inverted())@(h*.1)
        foot=f*.1; pole=pole*.1; axis=foot-hip; dist=axis.length; axis.normalize()
        L=length*.1
        if dist>2*L-.00001: foot=hip+axis*(2*L-.00001); dist=(foot-hip).length
        mid=(hip+foot)*.5; bend=pole-mid; bend-=axis*bend.dot(axis); bend.normalize()
        knee=mid+bend*math.sqrt(max(0,L*L-dist*dist/4))
        for pb,a,b in [(up,hip,knee),(lo,knee,foot)]:
            q=(b-a).to_track_quat('Y','Z'); pb.matrix=Matrix.Translation(a)@q.to_matrix().to_4x4()
            bpy.context.view_layer.update()

pose_stats=[]
for frame in range(1,43):
    scene.frame_set(frame)
    for pb in arm.pose.bones:
        pb.rotation_mode='QUATERNION'; pb.rotation_quaternion=(1,0,0,0); pb.location=(0,0,0); pb.scale=(1,1,1)
    lift=interp(frame,[(1,0),(5,.05),(13,1),(16,1),(18,.83),(22,.72),(34,0),(42,0)])
    extension=interp(frame,[(1,0),(13,0),(16,0),(18,1),(20,.94),(25,.18),(30,0),(42,0)])
    for bn,angle in [('abdomen_02',-.09),('abdomen_01',-.12),('abdomen_00',-.15),('thorax',-.40)]:
        local_world_rotation(arm.pose.bones[bn],(1,0,0),angle*lift)
    for label in ['L','R']:
        local_world_rotation(arm.pose.bones['merus_'+label],(1,0,0),-.035*lift+.10*extension)
        # Same strike endpoint, opposite path: rotate under the elbow, not above it.
        # Adjacent baked keys differ by < pi so quaternion interpolation keeps this arc.
        local_world_rotation(arm.pose.bones['propodus_'+label],(1,0,0),(2.75-2*math.pi)*extension-.075*lift)
        # Near-parallel rest arms need no compensating lateral yaw.
        local_world_rotation(arm.pose.bones['dactyl_'+label],(1,0,0),-.14*extension)
        local_world_rotation(arm.pose.bones['eye_'+label],(1,0,0),.43*lift)
    bpy.context.view_layer.update(); pose_legs(); bpy.context.view_layer.update()
    for pb in arm.pose.bones:
        pb.keyframe_insert('location',frame=frame,group=pb.name)
        pb.keyframe_insert('rotation_quaternion',frame=frame,group=pb.name)
        pb.keyframe_insert('scale',frame=frame,group=pb.name)
    pose_stats.append({'frame':frame,'head_z':arm.pose.bones['eye_L'].head.z,'club_y':arm.pose.bones['dactyl_L'].head.y,'club_z':arm.pose.bones['dactyl_L'].head.z,'tail':list(arm.pose.bones['tail'].head),'feet':[list(arm.pose.bones[lo].tail) for _,lo,*_ in leg_specs]})
arm.animation_data.action.name='Underhand_RearUp_Punch'
for label,f in [('FOLDED_IDLE',1),('BODY_RAISED',13),('WINDUP',16),('LOWER_ARC',17),('IMPACT',18),('RECOVER',30)]: scene.timeline_markers.new(label,frame=f)
scene.frame_set(1)
bpy.ops.object.select_all(action='DESELECT'); model.select_set(True); arm.select_set(True); bpy.context.view_layer.objects.active=arm
bpy.ops.export_scene.fbx(filepath=os.path.join(OUT,'mantis_shrimp_v5.fbx'),use_selection=True,object_types={'MESH','ARMATURE'},add_leaf_bones=False,axis_forward='-Z',axis_up='Y',bake_anim=True,bake_anim_use_all_actions=False,bake_anim_use_nla_strips=False,bake_anim_simplify_factor=0,path_mode='COPY',embed_textures=True)

studio=bpy.data.collections.new('Preview_Studio'); scene.collection.children.link(studio)
def studio_obj(ob):
    for c in list(ob.users_collection): c.objects.unlink(ob)
    studio.objects.link(ob)
def aim(ob,p): ob.rotation_euler=(Vector(p)-ob.location).to_track_quat('-Z','Y').to_euler()
bpy.ops.mesh.primitive_plane_add(size=200,location=(0,0,.002)); floor=bpy.context.object; floor.name='StudioFloor'; studio_obj(floor)
fm=bpy.data.materials.new('Slate_stage'); fm.use_nodes=True; fm.node_tree.nodes.get('Principled BSDF').inputs['Base Color'].default_value=(.055,.074,.080,1); fm.node_tree.nodes.get('Principled BSDF').inputs['Roughness'].default_value=.85; floor.data.materials.append(fm)
for name,loc,power,size,color in [('Key',(.4,-.8,1.4),32,.9,(1,.90,.76)),('Fill',(-.8,-.3,.6),11,1,(.60,.80,1)),('Rim',(.2,1,.9),38,.8,(.69,.88,1))]:
    data=bpy.data.lights.new(name,'AREA'); ob=bpy.data.objects.new(name,data); studio.objects.link(ob); ob.location=loc; data.energy=power; data.size=size; data.color=color; aim(ob,(0,0,.11))
world=scene.world; world.use_nodes=True; world.node_tree.nodes['Background'].inputs[0].default_value=(.19,.24,.26,1); world.node_tree.nodes['Background'].inputs[1].default_value=.35
data=bpy.data.cameras.new('PreviewCamera'); camera=bpy.data.objects.new('PreviewCamera',data); studio.objects.link(camera); scene.camera=camera
camera.location=(.83,-1.13,.72); aim(camera,(0,.025,.14)); data.type='ORTHO'; data.ortho_scale=.84
scene.render.engine='CYCLES'; scene.cycles.samples=24; scene.cycles.use_denoising=True
scene.render.resolution_x=1120; scene.render.resolution_y=880; scene.render.resolution_percentage=100
scene.view_settings.view_transform='AgX'
for screen in bpy.data.screens:
    for area in screen.areas:
        if area.type=='VIEW_3D':
            space=area.spaces.active; space.region_3d.view_distance=.9; space.region_3d.view_location=(0,0,.12); space.region_3d.view_rotation=camera.rotation_euler.to_quaternion(); space.clip_start=.001; space.shading.type='MATERIAL'
for ob in studio.objects: ob.hide_set(True)
arm.hide_set(True); bpy.ops.object.select_all(action='DESELECT'); model.select_set(True); bpy.context.view_layer.objects.active=model
bpy.ops.wm.save_as_mainfile(filepath=os.path.join(OUT,'mantis_shrimp_v5.blend'))
for frame,name in [(1,'preview_idle'),(13,'preview_raised'),(18,'preview_strike')]:
    scene.frame_set(frame); scene.render.filepath=os.path.join(OUT,name+'.png'); bpy.ops.render.render(write_still=True)
camera.location=(1.2,-.15,.43); aim(camera,(0,-.015,.14)); data.ortho_scale=.79
for frame,name in [(1,'side_folded'),(13,'side_raised'),(17,'side_lower_arc'),(18,'side_strike')]:
    scene.frame_set(frame); scene.render.filepath=os.path.join(OUT,name+'.png'); bpy.ops.render.render(write_still=True)
camera.location=(0,-1.2,.39); aim(camera,(0,-.10,.14)); data.ortho_scale=.59
for frame,name in [(1,'front_folded'),(13,'front_raised'),(18,'front_strike')]:
    scene.frame_set(frame); scene.render.filepath=os.path.join(OUT,name+'.png'); bpy.ops.render.render(write_still=True)
scene.frame_set(1); bpy.context.view_layer.update()
with open(os.path.join(OUT,'stats.json'),'w') as f:
    json.dump({'triangles':len(model.data.polygons),'vertices':len(model.data.vertices),'bones':len(arm.data.bones),'materials':len(model.data.materials),'texture':'1024x1024 albedo','animation':'Underhand_RearUp_Punch, 1-42 frames at 30fps','head_width_change':1.30,'abdomen_width_and_length_change':.82,'max_spine_pitch_degrees':math.degrees(.76),'poses':pose_stats},f,indent=2)
print('V5_COMPLETE',len(model.data.polygons),'triangles',len(arm.data.bones),'bones')

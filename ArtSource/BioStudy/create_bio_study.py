import bpy, math, os, json, bmesh
from mathutils import Vector
OUT=os.path.dirname(os.path.abspath(__file__))
base=os.path.join(os.path.dirname(OUT),'A6')
src=open(os.path.join(base,'source_base_v5.py'),encoding='utf-8-sig').read().split('# Bake construction coordinates')[0]
src=src.replace("plate('Head_carapace',-1.02,.78,1.53,.49,1.13","plate('Head_carapace',-1.02,.65,1.42,.40,1.13")
src=src.replace('return Vector((p.x*.82,-.29+(p.y+.29)*.82,1+(p.z-1)*.92))','return p.copy()')
src=src.replace('s*(1.06+i*.04)','s*(.90+i*.025)').replace('s*1.42,y+.39,.69','s*1.10,y+.25,.59').replace('length=.85;','length=.66;')
src=src.replace('elbow=Vector((s*.40,-1.89,.50)); wrist=Vector((s*.48,-1.40,.60))','elbow=Vector((s*.40,-1.89,.76)); wrist=Vector((s*.43,-1.12,.39))')
src=src.replace('elbow+Vector((s*.09,0,0)),wrist,.13,.12,11','elbow,wrist,.17,.12,11')
src=src.replace('wrist+Vector((0,-.14,-.22)),wrist+Vector((0,.19,.12)),.235,.21,2','wrist+Vector((0,-.56,.08)),wrist+Vector((0,.055,-.035)),.155,.12,1')
src=src.replace('wrist+Vector((0,.19,.12)),(.19,.15,.16)','wrist+Vector((0,.055,-.035)),(.165,.13,.13)')
src=src.replace('wrist+Vector((-s*.08,.05,.02)),wrist+Vector((-s*.09,-.34,-.23))','wrist+Vector((-s*.08,.01,-.025)),wrist+Vector((-s*.09,-.62,.18))')
src=src.replace('segments=12,rings=6','segments=32,rings=16').replace('5,eye,14,8,True','5,eye,40,24,True').replace('sides=8','sides=16').replace('-3.01+j*.09','-2.76+j*.09')
exec(compile(src,'biostudy_base','exec'))
# Additional rounded articulations and feeding appendages, in original construction units.
for up,lo,hip,knee,foot,pole,length in leg_specs:
 ell('Articular_knee',knee,(.055,.066,.058),3,lo,20,12,True)
 ell('Articular_hip',hip,(.081,.078,.073),3,up,20,12,True)
for side in [-1,1]:
 for i in range(3):
  a=Vector((side*(.10+i*.08),-1.72,1.02-i*.07));b=Vector((side*(.27+i*.08),-1.97,.88-i*.10));c=Vector((side*(.12+i*.04),-2.07,.73-i*.07))
  pod('Feeding_appendage',a,b,.045,.04,4,'thorax',.7);pod('Feeding_tip',b,c,.035,.029,10,'thorax',.2)
# Tiny sensory setae and annular joint ridges on each walking limb.
for up,lo,hip,knee,foot,pole,length in leg_specs:
 direction=(foot-knee).normalized();side=Vector((1 if foot.x>0 else -1,0,0))
 for j in range(3):
  t=.15+j*.12;center=knee.lerp(foot,t);radius=.068*(1-t)+.018*t
  bpy.ops.mesh.primitive_torus_add(major_radius=radius*.98,minor_radius=.007,major_segments=16,minor_segments=6,location=center)
  ob=bpy.context.object;ob.rotation_euler=direction.to_track_quat('Z','Y').to_euler();finish(ob,'Leg_annulus',10,lo,smooth=True)
 for j in range(9):
  t=.2+j*.072;a=knee.lerp(foot,t)+side*(.06*(1-t));b=a+side*(.045*(1-t))+Vector((0,.02,-.015));rod('Sensory_seta',a,b,.003,.0005,4,lo,5)
# Actual editable materials: colored chitin, fine pores, wetter joints, compound-eye band.
def organic(name,low,high,rough=.48,scale=7,bump=.055):
 m=bpy.data.materials.new(name);m.use_nodes=True;n=m.node_tree.nodes;l=m.node_tree.links;p=n.get('Principled BSDF');p.inputs['Roughness'].default_value=rough;p.inputs['Coat Weight'].default_value=.08;p.inputs['Coat Roughness'].default_value=.25
 tc=n.new('ShaderNodeTexCoord');noise=n.new('ShaderNodeTexNoise');noise.inputs['Scale'].default_value=scale;noise.inputs['Detail'].default_value=3;l.new(tc.outputs['Generated'],noise.inputs['Vector'])
 ramp=n.new('ShaderNodeValToRGB');ramp.color_ramp.elements[0].position=.24;ramp.color_ramp.elements[0].color=(*low,1);ramp.color_ramp.elements[1].position=.8;ramp.color_ramp.elements[1].color=(*high,1);l.new(noise.outputs['Fac'],ramp.inputs[0]);l.new(ramp.outputs[0],p.inputs['Base Color'])
 fine=n.new('ShaderNodeTexNoise');fine.inputs['Scale'].default_value=155;fine.inputs['Detail'].default_value=2;l.new(tc.outputs['Generated'],fine.inputs['Vector']);b=n.new('ShaderNodeBump');b.inputs['Strength'].default_value=bump;b.inputs['Distance'].default_value=.015;l.new(fine.outputs['Fac'],b.inputs['Height']);l.new(b.outputs[0],p.inputs['Normal']);return m
shell=organic('01 - blue olive chitin',(.012,.043,.032),(.075,.18,.095))
abd=organic('02 - abdominal chitin',(.022,.05,.023),(.13,.19,.063),.5)
rim=organic('03 - warm shell margins',(.23,.125,.047),(.53,.39,.16),.4)
soft=organic('04 - burgundy arthrodial membrane',(.075,.018,.014),(.21,.066,.033),.46)
cream=organic('05 - pale ventral chitin',(.23,.19,.095),(.49,.45,.25),.38)
club=organic('06 - striking appendage',(.029,.11,.097),(.16,.33,.22),.3)
heel=organic('07 - impact surface',(.20,.10,.045),(.58,.31,.13),.29)
eye=organic('08 - compound cornea',(.075,.13,.065),(.26,.30,.12),.32,18,.025)
n=eye.node_tree.nodes;l=eye.node_tree.links;p=n.get('Principled BSDF');old=p.inputs['Base Color'].links[0].from_socket
coord=n.new('ShaderNodeTexCoord');sep=n.new('ShaderNodeSeparateXYZ');l.new(coord.outputs['Generated'],sep.inputs[0]);sub=n.new('ShaderNodeMath');sub.operation='SUBTRACT';sub.inputs[1].default_value=.5;l.new(sep.outputs['Z'],sub.inputs[0]);ab=n.new('ShaderNodeMath');ab.operation='ABSOLUTE';l.new(sub.outputs[0],ab.inputs[0]);less=n.new('ShaderNodeMath');less.operation='LESS_THAN';less.inputs[1].default_value=.044;l.new(ab.outputs[0],less.inputs[0]);mix=n.new('ShaderNodeMixRGB');mix.inputs[2].default_value=(.033,.06,.027,1);l.new(less.outputs[0],mix.inputs[0]);l.new(old,mix.inputs[1]);l.new(mix.outputs[0],p.inputs['Base Color'])
vor=n.new('ShaderNodeTexVoronoi');vor.feature='DISTANCE_TO_EDGE';vor.inputs['Scale'].default_value=65;l.new(coord.outputs['Generated'],vor.inputs['Vector']);b=n.new('ShaderNodeBump');b.inputs['Strength'].default_value=.13;b.inputs['Distance'].default_value=.006;l.new(vor.outputs['Distance'],b.inputs['Height']);l.new(b.outputs[0],p.inputs['Normal'])
for ob in parts:
 name=ob.name
 chosen=shell
 if name.startswith(('Abdominal','Pleural','Central_telson','Inner_uropod','Outer_uropod')):chosen=abd
 if name.startswith(('Carina','Tail_carina','Uropod_ridge','Telson_tooth','Eye_collar','Propodal_ridge','Foot_tip','Antennule','Leg_annulus')):chosen=rim
 if name.startswith(('Carpal','Articular','Thorax_under')):chosen=soft
 if name.startswith(('Walking_tibia','Sensory_seta','Swimmeret','Feeding','Mandible','Maxilliped')):chosen=cream
 if name.startswith(('Merus','Folded_propodus','Dactyl')):chosen=club
 if name.startswith('Impact_heel'):chosen=heel
 if name.startswith('Compound_eye'):chosen=eye
 uv=ob.data.uv_layers.active
 edgepolys=[]
 if name.startswith(('Head_carapace','Abdominal')) and uv:
  for poly in ob.data.polygons:
   if all(int(uv.data[i].uv.x*4)==2 and int(uv.data[i].uv.y*4)==2 for i in poly.loop_indices):edgepolys.append(poly.index)
 ob.data.materials.clear();ob.data.materials.append(chosen);ob.data.materials.append(rim)
 for poly in ob.data.polygons:poly.material_index=1 if poly.index in edgepolys else 0;poly.use_smooth=True
 bm=bmesh.new();bm.from_mesh(ob.data);bmesh.ops.recalc_face_normals(bm,faces=bm.faces);bm.to_mesh(ob.data);bm.free()
 if name.startswith(('Head_carapace','Abdominal','Merus','Folded','Dactyl','Walking_coxa','Feeding')):
  sub=ob.modifiers.new('Rounded biological shell','SUBSURF');sub.levels=1;sub.render_levels=2
 else:
  bev=ob.modifiers.new('Softened chitin edges','BEVEL');bev.width=.018;bev.segments=3
 # Maintain the source as individually editable pieces; preview is deliberately not exported to the game.
 ob.location*=.1;ob.scale*=.1
 ob['preview_only']=True
# Studio, deliberately neutral so the model can be assessed without combat effects.
scene=bpy.context.scene
bpy.ops.mesh.primitive_plane_add(size=200,location=(0,0,.003));floor=bpy.context.object;floor.name='Studio floor';fm=bpy.data.materials.new('Warm gray floor');fm.use_nodes=True;fp=fm.node_tree.nodes.get('Principled BSDF');fp.inputs['Base Color'].default_value=(.125,.145,.14,1);fp.inputs['Roughness'].default_value=.8;floor.data.materials.append(fm)
def aim(o,p):o.rotation_euler=(Vector(p)-o.location).to_track_quat('-Z','Y').to_euler()
for name,loc,power,size,col in [('Key',(.25,-.45,.9),14,.6,(.88,.95,1)),('Fill',(-.65,-.15,.45),6,.5,(.67,.86,1)),('Rim',(.1,.7,.65),13,.4,(1,.78,.45))]:
 d=bpy.data.lights.new(name,'AREA');o=bpy.data.objects.new(name,d);scene.collection.objects.link(o);o.location=loc;d.energy=power;d.shape='DISK';d.size=size;d.color=col;aim(o,(0,.015,.1))
scene.world.use_nodes=True;scene.world.node_tree.nodes['Background'].inputs[0].default_value=(.2,.24,.23,1);scene.world.node_tree.nodes['Background'].inputs[1].default_value=.18
camdata=bpy.data.cameras.new('Study camera');cam=bpy.data.objects.new('Study camera',camdata);scene.collection.objects.link(cam);scene.camera=cam;camdata.type='ORTHO';camdata.ortho_scale=.67
scene.render.engine='CYCLES';scene.cycles.samples=40;scene.cycles.use_denoising=True;scene.render.resolution_x=1500;scene.render.resolution_y=1100;scene.render.resolution_percentage=100;scene.view_settings.view_transform='AgX'
cam.location=(.65,-1,.61);aim(cam,(0,.015,.115));bpy.ops.wm.save_as_mainfile(filepath=os.path.join(OUT,'Mantis_BioStudy.blend'))
views=[('01_three_quarter',(.65,-1,.61),(0,.015,.115),.67),('02_side',(.95,-.13,.35),(0,.02,.10),.72),('03_front',(0,-1,.34),(0,-.105,.10),.40),('04_face_detail',(.35,-1,.42),(0,-.165,.123),.33)]
for name,loc,target,size in views:
 cam.location=loc;aim(cam,target);camdata.ortho_scale=size;scene.render.filepath=os.path.join(OUT,name+'.png');bpy.ops.render.render(write_still=True)
json.dump({'preview_only':True,'editable_mesh_pieces':len(parts),'render_samples':40,'notes':'Procedural Cycles study; no game changes, no mobile optimization or new rig validation. Natural-proportion A6 design refined with smooth shells, chitin materials, eye band and articulated mouthparts.'},open(os.path.join(OUT,'study.json'),'w'),indent=2)
print('BIO_STUDY_COMPLETE')


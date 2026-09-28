import bpy, os, math, bmesh
from mathutils import Vector
root=os.path.dirname(os.path.abspath(__file__))
bpy.ops.wm.open_mainfile(filepath=os.path.join(root,'Mantis_A6.blend'))
arm=bpy.data.objects['MantisRig_A6']; arm.hide_set(False); bpy.context.scene.frame_set(1)
head=bpy.data.objects['01_Cephalothorax']; head.hide_set(False)
gold=bpy.data.materials.new('CrownGold');gold.diffuse_color=(.85,.49,.065,1);gold.use_nodes=True
bs=gold.node_tree.nodes.get('Principled BSDF');bs.inputs['Base Color'].default_value=(.85,.49,.065,1);bs.inputs['Metallic'].default_value=.72;bs.inputs['Roughness'].default_value=.26
verts=[];faces=[];n=12
for rad,zbase,tip in [(.041,.147,False),(.046,.174,True),(.037,.174,True),(.035,.147,False)]:
 for i in range(n):
  a=2*math.pi*i/n;z=zbase+(.029 if tip and i%2==0 else 0)
  verts.append((rad*math.cos(a),-.093+rad*math.sin(a),z))
for ring in range(4):
 for i in range(n):
  j=(i+1)%n;nr=(ring+1)%4;faces.append((ring*n+i,ring*n+j,nr*n+j,nr*n+i))
mesh=bpy.data.meshes.new('Crown mesh');mesh.from_pydata(verts,[],faces);mesh.update()
bm=bmesh.new();bm.from_mesh(mesh);bmesh.ops.recalc_face_normals(bm,faces=bm.faces);bm.to_mesh(mesh);bm.free()
crown=bpy.data.objects.new('Crown',mesh);bpy.context.collection.objects.link(crown);crown.data.materials.append(gold);crown.vertex_groups.new(name='thorax').add(list(range(len(verts))),1,'REPLACE')
bpy.ops.object.select_all(action='DESELECT');head.select_set(True);crown.select_set(True);bpy.context.view_layer.objects.active=head;bpy.ops.object.join();head.name='Crown_Cephalothorax'
bpy.ops.object.select_all(action='DESELECT');head.select_set(True);arm.select_set(True);bpy.context.view_layer.objects.active=arm
bpy.ops.export_scene.fbx(filepath=os.path.join(root,'CrownHead.fbx'),use_selection=True,object_types={'MESH','ARMATURE'},add_leaf_bones=False,axis_forward='-Z',axis_up='Y',bake_anim=False,path_mode='AUTO')
bpy.ops.wm.save_as_mainfile(filepath=os.path.join(root,'CrownHead.blend'))
print('CROWN_HEAD_EXPORTED')

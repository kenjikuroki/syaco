import bpy,os,json
p='C:/Users/kurok/StudioProject/shakopunch/ArtSource/D_Character/D_Mantis_Base.glb'
bpy.ops.wm.read_factory_settings(use_empty=True);bpy.ops.import_scene.gltf(filepath=p)
print('OBJECTS',[(o.name,o.type,len(o.data.vertices) if o.type=='MESH' else 0) for o in bpy.context.scene.objects])

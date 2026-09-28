# Unicorn costume set

Reference: reference.png. Blender source: Unicorn_Set.blend; reproducible generator: create_unicorn.py. Exports: Unicorn_Set.fbx and Unicorn_Set.glb. Front, side, and three-quarter previews are included.

Six interchangeable rigged modules: cephalothorax, left punch arm, right punch arm, abdomen, tail, walking legs. Five simple materials (pearl, gold, pink, lilac, sky). 30,956 source triangles; 30,931 imported Unity triangles; zero unweighted vertices. Existing D character skeleton is retained. Hair locks, feathers, and wings follow their assigned bones; no cloth or hair simulation is included.

In game: Customize > Shop > Unicorn set > Get free > Equip. Equipment has no gameplay stats or hitbox effects. Different sets can be mixed across the six slots. Testing-period acquisition does not consume shells.

Validation: Unity player build succeeded. -unicornTest passed six-part replacement, bone mapping, punch deformation, portrait rotation, mixed-set exclusion, and original-mesh restoration. -freeEquipmentTest passed all four free items, repeat acquisition, unchanged shell balance, equip navigation, collection, and preview restoration. Device performance has not been profiled on iOS.

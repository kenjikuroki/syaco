# Mantis A6 v1 — six-slot model prototype

Current proportions use a narrower, lower cephalothorax and restore the abdomen/tail dimensions that were previously compacted to 82%. The head width parameter changed from .88 to .65, height from .55 to .40, and length from 1.53 to 1.42. Pale compound eyes, shortened walking legs, narrow striking clubs and the six-slot structure remain. The earlier large-head version is backed up under QA/BeforeNaturalProportions; this is a stylized proportion prototype, not a zoological reconstruction.

Files: Mantis_A6.blend (editable source), Mantis_A6.fbx (Unity), mantis_albedo.png, front.png, rear.png, punch.png.

4588 triangles, 35 bones, 1024px shared atlas, 42-frame punch animation.
Slots: cephalothorax, left punch arm, right punch arm, abdomen, tail, walking-leg set. Each slot is a separate skinned mesh on one shared rig.

Unity prefab: Assets/Mantis/Art/A6/Mantis_A6_Modular.prefab, used by both fighters in Duel.unity. MantisModularBody.Replace accepts donor meshes built against the current rest skeleton. No wardrobe UI or alternate equipment has been created yet.

September 23 appendage revision: the distal striking body folds beneath the merus, with a continuous narrow propodus/dactyl silhouette and a rounded impact heel. The baked strike rotates below the hinge and its recovery reverses that path. Unity no longer lifts the idle/attacking clubs toward the eyes through CCD. Bone names and six slots remain stable, but arm rest positions changed: future donor arms must use this revised skeleton. This is a simplified smasher morphology; the carpal linkage is represented visually rather than mechanically simulated.

Reference: McHenry et al. (2012), Gearing for speed slows the predatory strike of a mantis shrimp, Figure 1. https://www.ocf.berkeley.edu/~claverie/Thomas%20Claverie/McHenry%20et%20al.,%202012.pdf

Rebuild with Blender: run create_A6.py, then fix_feet.py to bake toe tracking through the punch. The latter corrects the previous model's foot-tip separation during lift.

Face orientation repair: `outward_normals.py` now runs during generation. `repair_normals.py` also repaired the existing six-part blend/FBX without changing vertex counts, skin group counts or animation. The initial repair flipped 2,500 inward-facing triangles; all 116 connected closed shells have nonnegative signed volume afterward. Details: `normals_verification.json`. Unity retains its opaque, back-face-culled material; no transparency or double-sided rendering workaround is used.

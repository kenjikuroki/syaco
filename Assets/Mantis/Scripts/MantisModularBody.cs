using System.Collections.Generic;
using UnityEngine;
namespace MantisPunch
{
    public enum MantisBodySlot { Cephalothorax, LeftArm, RightArm, Abdomen, Tail, WalkingLegs }
    public sealed class MantisModularBody : MonoBehaviour
    {
        public SkinnedMeshRenderer[] parts = new SkinnedMeshRenderer[6];
        // Donors must use this body's rest skeleton and vertex weights (D_v1 for the D model).
        public bool Replace(MantisBodySlot slot, SkinnedMeshRenderer donor)
        {
            int index = (int)slot;
            if (index < 0 || index >= parts.Length || !parts[index] || !donor || !donor.sharedMesh) return false;
            var lookup = new Dictionary<string, Transform>();
            foreach (var bone in GetComponentsInChildren<Transform>(true)) lookup[bone.name] = bone;
            var mapped = new Transform[donor.bones.Length];
            for (int i = 0; i < mapped.Length; i++)
                if (!donor.bones[i] || !lookup.TryGetValue(donor.bones[i].name, out mapped[i])) return false;
            Transform root = null;
            if (donor.rootBone && !lookup.TryGetValue(donor.rootBone.name, out root)) return false;
            var target = parts[index]; target.sharedMesh = donor.sharedMesh; target.bones = mapped;
            target.rootBone = root; target.sharedMaterials = donor.sharedMaterials; target.localBounds = donor.localBounds;
            return true;
        }
    }
}

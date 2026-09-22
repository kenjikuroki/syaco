using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace MantisPunch
{
    public sealed class MantisVisual : MonoBehaviour
    {
        public AnimationClip punchClip;
        public Animator animator;
        public Transform leftClub, rightClub;
        PlayableGraph graph;
        AnimationClipPlayable playable;
        readonly List<Leg> legs = new List<Leg>();
        struct Leg { public Transform upper, lower, foot; public Vector3 footLocal; public float phase; }
        Vector3 basePosition;
        public float LastSample { get; private set; }
        public bool Ready => graph.IsValid() && punchClip && leftClub && rightClub;
        public Vector3 StrikePoint => (leftClub.position + rightClub.position) * .5f;

        void Awake()
        {
            basePosition = transform.localPosition;
            graph = PlayableGraph.Create("Mantis manual pose");
            graph.SetTimeUpdateMode(DirectorUpdateMode.Manual);
            playable = AnimationClipPlayable.Create(graph, punchClip);
            playable.SetApplyFootIK(false); playable.SetApplyPlayableIK(false);
            playable.SetSpeed(0);
            var output = AnimationPlayableOutput.Create(graph, "Mantis", animator);
            output.SetSourcePlayable(playable);
            animator.applyRootMotion = false;
            animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
            graph.Play(); Sample(0, 0, 0);
            foreach (string side in new[] { "L", "R" })
                for (int i = 0; i < 3; i++)
                {
                    Transform upper = Find("leg_upper_" + side + "_" + i);
                    Transform lower = Find("leg_lower_" + side + "_" + i);
                    Transform foot = Find("foot_" + side + "_" + i);
                    if (upper && lower && foot) legs.Add(new Leg { upper = upper, lower = lower, foot = foot,
                        footLocal = lower.InverseTransformPoint(foot.position), phase = i * 2.1f + (side == "L" ? 0 : Mathf.PI) });
                }
        }

        Transform Find(string name)
        {
            foreach (Transform child in GetComponentsInChildren<Transform>()) if (child.name == name) return child;
            return null;
        }

        public void Sample(float normalizedTime, float movement, float gaitTime)
        {
            if (!graph.IsValid()) return;
            LastSample = Mathf.Clamp01(normalizedTime);
            playable.SetTime(LastSample * punchClip.length);
            playable.SetDone(false); graph.Evaluate(0);
            transform.localPosition = basePosition;
            if (normalizedTime > .001f || movement <= .01f) return;
            transform.localPosition += Vector3.up * (Mathf.Sin(gaitTime * 20) * .009f * movement);
            foreach (Leg leg in legs)
            {
                float swing = Mathf.Sin(gaitTime * 11 + leg.phase) * 8 * movement;
                leg.upper.localRotation *= Quaternion.Euler(swing, 0, 0);
                leg.lower.localRotation *= Quaternion.Euler(-swing * .6f, 0, 0);
                leg.foot.position = leg.lower.TransformPoint(leg.footLocal);
            }
        }
        void OnDestroy() { if (graph.IsValid()) graph.Destroy(); }
    }
}

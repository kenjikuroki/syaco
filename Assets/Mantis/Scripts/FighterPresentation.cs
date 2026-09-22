using UnityEngine;

namespace MantisPunch
{
    public enum CombatBeat { Guard, Parry, Deflected, Clash, Knockout, Swing, Land, Kick, Prepare, GuardBreak }
    public sealed class FighterPresentation : MonoBehaviour
    {
        DuelFighter fighter;
        MantisVisual visual;
        Transform[] upper = new Transform[2], lower = new Transform[2], club = new Transform[2];
        Transform eyeL, eyeR;
        Transform thorax;
        Vector3 sweepDirection, contactPoint;
        int deflectedArm;
        float ChestRecoil => Mathf.SmoothStep(0, 1, Mathf.InverseLerp(.025f, .11f, GestureAge)) * (1 - Mathf.SmoothStep(0, 1, Mathf.InverseLerp(.55f, 1.05f, GestureAge)));
        public float DeflectionAmount => reaction == CombatBeat.Deflected ? GestureAmount : 0;
        float GestureAge => reactionAge;
        float GestureAmount => Mathf.SmoothStep(0, 1, Mathf.InverseLerp(0, .075f, GestureAge)) * (1 - Mathf.SmoothStep(0, 1, Mathf.InverseLerp(.55f, 1.15f, GestureAge)));
        Quaternion originalRotation;
        Vector3 originalPosition, recoilDirection, launchDirection;
        float recoilLeft, recoilStrength, reactionAge = 10, deathAge = -1, oldAttackAge;
        CombatBeat reaction;
        bool landed, swung, kicked;
        bool counterPunch;
        public Vector3 VisiblePosition => visual ? visual.transform.position : transform.position;
        public float DeathAge => deathAge;
        public void Initialize(DuelFighter owner)
        {
            fighter = owner; visual = owner.visual;
            originalPosition = visual.transform.localPosition; originalRotation = visual.transform.localRotation;
            Transform Find(string name) { foreach (var t in visual.GetComponentsInChildren<Transform>()) if (t.name == name) return t; return null; }
            for (int i = 0; i < 2; i++) { string side = i == 0 ? "L" : "R"; upper[i] = Find("merus_" + side); lower[i] = Find("propodus_" + side); club[i] = Find("dactyl_" + side); }
            eyeL = Find("eye_L"); eyeR = Find("eye_R");
            thorax = Find("thorax");
        }
        public void ResetPose()
        {
            deathAge = -1; landed = swung = false; reactionAge = 10; recoilLeft = 0;
            counterPunch = false;
            visual.transform.localPosition = originalPosition; visual.transform.localRotation = originalRotation;
            visual.Sample(.18f, 0, 0);
        }
        public void ActionStarted(DuelAction action)
        {
            swung = kicked = false; oldAttackAge = 0;
            counterPunch = action == DuelAction.Attack && reaction == CombatBeat.Parry && reactionAge < 1.05f;
            if (reaction == CombatBeat.Parry) reactionAge = 10;
            if (action == DuelAction.Attack || action == DuelAction.Feint)
                CombatEffects.Current?.Play(CombatBeat.Prepare, (eyeL.position + eyeR.position) * .5f, transform.forward);
        }
        public void Recoil(Vector3 direction, float strength)
        { recoilDirection = direction.normalized; recoilStrength = strength; recoilLeft = .3f; }
        public void React(CombatBeat beat, Vector3 direction, Vector3? impactPoint = null)
        {
            reaction = beat; reactionAge = 0;
            if (beat == CombatBeat.Deflected) deflectedArm = counterPunch ? 1 : 0;
            Vector3 contact = impactPoint ?? (transform.position + Vector3.up * .85f + transform.forward * .55f);
            if (beat == CombatBeat.Parry || beat == CombatBeat.Deflected)
            { sweepDirection = direction.normalized; contactPoint = contact; }
            if (beat == CombatBeat.Knockout)
            { deathAge = 0; launchDirection = direction.normalized; landed = false; }
            else if (beat == CombatBeat.Guard || beat == CombatBeat.Clash) Recoil(direction, beat == CombatBeat.Guard ? .38f : .18f);
            CombatEffects.Current?.Play(beat, contact, direction);
        }
        public void ApplyPose()
        {
            if (!fighter || !fighter.Alive) return;
            float age = fighter.Age;
            float sample = visual.LastSample;
            if (fighter.State == DuelState.Guard) sample = .18f;
            if (fighter.State == DuelState.Attack && age < DuelFighter.Windup) sample = Mathf.Lerp(.18f, 15f / 41, age / DuelFighter.Windup);
            if (fighter.State == DuelState.Feint) sample = age < .14f ? Mathf.Lerp(.18f, 15f / 41, age / DuelFighter.Windup) : Mathf.Lerp(Mathf.Lerp(.18f, 15f / 41, .14f / DuelFighter.Windup), .18f, (age - .14f) / .29f);
            if (fighter.State == DuelState.Parry) sample = .28f;
            if (reaction == CombatBeat.Parry && GestureAge < .38f) sample = .28f;
            if (reaction == CombatBeat.Deflected && GestureAmount > 0) sample = .30f;
            if (fighter.State == DuelState.Attack && age >= DuelFighter.Impact && age < DuelFighter.Impact + .055f) sample = 17f / 41;
            visual.Sample(sample, 0, 0);
            if (reaction == CombatBeat.Deflected && thorax && GestureAmount > 0)
            {
                float sign = Mathf.Sign(Vector3.Dot(sweepDirection, transform.right));
                float recoil = ChestRecoil;
                thorax.rotation = Quaternion.AngleAxis(-28 * recoil, transform.right)
                    * Quaternion.AngleAxis(6 * recoil * sign, Vector3.up)
                    * Quaternion.AngleAxis(-7 * recoil * sign, transform.forward) * thorax.rotation;
            }
            Vector3 face = (eyeL.position + eyeR.position) * .5f;
            if (thorax && reaction == CombatBeat.Parry && GestureAge < .55f)
                thorax.rotation = Quaternion.AngleAxis(-8 * Mathf.SmoothStep(0, 1, GestureAge / .075f), Vector3.up) * thorax.rotation;
            for (int i = 0; i < 2; i++)
            {
                float side = i == 0 ? -1 : 1;
                Vector3 target = face - Vector3.up * .12f + transform.forward * .1f + transform.right * side * .13f;
                float weight = fighter.State == DuelState.Guard ? 1 : 0;
                if (fighter.State == DuelState.Attack && age < .18f) weight = 1 - age / .18f;
                if (fighter.State == DuelState.Feint) weight = age < .18f ? 1 - age / .18f : Mathf.InverseLerp(.24f, .43f, age);
                if (fighter.State == DuelState.Attack && age >= DuelFighter.Windup)
                {
                    float extend = Mathf.InverseLerp(DuelFighter.Windup, DuelFighter.Impact, age) * (1 - Mathf.InverseLerp(.31f, .48f, age));
                    target = face + transform.forward * .58f - Vector3.up * .2f + transform.right * side * .12f;
                    weight = extend;
                }
                if (fighter.State == DuelState.Parry)
                {
                    float sweep = Mathf.Sin(Mathf.Clamp01(age / DuelFighter.ParryWindow) * Mathf.PI);
                    if (i == 0) target += transform.right * .16f * sweep + transform.forward * .10f;
                    weight = 1;
                }
                if (reaction == CombatBeat.Parry && GestureAge < .38f)
                {
                    float sweep = Mathf.SmoothStep(0, 1, Mathf.InverseLerp(0, .075f, GestureAge));
                    target = i == 0
                        ? face - Vector3.up * .14f + transform.forward * .24f + sweepDirection * (.22f * sweep - .08f)
                        : face - Vector3.up * .10f + transform.right * .13f + transform.forward * .02f;
                    weight = 1;
                }
                if (counterPunch && fighter.State == DuelState.Attack)
                {
                    float drive = Mathf.SmoothStep(0, 1, Mathf.InverseLerp(DuelFighter.Windup, DuelFighter.Impact, age));
                    float retract = 1 - Mathf.InverseLerp(DuelFighter.Impact + .055f, DuelFighter.AttackEnd, age);
                    Vector3 chamber = face - Vector3.up * .10f + transform.right * side * .13f + transform.forward * .02f;
                    target = i == 0 ? face - Vector3.up * .14f + transform.forward * .20f + transform.right * .10f
                        : Vector3.Lerp(chamber, face + transform.forward * .65f - Vector3.up * .18f + transform.right * .10f, drive * retract);
                    weight = 1;
                }
                if (fighter.State == DuelState.Recovery || fighter.State == DuelState.Stunned)
                { target = face + transform.right * side * .52f - Vector3.up * .4f; weight = .8f; }
                if (reaction == CombatBeat.Deflected && GestureAmount > 0)
                {
                    Vector3 thrown = face + Vector3.up * .48f + transform.right * (deflectedArm == 0 ? -.32f : .32f) - transform.forward * .06f;
                    target = i == deflectedArm ? Vector3.Lerp(contactPoint, thrown, Mathf.SmoothStep(0, 1, GestureAge / .055f))
                        : face - Vector3.up * .32f + transform.right * side * .18f + transform.forward * .06f;
                    weight = GestureAmount;
                }
                if (weight > 0) AimArm(i, target, weight);
            }
            if (fighter.State == DuelState.Attack && age >= DuelFighter.Windup && !swung)
            { swung = true; CombatEffects.Current?.Play(CombatBeat.Swing, visual.StrikePoint, transform.forward); }
            if (fighter.State == DuelState.Attack && age >= .14f && !kicked)
            { kicked = true; CombatEffects.Current?.Play(CombatBeat.Kick, transform.position + Vector3.up * .06f - transform.forward * .5f, -transform.forward); }
            oldAttackAge = age;
        }
        void AimArm(int i, Vector3 target, float weight)
        {
            // Two-joint CCD keeps the original elbow and folded-arm proportions.
            for (int pass = 0; pass < 3; pass++)
            {
                foreach (Transform joint in new[] { lower[i], upper[i] })
                {
                    Quaternion turn = Quaternion.FromToRotation(club[i].position - joint.position, target - joint.position);
                    joint.rotation = Quaternion.Slerp(Quaternion.identity, turn, weight * .55f) * joint.rotation;
                }
            }
        }
        void LateUpdate()
        {
            if (!fighter) return;
            float dt = Time.deltaTime * (CombatEffects.Current ? CombatEffects.Current.CombatSpeed : 1);
            reactionAge += dt;
            if (deathAge >= 0)
            {
                deathAge += dt;
                float t = Mathf.Max(0, deathAge - .11f), flight = Mathf.Clamp01(t / 1.1f);
                Vector3 worldOffset = launchDirection * (6 * (1 - Mathf.Pow(1 - flight, 2))) + Vector3.up * (Mathf.Sin(flight * Mathf.PI) * 2.2f);
                visual.transform.localPosition = originalPosition + transform.InverseTransformDirection(worldOffset);
                visual.transform.localRotation = originalRotation * Quaternion.Euler(flight * 490, 0, flight * 85);
                if (flight >= 1)
                {
                    float settle = Mathf.Clamp01((t - 1.1f) / .25f);
                    visual.transform.localRotation = Quaternion.Slerp(originalRotation * Quaternion.Euler(490, 0, 85), originalRotation * Quaternion.Euler(0, 0, 90), settle);
                    visual.transform.localPosition += Vector3.up * (.18f * settle);
                }
                if (flight >= 1 && !landed) { landed = true; CombatEffects.Current?.Play(CombatBeat.Land, visual.transform.position, Vector3.up); }
                return;
            }
            ApplyPose();
            float kick = recoilLeft > 0 ? Mathf.Sin((1 - recoilLeft / .3f) * Mathf.PI) : 0;
            recoilLeft = Mathf.Max(0, recoilLeft - dt);
            visual.transform.localPosition += transform.InverseTransformDirection(recoilDirection * recoilStrength * kick);
            float lean = reaction == CombatBeat.Guard && reactionAge < .3f ? Mathf.Sin(reactionAge / .3f * Mathf.PI) * 22 : 0;
            if (fighter.State == DuelState.Attack)
            {
                float drive = Mathf.InverseLerp(.14f, DuelFighter.Impact, fighter.Age) * (1 - Mathf.InverseLerp(.31f, .5f, fighter.Age));
                visual.transform.localPosition += transform.InverseTransformDirection(transform.forward * .18f * drive);
            }
            float sway = fighter.State == DuelState.Stunned ? Mathf.Sin(fighter.Age * 13) * 9 : 0;
            visual.transform.localRotation = originalRotation * Quaternion.Euler(-lean, 0, sway);
        }
    }
}


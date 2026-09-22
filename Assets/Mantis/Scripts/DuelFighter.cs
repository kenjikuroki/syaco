using UnityEngine;

namespace MantisPunch
{
    public enum DuelAction { None, Attack, Feint, Parry }
    public enum DuelState { Guard, Attack, Feint, Parry, Recovery, Stunned, Dead }

    // Both fighters are stepped and resolved together by MantisDuel, avoiding Update-order priority.
    public sealed class DuelFighter : MonoBehaviour
    {
        public MantisVisual visual;
        public FighterPresentation presentation;
        public DuelState State { get; private set; }
        public DuelAction LastAction { get; private set; }
        public float Guard { get; private set; } = 100;
        public float Age { get; private set; }
        public float TimeSinceGuard { get; private set; } = 10;
        public float GuardBreakAt { get; private set; } = -100;
        public int Actions { get; private set; }
        public bool PendingStrike { get; private set; }
        public bool StrikeSpent { get; private set; }
        public float range = 1.72f;
        public const float Windup = .19f, Impact = .24f, AttackRecovery = .20f, AttackEnd = Impact + AttackRecovery;
        public const float AttackCost = 10;
        bool attackWasBlocked;
        public const float LungeDistance = .80f, LungeStart = .08f;
        public float EffectiveReach => range + LungeDistance;
        Vector3 lungeDirection;
        public const float ClashWindow = .065f;
        public const float ParryWindow = .45f, ParryRecovery = .40f;
        public bool Vulnerable => State == DuelState.Feint || State == DuelState.Recovery || State == DuelState.Stunned || (State == DuelState.Attack && Age >= Impact && !attackWasBlocked);
        public bool ParryActive => State == DuelState.Parry && Age <= ParryWindow;
        public bool Alive => State != DuelState.Dead;
        public bool Available => State == DuelState.Guard;
        public string Feedback { get; private set; } = "";
        public float FeedbackLeft { get; private set; }
        CharacterController body;
        Vector3 velocity;
        float recoveryTime, gait;
        public void Initialize()
        {
            body = GetComponent<CharacterController>();
            GetComponent<MantisPlayer>().enabled = false;
            visual = GetComponent<MantisPlayer>().visual;
            presentation = GetComponent<FighterPresentation>();
            if (!presentation) presentation = gameObject.AddComponent<FighterPresentation>();
            presentation.Initialize(this);
        }
        public void ResetRound(Vector3 position, Quaternion rotation)
        {
            if (!body) Initialize();
            body.enabled = false; transform.SetPositionAndRotation(position, rotation); body.enabled = true;
            State = DuelState.Guard; Age = 0; Guard = 100; TimeSinceGuard = 10;
            velocity = Vector3.zero; PendingStrike = StrikeSpent = false; LastAction = DuelAction.None;
            attackWasBlocked = false;
            Feedback = ""; FeedbackLeft = 0;
            GuardBreakAt = -100;
            if (presentation) presentation.ResetPose();
        }
        public bool Begin(DuelAction action)
        {
            if (!Available || action == DuelAction.None) return false;
            if (action == DuelAction.Attack)
            {
                if (Guard < AttackCost) { Show("LOW GUARD"); return false; }
                Guard = Mathf.Max(0, Guard - AttackCost); TimeSinceGuard = 0;
                CombatEffects.Current?.CounterStarted(this);
            }
            attackWasBlocked = false;
            State = action == DuelAction.Attack ? DuelState.Attack : action == DuelAction.Feint ? DuelState.Feint : DuelState.Parry;
            LastAction = action; Actions++; Age = 0; PendingStrike = StrikeSpent = false;
            lungeDirection = transform.forward;
            if (presentation) presentation.ActionStarted(action);
            return true;
        }
        public void Step(float dt, Vector2 move, DuelFighter other)
        {
            FeedbackLeft = Mathf.Max(0, FeedbackLeft - dt); TimeSinceGuard += dt;
            if (!Alive) return;
            float previous = Age; Age += dt; gait += dt;
            if (State != DuelState.Stunned && TimeSinceGuard > 2) Guard = Mathf.Min(100, Guard + dt * (100f / 9));
            if (State == DuelState.Attack && previous < Impact && Age >= Impact && !StrikeSpent) PendingStrike = true;
            if (State == DuelState.Attack && Age >= AttackEnd && !PendingStrike) ToGuard();
            else if (State == DuelState.Feint && Age >= .43f) ToGuard();
            else if (State == DuelState.Parry && Age > ParryWindow) Recover(ParryRecovery, "PARRY EXPIRED");
            else if (State == DuelState.Recovery && Age >= recoveryTime) ToGuard();
            else if (State == DuelState.Stunned && Age >= 1.2f) { Guard = 100; TimeSinceGuard = 0; ToGuard(); }
            Vector3 desired = new Vector3(move.x, 0, move.y);
            desired = Vector3.ClampMagnitude(desired, 1);
            if (State == DuelState.Guard || ((State == DuelState.Attack || State == DuelState.Feint) && Age < .12f))
            {
                Vector3 facing = other.transform.position - transform.position; facing.y = 0;
                if (facing.sqrMagnitude > .001f) transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(facing), dt * 720);
            }
            float speed = State == DuelState.Guard ? 1.65f : State == DuelState.Feint ? .25f : 0;
            velocity = Vector3.MoveTowards(velocity, desired * speed, dt * 16);
            body.Move((velocity + Vector3.down * 3) * dt);
            if (State == DuelState.Attack)
            {
                float before = Mathf.SmoothStep(0, 1, Mathf.InverseLerp(LungeStart, Impact, previous));
                float after = Mathf.SmoothStep(0, 1, Mathf.InverseLerp(LungeStart, Impact, Age));
                float room = Mathf.Max(0, Vector3.Distance(transform.position, other.transform.position) - 1.55f);
                body.Move(lungeDirection * Mathf.Min(room, LungeDistance * (after - before)));
            }
            Pose();
        }
        void Pose()
        {
            float pose = 0;
            if (State == DuelState.Attack)
                pose = Age < Windup ? Mathf.Lerp(0, 15f / 41, Age / Windup) : Age < Impact ? Mathf.Lerp(15f / 41, 17f / 41, (Age - Windup) / (Impact - Windup)) : Mathf.Lerp(17f / 41, 1, (Age - Impact) / (AttackEnd - Impact));
            if (State == DuelState.Feint) pose = Age < .20f ? Mathf.Lerp(0, 15f / 41, Age / Windup) : Mathf.Lerp(15f / 41 * .20f / Windup, 0, (Age - .20f) / .23f);
            if (State == DuelState.Parry) pose = .24f;
            if (State == DuelState.Stunned || State == DuelState.Recovery) pose = .18f;
            visual.Sample(pose, velocity.magnitude / 1.65f, gait);
            if (presentation) presentation.ApplyPose();
        }
        void ToGuard() { State = DuelState.Guard; Age = 0; }
        public void Recover(float seconds, string message)
        { State = DuelState.Recovery; Age = 0; recoveryTime = seconds; PendingStrike = false; StrikeSpent = true; Show(message); }
        public void Show(string message) { Feedback = message; FeedbackLeft = .8f; }
        public bool InRange(DuelFighter other, float tolerance = 0)
        {
            Vector3 delta = other.transform.position - transform.position; delta.y = 0;
            return delta.magnitude <= range + tolerance && Vector3.Dot(transform.forward, delta.normalized) >= .72f;
        }
        public bool CanReach(DuelFighter other, float tolerance = 0)
        {
            float remaining = State == DuelState.Attack ? LungeDistance * (1 - Mathf.SmoothStep(0, 1, Mathf.InverseLerp(LungeStart, Impact, Age))) : LungeDistance;
            return InRange(other, remaining + tolerance);
        }
        public void SpendStrike() { PendingStrike = false; StrikeSpent = true; }
        public void Receive(DuelFighter attacker)
        {
            if (!Alive) return;
            if (ParryActive)
            {
                attacker.Recover(1.05f, "PARRIED!"); ToGuard(); Show("PARRY!");
                CombatEffects.Current?.FocusParry(this, attacker);
                Vector3 contact = (visual.StrikePoint + attacker.visual.StrikePoint) * .5f;
                presentation.React(CombatBeat.Parry, transform.right, contact);
                attacker.presentation.React(CombatBeat.Deflected, transform.right, contact);
                return;
            }
            if (Vulnerable)
            {
                State = DuelState.Dead; Show("ONE HIT!"); attacker.Show("IPPON!");
                presentation.React(CombatBeat.Knockout, attacker.transform.forward); return;
            }
            attacker.attackWasBlocked = true;
            Guard = Mathf.Max(0, Guard - 100f / 3); TimeSinceGuard = 0; Show("GUARD");
            body.Move(attacker.transform.forward * .08f);
            if (Guard < .01f)
            { Guard = 0; State = DuelState.Stunned; Age = 0; PendingStrike = false; GuardBreakAt = Time.unscaledTime; Show("GUARD BREAK!"); presentation.React(CombatBeat.GuardBreak, attacker.transform.forward); }
            else presentation.React(CombatBeat.Guard, attacker.transform.forward);
        }
        public static void Resolve(DuelFighter a, DuelFighter b)
        {
            // If a slow frame crosses both impacts, resolve the earlier attack first.
            if (a.PendingStrike && b.PendingStrike && b.Age > a.Age) { Resolve(b, a); return; }
            bool hitA = a.PendingStrike && a.InRange(b), hitB = b.PendingStrike && b.InRange(a);
            bool simultaneous = a.State == DuelState.Attack && b.State == DuelState.Attack && !a.StrikeSpent && !b.StrikeSpent && Mathf.Abs(a.Age - b.Age) <= ClashWindow;
            if ((hitA || hitB) && simultaneous)
            { a.SpendStrike(); b.SpendStrike(); a.Show("CLASH"); b.Show("CLASH"); a.presentation.React(CombatBeat.Clash, b.transform.forward); b.presentation.Recoil(a.transform.forward, .12f); return; }
            if (a.PendingStrike) { a.SpendStrike(); if (hitA) b.Receive(a); else a.Show("MISS"); }
            if (b.PendingStrike) { b.SpendStrike(); if (hitB && b.Alive) a.Receive(b); else if (!hitB) b.Show("MISS"); }
        }
    }
}


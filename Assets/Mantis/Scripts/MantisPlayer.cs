using Input = MantisPunch.PrototypeInput;
using System.Collections.Generic;
using UnityEngine;

namespace MantisPunch
{
    [RequireComponent(typeof(CharacterController))]
    public sealed class MantisPlayer : MonoBehaviour
    {
        public MantisVisual visual;
        public MantisPlayer opponent;
        public int health = 5;
        public bool duelMode;
        public bool Dodging => dodgeLeft > 0;
        public bool CanDodge => !Attacking && dodgeCooldown <= 0 && health > 0;
        public float DodgeCooldown => dodgeCooldown;
        float dodgeLeft, dodgeCooldown, stun;
        Vector3 dodgeDirection;
        public void Dodge(Vector2 input)
        {
            if (!CanDodge) return;
            dodgeDirection = input.sqrMagnitude > .01f ? new Vector3(input.x, 0, input.y).normalized : -transform.forward;
            dodgeLeft = .18f; dodgeCooldown = .8f;
        }
        public bool ReceivePunch(Vector3 direction)
        {
            if (health <= 0 || Dodging) return false;
            health--; stun = .22f; Attacking = buffered = false;
            controller.Move(direction * .28f);
            return true;
        }
        [Header("Feel: editable while playing")]
        [Range(.5f, 6)] public float moveSpeed = 2.4f;
        [Range(2, 30)] public float acceleration = 12;
        [Range(90, 1080)] public float turnSpeed = 540;
        [Range(.06f, .5f)] public float windup = .18f;
        [Range(.03f, .15f)] public float strikeTime = .06f;
        [Range(.12f, .8f)] public float recovery = .38f;
        [Range(0, 1)] public float attackMovement = .25f;
        [Range(.2f, 1)] public float hitRadius = .50f;
        public float hitPause = .045f;
        public readonly MantisControls controls = new MantisControls();
        [System.NonSerialized] public bool automation;
        [System.NonSerialized] public Vector2 automationMove;
        [System.NonSerialized] public bool automationAttack;
        public bool Attacking { get; private set; }
        public int AttackCount { get; private set; }
        public int TotalHits { get; private set; }
        public float LastHitTime { get; private set; } = -10;
        public float AttackProgress => Attacking ? clock / duration : 0;
        public Rect InputBlock { get; set; }
        public Vector3 HitCenter => visual.StrikePoint + transform.forward * .12f;
        public Vector3 Velocity => velocity;
        CharacterController controller;
        Vector3 velocity;
        float clock, duration, activeWindup, activeStrike, activeRecovery, pauseLeft, gait;
        bool impactDone, buffered;

        void Awake() { controller = GetComponent<CharacterController>(); }
        void Update()
        {
            Vector2 move;
            bool punch;
            if (automation) { move = automationMove; punch = automationAttack; automationAttack = false; }
            else { controls.Poll(InputBlock); move = controls.Move; punch = controls.ConsumeAttack(); }
            if (!automation && duelMode && (controls.ConsumeDodge() || UnityEngine.InputSystem.Keyboard.current?.leftShiftKey.wasPressedThisFrame == true)) Dodge(move);
            Tick(Time.deltaTime, move, punch);
            if (!automation && Input.GetKeyDown(KeyCode.R)) TrainingTarget.ResetAll();
        }

        public void Tick(float dt, Vector2 input, bool punch)
        {
            dodgeCooldown = Mathf.Max(0, dodgeCooldown - dt);
            if (duelMode && (health <= 0 || (opponent && opponent.health <= 0))) { visual.Sample(0, 0, gait); return; }
            if (stun > 0) { stun -= dt; visual.Sample(0, 0, gait); return; }
            if (dodgeLeft > 0) { dodgeLeft -= dt; controller.Move((dodgeDirection * 7 + Vector3.down * 3) * dt); visual.Sample(0, 0, gait); return; }
            if (punch)
            {
                if (!Attacking) BeginAttack();
                else if (clock > duration - .16f) buffered = true;
            }
            if (pauseLeft > 0) { pauseLeft -= dt; return; }
            Vector3 desired = new Vector3(input.x, 0, input.y);
            if (desired.sqrMagnitude > 1) desired.Normalize();
            Vector3 facing = opponent ? opponent.transform.position - transform.position : desired;
            facing.y = 0;
            if (facing.sqrMagnitude > .001f && (!Attacking || clock < activeWindup * .55f))
                transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(facing), turnSpeed * dt);
            float speed = moveSpeed * (Attacking ? attackMovement : 1);
            velocity = Vector3.MoveTowards(velocity, desired * speed, acceleration * dt);
            controller.Move((velocity + Vector3.down * 3) * dt);
            gait += dt;
            if (Attacking)
            {
                clock += dt;
                float sample;
                if (clock <= activeWindup) sample = Mathf.Lerp(0, 15f / 41, clock / activeWindup);
                else if (clock <= activeWindup + activeStrike) sample = Mathf.Lerp(15f / 41, 17f / 41, (clock - activeWindup) / activeStrike);
                else sample = Mathf.Lerp(17f / 41, 1, (clock - activeWindup - activeStrike) / activeRecovery);
                visual.Sample(sample, 0, gait);
                if (!impactDone && clock >= activeWindup + activeStrike)
                {
                    // Sample the impact pose explicitly even when a slow frame skips over it.
                    visual.Sample(17f / 41, 0, gait); Hit(); impactDone = true;
                }
                if (clock >= duration)
                {
                    Attacking = false; visual.Sample(0, velocity.magnitude / moveSpeed, gait);
                    if (buffered) { buffered = false; BeginAttack(); }
                }
            }
            else visual.Sample(0, velocity.magnitude / moveSpeed, gait);
        }

        void BeginAttack()
        {
            Attacking = true; AttackCount++; clock = 0; impactDone = false;
            activeWindup = Mathf.Max(.02f, windup); activeStrike = Mathf.Max(.02f, strikeTime); activeRecovery = Mathf.Max(.05f, recovery);
            duration = activeWindup + activeStrike + activeRecovery;
        }
        void Hit()
        {
            Physics.SyncTransforms();
            if (opponent && Vector3.Dot(transform.forward, (opponent.transform.position - transform.position).normalized) > .4f)
            {
                var collider = opponent.GetComponent<CharacterController>();
                if (Vector3.Distance(collider.ClosestPoint(HitCenter), HitCenter) <= hitRadius && opponent.ReceivePunch(transform.forward))
                { TotalHits++; LastHitTime = Time.time; pauseLeft = hitPause; }
            }
            var already = new HashSet<TrainingTarget>();
            foreach (Collider hit in Physics.OverlapSphere(HitCenter, hitRadius, ~0, QueryTriggerInteraction.Ignore))
            {
                TrainingTarget target = hit.GetComponentInParent<TrainingTarget>();
                if (!target || !already.Add(target)) continue;
                if (Vector3.Dot(transform.forward, (target.transform.position - transform.position).normalized) < .15f) continue;
                if (target.ReceiveHit(transform.forward)) { TotalHits++; LastHitTime = Time.time; pauseLeft = hitPause; }
            }
        }
        public void Teleport(Vector3 position, Quaternion rotation)
        {
            controller.enabled = false; transform.SetPositionAndRotation(position, rotation); controller.enabled = true;
            velocity = Vector3.zero; Attacking = buffered = false; clock = pauseLeft = 0;
            dodgeLeft = dodgeCooldown = stun = 0;
            visual.Sample(0, 0, gait); Physics.SyncTransforms();
        }
        void OnApplicationFocus(bool focus) { if (!focus) { controls.Reset(); velocity = Vector3.zero; } }
        void OnDrawGizmosSelected()
        {
            if (!visual || !visual.leftClub || !visual.rightClub) return;
            Gizmos.color = Color.yellow; Gizmos.DrawWireSphere(HitCenter, hitRadius);
        }
    }
}


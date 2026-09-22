using UnityEngine;

namespace MantisPunch
{
    public sealed class DuelBrain
    {
        enum Intent { Watch, Probe, Withdraw, Circle, Commit }
        Intent intent;
        float remaining, reaction, memoryAttack = 1, memoryParry = 1, memoryFeint = 1;
        int observedActions;
        float side = 1, parryCooldown, initiative;
        DuelAction observed;
        bool reacting;
        public string IntentName => intent.ToString();
        public void Reset() { intent = Intent.Watch; remaining = .7f; reacting = false; observedActions = 0; parryCooldown = 0; initiative = 0; memoryAttack = memoryParry = memoryFeint = 1; }
        public void Decide(float dt, DuelFighter self, DuelFighter opponent, out Vector2 movement, out DuelAction action)
        {
            movement = Vector2.zero; action = DuelAction.None;
            parryCooldown = Mathf.Max(0, parryCooldown - dt);
            if (self.Available) initiative += dt;
            Vector3 delta = opponent.transform.position - self.transform.position; delta.y = 0;
            float distance = delta.magnitude; Vector3 toward = delta.normalized, lateral = Vector3.Cross(toward, Vector3.up) * side;
            // Observe visible action starts only. No access to the player's input or queued command.
            if (opponent.Actions != observedActions)
            {
                observedActions = opponent.Actions; reaction = Random.Range(.19f, .34f); reacting = true;
                observed = opponent.LastAction;
            }
            if (reacting)
            {
                reaction -= dt;
                if (reaction <= 0)
                {
                    reacting = false;
                    if (observed == DuelAction.Attack) memoryAttack += .65f;
                    if (observed == DuelAction.Parry) memoryParry += .65f;
                    if (observed == DuelAction.Feint) memoryFeint += .65f;
                    // Attack and feint share an initial pose, so the CPU can be fooled.
                    if (self.Available && distance < self.EffectiveReach + .12f && (observed == DuelAction.Attack || observed == DuelAction.Feint))
                    {
                        float parryChance = Mathf.Clamp(.24f + (memoryAttack - memoryFeint) * .025f, .12f, .34f);
                        if (parryCooldown <= 0 && opponent.Age < DuelFighter.Impact && Random.value < parryChance)
                        { action = DuelAction.Parry; parryCooldown = Random.Range(1.8f, 3.2f); initiative = 0; }
                        else { intent = Intent.Withdraw; remaining = Random.Range(.3f, .6f); }
                    }
                }
            }
            memoryAttack = Mathf.Lerp(memoryAttack, 1, dt * .02f);
            memoryParry = Mathf.Lerp(memoryParry, 1, dt * .02f);
            memoryFeint = Mathf.Lerp(memoryFeint, 1, dt * .02f);
            if (!self.Available) return;
            remaining -= dt;
            if (remaining <= 0)
            {
                remaining = Random.Range(.35f, 1.1f);
                side = Random.value < .3f ? -side : side;
                float roll = Random.value;
                intent = roll < .20f ? Intent.Watch : roll < .42f ? Intent.Probe : roll < .60f ? Intent.Circle : roll < .72f ? Intent.Withdraw : Intent.Commit;
                if (self.Guard < 36) { intent = Intent.Withdraw; remaining = Random.Range(.65f, 1.2f); }
                if (opponent.Guard < 36 && Random.value < .65f) intent = Intent.Commit;
                if ((opponent.State == DuelState.Recovery || opponent.State == DuelState.Stunned) && !reacting && Random.value < .8f) intent = Intent.Commit;
            }
            if (initiative > 2.4f && self.Guard >= 36) intent = Intent.Commit;
            float target = self.EffectiveReach + .24f;
            if (intent == Intent.Probe) target = self.EffectiveReach - .06f;
            if (intent == Intent.Withdraw) target = self.EffectiveReach + .7f;
            if (intent == Intent.Commit) target = self.EffectiveReach - .18f;
            Vector3 move = Vector3.zero;
            if (distance > target + .08f) move += toward * .65f;
            if (distance < target - .08f) move -= toward * .8f;
            if (intent == Intent.Circle) move += lateral * .5f;
            if (intent == Intent.Watch && Mathf.Abs(distance - target) < .45f) move = Vector3.zero;
            if (intent == Intent.Probe && distance <= target + .06f) { intent = Intent.Withdraw; remaining = Random.Range(.25f, .65f); }
            if (intent == Intent.Commit && self.CanReach(opponent) && action == DuelAction.None)
            {
                float attackWeight = .85f + memoryFeint * .22f, feintWeight = .18f + memoryParry * .14f;
                float roll = Random.value * (attackWeight + feintWeight);
                action = roll < attackWeight ? DuelAction.Attack : DuelAction.Feint;
                if ((opponent.State == DuelState.Recovery || opponent.State == DuelState.Stunned) && !reacting) action = DuelAction.Attack;
                initiative = 0; intent = Intent.Withdraw; remaining = Random.Range(.45f, .9f); move = Vector3.zero;
            }
            // Keep away from walls rather than attempting to retreat through them.
            if (Mathf.Abs(self.transform.position.x) > 7.5f) move.x = -Mathf.Sign(self.transform.position.x) * .7f;
            if (Mathf.Abs(self.transform.position.z) > 7.5f) move.z = -Mathf.Sign(self.transform.position.z) * .7f;
            movement = Vector2.ClampMagnitude(new Vector2(move.x, move.z), 1);
        }
    }
}


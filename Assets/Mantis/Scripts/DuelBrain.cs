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
        public CpuPersonality Personality {get;private set;}=CpuPersonality.Balanced;
        public CpuPersonality Strategy {get;private set;}=CpuPersonality.Balanced;
        CpuStyle Style=>CpuStyle.For(Strategy);
        float strategyAge,passiveTime;int evidence;
        public void BeginMatch(CpuPersonality personality){Personality=personality;Strategy=personality==CpuPersonality.Adaptive?CpuPersonality.Balanced:personality;memoryAttack=memoryParry=memoryFeint=1;passiveTime=0;evidence=0;strategyAge=0;Reset();}
        public void ResetRound(int opponentActions){if(Personality==CpuPersonality.Adaptive&&evidence>=2)Strategy=CpuAdaptation.Choose(memoryAttack,memoryFeint,memoryParry,passiveTime);Reset();observedActions=opponentActions;strategyAge=0;}
        public string IntentName => intent.ToString();
        public void Reset() { intent = Intent.Watch; remaining = .7f; reacting = false; observedActions = 0; parryCooldown = 0; initiative = 0; }
        public void Decide(float dt, DuelFighter self, DuelFighter opponent, out Vector2 movement, out DuelAction action)
        {
            movement = Vector2.zero; action = DuelAction.None;
            strategyAge+=dt;
            if(opponent.State==DuelState.Guard&&Vector3.Distance(self.transform.position,opponent.transform.position)<self.EffectiveReach+.5f)passiveTime+=dt;
            else passiveTime=Mathf.Max(0,passiveTime-dt*.3f);
            if(Personality==CpuPersonality.Adaptive&&strategyAge>12&&evidence>=3){Strategy=CpuAdaptation.Choose(memoryAttack,memoryFeint,memoryParry,passiveTime);strategyAge=0;}
            CpuStyle style=Style;
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
                    reacting = false;evidence++;
                    if (observed == DuelAction.Attack) memoryAttack += .65f;
                    if (observed == DuelAction.Parry) memoryParry += .65f;
                    if (observed == DuelAction.Feint) memoryFeint += .65f;
                    // Attack and feint share an initial pose, so the CPU can be fooled.
                    if (self.Available && distance < self.EffectiveReach + .12f && (observed == DuelAction.Attack || observed == DuelAction.Feint))
                    {
                        float parryChance = Mathf.Clamp(style.parry + Mathf.Clamp(memoryAttack-memoryFeint,-3,3)*.015f, .10f, .62f);
                        if (parryCooldown <= 0 && opponent.Age < DuelFighter.Impact && Random.value < parryChance)
                        { action = DuelAction.Parry; parryCooldown = Strategy==CpuPersonality.Parry?Random.Range(1.0f,2.0f):Random.Range(1.8f, 3.2f); initiative = 0; }
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
                intent=roll<style.commit?Intent.Commit:roll<style.commit+style.retreat?Intent.Withdraw:roll<.80f?Intent.Probe:roll<.91f?Intent.Watch:Intent.Circle;
                if (self.Guard < (Strategy==CpuPersonality.Pressure?28:36)) { intent = Intent.Withdraw; remaining = Random.Range(.65f, 1.2f); }
                if (opponent.Guard < 36 && Random.value < (Strategy==CpuPersonality.Pressure?.9f:.65f)) intent = Intent.Commit;
                if ((opponent.State == DuelState.Recovery || opponent.State == DuelState.Stunned) && !reacting && Random.value < .8f) intent = Intent.Commit;
            }
            if (initiative > style.initiative && self.Guard >= 36) intent = Intent.Commit;
            if(Strategy==CpuPersonality.Pressure&&self.Guard>=DuelFighter.AttackCost+15&&opponent.TimeSinceGuard<1.1f&&opponent.State==DuelState.Guard&&initiative>.18f)intent=Intent.Commit;
            float target = self.EffectiveReach + style.spacing;
            if (intent == Intent.Probe) target = self.EffectiveReach - .06f;
            if (intent == Intent.Withdraw) target = self.EffectiveReach + .7f;
            if (intent == Intent.Commit) target = self.EffectiveReach - .18f;
            Vector3 move = Vector3.zero;
            if (distance > target + .08f) move += toward * .65f;
            if (distance < target - .08f) move -= toward * (Strategy==CpuPersonality.Retreat?1f:.8f);
            if (intent == Intent.Circle) move += lateral * .5f;
            if (intent == Intent.Watch && Mathf.Abs(distance - target) < .45f) move = Vector3.zero;
            if (intent == Intent.Probe && distance <= target + .06f) { intent = Intent.Withdraw; remaining = Random.Range(.25f, .65f); }
            if (intent == Intent.Commit && self.CanReach(opponent) && action == DuelAction.None)
            {
                float attackWeight=style.attack+Mathf.Min(memoryFeint,4)*.10f,feintWeight=style.feint+Mathf.Min(memoryParry,4)*.10f;
                if(Strategy==CpuPersonality.Pressure&&opponent.Guard<45)attackWeight*=1.7f;
                float roll = Random.value * (attackWeight + feintWeight);
                action = roll < attackWeight ? DuelAction.Attack : DuelAction.Feint;
                if ((opponent.State == DuelState.Recovery || opponent.State == DuelState.Stunned) && !reacting) action = DuelAction.Attack;
                initiative = 0; intent = action==DuelAction.Feint?Intent.Watch:Intent.Withdraw; remaining = action==DuelAction.Feint?.2f:Strategy==CpuPersonality.Aggressive?Random.Range(.25f,.5f):Strategy==CpuPersonality.Retreat?Random.Range(.65f,1.2f):Random.Range(.45f,.9f); move = Vector3.zero;
            }
            // Keep away from walls rather than attempting to retreat through them.
            if (Mathf.Abs(self.transform.position.x) > 7.5f) move.x = -Mathf.Sign(self.transform.position.x) * .7f;
            if (Mathf.Abs(self.transform.position.z) > 7.5f) move.z = -Mathf.Sign(self.transform.position.z) * .7f;
            movement = Vector2.ClampMagnitude(new Vector2(move.x, move.z), 1);
        }
    }
}


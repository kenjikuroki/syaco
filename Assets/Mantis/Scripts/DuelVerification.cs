using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
namespace MantisPunch
{
    public sealed class DuelVerification : MonoBehaviour
    {
        [Serializable] class Report { public bool passed; public List<string> checks = new List<string>(); public List<string> failures = new List<string>(); }
        Report report = new Report(); MantisDuel duel;
        void Check(bool ok, string message) { (ok ? report.checks : report.failures).Add(message); }
        void Reset(float distance = 1.4f)
        {
            duel.Restart();
            duel.Player.ResetRound(Vector3.zero, Quaternion.identity);
            duel.Enemy.ResetRound(new Vector3(0, 0, distance), Quaternion.Euler(0, 180, 0));
        }
        void Step(float seconds)
        {
            while (seconds > .0001f) { float dt = Mathf.Min(.005f, seconds); duel.Advance(dt, Vector2.zero, Vector2.zero); seconds -= dt; }
        }
        IEnumerator Start()
        {
            var args = Environment.GetCommandLineArgs(); int index = Array.IndexOf(args, "-duelTest");
            if (index < 0) yield break;
            string folder = args[index + 1]; Directory.CreateDirectory(folder);
            yield return null;
            duel = FindFirstObjectByType<MantisDuel>(); duel.Testing = true;
            Application.logMessageReceived += Log;
            var a = duel.Player; var b = duel.Enemy;
            Reset(2.50f);
            Check(a.CanReach(b) && !a.InRange(b), "Reach preview includes the short lunge");
            a.Begin(DuelAction.Attack); Step(.22f);
            Check(b.Guard == 100, "Attack preserves a brief readable windup");
            Step(.025f);
            Check(b.Guard < 100 && a.transform.position.z > .78f, "Attack lands by 245 ms from the extended reach");
            float lungeEnd = a.transform.position.z; Step(.3f);
            Check(Mathf.Abs(a.transform.position.z - lungeEnd) < .005f, "Recovery cannot keep lunging");
            Reset(2.58f); Check(!a.CanReach(b), "Reach preview rejects beyond lunge distance");
            a.Begin(DuelAction.Attack); Step(.3f); Check(b.Guard == 100, "Attack misses beyond the predicted reach");
            Reset(2.50f); a.Begin(DuelAction.Attack);
            for (int i = 0; i < 50; i++) duel.Advance(.005f, Vector2.zero, Vector2.up);
            Check(b.Guard == 100, "Retreat can still evade a boundary-range punch");
            Reset(3); a.Begin(DuelAction.Attack);
            for (int i = 0; i < 9; i++) duel.Advance(1f / 30, Vector2.zero, Vector2.zero);
            float at30 = a.transform.position.z;
            Reset(3); a.Begin(DuelAction.Attack);
            for (int i = 0; i < 36; i++) duel.Advance(1f / 120, Vector2.zero, Vector2.zero);
            Check(Mathf.Abs(at30 - a.transform.position.z) < .005f, "Lunge distance is consistent at 30 and 120 fps");
            Reset(); a.Begin(DuelAction.Attack); Step(.75f);
            Check(b.Alive && Mathf.Abs(b.Guard - 66.666f) < .1f, "Idle auto guard absorbs attack and loses one third of continuous gauge");
            a.Begin(DuelAction.Attack); Step(.75f); a.Begin(DuelAction.Attack); Step(.35f);
            Check(b.State == DuelState.Stunned && b.Guard == 0, "Three consecutive guards break guard without killing");
            Step(.38f); a.Begin(DuelAction.Attack); Step(.35f);
            Check(!b.Alive, "An attack during guard break kills in one hit");
            Reset(); a.Begin(DuelAction.Attack); Step(.15f); b.Begin(DuelAction.Parry); Step(.2f);
            Check(a.State == DuelState.Recovery && b.Alive && b.Guard == 100, "Parry beats attack and opens a large punish window");
            b.Begin(DuelAction.Attack); Step(.35f); Check(!a.Alive, "Successful parry allows lethal counterattack");
            Reset(); a.Begin(DuelAction.Attack); b.Begin(DuelAction.Feint); Step(.35f);
            Check(!b.Alive, "Attack beats vulnerable feint");
            Reset(); a.Begin(DuelAction.Feint); b.Begin(DuelAction.Parry); Step(.44f); a.Begin(DuelAction.Attack); Step(.35f);
            Check(!b.Alive, "Feint baits parry then punishes its recovery");
            Reset(); a.Begin(DuelAction.Attack); b.Begin(DuelAction.Attack); Step(.35f);
            Check(a.Alive && b.Alive && a.Guard == 90 && b.Guard == 90 && a.Feedback == "CLASH", "Clash costs only the attack expenditure for both fighters");
            Reset(); a.Begin(DuelAction.Attack); Step(.04f); b.Begin(DuelAction.Attack); Step(.35f);
            Check(a.Alive && b.Alive && a.StrikeSpent && b.StrikeSpent, "Near simultaneous attacks share a symmetric clash window");
            Reset(3); a.Begin(DuelAction.Attack); Step(.75f); Check(b.Guard == 100 && b.Alive, "Out of range attacks miss");
            Reset(); a.Begin(DuelAction.Attack); Step(.75f); float guard = b.Guard; Step(1);
            Check(Mathf.Abs(b.Guard - guard) < .1f, "Guard recovery waits two seconds after block");
            Step(2); Check(b.Guard > guard && b.Guard < 100, "Guard recovers gradually rather than in segments");
            Reset(); b.Begin(DuelAction.Parry); Step(.46f); Check(b.Vulnerable, "Missed parry has an exposed recovery");
            Reset(); b.Begin(DuelAction.Parry); Step(.20f); a.Begin(DuelAction.Attack); Step(.245f);
            Check(a.State == DuelState.Recovery && b.Alive && b.Guard == 100, "Early parry catches an attack 445 ms after activation");
            Reset(); a.Begin(DuelAction.Parry); Step(.20f); b.Begin(DuelAction.Attack); Step(.245f);
            Check(b.State == DuelState.Recovery && a.Alive && a.Guard == 100, "Both fighters use the same extended parry window");
            Reset(); b.Begin(DuelAction.Parry); Step(.23f); a.Begin(DuelAction.Attack); Step(.245f);
            Check(!b.Alive, "Attack after parry expires punishes the failed parry");
            Reset(); b.Begin(DuelAction.Parry); Step(.30f);
            Check(!b.Begin(DuelAction.Parry) && b.ParryActive, "Repeated parry input cannot refresh its active window");
            Step(.17f); Check(!b.Begin(DuelAction.Parry) && b.Vulnerable, "Parry cannot restart during vulnerable recovery");
            Step(.35f); Check(b.Vulnerable, "Failed parry still leaves a punishable opening");
            Step(.05f); Check(b.Available, "Failed parry returns to guard within 870 ms total");
            Reset(4); a.Begin(DuelAction.Attack); Step(.35f); Check(a.Vulnerable, "Missed attack interval exposes guard");
            Step(.08f); Check(a.Vulnerable, "Attack recovery remains exposed just before 200 ms");
            Step(.02f); Check(a.Available && !a.Vulnerable, "Auto guard returns after 200 ms attack recovery");
            Reset(); a.Begin(DuelAction.Attack); Step(.25f);
            Check(!a.Vulnerable && !a.Available, "Blocked punch is guarded but still locked in its return motion");
            Check(!a.Begin(DuelAction.Attack) && !a.Begin(DuelAction.Parry) && a.Guard == 90, "Blocked recovery cannot cancel or spend additional gauge");
            a.Receive(b);
            Check(a.Alive && Mathf.Abs(a.Guard - (90 - 100f / 3)) < .01f, "Return motion after a blocked punch guards an incoming hit");
            Step(.20f); Check(a.Available, "Blocked punch preserves its original attack interval");
            Reset(30);
            for (int i = 0; i < 10; i++) { Check(a.Begin(DuelAction.Attack), "Gauge funds attack " + (i + 1)); Step(.45f); }
            Check(a.Guard == 0 && a.Available && a.State != DuelState.Stunned, "Ten attacks exhaust gauge without self stun");
            Check(!a.Begin(DuelAction.Attack) && a.Available, "Insufficient gauge rejects attacks without locking control");
            Check(a.Begin(DuelAction.Feint), "Feint remains free at zero gauge"); Step(.45f);
            Check(a.Begin(DuelAction.Parry), "Parry remains free at zero gauge"); Step(.90f);
            Check(a.Guard == 0, "Attack resets the two second regeneration delay");
            Step(1.2f); Check(a.Guard >= 10 && a.Begin(DuelAction.Attack), "Waiting restores enough gauge to attack again");
            Reset(30);
            for (int i = 0; i < 10; i++) { b.Begin(DuelAction.Attack); Step(.45f); }
            Check(b.Guard == 0 && !b.Begin(DuelAction.Attack), "CPU uses the same attack cost and low gauge restriction");
            b.Receive(a); Check(b.State == DuelState.Stunned && b.Alive, "Blocking at zero gauge causes nonlethal guard break");
            Reset(); Check(a.InRange(b), "Reach indicator uses the combat reach test");
            Check(!a.Begin(DuelAction.None), "No action does not disable auto guard");
            a.Begin(DuelAction.Feint); Check(!a.Begin(DuelAction.Parry), "An action cannot be canceled into another action");
            Reset(); Check(a.Alive && b.Alive && a.Guard == 100 && b.Guard == 100, "New match resets both fighters");
            var brain = new DuelBrain(); brain.Reset(); int idle = 0, retreat = 0, approach = 0;
            Reset(b.EffectiveReach + .23f); UnityEngine.Random.InitState(37);
            for (int i = 0; i < 900; i++)
            { brain.Decide(.02f, b, a, out Vector2 movement, out DuelAction action); if (movement.magnitude < .01f) idle++; if (movement.y > .1f) retreat++; if (movement.y < -.1f) approach++; duel.Advance(.02f, Vector2.zero, movement); }
            Check(idle > 10 && retreat > 10 && approach > 10, "CPU varies waiting, approaches and retreats at the range boundary");
            Reset(2.4f); brain.Reset(); UnityEngine.Random.InitState(81);
            int attacks = 0, feints = 0, blindParries = 0;
            for (int i = 0; i < 3000; i++)
            {
                brain.Decide(.02f, b, a, out _, out DuelAction choice);
                if (choice == DuelAction.Attack) attacks++;
                if (choice == DuelAction.Feint) feints++;
                if (choice == DuelAction.Parry) blindParries++;
            }
            Check(attacks > 10 && feints > 0, "CPU initiates attacks and feints against a waiting opponent");
            Check(blindParries == 0, "CPU does not throw blind parries against an idle opponent");
            Reset(); brain.Reset(); a.Begin(DuelAction.Attack); Step(.25f);
            brain.Decide(.35f, b, a, out _, out DuelAction lateReaction);
            Check(lateReaction != DuelAction.Parry, "CPU does not parry after the observed strike has already landed");
            Reset(); yield return new WaitForSeconds(.8f); yield return new WaitForEndOfFrame(); ScreenCapture.CaptureScreenshot(Path.Combine(folder, "triangle-duel.png"));
            yield return new WaitForSeconds(.2f);
            report.passed = report.failures.Count == 0;
            File.WriteAllText(Path.Combine(folder, "report.json"), JsonUtility.ToJson(report, true));
            Application.logMessageReceived -= Log; Application.Quit(report.passed ? 0 : 1);
        }
        void Log(string message, string trace, LogType type) { if (type == LogType.Exception || type == LogType.Error) report.failures.Add(message); }
    }
}




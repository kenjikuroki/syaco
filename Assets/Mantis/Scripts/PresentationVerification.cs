using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
namespace MantisPunch
{
    public sealed class PresentationVerification : MonoBehaviour
    {
        [Serializable] class Report { public bool passed; public List<string> checks = new List<string>(); public List<string> failures = new List<string>(); }
        Report report = new Report(); MantisDuel duel; string folder;
        void Check(bool ok, string message) { (ok ? report.checks : report.failures).Add(message); }
        IEnumerator Shot(string name)
        { yield return new WaitForEndOfFrame(); ScreenCapture.CaptureScreenshot(Path.Combine(folder, name + ".png")); yield return null; }
        IEnumerator Start()
        {
            var args = Environment.GetCommandLineArgs(); int index = Array.IndexOf(args, "-presentationTest");
            if (index < 0) yield break;
            folder = args[index + 1]; Directory.CreateDirectory(folder);
            Application.logMessageReceived += Log;
            yield return null; duel = FindFirstObjectByType<MantisDuel>(); duel.Testing = true;
            duel.StartDemo(1); duel.Testing = true;
            yield return new WaitForSeconds(.7f);
            Check(Vector3.Dot(duel.arenaCamera.transform.position - duel.Player.transform.position, duel.Player.transform.forward) < -2, "Camera sits behind player");
            Check(Vector3.Distance(duel.arenaCamera.transform.position, duel.Player.transform.position) < 5.5f, "Camera is closer than previous arena view");
            yield return Shot("01-guard-stance");
            duel.StartDemo(1); duel.Testing = false;
            yield return new WaitForSeconds(1.02f); duel.Testing = true;
            Check(duel.Enemy.Alive && duel.Enemy.Guard < 100, "Guard demo blocks and pushes back without lethal damage");
            yield return Shot("02-guard-impact");
            duel.StartDemo(2); duel.Testing = false;
            yield return new WaitForSeconds(1.04f); duel.Testing = true;
            Check(duel.Enemy.State == DuelState.Recovery && duel.Player.Guard == 100, "Parry demo shows player deflecting and exposing opponent");
            yield return Shot("03-parry-impact");
            duel.StartDemo(3); duel.Testing = false;
            yield return new WaitForSeconds(.90f);
            Check(FindFirstObjectByType<WaterImpact>() != null, "Knockout emits the short water impact");
            yield return Shot("04a-water-impact");
            yield return new WaitForSeconds(.48f); duel.Testing = true;
            Check(FindFirstObjectByType<WaterImpact>() == null, "Water impact clears before the knockout flight finishes");
            Check(!duel.Enemy.Alive && duel.Player.Alive, "Direct hit still ends round in one blow");
            Check(Vector3.Distance(duel.Enemy.presentation.VisiblePosition, duel.Enemy.transform.position) > 1.5f, "Defeated model is launched visibly away");
            yield return Shot("04-knockout-flight");
            yield return new WaitForSeconds(1.2f);
            yield return Shot("05-knockout-landed");
            Check(CombatEffects.Current.Played >= 4, "Contact and movement events emit effects and audio");
            duel.Restart(); duel.Testing = true; yield return new WaitForSeconds(.2f);
            Check(duel.Enemy.presentation.DeathAge < 0 && duel.Enemy.Alive, "Restart clears knockout presentation");
            duel.StartDemo(1); duel.Testing = false;
            CombatEffects.Current.Play(CombatBeat.Parry, duel.Player.transform.position + Vector3.up, Vector3.forward);
            float playerAge = duel.Player.Age, enemyAge = duel.Enemy.Age;
            yield return new WaitForSeconds(.045f);
            Check(duel.Player.Age == playerAge && duel.Enemy.Age == enemyAge, "Hit stop freezes both combat clocks equally");
            duel.Testing = true;
            yield return new WaitForSeconds(.14f);
            Check(CombatEffects.Current.HitStop == 0 && CombatEffects.Current.CombatSpeed == .2f && Mathf.Abs(CombatEffects.Current.Zoom - 10) < .01f, "Parry transitions from stop to slow motion while retaining zoom");
            yield return new WaitForSeconds(1.05f);
            Check(CombatEffects.Current.HitStop == 0 && CombatEffects.Current.Zoom < .01f, "Hit stop and zoom automatically release");
            duel.StartDemo(2); duel.Testing = false;
            yield return new WaitForSeconds(1.04f); duel.Testing = true;
            Check(duel.arenaCamera.fieldOfView < 51, "Successful parry briefly tightens camera view");
            yield return Shot("06-parry-zoom");
            duel.Restart(); duel.Testing = true;
            duel.Player.ResetRound(Vector3.zero, Quaternion.identity);
            duel.Enemy.ResetRound(Vector3.forward * 1.4f, Quaternion.Euler(0, 180, 0));
            duel.Player.Begin(DuelAction.Parry); duel.Enemy.Begin(DuelAction.Attack);
            duel.Advance(.245f, Vector2.zero, Vector2.zero);
            duel.Testing = false;
            yield return new WaitForSeconds(.4f);
            Check(CombatEffects.Current.CombatSpeed == .2f && duel.Enemy.Age < .20f, "Parry briefly deflects at full speed then holds the opening in slow motion");
            duel.BufferCounter(DuelAction.Attack);
            yield return new WaitForSeconds(.06f);
            Check(duel.Player.State == DuelState.Attack && CombatEffects.Current.CombatSpeed == 1 && CombatEffects.Current.Zoom > 9.9f, "Counter input immediately returns to full speed with zoom held");
            yield return new WaitForSeconds(.22f);
            Check(!duel.Enemy.Alive && CombatEffects.Current.Zoom > 9.9f, "Counter knockout keeps zoom through impact stop");
            yield return new WaitForSeconds(.5f);
            Check(CombatEffects.Current.Zoom < .01f, "Counter impact releases zoom automatically");
            duel.Restart(); duel.Testing = true;
            duel.Player.ResetRound(Vector3.zero, Quaternion.identity);
            duel.Enemy.ResetRound(Vector3.forward * 1.4f, Quaternion.Euler(0, 180, 0));
            duel.Player.Begin(DuelAction.Parry); duel.Enemy.Begin(DuelAction.Attack);
            duel.Advance(.245f, Vector2.zero, Vector2.zero);
            yield return new WaitForSeconds(.8f);
            Check(CombatEffects.Current.Zoom > 9.9f, "Zoom gives time to choose a counter instead of immediately pulling back");
            yield return new WaitForSeconds(1.1f);
            Check(CombatEffects.Current.Zoom < .01f, "Zoom has a timeout even when no counter is entered");
            duel.StartDemo(4); duel.Testing = false;
            yield return new WaitForSeconds(2.69f); duel.Testing = true;
            Check(duel.Enemy.State == DuelState.Stunned && duel.Enemy.Guard == 0, "Guard break demo reaches dizzy state without knockout");
            Check(duel.arenaCamera.fieldOfView < 51, "Guard break has its own brief camera zoom");
            yield return Shot("07-guard-break");
            duel.Restart(); duel.Testing = true; duel.ShakeStrength = 0;
            yield return new WaitForSeconds(1.2f);
            Vector3 calmPosition = duel.arenaCamera.transform.position;
            CombatEffects.Current.Play(CombatBeat.Guard, duel.Player.transform.position, Vector3.forward);
            yield return new WaitForSeconds(.04f);
            Check(Vector3.Distance(calmPosition, duel.arenaCamera.transform.position) < .003f, "Disabling shake keeps the camera stable during guard impact");
            duel.Restart(); Check(CombatEffects.Current.Zoom == 0 && CombatEffects.Current.HitStop == 0, "Restart clears cinematic stop and zoom");
            var imported = duel.GetComponent<ImportedHitEffects>();
            int playedBeforePrepare = CombatEffects.Current.Played;
            CombatEffects.Current.Play(CombatBeat.Prepare, Vector3.up, Vector3.forward);
            Check(CombatEffects.Current.Played == playedBeforePrepare, "Attack and feint startup flash is removed");
            imported.Play(CombatBeat.Kick, duel.Player.transform.position, -duel.Player.transform.forward);
            var spray = GameObject.Find("Tail water spray");
            Check(spray && spray.GetComponent<ParticleSystem>() && spray.GetComponentsInChildren<LineRenderer>().Length == 0, "Lunge uses a small particle spray instead of tail lines");
            yield return new WaitForSecondsRealtime(.35f);
            Check(GameObject.Find("Tail water spray") == null, "Tail spray clears promptly");
            imported.Play(CombatBeat.Parry, Vector3.up, Vector3.forward);
            Check(GameObject.Find("Imported impact Parry") != null, "Short parry effect spawns");
            yield return new WaitForSecondsRealtime(.26f);
            Check(GameObject.Find("Imported impact Parry") == null, "Parry particles clear within 260 ms");
            imported.Play(CombatBeat.GuardBreak, Vector3.up, Vector3.forward);
            Check(GameObject.Find("Imported impact GuardBreak") != null, "Short guard break effect spawns");
            yield return new WaitForSecondsRealtime(.32f);
            Check(GameObject.Find("Imported impact GuardBreak") == null, "Guard break particles clear within 320 ms");
            report.passed = report.failures.Count == 0;
            File.WriteAllText(Path.Combine(folder, "report.json"), JsonUtility.ToJson(report, true));
            Application.logMessageReceived -= Log; Application.Quit(report.passed ? 0 : 1);
        }
        void Log(string message, string trace, LogType type) { if (type == LogType.Exception || type == LogType.Error) report.failures.Add(message); }
    }
}

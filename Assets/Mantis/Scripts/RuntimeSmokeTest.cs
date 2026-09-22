using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace MantisPunch
{
    public sealed class RuntimeSmokeTest : MonoBehaviour
    {
        [Serializable] sealed class Report
        {
            public bool passed;
            public string unity;
            public List<string> checks = new List<string>();
            public List<string> failures = new List<string>();
        }
        readonly Report report = new Report();
        MantisPlayer player;
        string directory;
        bool running, finished;
        float began;
        IEnumerator Start()
        {
            var args = Environment.GetCommandLineArgs();
            if (!args.Contains("-mantisSmokeTest")) yield break;
            int index = Array.IndexOf(args, "-mantisReport");
            directory = index >= 0 && index + 1 < args.Length ? args[index + 1] : Path.Combine(Application.persistentDataPath, "QA");
            Directory.CreateDirectory(directory); report.unity = Application.unityVersion;
            running = true; began = Time.realtimeSinceStartup;
            Application.logMessageReceived += OnLog;
            player = FindFirstObjectByType<MantisPlayer>();
            player.automation = true;
            yield return null;
            Check(player.visual.Ready, "Imported rig and animation graph are ready");
            Check(player.visual.GetComponentInChildren<SkinnedMeshRenderer>().sharedMaterial.mainTexture != null, "Albedo texture assigned");
            yield return new WaitForSeconds(.15f);
            yield return new WaitForEndOfFrame();
            ScreenCapture.CaptureScreenshot(Path.Combine(directory, "01-idle.png"));

            Vector3 start = player.transform.position;
            player.automationMove = Vector2.up;
            yield return new WaitForSeconds(.45f);
            float straight = (player.transform.position - start).magnitude;
            Check(straight > .55f, "Forward input moves the shrimp");
            player.Teleport(start, Quaternion.identity);
            player.automationMove = Vector2.one;
            yield return new WaitForSeconds(.45f);
            float diagonal = (player.transform.position - start).magnitude;
            Check(diagonal / straight < 1.18f && diagonal / straight > .82f, "Diagonal movement is normalized");
            player.automationMove = Vector2.right;
            yield return new WaitForSeconds(.30f);
            Check(Vector3.Dot(player.transform.forward, Vector3.right) > .95f, "Shrimp turns toward movement input");
            player.automationMove = Vector2.zero;
            yield return new WaitForSeconds(.30f);
            Check(player.Velocity.magnitude < .01f, "Releasing movement brakes to a stop");
            player.Teleport(new Vector3(-8, 0, -6), Quaternion.identity);
            player.automationMove = Vector2.left;
            yield return new WaitForSeconds(.6f);
            Check(player.transform.position.x > -8.4f, "Arena wall blocks movement");
            Check(player.transform.position.y > -.05f, "Player remains on the seafloor");
            player.automationMove = Vector2.zero;

            TrainingTarget target = FindObjectsByType<TrainingTarget>(FindObjectsSortMode.None).OrderBy(t => Mathf.Abs(t.transform.position.x)).First();
            player.Teleport(target.transform.position - Vector3.forward * 1.05f, Quaternion.identity);
            int before = target.HitCount;
            int attacks = player.AttackCount;
            player.automationAttack = true;
            yield return new WaitForSeconds(.10f);
            player.automationAttack = true;
            yield return new WaitForSeconds(.17f);
            yield return new WaitForEndOfFrame();
            ScreenCapture.CaptureScreenshot(Path.Combine(directory, "02-punch.png"));
            yield return new WaitForSeconds(.55f);
            Check(player.AttackCount == attacks + 1, "Repeated input during windup cannot restart the attack");
            Check(target.HitCount == before + 1, "A punch deals exactly one hit to the front target");
            Check(player.TotalHits > 0, "Hit counter receives combat feedback");
            Check(!player.Attacking && player.visual.LastSample < .001f, "Punch recovers to folded idle pose");
            File.WriteAllText(Path.Combine(directory, "combat-debug.txt"), "Player=" + player.transform.position + "\nClub=" + player.visual.StrikePoint + "\nTarget=" + target.transform.position + "\nHits=" + target.HitCount);

            player.Teleport(target.transform.position + Vector3.forward * 1.05f, Quaternion.identity);
            before = target.HitCount; player.automationAttack = true;
            yield return new WaitForSeconds(.85f);
            Check(target.HitCount == before, "Punch does not damage targets behind the shrimp");

            var controls = new MantisControls();
            controls.RoutePointer(11, controls.StickCenter, TouchPhase.Began);
            controls.RoutePointer(11, controls.StickCenter + Vector2.up * controls.Radius, TouchPhase.Moved);
            controls.RoutePointer(22, controls.AttackCenter, TouchPhase.Began);
            Check(controls.Stick.y > .95f && controls.ConsumeAttack(), "Independent touch fingers can move and punch together");
            controls.RoutePointer(22, controls.AttackCenter, TouchPhase.Ended);
            Check(controls.Stick.y > .95f, "Releasing punch finger does not release movement");
            controls.RoutePointer(11, controls.StickCenter, TouchPhase.Canceled);
            Check(controls.Stick == Vector2.zero && !controls.ConsumeAttack(), "Canceled touch clears movement without repeating attack");
            controls.RoutePointer(33, controls.AttackCenter, TouchPhase.Began, true);
            Check(!controls.ConsumeAttack(), "Tuning panel blocks underlying gameplay touches");
            target.ResetTarget(); Check(target.Health == target.maximumHealth, "Target reset restores health");
            player.Teleport(Vector3.zero, Quaternion.identity);
            FindFirstObjectByType<MantisHud>().tuning = true;
            yield return new WaitForSeconds(.4f);
            yield return new WaitForEndOfFrame();
            ScreenCapture.CaptureScreenshot(Path.Combine(directory, "03-tuning.png"));
            yield return new WaitForSeconds(.4f);
            Finish();
        }
        void Check(bool condition, string description)
        {
            if (condition) report.checks.Add(description); else report.failures.Add(description);
            Debug.Log((condition ? "PASS: " : "FAIL: ") + description);
        }
        void OnLog(string condition, string stack, LogType type)
        {
            if (type == LogType.Exception || type == LogType.Error || type == LogType.Assert) report.failures.Add(condition);
        }
        void Update()
        {
            if (running && !finished && Time.realtimeSinceStartup - began > 45)
            { report.failures.Add("Runtime smoke-test timeout"); Finish(); }
        }
        void Finish()
        {
            if (finished) return; finished = true;
            report.passed = report.failures.Count == 0;
            File.WriteAllText(Path.Combine(directory, "runtime-test.json"), JsonUtility.ToJson(report, true));
            Application.logMessageReceived -= OnLog;
            Debug.Log("MANTIS_RUNTIME_TEST " + (report.passed ? "PASS" : "FAIL"));
            Application.Quit(report.passed ? 0 : 1);
        }
    }
}

using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.InputSystem;

namespace MantisPunch
{
    public sealed class HitComparison : MonoBehaviour
    {
        public GameObject[] variants;
        public bool Active { get; private set; }
        public string Caption => "HIT COMPARISON: " + (selected + 1) + " / " + variants[selected].name.Trim() + "    1-4 SELECT / SPACE REPLAY";
        MantisDuel duel;
        ImportedHitEffects effects;
        int selected;
        float next;
        public void Select(int index)
        {
            selected = index; effects.hitPrefab = variants[index];
            duel.StartDemo(3); next = Time.unscaledTime + 3.5f;
        }
        IEnumerator Start()
        {
            yield return null;
            var args = Environment.GetCommandLineArgs();
            int test = Array.IndexOf(args, "-hitCompareTest");
            Active = test >= 0 || Array.IndexOf(args, "-hitCompare") >= 0;
            if (!Active) yield break;
            duel = GetComponent<MantisDuel>(); effects = GetComponent<ImportedHitEffects>();
            if (test < 0) { Select(0); yield break; }
            string folder = args[test + 1]; Directory.CreateDirectory(folder);
            bool passed = true;
            for (int i = 0; i < variants.Length; i++)
            {
                int before = effects.Played; Select(i); next = float.PositiveInfinity;
                yield return new WaitForSeconds(.94f);
                passed &= effects.Played > before && !duel.Enemy.Alive;
                yield return new WaitForEndOfFrame();
                ScreenCapture.CaptureScreenshot(Path.Combine(folder, (i + 1) + "-hit.png"));
                yield return new WaitForSeconds(1.5f);
            }
            File.WriteAllText(Path.Combine(folder, "result.txt"), passed ? "PASS: all four imported hit variants emitted on knockout" : "FAIL");
            Application.Quit(passed ? 0 : 1);
        }
        void Update()
        {
            if (!Active || !duel) return;
            var k = Keyboard.current;
            if (k?.digit1Key.wasPressedThisFrame == true) Select(0);
            else if (k?.digit2Key.wasPressedThisFrame == true) Select(1);
            else if (k?.digit3Key.wasPressedThisFrame == true) Select(2);
            else if (k?.digit4Key.wasPressedThisFrame == true) Select(3);
            else if (k?.spaceKey.wasPressedThisFrame == true) Select(selected);
            else if (Time.unscaledTime >= next) Select((selected + 1) % variants.Length);
        }
    }
}

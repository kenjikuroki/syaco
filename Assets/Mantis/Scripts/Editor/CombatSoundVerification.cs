using System;
using System.Collections.Generic;
using System.IO;
using MantisPunch;
using UnityEngine;

public static class CombatSoundVerification
{
    public static void Verify()
    {
        Directory.CreateDirectory("QA");
        int checkedClips = 0;
        foreach (CombatBeat beat in Enum.GetValues(typeof(CombatBeat))) for (int variant = 0; variant < 3; variant++)
        {
            var data = CombatSoundDesign.Render(beat, variant);
            double power = 0;
            foreach (float sample in data)
            {
                if (float.IsNaN(sample) || float.IsInfinity(sample) || Mathf.Abs(sample) > .781f) throw new Exception("Invalid audio sample " + beat);
                power += sample * sample;
            }
            if (Math.Sqrt(power / data.Length) < .005 || Math.Abs(data[0]) > .001 || Math.Abs(data[data.Length - 1]) > .005)
                throw new Exception("Silent clip or discontinuous edge: " + beat);
            checkedClips++;
        }
        var preview = new List<float>();
        foreach (var beat in new[] { CombatBeat.Swing, CombatBeat.Guard, CombatBeat.Parry, CombatBeat.GuardBreak, CombatBeat.Knockout })
        {
            foreach (var sample in CombatSoundDesign.Render(beat, 1)) preview.Add(sample * CombatSoundDesign.Gain(beat) * .65f);
            preview.AddRange(new float[CombatSoundDesign.Rate / 2]);
        }
        using (var writer = new BinaryWriter(File.Create("QA/PunchSoundPreview.wav")))
        {
            writer.Write(System.Text.Encoding.ASCII.GetBytes("RIFF")); writer.Write(36 + preview.Count * 2);
            writer.Write(System.Text.Encoding.ASCII.GetBytes("WAVEfmt ")); writer.Write(16); writer.Write((short)1); writer.Write((short)1);
            writer.Write(CombatSoundDesign.Rate); writer.Write(CombatSoundDesign.Rate * 2); writer.Write((short)2); writer.Write((short)16);
            writer.Write(System.Text.Encoding.ASCII.GetBytes("data")); writer.Write(preview.Count * 2);
            foreach (var sample in preview) writer.Write((short)(sample * 32767));
        }
        File.WriteAllText("QA/audio-verification.txt", "PASS: " + checkedClips + " clips; finite samples, peak <= 0.78, audible energy, smooth edges. Preview: swing, guard, parry, guard break, knockout. Device listening and mix evaluation remain subjective.");
    }
}

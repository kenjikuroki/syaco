using System;
using System.IO;
using UnityEditor;
using UnityEngine;

public static class ReefMusicBuilder
{
    const int Rate = 44100;
    const string Folder = "Assets/Mantis/Audio";
    const string Path = Folder + "/DeepReefLoop.wav";
    static double Hz(int note) => 440 * Math.Pow(2, (note - 69) / 12.0);
    public static AudioClip[] LoadBattleTracks()
    {
        string[] names = { "A_HeavyCombat", "B_RushCombat", "C_DeepDuel" };
        var tracks = new AudioClip[names.Length];
        for (int i = 0; i < names.Length; i++)
        {
            string path = Folder + "/" + names[i] + ".wav";
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
            var importer = (AudioImporter)AssetImporter.GetAtPath(path);
            if (!importer) throw new Exception("Missing approved music: " + path);
            var settings = importer.defaultSampleSettings;
            settings.loadType = AudioClipLoadType.Streaming; settings.compressionFormat = AudioCompressionFormat.PCM;
            settings.sampleRateSetting = AudioSampleRateSetting.PreserveSampleRate;
            importer.defaultSampleSettings = settings; importer.forceToMono = false; importer.SaveAndReimport();
            tracks[i] = AssetDatabase.LoadAssetAtPath<AudioClip>(path);
            if (!tracks[i] || tracks[i].channels != 2) throw new Exception("Invalid battle music " + path);
        }
        return tracks;
    }
    public static AudioClip Build()
    {
        if (!AssetDatabase.IsValidFolder(Folder)) AssetDatabase.CreateFolder("Assets/Mantis", "Audio");
        int count = (int)Math.Round(Rate * 60.0 / 88 * 64);
        double beat = (double)count / Rate / 64;
        var left = new float[count]; var right = new float[count];
        void Add(double start, double duration, double pan, Func<double, double, double> synth)
        {
            int offset = (int)Math.Round(start * Rate), n = (int)Math.Round(duration * Rate);
            double gl = Math.Sqrt((1 - pan) * .5), gr = Math.Sqrt((1 + pan) * .5);
            for (int i = 0; i < n; i++)
            {
                double t = (double)i / Rate, u = (double)i / n;
                double value = synth(t, u) * Math.Min(1, t / .003) * Math.Min(1, (duration - t) / .035);
                int index = (offset + i) % count;
                left[index] += (float)(value * gl); right[index] += (float)(value * gr);
            }
        }
        int[][] chords = { new[] { 50, 57, 60, 64 }, new[] { 46, 53, 57, 60 }, new[] { 43, 50, 53, 57 }, new[] { 45, 52, 55, 62 } };
        int[] bass = { 38, 34, 31, 33 };
        var random = new System.Random(882309);
        for (int bar = 0; bar < 16; bar++)
        {
            int chord = bar / 2 % 4;
            if (bar % 2 == 0)
            {
                for (int voice = 0; voice < 4; voice++)
                {
                    double hz = Hz(chords[chord][voice]), pan = (voice - 1.5) * .38;
                    Add(bar * 4 * beat, 8 * beat + .8, pan, (t, u) =>
                        .060 * Math.Pow(Math.Sin(Math.PI * u), 2) * (Math.Sin(2 * Math.PI * hz * t + .23 * Math.Sin(t * 1.1))
                        + .20 * Math.Sin(2 * Math.PI * hz * 2 * t) + .12 * Math.Sin(2 * Math.PI * hz * .998 * t)));
                }
            }
            double f = Hz(bass[chord]);
            Add(bar * 4 * beat, 1.8, 0, (t, u) => .13 * Math.Exp(-t * 2.3) * (1 - Math.Exp(-t * 24)) * (Math.Sin(2 * Math.PI * f * t) + .35 * Math.Sin(4 * Math.PI * f * t)));
            foreach (double step in new[] { 0.0, 2.65 })
                Add((bar * 4 + step) * beat, .37, 0, (t, u) => .24 * Math.Exp(-t * 15) * Math.Sin(2 * Math.PI * (69 * t + 2.0 * (1 - Math.Exp(-t * 35)))));
            foreach (double step in new[] { 1.5, 3.25 })
                Add((bar * 4 + step) * beat, .14, bar % 2 == 0 ? -.25 : .25, (t, u) => .032 * Math.Exp(-t * 35) * (Math.Sin(2 * Math.PI * 420 * t) + .5 * Math.Sin(2 * Math.PI * 673 * t)));
            // Sparse distant motifs leave room for attack and parry transients.
            if (bar % 2 == 1)
            {
                int[] melody = { 76, 72, 69, 74, 76, 79, 74, 69 };
                double hz = Hz(melody[bar / 2]);
                Add((bar * 4 + .5) * beat, 3.2, bar % 4 == 1 ? -.5 : .5, (t, u) =>
                    .052 * (1 - Math.Exp(-t * 28)) * Math.Exp(-t * 1.7) *
                    (Math.Sin(2 * Math.PI * hz * t + 1.3 * Math.Exp(-t * 3) * Math.Sin(2 * Math.PI * hz * 2 * t)) + .13 * Math.Sin(2 * Math.PI * hz * 3 * t)));
            }
            double lowNoise = 0;
            Add(bar * 4 * beat, 4 * beat + 1, -.2, (t, u) =>
            { lowNoise += .015 * (random.NextDouble() * 2 - 1 - lowNoise); return lowNoise * .1 * Math.Pow(Math.Sin(Math.PI * u), 2); });
        }
        // Circular stereo echoes wrap tails into the beginning rather than cutting them off.
        var dryL = (float[])left.Clone(); var dryR = (float[])right.Clone();
        int delayA = (int)(beat * .75 * Rate), delayB = (int)(beat * 1.5 * Rate);
        float peak = 0;
        for (int i = 0; i < count; i++)
        {
            left[i] += dryR[(i - delayA + count) % count] * .24f + dryL[(i - delayB + count) % count] * .12f;
            right[i] += dryL[(i - delayA + count) % count] * .24f + dryR[(i - delayB + count) % count] * .12f;
            peak = Mathf.Max(peak, Mathf.Abs(left[i]), Mathf.Abs(right[i]));
        }
        float scale = .72f / peak;
        if (Mathf.Abs(left[0] - left[count - 1]) * scale > .02f || Mathf.Abs(right[0] - right[count - 1]) * scale > .02f)
            throw new Exception("Music loop seam is discontinuous");
        using (var w = new BinaryWriter(File.Create(Path)))
        {
            w.Write(System.Text.Encoding.ASCII.GetBytes("RIFF")); w.Write(36 + count * 4); w.Write(System.Text.Encoding.ASCII.GetBytes("WAVEfmt "));
            w.Write(16); w.Write((short)1); w.Write((short)2); w.Write(Rate); w.Write(Rate * 4); w.Write((short)4); w.Write((short)16);
            w.Write(System.Text.Encoding.ASCII.GetBytes("data")); w.Write(count * 4);
            for (int i = 0; i < count; i++) { w.Write((short)(left[i] * scale * 32767)); w.Write((short)(right[i] * scale * 32767)); }
        }
        AssetDatabase.ImportAsset(Path, ImportAssetOptions.ForceSynchronousImport);
        var importer = (AudioImporter)AssetImporter.GetAtPath(Path);
        // PCM keeps exact sample loop boundaries; streaming avoids decoding a full track into memory.
        var settings = importer.defaultSampleSettings; settings.loadType = AudioClipLoadType.Streaming;
        settings.compressionFormat = AudioCompressionFormat.PCM; settings.sampleRateSetting = AudioSampleRateSetting.PreserveSampleRate;
        importer.defaultSampleSettings = settings; importer.forceToMono = false; importer.SaveAndReimport();
        Directory.CreateDirectory("QA");
        File.WriteAllText("QA/music-verification.txt", "PASS: original stereo composition, 88 BPM, 16 bars, " + ((double)count / Rate).ToString("F3") + " seconds. Peak 0.72. Circular tails and sample seam checked. PCM streaming.");
        return AssetDatabase.LoadAssetAtPath<AudioClip>(Path);
    }
}

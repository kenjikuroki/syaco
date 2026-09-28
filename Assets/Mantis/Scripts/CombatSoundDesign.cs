using System;
using UnityEngine;

namespace MantisPunch
{
    // Original layered synthesis: no external recordings or asset licenses required.
    public static class CombatSoundDesign
    {
        public const int Rate = 48000;
        static float Decay(float t, float seconds) => Mathf.Exp(-t / seconds);
        static float Tone(float t, float hz, float fall, float decay)
            => Mathf.Sin(2 * Mathf.PI * (hz * t + fall * .018f * (1 - Mathf.Exp(-t / .018f)))) * Decay(t, decay);
        public static float Gain(CombatBeat beat) => beat == CombatBeat.Kick ? .14f : beat == CombatBeat.Swing ? .34f
            : beat == CombatBeat.Land ? .43f : beat == CombatBeat.Guard ? .72f : beat == CombatBeat.Knockout ? 1 : .86f;
        public static float[] Render(CombatBeat beat, int variant)
        {
            bool parry = beat == CombatBeat.Parry, ko = beat == CombatBeat.Knockout;
            bool guard = beat == CombatBeat.Guard || beat == CombatBeat.Clash;
            bool broken = beat == CombatBeat.GuardBreak;
            bool movement = beat == CombatBeat.Swing || beat == CombatBeat.Kick;
            float length = ko ? .68f : parry ? .43f : broken ? .49f : movement ? .19f : .29f;
            float[] data = new float[Mathf.CeilToInt(Rate * length)];
            var random = new System.Random(431 + (int)beat * 137 + variant * 2909);
            float low = 0, middle = 0, dc = 0, peak = 0;
            float tuning = 1 + (variant - 1) * .027f;
            for (int i = 0; i < data.Length; i++)
            {
                float t = (float)i / Rate, n = (float)random.NextDouble() * 2 - 1;
                low += .027f * (n - low); middle += .25f * (n - middle);
                float crack = (n - middle) * Decay(t, .008f);
                float shell = (middle - low) * Decay(t, .040f);
                float value;
                if (movement)
                {
                    float envelope = Mathf.Pow(Mathf.Sin(Mathf.PI * t / length), 2);
                    value = (middle - low) * envelope * (beat == CombatBeat.Kick ? .7f : 1.3f);
                }
                else if (parry)
                {
                    value = .60f * crack + .43f * shell + .23f * Tone(t, 260 * tuning, 380, .038f);
                    value += .23f * Tone(t, 1620 * tuning, 0, .10f) + .14f * Tone(t, 2573 * tuning, 0, .073f)
                        + .09f * Tone(t, 3981 * tuning, 0, .047f);
                }
                else if (guard)
                {
                    value = .7f * crack + 1.6f * shell + .46f * Tone(t, 155 * tuning, 210, .045f)
                        + .23f * Tone(t, 340 * tuning, 110, .027f) + .13f * Tone(t, 710 * tuning, 0, .019f);
                }
                else
                {
                    float body = ko ? 78 : broken ? 110 : 125;
                    value = .85f * crack + 1.5f * shell + .55f * Tone(t, body * tuning, 190, ko ? .13f : .075f)
                        + .26f * Tone(t, body * 2.13f * tuning, 230, .062f)
                        + .16f * Tone(t, 390 * tuning, 500, .025f);
                    if (ko || broken)
                    {
                        // Delayed fracture and filtered spray widen the impact without a long hiss.
                        float fracture = Mathf.Max(0, t - .024f);
                        if (t > .024f) value += .85f * (middle - low) * Decay(fracture, .035f);
                        value += low * 1.9f * (1 - Decay(t, .009f)) * Decay(t, ko ? .15f : .09f);
                    }
                }
                // Soft saturation glues the layers; short ramps prevent clicks/DC offsets.
                value = (float)Math.Tanh(value * 1.25f);
                dc += .0013f * (value - dc); value -= dc;
                value *= Mathf.Min(1, t / .0006f) * Mathf.Clamp01((length - t) / .035f);
                data[i] = value; peak = Mathf.Max(peak, Mathf.Abs(value));
            }
            float scale = .78f / Mathf.Max(.001f, peak);
            for (int i = 0; i < data.Length; i++) data[i] *= scale;
            return data;
        }
        public static AudioClip Create(CombatBeat beat, int variant)
        {
            var data = Render(beat, variant);
            var clip = AudioClip.Create("Layered " + beat + " " + variant, data.Length, 1, Rate, false);
            clip.SetData(data, 0); return clip;
        }
    }
}

using UnityEngine;
using UnityEngine.InputSystem;

namespace MantisPunch
{
    public sealed class ReefMusic : MonoBehaviour
    {
        public AudioClip loop;
        public AudioClip[] battleTracks;
        readonly System.Random trackRandom = new System.Random();
        public int RoundSelections { get; private set; }
        [Range(0, 1)] public float volume = .24f;
        AudioSource source;
        float duckLeft, duckLevel = 1, fade;
        bool muted;
        public bool Playing => source && source.isPlaying;
        public float CurrentVolume => source ? source.volume : 0;
        void Awake()
        {
            source = gameObject.AddComponent<AudioSource>(); source.clip = loop;
            source.loop = true; source.playOnAwake = false; source.spatialBlend = 0; source.volume = 0;
            source.priority = 160;
            muted = PlayerPrefs.GetInt("Mantis.MusicMuted", 0) != 0;
        }
        public void StartRound()
        {
            if (battleTracks != null && battleTracks.Length > 0)
                loop = battleTracks[trackRandom.Next(battleTracks.Length)];
            source.Stop(); source.clip = loop;
            fade = 0; duckLeft = 0; duckLevel = 1; source.volume = 0;
            if (loop) { source.Play(); RoundSelections++; }
        }
        public void Accent(CombatBeat beat)
        {
            float level = beat == CombatBeat.Knockout ? .13f : beat == CombatBeat.Parry ? .32f : beat == CombatBeat.GuardBreak ? .25f : .72f;
            float duration = beat == CombatBeat.Knockout ? .65f : beat == CombatBeat.Parry ? .32f : .17f;
            duckLevel = Mathf.Min(duckLevel, level); duckLeft = Mathf.Max(duckLeft, duration);
            source.volume = Mathf.Min(source.volume, volume * duckLevel);
        }
        void Update()
        {
            if (Keyboard.current?.mKey.wasPressedThisFrame == true)
            { muted = !muted; PlayerPrefs.SetInt("Mantis.MusicMuted", muted ? 1 : 0); }
            float dt = Time.unscaledDeltaTime;
            fade = Mathf.MoveTowards(fade, 1, dt / 1.5f);
            duckLeft = Mathf.Max(0, duckLeft - dt);
            if (duckLeft == 0) duckLevel = Mathf.MoveTowards(duckLevel, 1, dt * 2.5f);
            source.volume = Mathf.MoveTowards(source.volume, muted ? 0 : volume * fade * duckLevel, dt * .7f);
        }
    }
}

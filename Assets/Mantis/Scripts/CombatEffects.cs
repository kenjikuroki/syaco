using System.Collections.Generic;
using UnityEngine;

namespace MantisPunch
{
    public sealed class CombatEffects : MonoBehaviour
    {
        public static CombatEffects Current { get; private set; }
        public float HitStop { get; private set; }
        public float Shake { get; private set; }
        public float Zoom { get; private set; }
        public bool VerticalShake { get; private set; }
        float zoomAge = 10, zoomStrength;
        bool parryZoom;
        float parrySequence, focusAge;
        DuelFighter focusWinner, focusLoser;
        bool focusReleased;
        public bool ParryCinematic => parrySequence > 0;
        public float CombatSpeed => HitStop > 0 ? 0 : ParryCinematic && parrySequence <= .82f ? .2f : 1;
        public void CounterStarted(DuelFighter fighter)
        { if (fighter == focusWinner) parrySequence = 0; }
        public void FocusParry(DuelFighter winner, DuelFighter loser)
        { focusWinner = winner; focusLoser = loser; focusAge = 0; focusReleased = false; }
        float parryDelay = -1;
        Vector3 parryPoint, parryDirection;
        public int Played { get; private set; }
        readonly Dictionary<CombatBeat, Material> materials = new Dictionary<CombatBeat, Material>();
        readonly Dictionary<CombatBeat, AudioClip[]> sounds = new Dictionary<CombatBeat, AudioClip[]>();
        readonly Dictionary<CombatBeat, int> soundVariations = new Dictionary<CombatBeat, int>();
        readonly List<CombatBurst> bursts = new List<CombatBurst>();
        AudioSource audioSource;
        public void Initialize(Shader shader)
        {
            Current = this;
            audioSource = gameObject.AddComponent<AudioSource>(); audioSource.spatialBlend = 0; audioSource.volume = .65f;
            audioSource.playOnAwake = false;
            foreach (CombatBeat beat in System.Enum.GetValues(typeof(CombatBeat)))
            {
                var material = new Material(shader);
                Color color = beat == CombatBeat.Guard ? new Color(1, .72f, .25f) : beat == CombatBeat.Parry ? new Color(.45f, .9f, 1) : beat == CombatBeat.Knockout ? new Color(1, .94f, .75f) : beat == CombatBeat.Land ? new Color(.6f, .48f, .28f) : new Color(.55f, .9f, .93f);
                material.SetColor("_BaseColor", color); materials[beat] = material;
                if (beat == CombatBeat.GuardBreak) material.SetColor("_BaseColor", new Color(1, .35f, .12f));
                sounds[beat] = new[] { CombatSoundDesign.Create(beat, 0), CombatSoundDesign.Create(beat, 1), CombatSoundDesign.Create(beat, 2) };
                soundVariations[beat] = 0;
            }
        }
        public void Play(CombatBeat beat, Vector3 position, Vector3 direction)
        {
            if (beat == CombatBeat.Prepare) return;
            if (beat == CombatBeat.Parry)
            {
                parryPoint = position; parryDirection = direction.normalized; parryDelay = -1;
                parrySequence = .96f; focusAge = 0; focusReleased = false;
                HitStop = Mathf.Max(HitStop, .06f);
                parryZoom = true; zoomAge = 0; zoomStrength = Zoom = 10;
                Emit(beat, position, direction);
                return;
            }
            Emit(beat, position, direction);
        }
        void Emit(CombatBeat beat, Vector3 position, Vector3 direction)
        {
            if (beat == CombatBeat.Deflected) return;
            Played++;
            if (beat == CombatBeat.Guard || beat == CombatBeat.Parry || beat == CombatBeat.Knockout || beat == CombatBeat.GuardBreak || beat == CombatBeat.Clash)
                GetComponent<ReefMusic>()?.Accent(beat);
            int variation = soundVariations[beat]; soundVariations[beat] = (variation + 1) % 3;
            audioSource.PlayOneShot(sounds[beat][variation], CombatSoundDesign.Gain(beat));
            var imported = GetComponent<ImportedHitEffects>();
            if (!imported || !imported.Play(beat, position, direction))
            {
                var burst = new GameObject("Combat " + beat).AddComponent<CombatBurst>();
                burst.Setup(position, direction, materials[beat], beat); bursts.Add(burst);
            }
            if (beat != CombatBeat.Swing && beat != CombatBeat.Land && beat != CombatBeat.Kick && beat != CombatBeat.Prepare)
            {
                HitStop = Mathf.Max(HitStop, beat == CombatBeat.Knockout ? .18f : beat == CombatBeat.GuardBreak ? .12f : beat == CombatBeat.Parry ? .06f : .05f);
                if (beat != CombatBeat.Parry) Shake = Mathf.Max(Shake, beat == CombatBeat.Knockout ? .18f : .055f);
                else { Shake = 0; }
                VerticalShake = beat == CombatBeat.Guard || beat == CombatBeat.GuardBreak;
                if (beat == CombatBeat.Parry || beat == CombatBeat.GuardBreak)
                { parryZoom = beat == CombatBeat.Parry; zoomAge = 0; zoomStrength = beat == CombatBeat.GuardBreak ? 13 : 10; Zoom = zoomStrength; }
            }
        }
        public void Clear()
        {
            GetComponent<ImportedHitEffects>()?.Clear();
            foreach (var burst in bursts) if (burst) Destroy(burst.gameObject);
            bursts.Clear(); HitStop = Shake = Zoom = 0; zoomAge = 10; audioSource.Stop();
            parryDelay = -1; parryZoom = false;
            parrySequence = focusAge = 0; focusWinner = focusLoser = null; focusReleased = false;
        }
        void Update()
        {
            if (Time.timeScale == 0) return;
            HitStop = Mathf.Max(0, HitStop - Time.unscaledDeltaTime);
            parrySequence = Mathf.Max(0, parrySequence - Time.unscaledDeltaTime);
            if (parryZoom)
            {
                focusAge += Time.unscaledDeltaTime;
                if (!ParryCinematic && (focusAge >= 1.6f || !focusWinner || !focusLoser ||
                    !focusWinner.Alive || !focusLoser.Alive ||
                    (focusWinner.State == DuelState.Attack && focusWinner.StrikeSpent) ||
                    (focusLoser.State != DuelState.Recovery && focusWinner.State != DuelState.Attack))) focusReleased = true;
            }
            if (parryDelay >= 0)
            {
                parryDelay -= Time.unscaledDeltaTime;
                if (parryDelay <= 0) { parryDelay = -1; Emit(CombatBeat.Parry, parryPoint, parryDirection); }
            }
            Shake = Mathf.MoveTowards(Shake, 0, Time.unscaledDeltaTime * .6f);
            if (parryZoom && (!focusReleased || HitStop > 0)) { zoomAge = 0; Zoom = zoomStrength; }
            else
            {
                zoomAge += Time.unscaledDeltaTime;
                Zoom = zoomStrength * (1 - Mathf.SmoothStep(0, 1, Mathf.InverseLerp(parryZoom ? 0 : .10f, parryZoom ? .18f : .4f, zoomAge)));
            }
        }
        void OnDestroy()
        {
            if (Current == this) Current = null;
            foreach (var material in materials.Values) Destroy(material);
            foreach (var variants in sounds.Values) foreach (var sound in variants) Destroy(sound);
        }
    }
    public sealed class CombatBurst : MonoBehaviour
    {
        LineRenderer[] lines; Vector3 origin, direction; float age, life, radius; CombatBeat kind;
        public void Setup(Vector3 position, Vector3 forward, Material material, CombatBeat beat)
        {
            origin = position; direction = forward; kind = beat; life = beat == CombatBeat.Knockout ? .55f : .28f;
            if (beat == CombatBeat.Prepare) life = .12f;
            radius = beat == CombatBeat.Knockout ? 2.1f : beat == CombatBeat.Land ? 1.1f : beat == CombatBeat.Kick ? .85f : .6f;
            if (beat == CombatBeat.GuardBreak) { radius = .95f; life = .45f; }
            if (beat == CombatBeat.Prepare) radius = .16f;
            if (beat == CombatBeat.Parry) { radius = .85f; life = .32f; }
            lines = new LineRenderer[beat == CombatBeat.Parry ? 21 : beat == CombatBeat.Swing ? 3 : 13];
            for (int i = 0; i < lines.Length; i++)
            {
                var line = new GameObject("Water streak").AddComponent<LineRenderer>(); line.transform.SetParent(transform);
                line.sharedMaterial = material; line.positionCount = i == 0 && beat != CombatBeat.Swing && beat != CombatBeat.Parry ? 33 : 2; line.useWorldSpace = true;
                lines[i] = line;
            }
            UpdateGeometry();
        }
        void Update() { age += Time.unscaledDeltaTime; if (age >= life) { Destroy(gameObject); return; } UpdateGeometry(); }
        void UpdateGeometry()
        {
            if (lines == null) return;
            float t = age / life;
            for (int i = 0; i < lines.Length; i++)
            {
                lines[i].widthMultiplier = (kind == CombatBeat.Knockout ? .065f : .028f) * (1 - t);
                if (kind == CombatBeat.Parry)
                {
                    Vector3 ray = (Quaternion.AngleAxis(i * 137.5f, Vector3.up) * new Vector3(1, .2f + Mathf.Sin(i * 2.3f), 0)).normalized;
                    float speed = radius * (1.2f + (i % 5) * .22f);
                    Vector3 tip = origin + ray * speed * t + Vector3.down * t * t * .35f;
                    lines[i].widthMultiplier = (.014f + i % 3 * .005f) * (1 - t);
                    lines[i].SetPosition(0, tip - ray * (.08f + .12f * (1 - t)));
                    lines[i].SetPosition(1, tip);
                    continue;
                }
                if (lines[i].positionCount > 2)
                {
                    for (int n = 0; n < 33; n++) { float a = n / 32f * Mathf.PI * 2; lines[i].SetPosition(n, origin + new Vector3(Mathf.Cos(a), kind == CombatBeat.Land ? .02f : Mathf.Sin(a) * .45f, Mathf.Sin(a)) * radius * (.15f + t)); }
                }
                else
                {
                    Vector3 ray = kind == CombatBeat.Swing ? direction : kind == CombatBeat.Kick ? Quaternion.AngleAxis((i - 6) * 9, Vector3.up) * direction + Vector3.up * .12f : Quaternion.AngleAxis(i * 137.5f, Vector3.up) * new Vector3(1, Mathf.Sin(i * 2) * .7f, 0);
                    Vector3 from = origin + ray * radius * t;
                    lines[i].SetPosition(0, from); lines[i].SetPosition(1, from + ray * radius * (1 - t) * .55f + (kind == CombatBeat.Swing ? Vector3.up * i * .035f : Vector3.zero));
                }
            }
        }
    }
}



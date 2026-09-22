using System.Collections.Generic;
using UnityEngine;

namespace MantisPunch
{
    public sealed class ImportedHitEffects : MonoBehaviour
    {
        public GameObject guardPrefab, parryPrefab, hitPrefab;
        public GameObject waterHitPrefab;
        public Shader particleShader;
        readonly Dictionary<Material, Material> converted = new Dictionary<Material, Material>();
        readonly List<GameObject> active = new List<GameObject>();
        Material sprayMaterial;
        Texture2D sprayTexture;
        public int Played { get; private set; }
        public bool Play(CombatBeat beat, Vector3 position, Vector3 direction)
        {
            if (beat == CombatBeat.Kick && particleShader) { PlayTailSpray(position, direction); return true; }
            bool water = beat == CombatBeat.Knockout && waterHitPrefab;
            GameObject prefab = water ? waterHitPrefab : beat == CombatBeat.Guard ? guardPrefab : beat == CombatBeat.Parry ? parryPrefab :
                beat == CombatBeat.Knockout || beat == CombatBeat.GuardBreak || beat == CombatBeat.Clash ? hitPrefab : null;
            if (!prefab || !particleShader) return false;
            active.RemoveAll(item => !item);
            var instance = Instantiate(prefab, position, Quaternion.LookRotation(direction.sqrMagnitude > .001f ? direction : Vector3.forward));
            instance.name = "Imported impact " + beat;
            if (water)
            {
                // Keep the authored spray, replace polygonal flashes and shockwave meshes.
                foreach (var system in instance.GetComponentsInChildren<ParticleSystem>(true))
                    if (!system.name.ToLowerInvariant().Contains("spark"))
                    { system.Stop(false, ParticleSystemStopBehavior.StopEmittingAndClear); var emission = system.emission; emission.enabled = false; }
                foreach (var renderer in instance.GetComponentsInChildren<MeshRenderer>(true)) renderer.enabled = false;
            }
            instance.transform.localScale = Vector3.one * (beat == CombatBeat.Guard ? .38f : beat == CombatBeat.Parry ? .55f : .7f);
            foreach (var renderer in instance.GetComponentsInChildren<ParticleSystemRenderer>(true))
            {
                var list = renderer.sharedMaterials;
                for (int i = 0; i < list.Length; i++)
                {
                    var source = list[i]; if (!source) continue;
                    if (!converted.TryGetValue(source, out var material))
                    {
                        material = new Material(particleShader) { name = source.name + " Mantis URP" };
                        if (source.HasProperty("_MainTex")) material.SetTexture("_MainTex", source.GetTexture("_MainTex"));
                        material.SetColor("_Tint", source.HasProperty("_Color") ? source.GetColor("_Color") : Color.white);
                        converted.Add(source, material);
                    }
                    list[i] = material;
                }
                renderer.sharedMaterials = list;
            }
            foreach (var system in instance.GetComponentsInChildren<ParticleSystem>(true))
            {
                system.Stop(false, ParticleSystemStopBehavior.StopEmittingAndClear);
                var main = system.main; main.loop = false; main.useUnscaledTime = true;
                main.maxParticles = Mathf.Min(main.maxParticles, 100);
                main.stopAction = ParticleSystemStopAction.None;
                if (water)
                {
                    main.startColor = new ParticleSystem.MinMaxGradient(new Color(.55f, 1, .92f), Color.white);
                    main.startLifetime = .15f;
                    var color = system.colorOverLifetime; color.enabled = true;
                    var fade = new Gradient(); fade.SetKeys(new[] { new GradientColorKey(Color.white, 0), new GradientColorKey(new Color(.4f, 1, .85f), 1) }, new[] { new GradientAlphaKey(1, 0), new GradientAlphaKey(0, 1) }); color.color = fade;
                    var speedColor = system.colorBySpeed; speedColor.enabled = false;
                }
            }
            foreach (var system in instance.GetComponentsInChildren<ParticleSystem>()) system.Play(false);
            if (water) instance.AddComponent<WaterImpact>().Initialize(particleShader, direction);
            if (beat == CombatBeat.Parry || beat == CombatBeat.GuardBreak)
                instance.AddComponent<ShortImpactLifetime>().Initialize(beat == CombatBeat.Parry ? .22f : .28f);
            active.Add(instance); Destroy(instance, 4); Played++; return true;
        }
        public void Clear() { foreach (var item in active) if (item) Destroy(item); active.Clear(); }
        void PlayTailSpray(Vector3 position, Vector3 direction)
        {
            if (!sprayMaterial)
            {
                sprayTexture = new Texture2D(32, 32, TextureFormat.RGBA32, false);
                sprayTexture.wrapMode = TextureWrapMode.Clamp;
                for (int y = 0; y < 32; y++) for (int x = 0; x < 32; x++)
                {
                    float r = new Vector2((x - 15.5f) / 15.5f, (y - 15.5f) / 15.5f).magnitude;
                    float alpha = Mathf.Exp(-Mathf.Pow((r - .58f) / .22f, 2)) * .55f;
                    sprayTexture.SetPixel(x, y, new Color(1, 1, 1, r >= 1 ? 0 : alpha));
                }
                sprayTexture.Apply();
                sprayMaterial = new Material(particleShader);
                sprayMaterial.SetTexture("_MainTex", sprayTexture); sprayMaterial.SetColor("_Tint", Color.white);
            }
            active.RemoveAll(item => !item);
            var root = new GameObject("Tail water spray"); root.transform.position = position + Vector3.up * .1f;
            var ps = root.AddComponent<ParticleSystem>(); ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            var main = ps.main; main.loop = false; main.playOnAwake = false; main.useUnscaledTime = true;
            main.simulationSpace = ParticleSystemSimulationSpace.World; main.maxParticles = 12;
            main.startLifetime = .24f; main.startSpeed = 0; main.startSize = .08f;
            var emission = ps.emission; emission.enabled = false;
            var shape = ps.shape; shape.enabled = false;
            var fade = ps.colorOverLifetime; fade.enabled = true;
            var gradient = new Gradient(); gradient.SetKeys(new[] { new GradientColorKey(new Color(.55f, .9f, .88f), 0), new GradientColorKey(new Color(.4f, .75f, .8f), 1) }, new[] { new GradientAlphaKey(.65f, 0), new GradientAlphaKey(0, 1) }); fade.color = gradient;
            ps.GetComponent<ParticleSystemRenderer>().sharedMaterial = sprayMaterial;
            ps.Play();
            Vector3 rear = direction.normalized, side = Vector3.Cross(rear, Vector3.up).normalized;
            for (int i = 0; i < 10; i++)
            {
                float offset = Mathf.Sin(i * 2.4f);
                ps.Emit(new ParticleSystem.EmitParams {
                    position = root.transform.position + side * offset * .13f,
                    velocity = rear * (.8f + i % 3 * .3f) + side * offset * .35f + Vector3.up * (.1f + i % 2 * .18f),
                    startSize = .045f + i % 4 * .014f, startLifetime = .17f + i % 3 * .035f,
                    startColor = Color.white
                }, 1);
            }
            active.Add(root); Destroy(root, .30f); Played++;
        }
        void OnDestroy() { Clear(); foreach (var material in converted.Values) Destroy(material); if (sprayMaterial) Destroy(sprayMaterial); if (sprayTexture) Destroy(sprayTexture); }
    }
    public sealed class ShortImpactLifetime : MonoBehaviour
    {
        float age, lifetime;
        bool stopped;
        ParticleSystem[] particles;
        Renderer[] renderers;
        Color[] tints;
        MaterialPropertyBlock properties;
        public void Initialize(float seconds)
        {
            properties = new MaterialPropertyBlock();
            lifetime = seconds;
            particles = GetComponentsInChildren<ParticleSystem>(true);
            renderers = GetComponentsInChildren<Renderer>(true);
            tints = new Color[renderers.Length];
            for (int i = 0; i < renderers.Length; i++)
                tints[i] = renderers[i].sharedMaterial && renderers[i].sharedMaterial.HasProperty("_Tint") ? renderers[i].sharedMaterial.GetColor("_Tint") : Color.white;
        }
        void Update()
        {
            age += Time.unscaledDeltaTime;
            if (!stopped && age >= .035f)
            {
                stopped = true;
                foreach (var particle in particles) if (particle) particle.Stop(false, ParticleSystemStopBehavior.StopEmitting);
            }
            float alpha = 1 - Mathf.SmoothStep(0, 1, Mathf.InverseLerp(lifetime - .08f, lifetime, age));
            for (int i = 0; i < renderers.Length; i++)
            {
                if (!renderers[i]) continue;
                renderers[i].GetPropertyBlock(properties);
                Color tint = tints[i]; tint.a *= alpha; properties.SetColor("_Tint", tint);
                renderers[i].SetPropertyBlock(properties);
            }
            if (age >= lifetime) Destroy(gameObject);
        }
    }
}

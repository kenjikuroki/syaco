using UnityEngine;

namespace MantisPunch
{
    public sealed class TrainingTarget : MonoBehaviour
    {
        public Renderer body;
        public int maximumHealth = 5;
        public int Health { get; private set; }
        public int HitCount { get; private set; }
        public float LastHit { get; private set; } = -10;
        Vector3 originalScale;
        MaterialPropertyBlock block;
        Color baseColor;
        float respawn;
        void Awake()
        {
            Health = maximumHealth; originalScale = body.transform.localScale;
            baseColor = body.sharedMaterial.color; block = new MaterialPropertyBlock();
        }
        public bool ReceiveHit(Vector3 direction)
        {
            if (Health <= 0) return false;
            Health--; HitCount++; LastHit = Time.time;
            if (Health == 0) respawn = Time.time + 2;
            return true;
        }
        void Update()
        {
            float flash = Mathf.Clamp01(1 - (Time.time - LastHit) / .25f);
            block.SetColor("_Color", Color.Lerp(baseColor, new Color(1, .9f, .55f), flash));
            block.SetColor("_BaseColor", Color.Lerp(baseColor, new Color(1, .9f, .55f), flash));
            body.SetPropertyBlock(block);
            body.transform.localScale = originalScale * (Health == 0 ? .45f : 1 + Mathf.Sin(flash * Mathf.PI) * .12f);
            if (Health == 0 && Time.time >= respawn) ResetTarget();
        }
        public void ResetTarget() { Health = maximumHealth; LastHit = -10; }
        public static void ResetAll()
        {
            foreach (TrainingTarget target in FindObjectsByType<TrainingTarget>(FindObjectsSortMode.None)) target.ResetTarget();
        }
    }
}

using UnityEngine;
namespace MantisPunch
{
    public sealed class WaterImpact : MonoBehaviour
    {
        LineRenderer[] lines = new LineRenderer[29];
        Material material;
        float age;
        Vector3 forward, side, up, origin;
        bool stopped;
        public void Initialize(Shader shader, Vector3 direction)
        {
            origin = transform.position; forward = direction.normalized;
            side = Vector3.Cross(forward, Vector3.up).normalized; up = Vector3.Cross(side, forward).normalized;
            material = new Material(shader); material.SetColor("_Tint", Color.white);
            for (int i = 0; i < lines.Length; i++)
            {
                var child = new GameObject("Water streak"); child.transform.SetParent(transform);
                var line = child.AddComponent<LineRenderer>(); lines[i] = line;
                line.sharedMaterial = material; line.useWorldSpace = true; line.positionCount = i < 5 ? 9 : 2;
                line.numCapVertices = 2;
            }
            UpdateGeometry();
        }
        void Update()
        {
            age += Time.unscaledDeltaTime;
            if (!stopped && age >= .025f)
            { stopped = true; foreach (var ps in GetComponentsInChildren<ParticleSystem>()) ps.Stop(false, ParticleSystemStopBehavior.StopEmitting); }
            if (age >= .30f) { Destroy(gameObject); return; }
            UpdateGeometry();
        }
        void UpdateGeometry()
        {
            for (int i = 0; i < lines.Length; i++)
            {
                var line = lines[i]; float life = i < 21 ? .15f : .30f;
                float t = Mathf.Clamp01(age / life); float fade = (1 - t) * (1 - t);
                line.startColor = new Color(.85f, 1, 1, fade); line.endColor = new Color(.25f, 1, .8f, 0);
                line.startWidth = (i < 5 ? .055f : .065f) * (1 - t); line.endWidth = .004f;
                if (i < 5)
                {
                    for (int j = 0; j < 9; j++)
                    {
                        float a = (i * 72 + j * 6) * Mathf.Deg2Rad;
                        float radius = (.15f + t * 1.15f) * (1 + .1f * Mathf.Sin(j * 2 + i));
                        line.SetPosition(j, origin + (side * Mathf.Cos(a) + up * Mathf.Sin(a)) * radius + forward * t * .25f);
                    }
                }
                else
                {
                    float a = i * 137.5f * Mathf.Deg2Rad;
                    Vector3 ray = (side * Mathf.Cos(a) + up * Mathf.Sin(a)) * .65f + forward * (i % 3 == 0 ? 1.6f : .5f);
                    Vector3 tip = origin + ray * (.15f + t * 1.4f);
                    line.SetPosition(0, tip - ray * (i < 21 ? .4f : .08f) * (1 - t)); line.SetPosition(1, tip);
                }
            }
        }
        void OnDestroy() { if (material) Destroy(material); }
    }
}

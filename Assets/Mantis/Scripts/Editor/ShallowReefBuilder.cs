using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

public static class ShallowReefBuilder
{
    const string Folder = "Assets/Mantis/Art/ShallowReef";
    static System.Random random;
    static float R(float a, float b) => Mathf.Lerp(a, b, (float)random.NextDouble());
    sealed class Geometry
    {
        public List<Vector3> vertices = new List<Vector3>();
        public List<int> triangles = new List<int>();
        public List<Color> colors = new List<Color>();
        public int Add(Vector3 p, float weight = 0) { vertices.Add(p); colors.Add(new Color(weight, 1, 1)); return vertices.Count - 1; }
        public void Tri(int a, int b, int c) { triangles.Add(a); triangles.Add(b); triangles.Add(c); }
        public void Rock(Vector3 center, Vector3 size)
        {
            int start = vertices.Count;
            for (int row = 0; row <= 5; row++)
            {
                float phi = row * Mathf.PI / 5;
                for (int col = 0; col < 9; col++)
                {
                    float angle = col * Mathf.PI * 2 / 9;
                    Vector3 p = new Vector3(Mathf.Sin(phi) * Mathf.Cos(angle), Mathf.Cos(phi), Mathf.Sin(phi) * Mathf.Sin(angle));
                    Add(center + Vector3.Scale(p, size) * R(.86f, 1.12f));
                }
            }
            for (int row = 0; row < 5; row++) for (int col = 0; col < 9; col++)
            {
                int a = start + row * 9 + col, b = start + row * 9 + (col + 1) % 9;
                Tri(a, b, a + 9); Tri(b, b + 9, a + 9);
            }
        }
        public void Branch(Vector3 a, Vector3 b, float radius)
        {
            Vector3 direction = (b - a).normalized;
            Vector3 x = Vector3.Cross(direction, Vector3.forward).normalized;
            if (x.sqrMagnitude < .1f) x = Vector3.right;
            Vector3 z = Vector3.Cross(direction, x);
            int start = vertices.Count;
            for (int row = 0; row < 2; row++) for (int j = 0; j < 6; j++)
            {
                float angle = j * Mathf.PI / 3;
                Add((row == 0 ? a : b) + (x * Mathf.Cos(angle) + z * Mathf.Sin(angle)) * radius * (row == 0 ? 1 : .55f));
            }
            for (int j = 0; j < 6; j++) { int a0 = start + j, b0 = start + (j + 1) % 6; Tri(a0, b0, a0 + 6); Tri(b0, b0 + 6, a0 + 6); }
            for (int j = 1; j < 5; j++) Tri(start + 6, start + 6 + j, start + 7 + j);
        }
        public void Grass(Vector3 basePoint, float height, float angle)
        {
            Vector3 side = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle));
            int start = vertices.Count;
            for (int row = 0; row <= 5; row++)
            {
                float t = row / 5f, width = .10f * (1 - t) + .004f;
                Vector3 center = basePoint + Vector3.up * height * t + side * (.27f * t * t);
                Add(center - side * width, t); Add(center + side * width, t);
            }
            for (int j = 0; j < 5; j++) { int a = start + j * 2; Tri(a, a + 2, a + 1); Tri(a + 1, a + 2, a + 3); }
        }
        public GameObject Save(string name, Transform root, Material material)
        {
            Mesh mesh = new Mesh { name = name };
            mesh.SetVertices(vertices); mesh.SetTriangles(triangles, 0); mesh.SetColors(colors); mesh.RecalculateNormals(); mesh.RecalculateBounds();
            // Include the vertex shader's maximum sway in culling bounds.
            var bounds = mesh.bounds; bounds.Expand(.5f); mesh.bounds = bounds;
            mesh = Store(mesh, name + ".asset");
            GameObject ob = new GameObject(name); ob.transform.SetParent(root, false);
            ob.AddComponent<MeshFilter>().sharedMesh = mesh;
            var renderer = ob.AddComponent<MeshRenderer>(); renderer.sharedMaterial = material;
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            return ob;
        }
    }
    static T Store<T>(T asset, string name) where T : Object
    {
        string path = Folder + "/" + name;
        T existing = AssetDatabase.LoadAssetAtPath<T>(path);
        if (existing) { EditorUtility.CopySerialized(asset, existing); Object.DestroyImmediate(asset); return existing; }
        AssetDatabase.CreateAsset(asset, path); return asset;
    }
    static Material Mat(string name, Color color, string shader = "Universal Render Pipeline/Lit")
    {
        var material = new Material(Shader.Find(shader)); material.SetColor("_BaseColor", color); material.SetFloat("_Smoothness", .12f);
        return Store(material, name + ".mat");
    }
    public static void Build(Camera camera)
    {
        if (!AssetDatabase.IsValidFolder(Folder)) AssetDatabase.CreateFolder("Assets/Mantis/Art", "ShallowReef");
        random = new System.Random(2309);
        GameObject root = new GameObject("Shallow Reef Stage");
        foreach (var ob in Object.FindObjectsByType<Transform>(FindObjectsSortMode.None))
        {
            if (ob.name == "Seafloor" || ob.name == "Sand marker" || ob.name == "Rock obstacle") Object.DestroyImmediate(ob.gameObject);
            else if (ob.name.EndsWith(" boundary"))
            {
                ob.SetParent(root.transform, true);
                // Preserve the original playable area; scenery sits beyond it.
                Object.DestroyImmediate(ob.GetComponent<MeshRenderer>()); Object.DestroyImmediate(ob.GetComponent<MeshFilter>());
            }
        }
        var sand = new Geometry(); sand.Add(new Vector3(-16, 0, -16)); sand.Add(new Vector3(-16, 0, 16)); sand.Add(new Vector3(16, 0, 16)); sand.Add(new Vector3(16, 0, -16)); sand.Tri(0, 1, 2); sand.Tri(0, 2, 3);
        var floor = sand.Save("Rippled sand", root.transform, Mat("Sand", new Color(.66f, .69f, .51f), "Mantis/Shallow Water Sand"));
        var collider = floor.AddComponent<BoxCollider>(); collider.center = new Vector3(0, -.1f, 0); collider.size = new Vector3(32, .2f, 32);
        var rocks = new[] { new Geometry(), new Geometry(), new Geometry() };
        var grass = new Geometry(); var coral = new Geometry(); var shells = new Geometry();
        for (int edge = 0; edge < 4; edge++) for (int n = 0; n < 11; n++)
        {
            float x = -10 + n * 2 + R(-.6f, .6f), z = R(9.1f, 11.5f);
            Vector3 p = Quaternion.Euler(0, edge * 90, 0) * new Vector3(x, 0, z);
            float height = R(.55f, 1.7f);
            rocks[(n + edge) % 3].Rock(p + Vector3.up * height * .35f, new Vector3(R(.9f, 1.8f), height, R(.8f, 1.4f)));
            Vector3 plant = Quaternion.Euler(0, edge * 90, 0) * new Vector3(R(-9, 9), 0, R(9.7f, 12.5f));
            for (int blade = 0, count = R(0, 1) < .5f ? 0 : 6; blade < count; blade++) grass.Grass(plant + new Vector3(R(-.35f, .35f), .02f, R(-.35f, .35f)), R(.6f, 1.6f), R(0, Mathf.PI * 2));
            if (R(0, 1) < .20f)
            {
                Vector3 stem = p * .90f;
                for (int branch = 0; branch < 5; branch++)
                {
                    Vector3 end = stem + new Vector3(R(-.4f, .4f), R(.4f, .95f), R(-.4f, .4f));
                    coral.Branch(stem, end, .065f);
                    coral.Branch(Vector3.Lerp(stem, end, .6f), end + new Vector3(.18f, .16f, .12f), .035f);
                }
            }
        }
        for (int i = 0; i < 48; i++)
        {
            float angle = R(0, Mathf.PI * 2), radius = R(4.5f, 8);
            shells.Rock(new Vector3(Mathf.Cos(angle) * radius, .015f, Mathf.Sin(angle) * radius), new Vector3(.10f, .035f, .075f));
        }
        ImportedReefBuilder.Build(root.transform);
        grass.Save("Swaying sea grass", root.transform, Mat("Sea grass", new Color(.12f, .40f, .27f), "Mantis/Sea Grass"));
        coral.Save("Coral branches", root.transform, Mat("Coral", new Color(.65f, .36f, .25f)));
        var particles = new GameObject("Suspended plankton"); particles.transform.SetParent(root.transform, false); particles.transform.localPosition = new Vector3(0, 1.7f, 0);
        var ps = particles.AddComponent<ParticleSystem>(); ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        var main = ps.main; main.loop = true; main.prewarm = true; main.startLifetime = 14; main.startSpeed = .025f; main.maxParticles = 90; main.startSize = new ParticleSystem.MinMaxCurve(.008f, .022f); main.startColor = new Color(.65f, .92f, .84f, .23f); main.simulationSpace = ParticleSystemSimulationSpace.Local;
        var emission = ps.emission; emission.rateOverTime = 5;
        var shape = ps.shape; shape.shapeType = ParticleSystemShapeType.Box; shape.scale = new Vector3(19, 3, 19);
        var velocity = ps.velocityOverLifetime; velocity.enabled = true; velocity.space = ParticleSystemSimulationSpace.Local; velocity.x = .035f; velocity.y = .018f; velocity.z = .012f;
        var particleMat = new Material(Shader.Find("Mantis/ImportedParticle")); particleMat.SetColor("_Tint", Color.white); ps.GetComponent<ParticleSystemRenderer>().sharedMaterial = Store(particleMat, "Plankton.mat");
        // Camera/environment setup belongs to the flat-screen scene, not the reusable stage.
        RenderSettings.fog = false; RenderSettings.ambientMode = AmbientMode.Flat; RenderSettings.ambientLight = new Color(.44f, .60f, .59f);
        camera.clearFlags = CameraClearFlags.SolidColor; camera.backgroundColor = new Color(.075f, .29f, .33f);
        var sun = GameObject.Find("Sun / underwater light").GetComponent<Light>(); sun.color = new Color(.88f, 1, .91f); sun.intensity = 1.25f; sun.transform.rotation = Quaternion.Euler(52, -28, 0);
        PrefabUtility.SaveAsPrefabAssetAndConnect(root, Folder + "/ShallowReef.prefab", InteractionMode.AutomatedAction);
        AssetDatabase.SaveAssets();
    }
}




using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public static class ImportedReefBuilder
{
    const string Shell = "Assets/seaShell/prefabs/URP/seaShell.prefab";
    const string Folder = "Assets/Mantis/Art/ShallowReef/";
    static readonly Dictionary<Material, Material> materials = new Dictionary<Material, Material>();
    static System.Random random;
    static float R(float a, float b) => Mathf.Lerp(a, b, (float)random.NextDouble());
    static Material Adapt(Material original)
    {
        if (!original) throw new Exception("Missing imported reef material");
        if (materials.TryGetValue(original, out var cached)) return cached;
        var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        mat.CopyPropertiesFromMaterial(original); mat.enableInstancing = true;
        mat.SetFloat("_Smoothness", .2f);
        string path = Folder + "Imported_" + original.name.Replace("/", "_") + ".mat";
        var existing = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (existing) { EditorUtility.CopySerialized(mat, existing); UnityEngine.Object.DestroyImmediate(mat); mat = existing; }
        else AssetDatabase.CreateAsset(mat, path);
        materials[original] = mat; return mat;
    }
    static void Place(GameObject prefab, Transform parent, Vector3 position, float size, float yaw, string label)
    {
        var ob = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        ob.name = label; ob.transform.SetParent(parent, false);
        ob.transform.rotation = Quaternion.Euler(0, yaw, 0)
            * (label.StartsWith("Seashell") ? Quaternion.Euler(R(12, 32), 0, R(-10, 10)) : Quaternion.Euler(R(-7, 7), 0, R(-6, 6)))
            * ob.transform.rotation;
        var renderers = ob.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0) throw new Exception("No renderer in " + prefab.name);
        Bounds bounds = renderers[0].bounds;
        foreach (var renderer in renderers) bounds.Encapsulate(renderer.bounds);
        float longest = Mathf.Max(bounds.size.x, bounds.size.y, bounds.size.z);
        if (longest < .001f) throw new Exception("Empty bounds for " + prefab.name);
        ob.transform.localScale *= size / longest;
        bounds = renderers[0].bounds; foreach (var renderer in renderers) bounds.Encapsulate(renderer.bounds);
        if (!label.StartsWith("Seashell"))
        {
            var scale=ob.transform.localScale;
            scale.y*=Mathf.Min(1,size*.38f/bounds.size.y); ob.transform.localScale=scale;
            bounds=renderers[0].bounds; foreach(var renderer in renderers) bounds.Encapsulate(renderer.bounds);
        }
        ob.transform.position += position - new Vector3(bounds.center.x, bounds.min.y + size * (label.StartsWith("Seashell") ? .10f : .025f), bounds.center.z);
        foreach (var renderer in renderers)
        {
            var shared = renderer.sharedMaterials;
            for (int i = 0; i < shared.Length; i++) shared[i] = Adapt(shared[i]);
            renderer.sharedMaterials = shared;
        }
        // Decorative models do not add collision obstacles to the existing duel area.
        foreach (var collider in ob.GetComponentsInChildren<Collider>()) UnityEngine.Object.DestroyImmediate(collider);
    }
    public static void Build(Transform root)
    {
        var shell = AssetDatabase.LoadAssetAtPath<GameObject>(Shell);
        if (!shell) throw new Exception("URP seashell prefab is missing");
        random = new System.Random(2319); materials.Clear();
        var rocks = new GameObject[11];
        for (int i = 0; i < rocks.Length; i++)
        {
            rocks[i] = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/PolyOne/Rocks Stylized/Prefabs/SM_Rocks_" + (i + 1).ToString("D2") + ".prefab");
            if (!rocks[i]) throw new Exception("Missing imported rock " + i);
        }
        var rockRoot = new GameObject("Imported rock formations"); rockRoot.transform.SetParent(root, false);
        var shellRoot = new GameObject("Imported seashell clusters"); shellRoot.transform.SetParent(root, false);
        int shellCount=0;
        int rockCount = 0;
        for (int edge = 0; edge < 4; edge++) for (int group = 0; group < 5; group++)
        {
            var rotation = Quaternion.Euler(0, edge * 90, 0);
            Vector3 center = rotation * new Vector3(-8 + group * 4 + R(-1.5f, 1.5f), 0, R(10.8f, 13.2f));
            Place(rocks[(edge * 5 + group) % 11], rockRoot.transform, center, R(2.7f, 3.8f), R(0, 360), "Reef rock large " + rockCount++);
            Vector3 offset = rotation * new Vector3(R(.8f, 1.35f), 0, R(-.6f, .3f));
            Place(rocks[(edge * 5 + group + 4) % 11], rockRoot.transform, center + offset, R(1.05f, 1.65f), R(0, 360), "Reef rock small " + rockCount++);
            if ((edge+group)%3==0)
                Place(shell,shellRoot.transform,center+rotation*new Vector3(R(-.8f,.8f),0,-.7f),R(.9f,1.2f),R(0,360),"Seashell "+shellCount++);
        }
        foreach(var renderer in shellRoot.GetComponentsInChildren<Renderer>())
            if(renderer.bounds.min.x<8.7f && renderer.bounds.max.x>-8.7f && renderer.bounds.min.z<8.7f && renderer.bounds.max.z>-8.7f)
                throw new Exception("Shell overlaps playable floor: "+renderer.name);
        int triangles = 0;
        foreach (var mesh in rockRoot.GetComponentsInChildren<MeshFilter>()) triangles += mesh.sharedMesh.triangles.Length / 3;
        foreach (var mesh in shellRoot.GetComponentsInChildren<MeshFilter>()) triangles += mesh.sharedMesh.triangles.Length / 3;
        Directory.CreateDirectory("QA");
        File.WriteAllText("QA/imported-reef.txt", "PASS: " + rockCount + " imported rocks, " + shellCount + " seashells, " + triangles + " placed triangles; source prefabs preserved, decorative colliders removed.");
    }
}

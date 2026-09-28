using System;
using System.IO;
using UnityEngine;
using UnityEditor;
using MantisPunch;
public static class A6ModelBuilder
{
    public static void Build()
    {
        const string root = "Assets/Mantis/Art/A6/";
        var importer = (ModelImporter)AssetImporter.GetAtPath(root + "Mantis_A6.fbx");
        importer.animationType = ModelImporterAnimationType.Generic;
        importer.importAnimation = true; importer.SaveAndReimport();
        var model = AssetDatabase.LoadAssetAtPath<GameObject>(root + "Mantis_A6.fbx");
        var instance = (GameObject)PrefabUtility.InstantiatePrefab(model);
        instance.name = "Mantis A6 Modular";
        var material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        material.SetTexture("_BaseMap", AssetDatabase.LoadAssetAtPath<Texture2D>(root + "mantis_albedo.png"));
        material.SetFloat("_Smoothness", .22f);
        var existing = AssetDatabase.LoadAssetAtPath<Material>(root + "A6Shell.mat");
        if (existing) { EditorUtility.CopySerialized(material, existing); UnityEngine.Object.DestroyImmediate(material); material = existing; }
        else AssetDatabase.CreateAsset(material, root + "A6Shell.mat");
        var body = instance.AddComponent<MantisModularBody>();
        string[] names = { "01_Cephalothorax", "02_PunchArm_L", "03_PunchArm_R", "04_Abdomen", "05_Tail", "06_WalkingLegs" };
        var renderers = instance.GetComponentsInChildren<SkinnedMeshRenderer>();
        if (renderers.Length != 6) throw new Exception("Expected exactly six skinned body parts");
        int triangles = 0;
        for (int i = 0; i < names.Length; i++)
        {
            foreach (var renderer in renderers) if (renderer.name == names[i]) body.parts[i] = renderer;
            var part = body.parts[i]; if (!part) throw new Exception("Missing slot " + names[i]);
            part.sharedMaterial = material;
            if (part.bones.Length == 0) throw new Exception("Unrigged part");
            foreach (var bone in part.bones) if (!bone) throw new Exception("Missing deform bone");
            if (!body.Replace((MantisBodySlot)i, part)) throw new Exception("Slot remapping failed");
            triangles += part.sharedMesh.triangles.Length / 3;
        }
        bool animation = false;
        foreach (var asset in AssetDatabase.LoadAllAssetsAtPath(root + "Mantis_A6.fbx"))
            if (asset is AnimationClip clip && !clip.name.StartsWith("__preview__") && clip.length > 0) animation = true;
        if (!animation) throw new Exception("Missing punch animation");
        PrefabUtility.SaveAsPrefabAsset(instance, root + "Mantis_A6_Modular.prefab");
        UnityEngine.Object.DestroyImmediate(instance); AssetDatabase.SaveAssets();
        Directory.CreateDirectory("QA"); File.WriteAllText("QA/A6-model.txt", "PASS: six skinned slots, bone remapping, imported animation; triangles=" + triangles);
    }
}

using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.Rendering;
using MantisPunch;

public static class PrototypeBuilder
{
    const string Root = "Assets/Mantis/";
    [MenuItem("Mantis/Rebuild practice scene")]
    public static void Setup()
    {
        Directory.CreateDirectory(Root + "Generated");
        AssetDatabase.Refresh();
        PlayerSettings.companyName = "MantisLab";
        PlayerSettings.productName = "Mantis Punch Lab";
        PlayerSettings.defaultScreenWidth = 1280; PlayerSettings.defaultScreenHeight = 800;
        PlayerSettings.fullScreenMode = FullScreenMode.Windowed;
        PlayerSettings.runInBackground = true;
        PlayerSettings.defaultInterfaceOrientation = UIOrientation.LandscapeLeft;
        PlayerSettings.colorSpace = ColorSpace.Linear;
        QualitySettings.vSyncCount = 0; QualitySettings.antiAliasing = 2;
        QualitySettings.shadows = ShadowQuality.All; QualitySettings.shadowDistance = 30;

        var importer = (ModelImporter)AssetImporter.GetAtPath(Root + "Art/Mantis.fbx");
        importer.animationType = ModelImporterAnimationType.Generic;
        importer.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel;
        importer.importAnimation = true; importer.optimizeGameObjects = false;
        importer.animationCompression = ModelImporterAnimationCompression.Off;
        importer.materialImportMode = ModelImporterMaterialImportMode.None;
        importer.isReadable = true;
        var clips = importer.defaultClipAnimations;
        foreach (var c in clips)
        {
            c.name = "Underhand_RearUp_Punch"; c.loopTime = false;
            c.lockRootRotation = true; c.lockRootPositionXZ = true; c.lockRootHeightY = true;
            c.keepOriginalOrientation = true; c.keepOriginalPositionXZ = true; c.keepOriginalPositionY = true;
        }
        importer.clipAnimations = clips; importer.SaveAndReimport();
        var clip = AssetDatabase.LoadAllAssetsAtPath(Root + "Art/Mantis.fbx").OfType<AnimationClip>().First(c => !c.name.StartsWith("__preview__") && c.length > .5f);
        var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(Root + "Art/MantisAlbedo.png");
        var shell = Material("Shell", new Color(1, 1, 1), .22f); shell.mainTexture = texture;
        var sand = Material("SeaFloor", new Color(.15f, .25f, .26f), .05f);
        var rock = Material("Rock", new Color(.10f, .18f, .19f), .15f);
        var gold = Material("Target", new Color(.78f, .39f, .12f), .25f);
        var trim = Material("TargetTrim", new Color(.14f, .60f, .57f), .3f);
        var dark = Material("TargetBase", new Color(.08f, .11f, .12f), .05f);
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        RenderSettings.ambientMode = AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(.40f, .56f, .62f);
        RenderSettings.fog = true; RenderSettings.fogMode = FogMode.ExponentialSquared;
        RenderSettings.fogColor = new Color(.045f, .15f, .20f); RenderSettings.fogDensity = .026f;
        var light = new GameObject("Sun / underwater light").AddComponent<Light>();
        light.type = LightType.Directional; light.intensity = 1.4f; light.color = new Color(.80f, .94f, 1);
        light.shadows = LightShadows.Soft; light.transform.rotation = Quaternion.Euler(50, -35, 0);
        Cube("Seafloor", new Vector3(0, -.1f, 0), new Vector3(20, .2f, 20), sand);
        Cube("North boundary", new Vector3(0, .4f, 9), new Vector3(18, .8f, .7f), rock);
        Cube("South boundary", new Vector3(0, .4f, -9), new Vector3(18, .8f, .7f), rock);
        Cube("West boundary", new Vector3(-9, .4f, 0), new Vector3(.7f, .8f, 18), rock);
        Cube("East boundary", new Vector3(9, .4f, 0), new Vector3(.7f, .8f, 18), rock);
        // Small marks and rocks give speed and distance cues without a large environment asset.
        var random = new System.Random(71);
        for (int i = 0; i < 32; i++)
        {
            float x = (float)random.NextDouble() * 16 - 8, z = (float)random.NextDouble() * 16 - 8;
            var mark = Cube("Sand marker", new Vector3(x, .003f, z), new Vector3(.045f, .003f, .24f), trim);
            UnityEngine.Object.DestroyImmediate(mark.GetComponent<Collider>());
        }
        foreach (var p in new[] { new Vector3(-5, .35f, -3), new Vector3(5, .35f, 1), new Vector3(-5, .35f, 4), new Vector3(4, .35f, -5) })
        {
            var stone = GameObject.CreatePrimitive(PrimitiveType.Sphere); stone.name = "Rock obstacle";
            stone.transform.position = p; stone.transform.localScale = new Vector3(1.5f, .7f, 1.0f); stone.GetComponent<Renderer>().sharedMaterial = rock;
        }
        var playerObject = new GameObject("MantisPlayer");
        var controller = playerObject.AddComponent<CharacterController>();
        controller.radius = .36f; controller.height = .70f; controller.center = Vector3.up * .35f;
        controller.skinWidth = .025f; controller.stepOffset = .12f; controller.minMoveDistance = 0;
        var player = playerObject.AddComponent<MantisPlayer>();
        var modelAsset = AssetDatabase.LoadAssetAtPath<GameObject>(Root + "Art/Mantis.fbx");
        var model = (GameObject)PrefabUtility.InstantiatePrefab(modelAsset);
        model.name = "Mantis visual v5"; model.transform.SetParent(playerObject.transform, false);
        Transform Find(string name) => model.GetComponentsInChildren<Transform>().First(t => t.name == name);
        var eyes = (Find("eye_L").position + Find("eye_R").position) / 2;
        Vector3 facing = eyes - Find("tail").position; facing.y = 0;
        model.transform.localRotation = Quaternion.FromToRotation(facing.normalized, Vector3.forward) * model.transform.localRotation;
        var renderer = model.GetComponentInChildren<SkinnedMeshRenderer>(); renderer.sharedMaterial = shell;
        var size = renderer.bounds.size; model.transform.localScale *= 1.9f / Mathf.Max(size.x, size.z);
        var bounds = renderer.bounds;
        model.transform.localPosition -= new Vector3(bounds.center.x, bounds.min.y, bounds.center.z);
        renderer.updateWhenOffscreen = true;
        var visual = model.AddComponent<MantisVisual>();
        visual.animator = model.GetComponent<Animator>(); visual.punchClip = clip;
        visual.leftClub = Find("dactyl_L"); visual.rightClub = Find("dactyl_R"); player.visual = visual;
        PrefabUtility.SaveAsPrefabAsset(playerObject, Root + "Generated/MantisPlayer.prefab");
        foreach (Vector3 p in new[] { new Vector3(0, 0, 3.4f), new Vector3(-3, 0, 5), new Vector3(3, 0, 5) })
        {
            var target = new GameObject("Training buoy"); target.transform.position = p;
            var hit = target.AddComponent<CapsuleCollider>(); hit.center = new Vector3(0, .55f, 0); hit.height = 1.1f; hit.radius = .38f;
            var baseObject = Cylinder("Base", target.transform, new Vector3(0, .09f, 0), new Vector3(.7f, .09f, .7f), dark);
            var bodyObject = Cylinder("Punch surface", target.transform, new Vector3(0, .65f, 0), new Vector3(.64f, .4f, .64f), gold);
            Cylinder("Upper band", target.transform, new Vector3(0, 1.00f, 0), new Vector3(.68f, .035f, .68f), trim);
            Cylinder("Lower band", target.transform, new Vector3(0, .30f, 0), new Vector3(.68f, .035f, .68f), trim);
            var training = target.AddComponent<TrainingTarget>(); training.body = bodyObject.GetComponent<Renderer>();
        }
        var cameraObject = new GameObject("Main Camera"); cameraObject.tag = "MainCamera";
        var camera = cameraObject.AddComponent<Camera>(); camera.fieldOfView = 43; camera.nearClipPlane = .05f; camera.farClipPlane = 80;
        camera.backgroundColor = RenderSettings.fogColor; camera.clearFlags = CameraClearFlags.SolidColor;
        cameraObject.transform.position = new Vector3(0, 4.68f, -6.5f); cameraObject.transform.LookAt(Vector3.up * .4f);
        cameraObject.AddComponent<AudioListener>();
        var follow = cameraObject.AddComponent<MantisCamera>(); follow.player = player;
        var hud = new GameObject("Controls and tuning").AddComponent<MantisHud>(); hud.player = player; hud.followCamera = follow;
        new GameObject("Runtime verification").AddComponent<RuntimeSmokeTest>();
        EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene(), Root + "Generated/Practice.unity");
        EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(Root + "Generated/Practice.unity", true) };
        AssetDatabase.SaveAssets();
        Directory.CreateDirectory("QA");
        File.WriteAllText("QA/editor-import.txt", "Unity " + Application.unityVersion + "\nClip: " + clip.name + " / " + clip.length + " seconds\nVertices: " + renderer.sharedMesh.vertexCount + "\nForward: " + playerObject.transform.InverseTransformDirection(Find("eye_L").position - Find("tail").position) + "\n");
        Debug.Log("MANTIS_SETUP_COMPLETE");
    }
    [MenuItem("Mantis/Build Windows prototype")]
    public static void BuildWindows()
    {
        Setup();
        Directory.CreateDirectory("Builds/Windows");
        var result = BuildPipeline.BuildPlayer(new BuildPlayerOptions {
            scenes = new[] { Root + "Generated/Practice.unity" }, locationPathName = "Builds/Windows/MantisPunch.exe",
            target = BuildTarget.StandaloneWindows64, options = BuildOptions.Development });
        File.WriteAllText("QA/build-result.txt", result.summary.result + "\nErrors: " + result.summary.totalErrors + "\nWarnings: " + result.summary.totalWarnings);
        if (result.summary.result != BuildResult.Succeeded) throw new Exception("Windows build failed");
        Debug.Log("MANTIS_BUILD_COMPLETE");
    }
    static Material Material(string name, Color color, float smooth)
    {
        string path = Root + "Generated/" + name + ".mat";
        var material = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (!material) { material = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard")); AssetDatabase.CreateAsset(material, path); }
        material.color = color; material.SetFloat("_Glossiness", smooth); if (material.HasProperty("_Smoothness")) material.SetFloat("_Smoothness", smooth); material.SetFloat("_Metallic", 0); return material;
    }
    static GameObject Cube(string name, Vector3 position, Vector3 scale, Material material)
    {
        var obj = GameObject.CreatePrimitive(PrimitiveType.Cube); obj.name = name; obj.transform.position = position; obj.transform.localScale = scale;
        obj.GetComponent<Renderer>().sharedMaterial = material; return obj;
    }
    static GameObject Cylinder(string name, Transform parent, Vector3 localPosition, Vector3 scale, Material material)
    {
        var obj = GameObject.CreatePrimitive(PrimitiveType.Cylinder); obj.name = name; obj.transform.SetParent(parent, false);
        obj.transform.localPosition = localPosition; obj.transform.localScale = scale; obj.GetComponent<Renderer>().sharedMaterial = material;
        UnityEngine.Object.DestroyImmediate(obj.GetComponent<Collider>()); return obj;
    }
}


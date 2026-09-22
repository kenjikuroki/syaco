using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Build.Reporting;
using MantisPunch;
using System.IO;

public static class DuelBuilder
{
    [MenuItem("Mantis/Build duel prototype")]
    public static void Build()
    {
        EditorSceneManager.OpenScene("Assets/Mantis/Generated/Practice.unity");
        foreach (var target in Object.FindObjectsByType<TrainingTarget>(FindObjectsSortMode.None)) Object.DestroyImmediate(target.gameObject);
        foreach (var hud in Object.FindObjectsByType<MantisHud>(FindObjectsSortMode.None)) Object.DestroyImmediate(hud.gameObject);
        foreach (var test in Object.FindObjectsByType<RuntimeSmokeTest>(FindObjectsSortMode.None)) Object.DestroyImmediate(test.gameObject);
        var player = Object.FindFirstObjectByType<MantisPlayer>();
        var cpu = Object.Instantiate(player); cpu.name = "CPU Mantis";
        player.duelMode = cpu.duelMode = true; player.opponent = cpu; cpu.opponent = player;
        player.controls.Duel = true; cpu.automation = true;
        player.windup = cpu.windup = .26f; player.recovery = cpu.recovery = .5f;
        // Runtime controls are not serialized; initialise these in the duel component too.
        var shell = new Material(cpu.visual.GetComponentInChildren<Renderer>().sharedMaterial);
        shell.color = new Color(1, .57f, .34f);
        const string matPath = "Assets/Mantis/Generated/CpuShell.mat";
        var existing = AssetDatabase.LoadAssetAtPath<Material>(matPath);
        if (existing) { EditorUtility.CopySerialized(shell, existing); Object.DestroyImmediate(shell); shell = existing; }
        else AssetDatabase.CreateAsset(shell, matPath);
        cpu.visual.GetComponentInChildren<Renderer>().sharedMaterial = shell;
        var follow = Object.FindFirstObjectByType<MantisCamera>(); var camera = follow.GetComponent<Camera>(); Object.DestroyImmediate(follow);
        var duel = new GameObject("Duel rules and CPU").AddComponent<MantisDuel>(); duel.player = player; duel.cpu = cpu; duel.arenaCamera = camera;
        duel.indicatorShader = Shader.Find("Universal Render Pipeline/Unlit");
        var imported = duel.gameObject.AddComponent<ImportedHitEffects>();
        const string fxRoot = "Assets/Matthew Guz/Hits Effects FREE/Prefab/";
        imported.guardPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(fxRoot + "Basic Hit .prefab");
        imported.parryPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(fxRoot + "Lightning Hit Blue.prefab");
        imported.hitPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(fxRoot + "Basic Hit 2.prefab");
        imported.waterHitPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(fxRoot + "1.2/Basic Hit 8  (NEW).prefab");
        imported.particleShader = AssetDatabase.LoadAssetAtPath<Shader>("Assets/Mantis/ImportedParticle.shader");
        var comparison = duel.gameObject.AddComponent<HitComparison>();
        comparison.variants = new[] {
            AssetDatabase.LoadAssetAtPath<GameObject>(fxRoot + "Basic Hit .prefab"),
            AssetDatabase.LoadAssetAtPath<GameObject>(fxRoot + "Basic Hit 2.prefab"),
            AssetDatabase.LoadAssetAtPath<GameObject>(fxRoot + "Basic Hit 7.prefab"),
            AssetDatabase.LoadAssetAtPath<GameObject>(fxRoot + "1.2/Basic Hit 8  (NEW).prefab")
        };
        if (!imported.guardPrefab || !imported.parryPrefab || !imported.hitPrefab || !imported.particleShader)
            throw new System.Exception("Imported impact assets are missing");
        duel.gameObject.AddComponent<DuelVerification>();
        duel.gameObject.AddComponent<PresentationVerification>();
        player.transform.position = new Vector3(0, 0, -1.6f); cpu.transform.position = new Vector3(0, 0, 1.6f); cpu.transform.rotation = Quaternion.Euler(0, 180, 0);
        const string scene = "Assets/Mantis/Generated/Duel.unity";
        EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene(), scene);
        AssetDatabase.SaveAssets(); Directory.CreateDirectory("Builds/DeflectRecoilDuel");
        var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions { scenes = new[] { scene }, locationPathName = "Builds/DeflectRecoilDuel/ShakoDeflectRecoil.exe", target = BuildTarget.StandaloneWindows64, options = BuildOptions.Development });
        File.WriteAllText("QA/duel-build.txt", report.summary.result + " errors=" + report.summary.totalErrors);
        if (report.summary.result != BuildResult.Succeeded) throw new System.Exception("Duel build failed");
    }
}










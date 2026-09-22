using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEditor.SceneManagement;

[InitializeOnLoad]
public static class MantisSetupRequest
{
    static MantisSetupRequest() { EditorApplication.update += Tick; }
    static void Tick()
    {
        const string request = "QA/triangle-request.txt";
        if (!File.Exists(request) || EditorApplication.isCompiling || EditorApplication.isUpdating || EditorApplication.isPlayingOrWillChangePlaymode) return;
        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().isDirty) return;
        File.Delete(request);
        try { DuelBuilder.Build(); }
        catch (System.Exception e) { Debug.LogException(e); File.WriteAllText("QA/setup-error.txt", e.ToString()); }
    }
}


using UnityEngine;
using UnityEditor;
using System.Diagnostics;

public class GameSettings : EditorWindow
{
    private bool IsServer = false;

    [MenuItem("BuildSettings/GameSettings")]
    static void Init()
    {
        GameSettings Window = (GameSettings)EditorWindow.GetWindow(typeof(GameSettings));
        Window.Show();
    }

    void OnGUI()
    {
        GUILayout.Label("Game Settings", EditorStyles.boldLabel);
        IsServer = EditorGUILayout.Toggle("Is Server", IsServer);
        
        if(IsServer && PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone) != "SERVER")
        {
            PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone, "SERVER");
        }
        else if(!IsServer && PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone) != "")
        {
            PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone, "");
        }

        if (GUILayout.Button("Run Standalone"))
        {
            string path = EditorUtility.SaveFilePanel("Choose Location of Built Game", "", "", "exe");
            BuildPipeline.BuildPlayer(EditorBuildSettings.scenes, path, EditorUserBuildSettings.activeBuildTarget, BuildOptions.None);

            Process proc = new Process();
            proc.StartInfo.FileName = path;
            proc.Start();
        }
    }
}

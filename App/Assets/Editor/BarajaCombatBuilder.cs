using UnityEditor;
using UnityEngine;

/// <summary>
/// Standalone Windows build of the combat scene, for a real rendered window
/// to screenshot (batchmode renders nothing). Run via:
/// Unity -batchmode -quit -projectPath . -executeMethod BarajaCombatBuilder.BuildWindows
/// </summary>
public static class BarajaCombatBuilder
{
    public static void BuildWindows()
    {
        System.IO.Directory.CreateDirectory("Builds");
        var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
        {
            scenes = new[] { "Assets/Scenes/Combat.unity" },
            locationPathName = "Builds/BarajaCombat.exe",
            target = BuildTarget.StandaloneWindows64,
            options = BuildOptions.None
        });
        Debug.Log("BarajaCombatBuilder result: " + report.summary.result);
    }
}

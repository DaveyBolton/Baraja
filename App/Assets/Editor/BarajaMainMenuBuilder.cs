using UnityEditor;
using UnityEngine;

/// <summary>
/// Standalone Windows build of just the main menu scene, for screenshot
/// verification (batchmode renders nothing). Run via:
/// Unity -batchmode -quit -projectPath . -executeMethod BarajaMainMenuBuilder.BuildWindows
/// </summary>
public static class BarajaMainMenuBuilder
{
    public static void BuildWindows()
    {
        System.IO.Directory.CreateDirectory("Builds");
        var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
        {
            scenes = new[] { "Assets/Scenes/MainMenu.unity" },
            locationPathName = "Builds/BarajaMenu.exe",
            target = BuildTarget.StandaloneWindows64,
            options = BuildOptions.None
        });
        Debug.Log("BarajaMainMenuBuilder result: " + report.summary.result);
    }
}

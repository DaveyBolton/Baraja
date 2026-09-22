using UnityEditor;
using UnityEngine;

/// <summary>
/// The actual playable build - all three scenes, Splash first so it's the
/// boot scene (5s branded splash, then loads MainMenu itself - see
/// SplashScreen.cs), MainMenu second, Combat third so
/// SceneManager.LoadScene("Combat") from the Play button has something to
/// load. BarajaMainMenuBuilder/BarajaCombatBuilder each build a SINGLE
/// scene in isolation for fast screenshot verification - neither of those
/// .exe's can actually complete Splash -> Menu -> Play -> Combat since the
/// other scenes were never included in that build. Run via:
/// Unity -batchmode -executeMethod BarajaFullGameBuilder.BuildWindows -quit
/// </summary>
public static class BarajaFullGameBuilder
{
    public static void BuildWindows()
    {
        System.IO.Directory.CreateDirectory("Builds");
        var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
        {
            scenes = new[]
            {
                "Assets/Scenes/Splash.unity",
                "Assets/Scenes/MainMenu.unity",
                "Assets/Scenes/Combat.unity",
            },
            locationPathName = "Builds/Baraja.exe",
            target = BuildTarget.StandaloneWindows64,
            options = BuildOptions.None
        });
        Debug.Log("BarajaFullGameBuilder result: " + report.summary.result);
    }
}

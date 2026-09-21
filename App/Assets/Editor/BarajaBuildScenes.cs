using System.Linq;
using UnityEditor;

/// <summary>
/// Shared helper so BarajaMainMenuSceneBuilder and BarajaCombatSceneBuilder can
/// each register their own scene in EditorBuildSettings without wiping out
/// whatever the other one already registered - each Build() runs independently
/// via -executeMethod, so overwriting the whole list from either one meant
/// whichever ran last silently dropped the other from the build.
/// </summary>
public static class BarajaBuildScenes
{
    // desiredIndex keeps MainMenu first (index 0, the boot scene) regardless
    // of which builder happens to run second.
    public static void Register(string scenePath, int desiredIndex)
    {
        var scenes = EditorBuildSettings.scenes.Where(s => s.path != scenePath).ToList();
        int insertAt = System.Math.Max(0, System.Math.Min(desiredIndex, scenes.Count));
        scenes.Insert(insertAt, new EditorBuildSettingsScene(scenePath, true));
        EditorBuildSettings.scenes = scenes.ToArray();
    }
}

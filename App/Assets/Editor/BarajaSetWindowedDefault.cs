using UnityEditor;
using UnityEngine;

/// <summary>
/// Sets the standalone player's default launch mode to windowed instead of
/// fullscreen, run via:
/// Unity -batchmode -executeMethod BarajaSetWindowedDefault.Run -quit
/// Without this, a build launched by double-clicking the .exe (no -screen-
/// width/-height args, which every test run in this project passes
/// explicitly) uses the project defaults, which for a new Unity project is
/// fullscreen at the desktop's native resolution.
/// </summary>
public static class BarajaSetWindowedDefault
{
    public static void Run()
    {
        PlayerSettings.fullScreenMode = FullScreenMode.Windowed;
        PlayerSettings.defaultIsNativeResolution = false;
        PlayerSettings.defaultScreenWidth = 540;
        PlayerSettings.defaultScreenHeight = 960;
        PlayerSettings.resizableWindow = true;
        AssetDatabase.SaveAssets();
        Debug.Log("BarajaSetWindowedDefault: standalone default is now windowed, 540x960.");
    }
}

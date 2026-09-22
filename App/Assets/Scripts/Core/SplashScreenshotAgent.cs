using System.Collections;
using UnityEngine;

namespace Baraja.Core
{
    // Auto-captures a screenshot of the splash screen shortly after it
    // appears, then quits. Installs itself only when launched with
    // -srshots, same pattern as MenuScreenshotAgent/CombatScreenshotAgent.
    public class SplashScreenshotAgent : MonoBehaviour
    {
        [RuntimeInitializeOnLoadMethod]
        static void Install()
        {
            if (System.Array.IndexOf(System.Environment.GetCommandLineArgs(), "-srshots") < 0) return;
            var go = new GameObject("SplashScreenshotAgent");
            go.AddComponent<SplashScreenshotAgent>();
            Object.DontDestroyOnLoad(go);
        }

        private void Start()
        {
            if (FindObjectOfType<SplashScreen>() == null) return;
            StartCoroutine(RunSequence());
        }

        private IEnumerator RunSequence()
        {
            string shotDir = System.IO.Path.Combine(
                System.IO.Directory.GetParent(Application.dataPath).FullName, "Shots");
            System.IO.Directory.CreateDirectory(shotDir);

            yield return new WaitForSeconds(0.5f);
            ScreenCapture.CaptureScreenshot(System.IO.Path.Combine(shotDir, "splash_01.png"));
            Debug.Log("SRSHOT splash");
            // Used to Application.Quit() here ~1s in - fine when -srshots
            // only ever needed the splash screen, but MenuScreenshotAgent
            // and CombatScreenshotAgent install on that same flag and need
            // the process to keep running well past this point (through
            // Menu, then a forced load into Combat). Whichever of those
            // finishes last owns calling Quit() now.
        }
    }
}

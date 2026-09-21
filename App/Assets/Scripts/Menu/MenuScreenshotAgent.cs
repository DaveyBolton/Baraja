using System.Collections;
using UnityEngine;

namespace Baraja.Menu
{
    // Auto-clicks through every panel of the main menu and captures a
    // screenshot of each, then quits. Installs itself only when launched with
    // -srshots. Drives the same Button.onClick delegates a real tap would
    // fire, rather than simulating pointer input, since the goal is to verify
    // each panel renders and wires correctly, not to test the input system.
    public class MenuScreenshotAgent : MonoBehaviour
    {
        [RuntimeInitializeOnLoadMethod]
        static void Install()
        {
            if (System.Array.IndexOf(System.Environment.GetCommandLineArgs(), "-srshots") < 0) return;
            var go = new GameObject("MenuScreenshotAgent");
            go.AddComponent<MenuScreenshotAgent>();
            Object.DontDestroyOnLoad(go);
        }

        private string _shotDir;
        private int _shotCount;

        private void Start()
        {
            _shotDir = System.IO.Path.Combine(
                System.IO.Directory.GetParent(Application.dataPath).FullName, "Shots");
            System.IO.Directory.CreateDirectory(_shotDir);
            StartCoroutine(RunSequence());
        }

        private void Shot(string label)
        {
            _shotCount++;
            ScreenCapture.CaptureScreenshot(System.IO.Path.Combine(_shotDir, $"menu_{_shotCount:00}_{label}.png"));
            Debug.Log("SRSHOT " + label);
        }

        private IEnumerator RunSequence()
        {
            // Both screenshot agents install on -srshots regardless of which
            // single-scene test build is running (Assembly-CSharp has every
            // script regardless of which scene a given build includes), so
            // this one quietly stands down rather than quitting the whole
            // process when it's not the combat build's agent that's active.
            var ui = FindObjectOfType<MainMenuUI>();
            if (ui == null) yield break;

            // ScreenCapture.CaptureScreenshot defers its actual disk write past
            // the end of the current frame, so mutating UI state again right
            // after calling it (even after a short wait) captures the state
            // that was written a step late - every shot came out one step
            // behind the label until a settle pause was added after each one.
            yield return new WaitForSeconds(0.3f);
            Shot("title");
            yield return new WaitForSeconds(0.4f);

            ui.HowToPlayButton.onClick.Invoke();
            yield return new WaitForSeconds(0.2f);
            Shot("tutorial_page1");
            yield return new WaitForSeconds(0.4f);

            ui.TutorialNextButton.onClick.Invoke();
            yield return new WaitForSeconds(0.2f);
            Shot("tutorial_page2");
            yield return new WaitForSeconds(0.4f);

            ui.TutorialCloseButton.onClick.Invoke();
            yield return new WaitForSeconds(0.1f);

            ui.OptionsButton.onClick.Invoke();
            yield return new WaitForSeconds(0.2f);
            Shot("options_spanish");
            yield return new WaitForSeconds(0.4f);

            ui.LanguageButton.onClick.Invoke();
            yield return new WaitForSeconds(0.2f);
            Shot("options_english");
            yield return new WaitForSeconds(0.4f);

            ui.OptionsCloseButton.onClick.Invoke();
            yield return new WaitForSeconds(0.1f);

            ui.StoreButton.onClick.Invoke();
            yield return new WaitForSeconds(0.2f);
            Shot("store");
            yield return new WaitForSeconds(0.4f);

            Application.Quit();
        }
    }
}

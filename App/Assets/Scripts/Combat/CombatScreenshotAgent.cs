using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Baraja.Combat
{
    // Auto-plays fight 1 and captures periodic screenshots, then quits.
    // Installs itself only when launched with -srshots, so it costs nothing
    // in a normal build. Mirrors Spirit-Run's ScreenshotAgent pattern: verify
    // a headless run by instrumenting it, not by watching someone play.
    public class CombatScreenshotAgent : MonoBehaviour
    {
        [RuntimeInitializeOnLoadMethod]
        static void Install()
        {
            if (System.Array.IndexOf(System.Environment.GetCommandLineArgs(), "-srshots") < 0) return;
            var go = new GameObject("CombatScreenshotAgent");
            go.AddComponent<CombatScreenshotAgent>();
            Object.DontDestroyOnLoad(go);
        }

        private CombatManager _manager;
        private CombatUI _ui;
        private int _shotCount;
        private int _shotsAfterEnd;
        private float _timer;
        private bool _ended;

        private string _shotDir;

        private bool _tutorialHandled;

        private void Start()
        {
            // Absolute path anchored to the build folder (one level up from
            // "<Product>_Data") - a relative path resolves against whatever the
            // OS considers the current directory at launch, which is not
            // reliably the exe's own folder.
            _shotDir = System.IO.Path.Combine(
                System.IO.Directory.GetParent(Application.dataPath).FullName, "Shots");
            System.IO.Directory.CreateDirectory(_shotDir);
            StartCoroutine(HandleTutorialThenPlay());
        }

        // The first-time tutorial banner (CombatTutorialHint) blocks the board
        // until dismissed - click through a couple of pages to verify it
        // renders, then fast-forward the rest so the timed auto-play loop
        // below sees a clean board from turn 1 instead of several turns
        // silently passing behind a banner nobody photographed.
        private IEnumerator HandleTutorialThenPlay()
        {
            var hint = FindObjectOfType<CombatTutorialHint>();
            if (hint != null && hint.Panel.activeSelf)
            {
                yield return new WaitForSeconds(0.3f);
                ScreenCapture.CaptureScreenshot(System.IO.Path.Combine(_shotDir, "combat_tutorial_page1.png"));
                yield return new WaitForSeconds(0.3f);

                hint.NextButton.onClick.Invoke();
                yield return new WaitForSeconds(0.2f);
                ScreenCapture.CaptureScreenshot(System.IO.Path.Combine(_shotDir, "combat_tutorial_page2.png"));
                yield return new WaitForSeconds(0.3f);

                while (hint.Panel.activeSelf) hint.NextButton.onClick.Invoke();
            }
            _tutorialHandled = true;
        }

        // -srslow widens the gap between steps for a demo capture meant to be
        // looked at frame by frame, rather than the fast default used to just
        // verify the loop resolves correctly.
        private static readonly bool Slow =
            System.Array.IndexOf(System.Environment.GetCommandLineArgs(), "-srslow") >= 0;

        private void Update()
        {
            if (!_tutorialHandled) return;

            if (_manager == null)
            {
                _manager = FindObjectOfType<CombatManager>();
                _ui = FindObjectOfType<CombatUI>();
                if (_manager == null) return;
                _manager.OnCombatEnded += _ => _ended = true;
            }

            _timer += Time.unscaledDeltaTime;
            float interval = Slow ? 1.8f : 0.6f;
            if (_timer < interval) return;
            _timer = 0f;

            if (!_ended) AutoPlayStep();

            _shotCount++;
            ScreenCapture.CaptureScreenshot(System.IO.Path.Combine(_shotDir, $"combat_{_shotCount:00}.png"));
            Debug.Log("SRSHOT " + _shotCount);

            if (_ended && ++_shotsAfterEnd >= 2) Application.Quit();
            if (_shotCount >= 20) Application.Quit(); // safety cap against a stuck state
        }

        private void AutoPlayStep()
        {
            var target = _manager.Enemies.Find(e => !e.IsDead);
            foreach (var card in new List<CardData>(_manager.PlayerDeck.Hand))
            {
                if (_manager.CanPlay(card))
                {
                    _manager.PlayCard(card, target);
                    return;
                }
            }
            _manager.EndPlayerTurn();
        }
    }
}

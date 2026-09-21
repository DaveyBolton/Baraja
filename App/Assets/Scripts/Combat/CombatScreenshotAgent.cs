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

        private void Start()
        {
            // Absolute path anchored to the build folder (one level up from
            // "<Product>_Data") - a relative path resolves against whatever the
            // OS considers the current directory at launch, which is not
            // reliably the exe's own folder.
            _shotDir = System.IO.Path.Combine(
                System.IO.Directory.GetParent(Application.dataPath).FullName, "Shots");
            System.IO.Directory.CreateDirectory(_shotDir);
        }

        // -srslow widens the gap between steps for a demo capture meant to be
        // looked at frame by frame, rather than the fast default used to just
        // verify the loop resolves correctly.
        private static readonly bool Slow =
            System.Array.IndexOf(System.Environment.GetCommandLineArgs(), "-srslow") >= 0;

        private void Update()
        {
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

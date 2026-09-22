using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Baraja.Menu;
using Baraja.Core;

namespace Baraja.Combat
{
    // Unlike CombatScreenshotAgent and BarajaCombatSmokeTest (both call
    // CombatManager.PlayCard/EndPlayerTurn DIRECTLY, bypassing every button,
    // double-tap handler, and scroll gesture entirely), this drives the REAL
    // input path through Unity's EventSystem - the same dispatch a real
    // double-tap, click, or swipe goes through. Game logic passing headless
    // and screenshots rendering correctly say nothing about whether the UI
    // is actually wired to that logic; this is what actually proves it.
    //
    // Covers, in order: tutorial dismissal via the X, single-tap NOT
    // playing a card, double-tap playing one, dragging the hand's
    // ScrollRect actually scrolling it, enemy target selection via click,
    // End Turn actually advancing the turn, and playing a FULL fight to
    // completion purely through simulated taps/clicks - then verifying the
    // game actually progressed to fight 2 with a different enemy (the
    // exact thing that was reported broken).
    public class CombatUITestAgent : MonoBehaviour
    {
        [RuntimeInitializeOnLoadMethod]
        static void Install()
        {
            if (System.Array.IndexOf(System.Environment.GetCommandLineArgs(), "-srtest") < 0) return;
            var go = new GameObject("CombatUITestAgent");
            go.AddComponent<CombatUITestAgent>();
            Object.DontDestroyOnLoad(go);
        }

        private readonly List<string> _failures = new List<string>();

        private void Start() => StartCoroutine(RunTest());

        private void Fail(string message)
        {
            _failures.Add(message);
            Debug.Log("SRTEST FAIL: " + message);
        }

        private void Pass(string message) => Debug.Log("SRTEST PASS: " + message);

        private IEnumerator RunTest()
        {
            // Boot scene is now Splash (5s branded intro) before MainMenu -
            // wait it out the same way we wait out any other scene
            // transition below, rather than immediately concluding
            // "not the combat build" because neither MainMenuUI nor
            // CombatUI exist in the scene yet.
            if (FindObjectOfType<SplashScreen>() != null)
            {
                string splashScene = SceneManager.GetActiveScene().name;
                float splashWaited = 0f;
                while (SceneManager.GetActiveScene().name == splashScene && splashWaited < 8f)
                {
                    yield return null;
                    splashWaited += Time.unscaledDeltaTime;
                }
                if (SceneManager.GetActiveScene().name == splashScene)
                {
                    Fail("splash screen never advanced to the main menu");
                    Debug.Log("SRTEST SUMMARY: 1 FAILURE(S) - splash never advanced");
                    Application.Quit();
                    yield break;
                }
                Pass("splash screen advanced to the main menu after its timer");
            }

            // If this is the full game (boots to the main menu, not
            // straight into combat), click Play for real and wait for the
            // scene to actually change - this is what proves Menu -> Play
            // -> Combat works end to end, not just the combat scene in
            // isolation.
            var menuUi = FindObjectOfType<MainMenuUI>();
            if (menuUi != null)
            {
                if (menuUi.PlayButton == null) { Fail("main menu has no PlayButton wired"); Application.Quit(); yield break; }
                string sceneBefore = SceneManager.GetActiveScene().name;
                SimulateClick(menuUi.PlayButton.gameObject);
                float waited = 0f;
                while (SceneManager.GetActiveScene().name == sceneBefore && waited < 5f)
                {
                    yield return null;
                    waited += Time.unscaledDeltaTime;
                }
                if (SceneManager.GetActiveScene().name == sceneBefore)
                {
                    Fail("clicking Play on the main menu never loaded the Combat scene");
                    Debug.Log("SRTEST SUMMARY: 1 FAILURE(S) - clicking Play never loaded Combat");
                    Application.Quit();
                    yield break;
                }
                Pass("clicking Play loaded the Combat scene");
                yield return null;
                yield return null;
            }

            var ui = FindObjectOfType<CombatUI>();
            if (ui == null) { Debug.Log("SRTEST SKIP: not the combat build"); Application.Quit(); yield break; }
            var manager = ui.Manager;

            // --- Dismiss the tutorial via the real X button ---
            var hint = FindObjectOfType<CombatTutorialHint>();
            yield return null;
            if (hint != null && hint.Panel.activeSelf)
            {
                if (hint.CloseButton == null) Fail("tutorial has no CloseButton wired");
                else
                {
                    SimulateClick(hint.CloseButton.gameObject);
                    yield return null;
                    if (hint.Panel.activeSelf) Fail("tutorial X did not close the panel");
                    else Pass("tutorial X closed the panel");
                }
            }
            yield return null;
            yield return null;

            string firstEnemyId = manager.Enemies.Count > 0 ? manager.Enemies[0].Data.Id : null;

            // --- Dave reported "double tap the card, that IS the end turn
            //     action" - every other test in this file dispatches
            //     click/drag events DIRECTLY to a known target via
            //     ExecuteEvents.Execute(target, ...), which completely
            //     skips the raycasting step a real tap goes through to
            //     figure out WHAT was actually tapped. That's a real blind
            //     spot: a bug where a tap's screen position resolves to
            //     the wrong UI element would pass every other test here
            //     and still misfire for a real player. This one actually
            //     raycasts from the card's own reported screen position,
            //     the same way EventSystem resolves a genuine click. ---
            yield return TestRaycastHitsIntendedTargets(ui);

            // --- Single tap must NOT play; double-tap must ---
            yield return TestDoubleTapDiscipline(ui, manager);

            // --- Dragging the hand must actually scroll it ---
            yield return TestHandScroll(ui);

            // --- Clicking a different enemy portrait must change the
            //     selected target (only meaningful with 2+ enemies) ---
            yield return TestEnemyTargeting(ui);

            // --- Play an entire fight to completion using ONLY simulated
            //     double-taps and End Turn clicks - the actual gameplay
            //     loop end to end, not one isolated action. ---
            yield return PlayFullFightViaRealClicks(ui, manager);

            // --- Verify the fight actually progressed to a NEW encounter
            //     with a different enemy, not stuck on the same one. ---
            yield return new WaitForSeconds(2.5f); // AfterCombat()'s own delay
            yield return null;
            var managerAfter = ui.Manager; // same instance, but re-read Enemies fresh
            if (managerAfter.Enemies.Count == 0)
            {
                Fail("no enemies present after fight ended - did the run progress at all?");
            }
            else
            {
                string secondEnemyId = managerAfter.Enemies[0].Data.Id;
                if (firstEnemyId != null && secondEnemyId == firstEnemyId)
                    Fail($"enemy card never changed - still fighting '{secondEnemyId}' after the fight ended");
                else
                    Pass($"fight progressed - now facing '{secondEnemyId}' (was '{firstEnemyId}')");
            }

            string summary = _failures.Count == 0
                ? "SRTEST SUMMARY: ALL PASS"
                : $"SRTEST SUMMARY: {_failures.Count} FAILURE(S) - " + string.Join(" | ", _failures);
            Debug.Log(summary);
            yield return null;
            Application.Quit();
        }

        private IEnumerator TestRaycastHitsIntendedTargets(CombatUI ui)
        {
            if (FindObjectOfType<GraphicRaycaster>() == null)
            {
                Fail("no GraphicRaycaster found - can't verify real click targeting");
                yield break;
            }

            Transform card = FindFirstCard(ui);
            if (card == null) { Fail("no card in hand to raycast-test"); yield break; }

            GameObject hitCard = RaycastTopHit(card.position);
            if (hitCard == null)
                Fail("raycasting a hand card's own screen position hit NOTHING - a real tap there would do nothing");
            else if (hitCard != card.gameObject)
                Fail($"raycasting a hand card's own screen position hit '{hitCard.name}' instead of the card itself - a real tap there would hit the wrong element");
            else
                Pass("raycasting a hand card's own screen position correctly hits the card");

            if (ui.EndTurnButton != null)
            {
                GameObject hitEndTurn = RaycastTopHit(ui.EndTurnButton.transform.position);
                if (hitEndTurn == null || !hitEndTurn.transform.IsChildOf(ui.EndTurnButton.transform))
                    Fail($"raycasting End Turn's own screen position hit '{hitEndTurn?.name}' instead of End Turn");
                else
                    Pass("raycasting End Turn's own screen position correctly hits End Turn");

                if (hitCard == ui.EndTurnButton.gameObject)
                    Fail("CRITICAL: the card's own screen position raycasts to the End Turn button - this is exactly the reported bug");
            }
        }

        private static GameObject RaycastTopHit(Vector3 worldPos)
        {
            Vector2 screenPos = RectTransformUtility.WorldToScreenPoint(null, worldPos);
            var pointerData = new PointerEventData(EventSystem.current) { position = screenPos };
            var results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pointerData, results);
            return results.Count > 0 ? results[0].gameObject : null;
        }

        private IEnumerator TestDoubleTapDiscipline(CombatUI ui, CombatManager manager)
        {
            Transform card = FindAnyPlayableCard(ui);
            if (card == null) { Fail("no playable card found in hand"); yield break; }

            int before = manager.PlayerDeck.Hand.Count;
            SimulateClick(card.gameObject, clickCount: 1);
            yield return null;
            yield return null;
            if (manager.PlayerDeck.Hand.Count != before)
                Fail($"single tap played a card (hand {before} -> {manager.PlayerDeck.Hand.Count}); should require a double-tap");
            else
                Pass("single tap did not play a card");

            // Re-find in case anything rebuilt (it shouldn't have).
            card = FindAnyPlayableCard(ui);
            if (card == null) { Fail("playable card disappeared after single tap"); yield break; }

            int beforeDouble = manager.PlayerDeck.Hand.Count;
            SimulateClick(card.gameObject, clickCount: 2);
            yield return null;
            yield return null;
            int afterDouble = manager.PlayerDeck.Hand.Count;
            if (afterDouble == beforeDouble)
                Fail($"double-tap did not play the card (hand stayed at {afterDouble})");
            else
                Pass($"double-tap played a card (hand {beforeDouble} -> {afterDouble})");
        }

        private IEnumerator TestHandScroll(CombatUI ui)
        {
            var scrollRect = ui.HandScrollRect;
            if (scrollRect == null) { Fail("no HandScrollRect found"); yield break; }

            if (scrollRect.content.rect.width <= scrollRect.viewport.rect.width)
            {
                Debug.Log("SRTEST SKIP: hand fits entirely in the viewport at this hand size, nothing to scroll");
                yield break;
            }

            float before = scrollRect.horizontalNormalizedPosition;
            var scrollGO = scrollRect.gameObject;
            var beginData = new PointerEventData(EventSystem.current) { position = new Vector2(700, 300) };
            ExecuteEvents.Execute(scrollGO, beginData, ExecuteEvents.beginDragHandler);

            var dragData = new PointerEventData(EventSystem.current) { position = new Vector2(100, 300) }; // drag left 600px
            ExecuteEvents.Execute(scrollGO, dragData, ExecuteEvents.dragHandler);
            ExecuteEvents.Execute(scrollGO, dragData, ExecuteEvents.endDragHandler);

            yield return null;
            yield return null;

            float after = scrollRect.horizontalNormalizedPosition;
            if (Mathf.Approximately(before, after)) Fail("dragging the hand did not scroll it");
            else Pass("dragging the hand scrolled it");
        }

        private IEnumerator TestEnemyTargeting(CombatUI ui)
        {
            if (ui.EnemyPanelCount < 2)
            {
                Debug.Log("SRTEST SKIP: enemy targeting needs a multi-enemy fight, this one has " + ui.EnemyPanelCount);
                yield break;
            }

            var initial = ui.SelectedTarget;
            var otherButton = ui.GetEnemyButton(1);
            SimulateClick(otherButton.gameObject);
            yield return null;

            if (ui.SelectedTarget == initial) Fail("clicking a different enemy did not change the selected target");
            else Pass("clicking a different enemy changed the selected target");
        }

        private IEnumerator PlayFullFightViaRealClicks(CombatUI ui, CombatManager manager)
        {
            const int maxActions = 60; // generous safety cap against an infinite loop if something's wedged
            const int maxWaitFrames = 1200; // ~separate safety net for EndTurnSequence's real-time reveal/sweep animation
            int framesWaited = 0;
            for (int i = 0; i < maxActions; i++)
            {
                if (ui.CombatEnded) { Pass($"fight completed in {i} simulated actions"); yield break; }

                // EndTurnButton also goes non-interactable DURING the enemy
                // move reveal + sweep animation now (EndTurnSequence), not
                // just when the fight is actually over - wait that out
                // instead of spamming no-op clicks against it or, worse,
                // mistaking "mid-animation" for "fight over".
                if (!ui.EndTurnButton.interactable)
                {
                    if (++framesWaited > maxWaitFrames)
                    {
                        Fail("EndTurnButton stayed non-interactable and combat never ended - looks wedged");
                        yield break;
                    }
                    i--;
                    yield return null;
                    continue;
                }

                // Only double-tap a card that's actually affordable right
                // now; otherwise this has to End Turn or it just spins
                // forever re-tapping a card that can never play, which is
                // exactly what happened the first run.
                Transform card = FindAnyPlayableCard(ui);
                if (card != null)
                {
                    SimulateClick(card.gameObject, clickCount: 2);
                }
                else
                {
                    SimulateClick(ui.EndTurnButton.gameObject);
                }
                yield return null;
                yield return null;
                yield return null;
            }

            Fail($"fight did not complete within {maxActions} simulated actions - EndTurnButton still interactable");
        }

        // Every card in hand is raycastable now - there's no "focused" card
        // to single out anymore, so this is just "the first one," used
        // where the test only needs *some* real card to poke at.
        // Child index 0 used to always be the leftmost, on-screen card
        // because the hand always started scrolled fully left. It no
        // longer is - the hand now starts scrolled to its horizontal
        // center, so card 0 can be masked off-screen to the left. Pick
        // whichever card is actually inside the ScrollRect's Viewport right
        // now, so the "raycast the card's own position" test below stays
        // valid regardless of scroll position.
        private static Transform FindFirstCard(CombatUI ui)
        {
            if (ui.HandContainer.childCount == 0) return null;
            RectTransform viewport = ui.HandScrollRect != null ? ui.HandScrollRect.viewport : null;
            if (viewport != null)
            {
                foreach (Transform child in ui.HandContainer)
                {
                    Vector2 screenPos = RectTransformUtility.WorldToScreenPoint(null, child.position);
                    if (RectTransformUtility.RectangleContainsScreenPoint(viewport, screenPos, null))
                        return child;
                }
            }
            return ui.HandContainer.GetChild(0); // fallback: no ScrollRect, or somehow nothing visible
        }

        // The first card that's actually affordable right now (CardHandEntry
        // drives Button.interactable directly off CombatManager.CanPlay).
        private static Transform FindAnyPlayableCard(CombatUI ui)
        {
            foreach (Transform child in ui.HandContainer)
            {
                var button = child.GetComponent<Button>();
                if (button != null && button.interactable) return child;
            }
            return null;
        }

        // Routes through the SAME IPointerClickHandler/drag dispatch a real
        // mouse/touch input uses - not a shortcut like calling
        // onClick.Invoke() directly, which would prove the delegate works
        // without proving input would ever reach it in the first place.
        private static void SimulateClick(GameObject target, int clickCount = 1)
        {
            var eventData = new PointerEventData(EventSystem.current)
            {
                clickCount = clickCount,
                position = RectTransformUtility.WorldToScreenPoint(null, target.transform.position)
            };
            ExecuteEvents.Execute(target, eventData, ExecuteEvents.pointerClickHandler);
        }
    }
}

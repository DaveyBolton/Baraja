using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Baraja.Store;
using Baraja.Core;

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
            // The full game now boots into a 5s Splash scene before
            // MainMenu loads (see Baraja.Core.SplashScreen) - poll for a
            // few seconds instead of checking once immediately, or this
            // agent gives up before MainMenu ever appears.
            MainMenuUI ui = null;
            float waited = 0f;
            while (ui == null && waited < 8f)
            {
                ui = FindObjectOfType<MainMenuUI>();
                if (ui != null) break;
                yield return null;
                waited += Time.unscaledDeltaTime;
            }

            // Both screenshot agents install on -srshots regardless of which
            // single-scene test build is running (Assembly-CSharp has every
            // script regardless of which scene a given build includes), so
            // this one quietly stands down rather than quitting the whole
            // process when it's not the combat build's agent that's active.
            if (ui == null) yield break;

            // ScreenCapture.CaptureScreenshot defers its actual disk write past
            // the end of the current frame, so mutating UI state again right
            // after calling it (even after a short wait) captures the state
            // that was written a step late - every shot came out one step
            // behind the label until a settle pause was added after each one.
            yield return new WaitForSeconds(0.3f);
            Shot("title");
            yield return new WaitForSeconds(0.4f);

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

            // Frame verification pass (-srshots only, this build): drive the
            // actual Buy/Equip button delegates the same way MenuScreenshotAgent
            // already drives Options/Language, to prove the CardFrame row logic
            // works end to end, not just that it renders once. Rows are laid
            // out in StoreDatabase.All order - found dynamically by FrameId
            // below rather than a hardcoded index, since StoreDatabase.All has
            // grown (frame_silver/frame_red added alongside frame_gold/
            // frame_rainbow) and a hardcoded index would silently drift to the
            // wrong row the next time a new item is inserted. RefreshStoreList
            // destroys and rebuilds every row after each click, so the
            // Buy/Equip Button must be re-found fresh from StoreListContainer
            // after every invoke rather than reusing a stale reference.
            int RowIndexForFrame(string frameId) =>
                StoreDatabase.All.FindIndex(i => i.Kind == StoreItemKind.CardFrame && i.FrameId == frameId);
            int RowIndexForItemId(string itemId) => StoreDatabase.All.FindIndex(i => i.Id == itemId);

            Transform Row(int index) =>
                index >= 0 && ui.StoreListContainer.childCount > index ? ui.StoreListContainer.GetChild(index) : null;
            Button RowButton(int index) => Row(index)?.Find("BuyButton").GetComponent<Button>();
            Text RowButtonLabel(int index) => RowButton(index)?.GetComponentInChildren<Text>();

            // The frame rows sit below the fold in the store's ScrollRect -
            // scroll to the bottom before each frame-row shot so the row's
            // button label is actually visible in the capture, not just
            // exercised via the click log.
            var storeScrollRect = ui.StoreListContainer.GetComponentInParent<ScrollRect>();
            void ScrollToBottom()
            {
                if (storeScrollRect != null) storeScrollRect.verticalNormalizedPosition = 0f;
            }

            // Generous settle time around each click+shot pair here - the 0.2s/
            // 0.4s gaps used elsewhere in this file (and that work fine for the
            // title/options/store shots above) turned out NOT to be enough
            // when several state-mutating clicks and captures are chained back
            // to back: CaptureScreenshot's deferred write (see comment above)
            // fell further and further behind the actual game state with each
            // successive call, so earlier attempts here produced screenshots
            // that were several clicks stale (e.g. the "insufficient funds"
            // shot showing the frame already purchased AND equipped). A full
            // second on each side gives every prior write time to flush
            // before the next RefreshStoreList rebuild and capture fire.
            IEnumerator ClickThenShot(Button button, string label)
            {
                button.onClick.Invoke();
                yield return new WaitForSeconds(1.0f);
                ScrollToBottom();
                yield return new WaitForEndOfFrame();
                Shot(label);
                yield return new WaitForSeconds(1.0f);
            }

            // Buys (if not already owned) then equips a single CardFrame row
            // by FrameId, topping up petals first if the balance is short -
            // this is the reusable form of the original rainbow-only block,
            // so the exact same mock-purchase flow can be pointed at silver
            // and red without three copy-pasted hardcoded-index blocks.
            IEnumerator BuyAndEquipFrame(string frameId)
            {
                int rowIndex = RowIndexForFrame(frameId);
                var item = StoreDatabase.All.Find(i => i.Kind == StoreItemKind.CardFrame && i.FrameId == frameId);
                if (rowIndex < 0 || item == null)
                {
                    Debug.LogError("SRSHOT frame row not found for FrameId=" + frameId);
                    yield break;
                }

                ScrollToBottom();
                bool alreadyOwned = PlayerEntitlements.OwnsFrame(frameId);
                if (!alreadyOwned && PlayerEntitlements.PetalBalance < item.PetalCost)
                {
                    // Not enough petals yet - exercise the "insufficient
                    // funds" path first, then top up via the same mock
                    // medium-petal-pouch purchase the rainbow test always
                    // used (row found dynamically by item id, same reason
                    // as the frame rows above).
                    yield return ClickThenShot(RowButton(rowIndex), $"store_{frameId}_insufficient");
                    var petalRow = RowButton(RowIndexForItemId("petals_medium"));
                    if (petalRow != null)
                        yield return ClickThenShot(petalRow, "store_after_petal_purchase");
                }

                if (!PlayerEntitlements.OwnsFrame(frameId))
                    yield return ClickThenShot(RowButton(rowIndex), $"store_{frameId}_purchased"); // Buy -> now owned

                yield return ClickThenShot(RowButton(rowIndex), $"store_{frameId}_equipped"); // Equip -> now equipped

                if (PlayerEntitlements.EquippedFrame == frameId)
                    Debug.Log($"SRSHOT frame equip check PASS: {frameId} is now EquippedFrame");
                else
                    Debug.LogError($"SRSHOT frame equip check FAIL: expected EquippedFrame={frameId}, got {PlayerEntitlements.EquippedFrame}");
            }

            if (RowIndexForFrame("rainbow") >= 0)
            {
                yield return BuyAndEquipFrame("rainbow");
                yield return BuyAndEquipFrame("silver");

                // Mutual-exclusivity check: equipping silver must have
                // un-equipped rainbow - confirm rainbow's own row now reads
                // Equip/Equipar again, not Equipado/Equipped, before moving on.
                var rainbowLabel = RowButtonLabel(RowIndexForFrame("rainbow"));
                if (rainbowLabel != null && (rainbowLabel.text == "Equipado" || rainbowLabel.text == "Equipped"))
                    Debug.LogError("SRSHOT mutual-exclusivity FAIL: rainbow still shows Equipped after equipping silver");
                else
                    Debug.Log("SRSHOT mutual-exclusivity check PASS: rainbow row no longer shows Equipped after equipping silver");

                yield return BuyAndEquipFrame("red");

                var silverLabel = RowButtonLabel(RowIndexForFrame("silver"));
                if (silverLabel != null && (silverLabel.text == "Equipado" || silverLabel.text == "Equipped"))
                    Debug.LogError("SRSHOT mutual-exclusivity FAIL: silver still shows Equipped after equipping red");
                else
                    Debug.Log("SRSHOT mutual-exclusivity check PASS: silver row no longer shows Equipped after equipping red");

                // Optional: -srequipfinal <frameId> re-equips a specific
                // already-owned frame as the very last step, so a follow-up
                // combat -srshots run (a separate process - PlayerPrefs is
                // how state crosses between them) can be pointed at whichever
                // single frame's card art needs a fresh screenshot, instead
                // of always being left on whatever this sequence equips last.
                var cmdArgs = System.Environment.GetCommandLineArgs();
                int finalArgIndex = System.Array.IndexOf(cmdArgs, "-srequipfinal");
                if (finalArgIndex >= 0 && finalArgIndex + 1 < cmdArgs.Length)
                {
                    string finalFrame = cmdArgs[finalArgIndex + 1];
                    int finalRow = RowIndexForFrame(finalFrame);
                    if (finalRow >= 0)
                        yield return ClickThenShot(RowButton(finalRow), $"store_{finalFrame}_final_equip");
                }

                // Optional: -srlangfinal es|en pins GameSettings.Spanish to a
                // known value as the very last step, regardless of how many
                // times the Options language toggle above has flipped it this
                // run (or across repeated runs, since it's the same persisted
                // PlayerPrefs flag) - so a follow-up combat -srshots run can
                // reliably verify Spanish (or English) card art for whichever
                // frame -srequipfinal just set, without having to reason
                // about toggle parity across separate process runs.
                int langArgIndex = System.Array.IndexOf(cmdArgs, "-srlangfinal");
                if (langArgIndex >= 0 && langArgIndex + 1 < cmdArgs.Length)
                {
                    GameSettings.Spanish = cmdArgs[langArgIndex + 1] == "es";
                    Debug.Log("SRSHOT language pinned to " + (GameSettings.Spanish ? "es" : "en"));
                }
            }

            Application.Quit();
        }
    }
}

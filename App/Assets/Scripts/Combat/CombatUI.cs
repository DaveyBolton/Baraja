using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Baraja.Core;
using Baraja.Store;

namespace Baraja.Combat
{
    // Displays CombatManager's state and forwards clicks back into it.
    // All card/enemy art is imported as Default/non-Sprite textures (matches
    // the rest of the project), so this UI uses RawImage, not Image+Sprite.
    public class CombatUI : MonoBehaviour
    {
        [HideInInspector] public CombatManager Manager;
        [HideInInspector] public bool Spanish = true;

        [HideInInspector] public Text PlayerHpText;
        [HideInInspector] public Text PlayerEnergyText;
        // Round progress toward a full cleared run (Baraja.Store.
        // PlayerEntitlements.RoundProgress) - top-left, third line under
        // HP/Energy. Was "Streak" (top-center, whole-number full-run
        // count) - redesigned per Dave into a fractional per-fight climb,
        // and moved here after floating damage numbers were found piling
        // up on the old top-center spot.
        [HideInInspector] public Text PlayerRoundText;
        // Small note under PlayerRoundText, shown only while the current
        // run's RoundProgress is at or past PlayerEntitlements.
        // PersonalBestRound - hidden otherwise (Dave: "when you pass your
        // personal best, maybe a note underneath").
        [HideInInspector] public Text PlayerRoundBestNoteText;
        [HideInInspector] public Text LogText;
        [HideInInspector] public ScrollRect LogScrollRect;
        [HideInInspector] public Button EndTurnButton;
        // Top-center, between the gold/silver stat blocks - the only manual
        // way back to the main menu from an active fight (before this, only
        // an automatic redirect after a loss or clearing the whole run).
        [HideInInspector] public Button MenuButton;
        // End Turn's gem + label are baked into ONE texture (BarajaButtonBaker),
        // not an image with a separate live Text child - language switches by
        // swapping which baked texture this RawImage shows.
        [HideInInspector] public RawImage EndTurnButtonImage;
        [HideInInspector] public Texture2D EndTurnTextureEN;
        [HideInInspector] public Texture2D EndTurnTextureES;

        [HideInInspector] public Transform EnemyContainer;
        [HideInInspector] public GameObject EnemyPanelPrefab;
        // Top-right HUD block (mirrors PlayerHpText/PlayerEnergyText's
        // top-left one, silver instead of gold) - one instance per live
        // enemy, stacked vertically underneath EnemyStatsContainer's own
        // anchor point. HP/Intent used to be live text baked onto the
        // enemy card itself; moved here so it reads at a fixed size
        // regardless of how big the card is on a given screen.
        [HideInInspector] public Transform EnemyStatsContainer;
        [HideInInspector] public GameObject EnemyStatsPrefab;

        [HideInInspector] public Transform HandContainer;
        [HideInInspector] public GameObject CardButtonPrefab;
        // The ScrollRect the hand's cards scroll inside of - HandContainer
        // is its Content child. Only actually used by CombatUITestAgent so
        // far (to drive/verify a real scroll gesture), but a direct
        // reference beats reaching up two parents from HandContainer.
        [HideInInspector] public ScrollRect HandScrollRect;

        [HideInInspector] public Transform FloatingTextLayer;
        [HideInInspector] public PlayedCardStack PlayedPile;
        [HideInInspector] public PlayedCardStack EnemyPlayedPile;

        // Maps each of the 6 telegraphed move types to its silver-framed
        // card art (Resources/Art/EnemyMoves/<EN|ES>/<key>.png) - see
        // OnEnemyMovePlayedFeedback.
        private static readonly Dictionary<IntentKind, string> EnemyMoveArt = new Dictionary<IntentKind, string>
        {
            { IntentKind.Attack, "attack" },
            { IntentKind.WeakenAttack, "weaken_attack" },
            { IntentKind.Weaken, "weaken" },
            { IntentKind.HealSelf, "heal" },
            { IntentKind.BlockSelf, "block" },
            { IntentKind.BuffSelfDamage, "buff" },
        };

        private readonly List<EnemyPanel> _enemyPanels = new List<EnemyPanel>();
        private EnemyCombatant _selectedTarget;
        private readonly List<string> _logLines = new List<string>();
        private readonly List<CardData> _lastHand = new List<CardData>();
        private readonly Queue<Texture2D> _pendingEnemyReveals = new Queue<Texture2D>();
        private bool _resolvingTurn;
        private bool _combatEnded;
        // The one hand card currently armed by a first tap, awaiting a
        // confirming second tap (see OnCardTapped) - null when nothing's
        // pending. Cleared by RebuildHand whenever the hand's own contents
        // change, since the entries it could point at get destroyed then.
        private CardHandEntry _pendingEntry;

        // Read-only, for CombatUITestAgent to verify clicks actually
        // changed state rather than just not-crashing.
        public EnemyCombatant SelectedTarget => _selectedTarget;
        public Button GetEnemyButton(int index) => _enemyPanels[index].Button;
        public int EnemyPanelCount => _enemyPanels.Count;
        // EndTurnButton.interactable now also goes false DURING the enemy
        // move reveal/sweep (see EndTurnSequence) - CombatEnded is the only
        // reliable "the fight is actually over" signal for CombatUITestAgent.
        public bool CombatEnded => _combatEnded;

        private class EnemyPanel
        {
            public EnemyCombatant Enemy;
            public RawImage Image;
            public Button Button;
            public GameObject StatsGO;
            public Text HpText;
            public Text IntentText;
        }

        // Vertical gap between one enemy's stats block and the next when a
        // fight has more than one enemy (e.g. Twin Skulls). Each block's
        // real content is 106 tall (HpText 56 + IntentText 50, see
        // MakeEnemyStatsPrefab) even though the prefab's own declared box
        // was only 100 - at the old 120 spacing that left just 14px of
        // actual margin, not enough: the two blocks' text visibly ran into
        // each other in real Twin Skulls screenshots (Dave: "make their
        // dual cards smaller so they dont cover text" - this overlap
        // turned out to be independent of card size entirely).
        private const float EnemyStatsBlockSpacing = 150f;

        private int _runIndex;

        private void Awake()
        {
            EndTurnButton.onClick.AddListener(OnEndTurnClicked);
            MenuButton.onClick.AddListener(OnMenuClicked);

            // Subscribed once here, not in BeginFight() - BeginFight() now
            // runs again for every subsequent fight in the run (see
            // OnCombatEnded), and Manager is the same CombatManager
            // instance for the scene's whole lifetime, so re-subscribing
            // there would stack a duplicate listener per fight.
            Manager.OnStateChanged += Refresh;
            Manager.OnLog += OnLog;
            Manager.OnCombatEnded += OnCombatEnded;
            Manager.OnEnemyDamaged += OnEnemyDamagedFeedback;
            Manager.OnPlayerDamaged += OnPlayerDamagedFeedback;
            Manager.OnEnemyBlocked += OnEnemyBlockedFeedback;
            Manager.OnPlayerBlocked += OnPlayerBlockedFeedback;
            Manager.OnEnemyMovePlayed += OnEnemyMovePlayedFeedback;
        }

        public void BeginFight(int fightNumber)
        {
            // Read fresh rather than trusting whatever the scene builder set:
            // the player may have flipped the language in Options since this
            // scene was built.
            Spanish = GameSettings.Spanish;
            Manager.Spanish = Spanish;
            EndTurnButton.interactable = true; // re-enable after a previous fight ended
            _combatEnded = false;
            _resolvingTurn = false;
            _pendingEnemyReveals.Clear();
            EndTurnButtonImage.texture = Spanish ? EndTurnTextureES : EndTurnTextureEN;

            _runIndex = EncounterDatabase.Run.FindIndex(e => fightNumber == 0 ? e.IsBoss : e.FightNumber == fightNumber);
            _logLines.Clear();
            LogText.text = "";
            PlayedPile.Clear();
            EnemyPlayedPile.Clear();

            Manager.StartEncounter(fightNumber);
            BuildEnemyPanels();
            RefreshRoundText();
            Refresh();
        }

        private void RefreshRoundText()
        {
            float progress = PlayerEntitlements.RoundProgress;
            PlayerRoundText.text = Spanish ? $"Ronda: {progress:F2}" : $"Round: {progress:F2}";

            // Live comparison, not a stored per-run flag: progress only
            // reaches/exceeds PersonalBestRound once it's genuinely past a
            // prior run's record (RecordRoundWin keeps them in lockstep the
            // moment a new best is set), and a loss's reset to 0 makes this
            // false again on its own without any extra bookkeeping here.
            bool atPersonalBest = progress > 0f && progress >= PlayerEntitlements.PersonalBestRound;
            PlayerRoundBestNoteText.gameObject.SetActive(atPersonalBest);
            if (atPersonalBest)
                PlayerRoundBestNoteText.text = Spanish ? "¡Nuevo récord personal!" : "New personal best!";
        }

        private void BuildEnemyPanels()
        {
            foreach (var panel in _enemyPanels)
            {
                Destroy(panel.Button.gameObject);
                Destroy(panel.StatsGO);
            }
            _enemyPanels.Clear();

            // Multi-enemy fights (Twin Skulls, so far the only one) pack
            // 2+ cards side by side in the same HorizontalLayoutGroup that
            // holds one centered card the rest of the time - at the
            // single-enemy size (417 wide) that pushes the pair's combined
            // width out far enough that the LEFT card's left edge lands
            // inside the top-left PlayerRoundText/PlayerRoundBestNoteText
            // column (confirmed in a real screenshot: "ROUND:"/"New
            // personal best!" both showing clipped, cut off by the card's
            // opaque frame). Shrinking to 300 wide keeps the pair's
            // combined width comfortably clear of that column while
            // staying centered on screen, same aspect ratio either way.
            float cardW = Manager.Enemies.Count > 1 ? 300f : 417f;
            float cardH = cardW * 1040f / 736f;

            foreach (var enemy in Manager.Enemies)
            {
                var go = Instantiate(EnemyPanelPrefab, EnemyContainer);
                go.SetActive(true);
                go.GetComponent<RectTransform>().sizeDelta = new Vector2(cardW, cardH);

                var statsGO = Instantiate(EnemyStatsPrefab, EnemyStatsContainer);
                statsGO.SetActive(true);
                statsGO.GetComponent<RectTransform>().anchoredPosition =
                    new Vector2(0, -_enemyPanels.Count * EnemyStatsBlockSpacing);

                var panel = new EnemyPanel
                {
                    Enemy = enemy,
                    Image = go.transform.Find("Image").GetComponent<RawImage>(),
                    Button = go.GetComponent<Button>(),
                    StatsGO = statsGO,
                    HpText = statsGO.transform.Find("HpText").GetComponent<Text>(),
                    IntentText = statsGO.transform.Find("IntentText").GetComponent<Text>(),
                };
                panel.Image.texture = LoadEnemyTexture(enemy.Data.ArtId);
                panel.Button.onClick.AddListener(() => SelectTarget(panel.Enemy));
                _enemyPanels.Add(panel);
            }

            _selectedTarget = _enemyPanels.Count > 0 ? _enemyPanels[0].Enemy : null;
        }

        private void SelectTarget(EnemyCombatant enemy)
        {
            if (enemy.IsDead) return;
            _selectedTarget = enemy;
            Refresh();
        }

        private void Refresh()
        {
            string blockWord = Spanish ? "Bloqueo" : "Block";
            PlayerHpText.text = $"HP {Manager.Player.Hp}/{Manager.Player.MaxHp}  {blockWord} {Manager.Player.Block}";
            PlayerEnergyText.text = Spanish
                ? $"Energía {Manager.Player.Energy}/{Manager.Player.MaxEnergy}"
                : $"Energy {Manager.Player.Energy}/{Manager.Player.MaxEnergy}";

            foreach (var panel in _enemyPanels)
            {
                bool dead = panel.Enemy.IsDead;
                panel.HpText.text = dead ? "" : $"HP {panel.Enemy.Hp}/{panel.Enemy.MaxHp}  {blockWord} {panel.Enemy.Block}";
                panel.IntentText.text = dead ? "" : DescribeIntent(panel.Enemy.CurrentIntent);
                panel.Button.interactable = !dead;
                panel.Image.color = panel.Enemy == _selectedTarget ? Color.white : new Color(0.7f, 0.7f, 0.7f);
            }

            RebuildHand();
        }

        private void RebuildHand()
        {
            var hand = Manager.PlayerDeck.Hand;

            // Selecting a target (tapping an enemy) calls Refresh(), which
            // called this unconditionally - destroying and recreating every
            // hand card just because the SELECTED ENEMY changed, not the
            // hand itself. That's what was flickering: a full rebuild (new
            // GameObjects, fresh texture loads, scale reset to prefab
            // default for one frame) on every single enemy tap. Only
            // rebuild when the hand's actual contents changed; otherwise
            // just refresh which cards are playable in place.
            bool sameCards = _lastHand.Count == hand.Count;
            if (sameCards)
            {
                for (int i = 0; i < hand.Count; i++)
                {
                    if (_lastHand[i] != hand[i]) { sameCards = false; break; }
                }
            }

            // Defends against the same class of bug either way: childCount
            // is only trusted up to hand.Count, never indexed past it.
            if (sameCards && HandContainer.childCount == hand.Count)
            {
                int i = 0;
                foreach (Transform child in HandContainer)
                {
                    var entry = child.GetComponent<CardHandEntry>();
                    if (entry != null) entry.CanPlay = Manager.CanPlay(hand[i]);
                    i++;
                }
                return;
            }

            // Destroy() defers actual removal to end-of-frame - a card
            // whose own effect draws a replacement in the same tick (e.g.
            // an end-of-turn discard immediately followed by the next
            // turn's draw, both within one EndPlayerTurn() call) could
            // rebuild twice before the first pass's children were actually
            // gone, leaving stale ones alongside the new ones and blowing
            // out the index above. Detaching immediately (not just
            // Destroy-ing) makes HandContainer.childCount correct the
            // instant this loop finishes, regardless of GC timing.
            // Old entries are about to be destroyed - a lingering reference
            // to one as the "pending" card would leave OnCardTapped
            // comparing against a dead object next tap.
            _pendingEntry = null;

            var toRemove = new List<Transform>();
            foreach (Transform child in HandContainer) toRemove.Add(child);
            foreach (var child in toRemove)
            {
                child.SetParent(null);
                Destroy(child.gameObject);
            }

            foreach (var card in hand)
            {
                var go = Instantiate(CardButtonPrefab, HandContainer);
                go.SetActive(true);
                var image = go.GetComponent<RawImage>();
                image.texture = LoadCardTexture(card.Id);
                var entry = go.AddComponent<CardHandEntry>();
                entry.CanPlay = Manager.CanPlay(card);
                var tapper = go.AddComponent<TapToSelectCard>();
                tapper.OnTap = () => OnCardTapped(entry, card);
            }

            _lastHand.Clear();
            _lastHand.AddRange(hand);

            // Start centered rather than scrolled all the way left - Dave:
            // "have it start with a card centered in the center of the
            // playing surface." ForceUpdateCanvases first so the
            // ContentSizeFitter has already sized Content to the new hand
            // before normalized position is computed against it (same fix
            // as the log's own auto-scroll-to-bottom, just centered instead
            // of pinned to an edge). Harmless when the hand fits on screen
            // and isn't scrollable at all - ScrollRect just clamps it.
            Canvas.ForceUpdateCanvases();
            if (HandScrollRect != null) HandScrollRect.horizontalNormalizedPosition = 0.5f;
        }

        // First tap on a card arms it (scales it up, sets it pending);
        // tapping that SAME armed card again confirms and plays it;
        // tapping a DIFFERENT card instead swaps the pending selection
        // over to that one rather than playing anything - so pulling back
        // an armed card and playing a different one costs nothing.
        private void OnCardTapped(CardHandEntry entry, CardData card)
        {
            if (_pendingEntry == entry)
            {
                SetPendingEntry(null);
                PlayCard(card);
                return;
            }
            SetPendingEntry(entry);
        }

        private void SetPendingEntry(CardHandEntry entry)
        {
            if (_pendingEntry != null) _pendingEntry.IsPending = false;
            _pendingEntry = entry;
            if (_pendingEntry != null) _pendingEntry.IsPending = true;
        }

        private void PlayCard(CardData card)
        {
            bool couldPlay = Manager.CanPlay(card);
            Manager.PlayCard(card, _selectedTarget);
            if (couldPlay) PlayedPile.Push(LoadCardTexture(card.Id));
        }

        private void OnEndTurnClicked()
        {
            if (_resolvingTurn) return; // already mid-animation from a previous click
            StartCoroutine(EndTurnSequence());
        }

        // Abandons the fight in progress - no confirmation, matching how
        // simple/immediate the tutorial hint's own close button is. Add a
        // confirmation step here first if accidental taps turn out to be a
        // real problem in practice.
        private void OnMenuClicked() => SceneManager.LoadScene("MainMenu");

        // EndPlayerTurn() resolves the whole round synchronously (discard,
        // full enemy turn, next player turn's draw all happen before that
        // call returns) - so every enemy move card queued up in
        // _pendingEnemyReveals by now already happened from the game's
        // point of view. This coroutine is purely presentational: reveal
        // them to the player one at a time with a real gap, hold the final
        // board a beat, then sweep - instead of the old version, which
        // pushed every enemy card AND swept the whole table in the same
        // single frame (so only the last card, mid-vanish, was ever
        // actually visible).
        private IEnumerator EndTurnSequence()
        {
            _resolvingTurn = true;
            EndTurnButton.interactable = false;

            Manager.EndPlayerTurn();

            while (_pendingEnemyReveals.Count > 0)
            {
                EnemyPlayedPile.Push(_pendingEnemyReveals.Dequeue());
                yield return new WaitForSeconds(0.6f);
            }

            // Fight just ended (OnCombatEnded already fired, during
            // EndPlayerTurn above) - skip the sweep and leave the button
            // disabled, AfterCombat owns the transition from here.
            if (!_combatEnded)
            {
                yield return new WaitForSeconds(0.5f); // hold the final board before clearing it
                PlayedPile.SweepAway(FloatingTextLayer);
                EnemyPlayedPile.SweepAway(FloatingTextLayer);
                // Log resets per round, same as the piles - Dave's call:
                // it's "what happened this turn," not a running transcript
                // of the whole fight.
                _logLines.Clear();
                LogText.text = "";
                EndTurnButton.interactable = true;
            }

            _resolvingTurn = false;
        }

        private void OnLog(string line)
        {
            _logLines.Add(line);
            // The log is a scrolling view now (MakeLogScrollView) - it
            // can't overflow onto anything else no matter how much text
            // there is, so this is just a sanity cap against unbounded
            // growth in one very long round, not a "must fit the box" cap.
            // ClearLogForNewTurn (below) resets it well before this ever
            // matters in practice.
            if (_logLines.Count > 40) _logLines.RemoveAt(0);
            LogText.text = string.Join("\n", _logLines);

            // ContentSizeFitter's new height from this text change isn't
            // applied until the next layout pass - force it now so
            // scrolling to the bottom uses the height AFTER this line was
            // added, not the stale one from before it.
            Canvas.ForceUpdateCanvases();
            if (LogScrollRect != null) LogScrollRect.verticalNormalizedPosition = 0f;
        }

        private void OnCombatEnded(bool won)
        {
            OnLog(Spanish ? (won ? "Ganaste." : "Has caído.") : (won ? "You win." : "You have fallen."));
            EndTurnButton.interactable = false;
            _combatEnded = true;

            // Every fight's outcome touches Round progress now, not just
            // the run's final one - a won fight (whether or not the run
            // continues after it) is a round win; any loss ends the run
            // and resets progress, regardless of how far in it happened.
            if (won) PlayerEntitlements.RecordRoundWin();
            else PlayerEntitlements.RecordRunLoss();
            RefreshRoundText();

            StartCoroutine(AfterCombat(won));
        }

        // There's no run/map screen yet (CombatBootstrap's own comment says
        // so) - without this, winning fight 1 just sat there forever with
        // the same enemy on screen, since nothing ever called
        // StartEncounter again. Wins step through EncounterDatabase.Run in
        // order; a loss, or clearing the whole run (beating the boss,
        // last in Run), returns to the main menu.
        private IEnumerator AfterCombat(bool won)
        {
            yield return new WaitForSeconds(2f);

            int nextIndex = _runIndex + 1;
            if (won && nextIndex < EncounterDatabase.Run.Count)
            {
                BeginFight(EncounterDatabase.Run[nextIndex].FightNumber);
            }
            else
            {
                SceneManager.LoadScene("MainMenu");
            }
        }

        // Terms match TutorialPages.cs's Spanish "La Intención Enemiga" page
        // (Ataque, Debilitar, Curarse, Bloquearse, Potenciarse) so the live
        // intent text and the tutorial that explains it use the same words.
        private string DescribeIntent(IntentStep step)
        {
            if (Spanish)
            {
                switch (step.Kind)
                {
                    case IntentKind.Attack: return $"Ataque {step.Amount}";
                    case IntentKind.WeakenAttack: return $"Debilitar {step.Amount} + Ataque {step.Amount2}";
                    case IntentKind.Weaken: return $"Debilitar {step.Amount}";
                    case IntentKind.HealSelf: return $"Curar {step.Amount}";
                    case IntentKind.BlockSelf: return $"Bloqueo {step.Amount}";
                    case IntentKind.BuffSelfDamage: return $"Potenciar +{step.Amount} daño";
                    default: return "";
                }
            }
            switch (step.Kind)
            {
                case IntentKind.Attack: return $"Attack {step.Amount}";
                case IntentKind.WeakenAttack: return $"Weaken {step.Amount} + Attack {step.Amount2}";
                case IntentKind.Weaken: return $"Weaken {step.Amount}";
                case IntentKind.HealSelf: return $"Heal {step.Amount}";
                case IntentKind.BlockSelf: return $"Block {step.Amount}";
                case IntentKind.BuffSelfDamage: return $"Buff +{step.Amount} dmg";
                default: return "";
            }
        }

        // --- Structured combat feedback: floating damage/block numbers +
        // a brief hit-flash, so playing a card or the enemy acting is
        // visibly felt, not just a silent state change and a small log
        // line. Delayed one frame before capturing/restoring "original"
        // color so this always runs AFTER the same-frame Refresh() that
        // follows the event that triggered it (otherwise Refresh sets the
        // panel's real color right back over the flash before it renders).
        // Enemy panel is ~340x480 (card art aspect); Image.transform.position
        // is its CENTER, so +60 up only reached the bust's neck/chin - small,
        // and fighting the art for contrast. +260 clears the top edge of the
        // card frame entirely, landing in the open backdrop above it.
        private static readonly Vector3 EnemyFloatOffset = Vector3.up * 260f;

        // PlayerHpText is top-left anchored/pivoted (anchoredPosition 30,-30
        // from the screen's actual top-left corner) - transform.position IS
        // that corner. The old "+20 up" pushed the spawn point almost to the
        // literal top edge of the canvas, so the popup rendered half off
        // screen (this is exactly the clipped "+8" bug). Spawn from the
        // text's own visual CENTER instead (half its 540x70 box, offset from
        // the pivot corner) so there's real room to rise into the open space
        // above it before the enemy row.
        private static readonly Vector3 PlayerFloatOffset = new Vector3(270f, -35f, 0f);

        private void OnEnemyDamagedFeedback(EnemyCombatant enemy, int amount)
        {
            var panel = _enemyPanels.Find(p => p.Enemy == enemy);
            if (panel == null) return;
            FloatingCombatText.Spawn(FloatingTextLayer, panel.Image.transform.position + EnemyFloatOffset,
                $"-{amount}", new Color(1f, 0.25f, 0.2f));
            StartCoroutine(FlashGraphic(panel.Image, new Color(1f, 0.3f, 0.3f)));
        }

        private void OnPlayerDamagedFeedback(int amount)
        {
            FloatingCombatText.Spawn(FloatingTextLayer, PlayerHpText.transform.position + PlayerFloatOffset,
                $"-{amount}", new Color(1f, 0.25f, 0.2f));
            StartCoroutine(FlashGraphic(PlayerHpText, new Color(1f, 0.3f, 0.3f)));
        }

        private void OnEnemyBlockedFeedback(EnemyCombatant enemy, int amount)
        {
            var panel = _enemyPanels.Find(p => p.Enemy == enemy);
            if (panel == null) return;
            FloatingCombatText.Spawn(FloatingTextLayer, panel.Image.transform.position + EnemyFloatOffset,
                $"+{amount}", new Color(0.4f, 0.75f, 1f));
        }

        private void OnPlayerBlockedFeedback(int amount)
        {
            FloatingCombatText.Spawn(FloatingTextLayer, PlayerHpText.transform.position + PlayerFloatOffset,
                $"+{amount}", new Color(0.4f, 0.75f, 1f));
        }

        // Manager.EndPlayerTurn() resolves the WHOLE enemy turn (every
        // enemy, every step of their pattern) synchronously in one call -
        // "played so fast [the enemy cards] are almost unseeable" was that
        // loop firing this event several times in the same single frame,
        // so only the last card was ever actually rendered before the
        // sweep animation (also triggered right after, same frame) erased
        // it. Queuing instead of pushing immediately lets EndTurnSequence
        // reveal them one at a time with a real gap between each.
        private void OnEnemyMovePlayedFeedback(EnemyCombatant enemy, IntentKind kind)
        {
            if (!EnemyMoveArt.TryGetValue(kind, out string key)) return;
            _pendingEnemyReveals.Enqueue(LoadEnemyMoveTexture(key));
        }

        // Same defensive-destroy check as PlayedCardStack.PunchIn: an
        // enemy panel this is flashing can be destroyed by the next
        // BeginFight() (new encounter) before a 0.25s flash finishes.
        private IEnumerator FlashGraphic(Graphic graphic, Color flashColor)
        {
            yield return null; // let this frame's Refresh() settle the real color first
            if (graphic == null) yield break;
            Color original = graphic.color;
            graphic.color = flashColor;
            const float duration = 0.25f;
            float t = 0f;
            while (t < duration)
            {
                t += Time.unscaledDeltaTime;
                if (graphic == null) yield break;
                graphic.color = Color.Lerp(flashColor, original, t / duration);
                yield return null;
            }
            if (graphic == null) yield break;
            graphic.color = original;
        }

        private Texture2D LoadCardTexture(string cardId)
        {
            string folder = Spanish ? "ES" : "EN";
            string frame = PlayerEntitlements.EquippedFrame;
            return Resources.Load<Texture2D>($"Art/Cards/{frame}/{folder}/{cardId}");
        }

        private Texture2D LoadEnemyTexture(string artId)
        {
            string folder = Spanish ? "ES" : "EN";
            return Resources.Load<Texture2D>($"Art/EnemyCards/{folder}/{artId}");
        }

        private Texture2D LoadEnemyMoveTexture(string key)
        {
            string folder = Spanish ? "ES" : "EN";
            return Resources.Load<Texture2D>($"Art/EnemyMoves/{folder}/{key}");
        }

        private void OnDestroy()
        {
            if (Manager == null) return;
            Manager.OnStateChanged -= Refresh;
            Manager.OnLog -= OnLog;
            Manager.OnCombatEnded -= OnCombatEnded;
            Manager.OnEnemyDamaged -= OnEnemyDamagedFeedback;
            Manager.OnPlayerDamaged -= OnPlayerDamagedFeedback;
            Manager.OnEnemyBlocked -= OnEnemyBlockedFeedback;
            Manager.OnPlayerBlocked -= OnPlayerBlockedFeedback;
            Manager.OnEnemyMovePlayed -= OnEnemyMovePlayedFeedback;
        }
    }
}

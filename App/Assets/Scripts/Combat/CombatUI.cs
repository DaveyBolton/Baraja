using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Baraja.Core;

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
        [HideInInspector] public Text LogText;
        [HideInInspector] public Button EndTurnButton;

        [HideInInspector] public Transform EnemyContainer;
        [HideInInspector] public GameObject EnemyPanelPrefab;

        [HideInInspector] public Transform HandContainer;
        [HideInInspector] public GameObject CardButtonPrefab;

        private readonly List<EnemyPanel> _enemyPanels = new List<EnemyPanel>();
        private EnemyCombatant _selectedTarget;
        private readonly List<string> _logLines = new List<string>();

        private class EnemyPanel
        {
            public EnemyCombatant Enemy;
            public RawImage Image;
            public Text HpText;
            public Text IntentText;
            public Button Button;
        }

        private void Awake()
        {
            EndTurnButton.onClick.AddListener(OnEndTurnClicked);
        }

        public void BeginFight(int fightNumber)
        {
            // Read fresh rather than trusting whatever the scene builder set:
            // the player may have flipped the language in Options since this
            // scene was built.
            Spanish = GameSettings.Spanish;
            Manager.Spanish = Spanish;

            Manager.OnStateChanged += Refresh;
            Manager.OnLog += OnLog;
            Manager.OnCombatEnded += OnCombatEnded;
            Manager.StartEncounter(fightNumber);
            BuildEnemyPanels();
            Refresh();
        }

        private void BuildEnemyPanels()
        {
            foreach (var panel in _enemyPanels) Destroy(panel.Button.gameObject);
            _enemyPanels.Clear();

            foreach (var enemy in Manager.Enemies)
            {
                var go = Instantiate(EnemyPanelPrefab, EnemyContainer);
                go.SetActive(true);
                var panel = new EnemyPanel
                {
                    Enemy = enemy,
                    Image = go.transform.Find("Image").GetComponent<RawImage>(),
                    HpText = go.transform.Find("HpText").GetComponent<Text>(),
                    IntentText = go.transform.Find("IntentText").GetComponent<Text>(),
                    Button = go.GetComponent<Button>(),
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
            PlayerHpText.text = $"HP {Manager.Player.Hp}/{Manager.Player.MaxHp}  Block {Manager.Player.Block}";
            PlayerEnergyText.text = $"Energy {Manager.Player.Energy}/{Manager.Player.MaxEnergy}";

            foreach (var panel in _enemyPanels)
            {
                bool dead = panel.Enemy.IsDead;
                panel.HpText.text = dead ? "—" : $"{panel.Enemy.Data.DisplayName(Spanish)}\nHP {panel.Enemy.Hp}/{panel.Enemy.MaxHp}  Block {panel.Enemy.Block}";
                panel.IntentText.text = dead ? "" : DescribeIntent(panel.Enemy.CurrentIntent);
                panel.Button.interactable = !dead;
                panel.Image.color = panel.Enemy == _selectedTarget ? Color.white : new Color(0.7f, 0.7f, 0.7f);
            }

            RebuildHand();
        }

        private void RebuildHand()
        {
            foreach (Transform child in HandContainer) Destroy(child.gameObject);

            foreach (var card in Manager.PlayerDeck.Hand)
            {
                var go = Instantiate(CardButtonPrefab, HandContainer);
                go.SetActive(true);
                var image = go.GetComponent<RawImage>();
                image.texture = LoadCardTexture(card.Id);
                var button = go.GetComponent<Button>();
                bool canPlay = Manager.CanPlay(card);
                button.interactable = canPlay;
                button.onClick.AddListener(() => PlayCard(card));
            }
        }

        private void PlayCard(CardData card)
        {
            Manager.PlayCard(card, _selectedTarget);
        }

        private void OnEndTurnClicked() => Manager.EndPlayerTurn();

        private void OnLog(string line)
        {
            _logLines.Add(line);
            if (_logLines.Count > 4) _logLines.RemoveAt(0);
            LogText.text = string.Join("\n", _logLines);
        }

        private void OnCombatEnded(bool won)
        {
            OnLog(won ? "You win." : "You have fallen.");
            EndTurnButton.interactable = false;
        }

        private string DescribeIntent(IntentStep step)
        {
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

        private Texture2D LoadCardTexture(string cardId)
        {
            string folder = Spanish ? "ES" : "EN";
            return Resources.Load<Texture2D>($"Art/Cards/{folder}/{cardId}");
        }

        private Texture2D LoadEnemyTexture(string artId)
        {
            return Resources.Load<Texture2D>($"Art/Enemies/{artId}");
        }

        private void OnDestroy()
        {
            if (Manager == null) return;
            Manager.OnStateChanged -= Refresh;
            Manager.OnLog -= OnLog;
            Manager.OnCombatEnded -= OnCombatEnded;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Baraja.Combat
{
    // Drives one turn-based 1v1 (or 1v2 for Doble Calavera) fight per
    // COMBAT.md / ENEMIES.md / CARDS.md. No branching AI: enemies just walk
    // their telegraphed IntentStep pattern and wrap around.
    public class CombatManager : MonoBehaviour
    {
        public const int StartingHp = 50;
        public const int StartingEnergy = 3;
        public const int HandSize = 5;

        // Set by CombatUI before StartEncounter, from GameSettings.Spanish - kept
        // as a plain bool rather than a Core.GameSettings reference so this class
        // stays UI/persistence-agnostic.
        public bool Spanish = true;

        public PlayerState Player { get; private set; }
        public List<EnemyCombatant> Enemies { get; private set; } = new List<EnemyCombatant>();
        public Deck PlayerDeck { get; private set; }

        public event Action OnStateChanged;
        public event Action<string> OnLog;
        public event Action<bool> OnCombatEnded; // true = player won

        // Structured (not string) damage/block events, purely for visual
        // feedback (floating numbers, hit flashes) - OnLog already
        // describes what happened in text, but a player watching the
        // board has no way to connect "a card disappeared" to "something
        // happened" without something landing ON the thing that got hit.
        public event Action<EnemyCombatant, int> OnEnemyDamaged;
        public event Action<int> OnPlayerDamaged;
        public event Action<EnemyCombatant, int> OnEnemyBlocked;
        public event Action<int> OnPlayerBlocked;

        // Fired once per enemy per turn, right as their telegraphed intent
        // starts resolving - lets CombatUI show an actual "card" for
        // what the enemy just did (see CombatUI.EnemyMoveArt), instead of
        // only the HP/block numbers changing silently.
        public event Action<EnemyCombatant, IntentKind> OnEnemyMovePlayed;

        private bool _combatOver;

        public void StartEncounter(int fightNumber)
        {
            var encounter = EncounterDatabase.Run.FirstOrDefault(e =>
                fightNumber == 0 ? e.IsBoss : e.FightNumber == fightNumber);
            if (encounter == null)
            {
                Log($"No encounter found for fight {fightNumber}");
                return;
            }

            Player = new PlayerState(StartingHp, StartingEnergy);
            PlayerDeck = new Deck(CardDatabase.All);
            Enemies = encounter.EnemyIds
                .Select(id => new EnemyCombatant(EnemyDatabase.ById(id)))
                .ToList();
            _combatOver = false;

            if (Spanish) Log(encounter.IsBoss ? "La Catrina aparece." : $"Combate {fightNumber} comienza.");
            else Log(encounter.IsBoss ? "La Catrina appears." : $"Fight {fightNumber} begins.");
            BeginPlayerTurn();
        }

        private void BeginPlayerTurn()
        {
            if (_combatOver) return;

            Player.Energy = StartingEnergy + Player.EnergyPerTurnBonus;
            if (Player.BlockPerTurnBonus > 0)
            {
                Player.GainBlock(Player.BlockPerTurnBonus);
                OnPlayerBlocked?.Invoke(Player.BlockPerTurnBonus);
            }
            PlayerDeck.DrawToHand(HandSize);
            Notify();
        }

        public bool CanPlay(CardData card) => !_combatOver && Player.Energy >= card.Cost && PlayerDeck.Hand.Contains(card);

        public void PlayCard(CardData card, EnemyCombatant target)
        {
            if (!CanPlay(card)) return;

            Player.Energy -= card.Cost;
            ResolveCardEffect(card, target);
            PlayerDeck.PlayFromHand(card);

            if (card.Type == CardType.Power && Player.PowerPlayedDamage > 0)
            {
                string relicName = CardName(CardDatabase.ById("reliquia_dorada"));
                foreach (var e in AliveEnemies()) DamageEnemy(e, Player.PowerPlayedDamage, relicName);
            }

            CheckForVictory();
            Notify();
        }

        private void ResolveCardEffect(CardData card, EnemyCombatant target)
        {
            switch (card.Effect)
            {
                case EffectType.DealDamage:
                    if (target != null) DamageEnemy(target, card.Amount, CardName(card));
                    break;

                case EffectType.DealDamageMultiHit:
                    if (target != null)
                        for (int i = 0; i < card.Amount2; i++) DamageEnemy(target, card.Amount, CardName(card));
                    break;

                case EffectType.DealDamageAllEnemies:
                    foreach (var e in AliveEnemies()) DamageEnemy(e, card.Amount, CardName(card));
                    break;

                case EffectType.DealDamageApplyBurn:
                    if (target != null)
                    {
                        DamageEnemy(target, card.Amount, CardName(card));
                        target.Status.Add(StatusType.Burn, card.Amount2);
                    }
                    break;

                case EffectType.DealDamageApplyFreeze:
                    if (target != null)
                    {
                        DamageEnemy(target, card.Amount, CardName(card));
                        target.Status.SetFlag(StatusType.Freeze);
                    }
                    break;

                case EffectType.DealDamageExecute:
                    if (target != null)
                    {
                        bool below50 = target.Hp <= target.MaxHp / 2;
                        DamageEnemy(target, below50 ? card.Amount2 : card.Amount, CardName(card));
                    }
                    break;

                case EffectType.GainBlock:
                    Player.GainBlock(card.Amount);
                    OnPlayerBlocked?.Invoke(card.Amount);
                    break;

                case EffectType.GainBlockUntargetable:
                    Player.GainBlock(card.Amount);
                    OnPlayerBlocked?.Invoke(card.Amount);
                    Player.Status.SetFlag(StatusType.Untargetable);
                    break;

                case EffectType.GainBlockDraw:
                    Player.GainBlock(card.Amount);
                    OnPlayerBlocked?.Invoke(card.Amount);
                    PlayerDeck.DrawToHand(card.Amount2);
                    break;

                case EffectType.Draw:
                    PlayerDeck.DrawToHand(card.Amount);
                    break;

                case EffectType.GainBlockRemoveDebuff:
                    Player.GainBlock(card.Amount);
                    OnPlayerBlocked?.Invoke(card.Amount);
                    Player.Status.ClearAllDebuffs();
                    break;

                case EffectType.RandomEffect:
                    ResolveCatrinasLuck(card, target);
                    break;

                case EffectType.PowerReflect:
                    Player.ReflectAmount += card.Amount;
                    break;

                case EffectType.PowerBurnEnemyEachTurn:
                    Player.BurnEnemyEachTurn += card.Amount;
                    break;

                case EffectType.PowerEnergyPerTurn:
                    Player.EnergyPerTurnBonus += card.Amount;
                    break;

                case EffectType.PowerBlockPerTurn:
                    Player.BlockPerTurnBonus += card.Amount;
                    break;

                case EffectType.PowerDamageOnPowerPlayed:
                    Player.PowerPlayedDamage += card.Amount;
                    break;

                case EffectType.PowerDoubleBurnEndOfTurn:
                    Player.DoubleEnemyBurnAtTurnEnd = true;
                    break;
            }
        }

        private void ResolveCatrinasLuck(CardData card, EnemyCombatant target)
        {
            int roll = UnityEngine.Random.Range(0, 3);
            if (roll == 0 && target != null) DamageEnemy(target, card.Amount, CardName(card));
            else if (roll == 1) { Player.GainBlock(card.Amount2); OnPlayerBlocked?.Invoke(card.Amount2); }
            else PlayerDeck.DrawToHand(card.Amount3);
        }

        private void DamageEnemy(EnemyCombatant enemy, int rawDamage, string source)
        {
            if (enemy == null || enemy.IsDead) return;
            int dealt = Player.ModifyOutgoingDamage(rawDamage);
            enemy.TakeDamage(dealt);
            Log(Spanish ? $"{source} golpea a {EnemyName(enemy)} por {dealt}." : $"{source} hits {EnemyName(enemy)} for {dealt}.");
            OnEnemyDamaged?.Invoke(enemy, dealt);
        }

        private string CardName(CardData card) => card.DisplayName(Spanish);
        private string EnemyName(EnemyCombatant enemy) => enemy.Data.DisplayName(Spanish);

        public void EndPlayerTurn()
        {
            if (_combatOver) return;

            // Powers that trigger at end of the player's turn.
            if (Player.BurnEnemyEachTurn > 0)
                foreach (var e in AliveEnemies()) e.Status.Add(StatusType.Burn, Player.BurnEnemyEachTurn);

            if (Player.DoubleEnemyBurnAtTurnEnd)
                foreach (var e in AliveEnemies()) e.Status.Add(StatusType.Burn, e.Status.Get(StatusType.Burn));

            PlayerDeck.DiscardHand();
            Player.ClearBlockAtTurnEnd();

            RunEnemyTurn();
            if (_combatOver) return;

            BeginPlayerTurn();
        }

        private void RunEnemyTurn()
        {
            bool wasUntargetable = Player.Status.Has(StatusType.Untargetable);
            Player.Status.Clear(StatusType.Untargetable);

            foreach (var enemy in AliveEnemies())
            {
                var step = enemy.CurrentIntent;
                ResolveEnemyStep(enemy, step, wasUntargetable);
                enemy.AdvancePattern();

                int burnDmg = enemy.Status.TickBurn();
                if (burnDmg > 0)
                {
                    enemy.TakeDamage(burnDmg);
                    Log(Spanish ? $"{EnemyName(enemy)} se quema por {burnDmg}." : $"{EnemyName(enemy)} burns for {burnDmg}.");
                    OnEnemyDamaged?.Invoke(enemy, burnDmg);
                }

                if (CheckForVictory()) return;
            }

            int playerBurnDmg = Player.Status.TickBurn();
            if (playerBurnDmg > 0)
            {
                Player.TakeDamage(playerBurnDmg);
                OnPlayerDamaged?.Invoke(playerBurnDmg);
            }

            if (Player.IsDead)
            {
                EndCombat(false);
                return;
            }

            Notify();
        }

        private void ResolveEnemyStep(EnemyCombatant enemy, IntentStep step, bool playerUntargetable)
        {
            OnEnemyMovePlayed?.Invoke(enemy, step.Kind);
            switch (step.Kind)
            {
                case IntentKind.Attack:
                    AttackPlayer(enemy, enemy.BonusDamage + step.Amount, playerUntargetable);
                    break;

                case IntentKind.WeakenAttack:
                    if (!playerUntargetable) Player.Status.Add(StatusType.Weaken, step.Amount);
                    AttackPlayer(enemy, enemy.BonusDamage + step.Amount2, playerUntargetable);
                    break;

                case IntentKind.Weaken:
                    if (!playerUntargetable) Player.Status.Add(StatusType.Weaken, step.Amount);
                    break;

                case IntentKind.HealSelf:
                    enemy.Heal(step.Amount);
                    break;

                case IntentKind.BlockSelf:
                    enemy.GainBlock(step.Amount);
                    OnEnemyBlocked?.Invoke(enemy, step.Amount);
                    break;

                case IntentKind.BuffSelfDamage:
                    enemy.BonusDamage += step.Amount;
                    break;
            }
        }

        private void AttackPlayer(EnemyCombatant enemy, int rawDamage, bool playerUntargetable)
        {
            if (playerUntargetable)
            {
                string gravestName = CardName(CardDatabase.ById("tumba_sellada"));
                Log(Spanish
                    ? $"El ataque de {EnemyName(enemy)} no encuentra objetivo ({gravestName})."
                    : $"{EnemyName(enemy)}'s attack finds no target ({gravestName}).");
                return;
            }

            int dealt = enemy.ModifyOutgoingDamage(rawDamage);
            Player.TakeDamage(dealt);
            Log(Spanish ? $"{EnemyName(enemy)} te golpea por {dealt}." : $"{EnemyName(enemy)} hits you for {dealt}.");
            OnPlayerDamaged?.Invoke(dealt);

            if (Player.ReflectAmount > 0)
            {
                enemy.TakeDamage(Player.ReflectAmount);
                string crownName = CardName(CardDatabase.ById("corona_de_espinas"));
                Log(Spanish ? $"{crownName} refleja {Player.ReflectAmount} de vuelta." : $"{crownName} reflects {Player.ReflectAmount} back.");
                OnEnemyDamaged?.Invoke(enemy, Player.ReflectAmount);
            }
        }

        private IEnumerable<EnemyCombatant> AliveEnemies() => Enemies.Where(e => !e.IsDead);

        private bool CheckForVictory()
        {
            if (_combatOver) return true;
            if (!Enemies.Any(e => !e.IsDead))
            {
                EndCombat(true);
                return true;
            }
            return false;
        }

        private void EndCombat(bool won)
        {
            _combatOver = true;
            Log(Spanish ? (won ? "¡Victoria!" : "Derrota.") : (won ? "Victory." : "Defeat."));
            Notify();
            OnCombatEnded?.Invoke(won);
        }

        private void Log(string message) => OnLog?.Invoke(message);
        private void Notify() => OnStateChanged?.Invoke();
    }
}

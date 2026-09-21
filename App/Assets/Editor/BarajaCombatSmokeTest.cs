using System.Collections.Generic;
using UnityEngine;
using Baraja.Combat;

/// <summary>
/// Headless combat-logic smoke test, run via:
/// Unity -batchmode -executeMethod BarajaCombatSmokeTest.Run -quit
/// Auto-plays every fight in the run (1-9 plus the boss, fight 0) to
/// completion using a dumb "play whatever fits, attack the first alive
/// enemy" policy, with no Canvas/UI involved, to catch logic bugs across
/// the whole enemy roster and card pool without needing a person to click
/// through every scene.
/// </summary>
public static class BarajaCombatSmokeTest
{
    private static readonly int[] FightNumbers = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 0 };

    public static void Run()
    {
        int passCount = 0;
        int winCount = 0;
        var failures = new List<string>();

        foreach (int fightNumber in FightNumbers)
        {
            string label = fightNumber == 0 ? "BOSS" : $"fight {fightNumber}";
            var go = new GameObject($"SmokeTestCombatManager_{label}");
            var manager = go.AddComponent<CombatManager>();

            bool ended = false;
            bool won = false;
            manager.OnLog += line => Debug.Log($"SRCOMBAT [{label}]: " + line);
            manager.OnCombatEnded += w => { ended = true; won = w; };

            manager.StartEncounter(fightNumber);
            if (manager.Enemies.Count == 0)
            {
                failures.Add($"{label}: StartEncounter produced zero enemies.");
                Object.DestroyImmediate(go);
                continue;
            }

            string enemyList = string.Join(", ", manager.Enemies.ConvertAll(
                e => $"{e.Data.NameEs} {e.Hp}/{e.MaxHp}"));
            Debug.Log($"SRCOMBAT [{label}] start: player {manager.Player.Hp}/{manager.Player.MaxHp} vs {enemyList}");

            int safetyTurns = 0;
            while (!ended && safetyTurns < 50)
            {
                safetyTurns++;
                var target = manager.Enemies.Find(e => !e.IsDead);

                bool playedSomething;
                do
                {
                    playedSomething = false;
                    foreach (var card in new List<CardData>(manager.PlayerDeck.Hand))
                    {
                        if (manager.CanPlay(card))
                        {
                            manager.PlayCard(card, target);
                            playedSomething = true;
                            break;
                        }
                    }
                } while (playedSomething && !ended);

                if (ended) break;
                manager.EndPlayerTurn();
            }

            if (!ended)
            {
                failures.Add($"{label}: did not resolve within 50 turns (infinite loop or stuck state).");
            }
            else
            {
                passCount++;
                if (won) winCount++;
                Debug.Log($"SRCOMBAT [{label}] result: {(won ? "WIN" : "LOSS")} after {safetyTurns} turns, " +
                           $"player HP {manager.Player.Hp}/{manager.Player.MaxHp}.");
            }

            Object.DestroyImmediate(go);
        }

        Debug.Log($"SRCOMBAT SUMMARY: {passCount}/{FightNumbers.Length} resolved cleanly, {winCount} wins.");
        if (failures.Count > 0)
        {
            foreach (var f in failures) Debug.LogError("SRCOMBAT FAILED: " + f);
        }
        else
        {
            Debug.Log("SRCOMBAT PASS: every fight in the run resolved without hanging or throwing.");
        }
    }
}

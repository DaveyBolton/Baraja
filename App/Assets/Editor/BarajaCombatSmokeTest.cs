using UnityEngine;
using Baraja.Combat;

/// <summary>
/// Headless combat-logic smoke test, run via:
/// Unity -batchmode -executeMethod BarajaCombatSmokeTest.Run -quit
/// Plays fight 1 to completion using a dumb "play whatever fits, attack the
/// only enemy" policy, with no Canvas/UI involved, to catch logic bugs in
/// CombatManager without needing a person to click through the scene.
/// </summary>
public static class BarajaCombatSmokeTest
{
    public static void Run()
    {
        var go = new GameObject("SmokeTestCombatManager");
        var manager = go.AddComponent<CombatManager>();

        bool ended = false;
        bool won = false;
        manager.OnLog += line => Debug.Log("SRCOMBAT: " + line);
        manager.OnCombatEnded += w => { ended = true; won = w; };

        manager.StartEncounter(1);
        Debug.Log($"SRCOMBAT start: player {manager.Player.Hp}/{manager.Player.MaxHp}, " +
                   $"enemy {manager.Enemies[0].DisplayName} {manager.Enemies[0].Hp}/{manager.Enemies[0].MaxHp}");

        int safetyTurns = 0;
        while (!ended && safetyTurns < 50)
        {
            safetyTurns++;
            var target = manager.Enemies.Find(e => !e.IsDead);

            bool playedSomething;
            do
            {
                playedSomething = false;
                foreach (var card in new System.Collections.Generic.List<CardData>(manager.PlayerDeck.Hand))
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
            Debug.LogError("SRCOMBAT FAILED: combat did not resolve within 50 turns (infinite loop or stuck state).");
            return;
        }

        Debug.Log($"SRCOMBAT result: {(won ? "WIN" : "LOSS")} after {safetyTurns} turns. " +
                   $"Player HP {manager.Player.Hp}/{manager.Player.MaxHp}.");
        Debug.Log(won ? "SRCOMBAT PASS" : "SRCOMBAT PASS (loss is a valid outcome, loop terminated correctly)");
    }
}

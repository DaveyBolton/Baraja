# Baraja de los Muertos

A roguelike deckbuilder — Día de los Muertos card game, in the spirit of Slay
the Spire / Balatro, reusing Calaverita's visual identity (jeweled sugar-skull
suits, gold filigree UI, papel picado / copal smoke motifs).

Status: concept / pre-production. Unity project scaffolded, nothing built yet.

## Stack

Unity 6000.0.83f1 — same engine/version as Calaverita and Spirit-Run, so the
existing Android build pipeline and IAP/ads integration patterns carry over.
Targeting Android first.

## Locked so far

- Name: **Baraja de los Muertos**
- Genre: roguelike deckbuilder, 1v1 turn-based combat, run-based structure
- Localization: English / Español toggle in Options, required from the start
  (not bolted on later — every card needs bilingual strings)
- Art: reusing Calaverita's 13-icon piece roster as a card suit/rarity
  taxonomy, plus its UI chrome (buttons, headers, wordmark style)
- See [GAMEPLAY.md](GAMEPLAY.md) for the rules, [design/CARDS.md](design/CARDS.md)
  for the v1 card list, [design/COMBAT.md](design/COMBAT.md) for the combat math

## v1 scope (deliberately minimal)

15-20 unique cards, linear run of 8-10 fights + 1 boss, deck grows by picking
1 of 3 cards after each win. No branching map, no relics, no
meta-progression between runs yet — those are explicitly deferred until the
core loop proves fun.

## Next steps

- Lock enemy design (the first boss + the 8-10 regular fights)
- Card frame/layout template (art asset, doesn't exist yet)
- Combat scene scaffold

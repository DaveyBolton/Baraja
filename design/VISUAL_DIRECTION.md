# Visual direction — decided 2026-09-20

Dave's bar: this needs to be **visually stunning**, and he's open to small
animations on the cards. That's a real scope input, so it's logged as a
decision, not a vibe.

## The tension, named honestly

"Stunning + animated" pulls against the v1 discipline of shipping the
minimal loop first. Full per-card frame animation is real production time
and is explicitly NOT a v1 thing. The resolution: treat "stunning" as a
few things done exceptionally well, not everything animated.

## What's in scope for v1

- **Character/card art itself**, generated via the ArtEngine pipeline
  (`C:\Dev\Stephen-AI-Studio\ArtEngine`, `forge.py`, FLUX.2 klein, local,
  no cloud). Costs nothing extra beyond normal art time, it's a prompting
  quality question, not a budget question. First enemy batch (2026-09-20)
  landed strong on the first or second pass for every character.
- **One idle shimmer/parallax shader on card art.** Cheap (shader-level,
  applies to every card for free), reads as production polish.
- **One signature play-VFX**, built from the Copal Smoke motif already
  reused from Calaverita, fires when any card is played. One polished
  effect done well beats mediocre effects on every card.
- **A foil/rarity shader tier reserved for the Gold/Legendary suit only**
  (Reliquia Dorada, Corazón de Rubí, etc.), so the top-end cards feel
  special without paying that cost across all 18 cards.

## Explicitly deferred

Full per-card frame-by-frame animation, unique VFX per card, animated
character portraits (idle breathing loops, etc.). Revisit only after v1
ships and the core loop is proven fun — the same discipline already
applied to relics/meta-progression/branching map.

## Enemy art status (2026-09-20)

All 6 enemy types generated and approved on the first look:
Calaca Menor, Perro Xolo, Catrina Menor, La Catrina (boss), Alma en Pena,
Guardián de Ofrenda, Doble Calavera (needed one re-prompt — the first pass
put the twin pair inside a literal picture frame because the prompt said
"framed," fixed by dropping that word). Stat-bumped repeats (Calaca Menor
v2, Alma en Pena v2, Guardián de Ofrenda v2 per `ENEMIES.md`) reuse the
same art, no new generation needed.

Output lives in `C:\Dev\Stephen-AI-Studio\ArtEngine\out\baraja_enemies_v1\`,
`baraja_enemies_v2\`, and `baraja_enemies_v2_fix\` — not yet copied into
the Unity project's Assets.

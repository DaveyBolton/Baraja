# Enemies — v1 (9 regular fights + 1 boss)

Six enemy types cover nine fights (a few repeat with a stat bump), keeping
content cost low per the v1 discipline. Each teaches or re-tests one
mechanic so the run has a real difficulty curve, not just bigger numbers.

| # | Enemy (ES / EN) | HP | Pattern (telegraphed, cycles) | Teaches |
|---|---|---|---|---|
| 1 | Calaca Menor / Lesser Calaca | 18 | Attack 6 → Attack 6 | Tutorial: pure damage race |
| 2 | Alma en Pena / Wandering Soul | 20 | Debuff (Weaken, -1 dmg dealt) → Attack 7 | Manage a debuff, kill it fast |
| 3 | Calaca Menor v2 / Lesser Calaca | 22 | Attack 7 → Attack 7 | Same lesson, higher stakes |
| 4 | Perro Xolo / Xolo Dog | 22 | Attack 5 → Attack 5 → Heal self 5 | Racing a self-healer, burst matters |
| 5 | Guardián de Ofrenda / Offering Guardian | 30 | Block self 8 → Attack 9 → Attack 9 | Enemy block exists too, don't stall |
| 6 | Alma en Pena v2 / Wandering Soul | 26 | Debuff (Weaken, -2 dmg dealt) → Attack 9 | Debuff mgmt, higher cost of ignoring it |
| 7 | Catrina Menor / Lesser Catrina | 28 | Attack 6 → Buff self (+2 dmg, permanent) → Attack (scales) | Snowball threat, must not let it ride |
| 8 | Doble Calavera / Twin Skulls | 15 + 15 (2 enemies) | One attacks 5/turn, other alternates Block 6 / Attack 5 | AoE payoff (Marigold Bomb shines) |
| 9 | Guardián de Ofrenda v2 / Offering Guardian | 38 | Block self 10 → Attack 11 → Attack 11 | Hardest regular wall before the boss |

## Boss: La Catrina

HP 80. Three-move telegraphed cycle, no branching AI needed:

1. **Golpe Elegante** (Elegant Strike) — Attack, 12 dmg
2. **Mirada de Juicio** (Judging Gaze) — Debuff (Weaken, -3 dmg dealt) + Attack, 6 dmg
3. **Florecer de Cempasúchil** (Marigold Bloom) — Buff self, +4 dmg permanently for rest of fight

Cycle repeats 1→2→3. Deliberately remixes mechanics already seen in fights
2/6 (Weaken) and 7 (self-buff snowball) at higher stakes, rather than
introducing anything new, so the boss tests what the run already taught
instead of ambushing the player with a fresh mechanic.

Deferred to post-v1: elite fights outside this line, a second act/enemy
pool, any real branching AI.

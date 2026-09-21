using System.Collections.Generic;

namespace Baraja.Combat
{
    public enum IntentKind
    {
        Attack,
        Weaken,        // debuff only, no damage this step
        HealSelf,
        BlockSelf,
        BuffSelfDamage,  // permanent +damage to this enemy's own future Attack steps
        WeakenAttack     // combined debuff + attack in one step (La Catrina step 2)
    }

    // One step of an enemy's telegraphed, cycling pattern. No branching AI in v1 -
    // CombatManager just walks this list and wraps back to index 0.
    public class IntentStep
    {
        public IntentKind Kind;
        public int Amount;   // attack dmg / weaken stacks / heal amount / block amount / buff amount
        public int Amount2;  // WeakenAttack only: the attack dmg half of the combined step
    }

    public class EnemyData
    {
        public string Id;
        public string ArtId; // Resources/Art/Enemies/<ArtId>.png - shared across stat-bumped repeats of the same type
        public string NameEs;
        public string NameEn;
        public int MaxHp;
        public List<IntentStep> Pattern;
        public string Teaches;

        public string DisplayName(bool spanish) => spanish ? NameEs : NameEn;
    }

    public static class EnemyDatabase
    {
        public static readonly List<EnemyData> All = new List<EnemyData>
        {
            new EnemyData { Id = "calaca_menor_1", ArtId = "calaca_menor", NameEs = "Calaca Menor", NameEn = "Lesser Calaca",
                MaxHp = 18, Teaches = "Tutorial: pure damage race",
                Pattern = new List<IntentStep> {
                    new IntentStep { Kind = IntentKind.Attack, Amount = 6 },
                    new IntentStep { Kind = IntentKind.Attack, Amount = 6 },
                } },

            new EnemyData { Id = "alma_en_pena_1", ArtId = "alma_en_pena", NameEs = "Alma en Pena", NameEn = "Wandering Soul",
                MaxHp = 20, Teaches = "Manage a debuff, kill it fast",
                Pattern = new List<IntentStep> {
                    new IntentStep { Kind = IntentKind.Weaken, Amount = 1 },
                    new IntentStep { Kind = IntentKind.Attack, Amount = 7 },
                } },

            new EnemyData { Id = "calaca_menor_2", ArtId = "calaca_menor", NameEs = "Calaca Menor", NameEn = "Lesser Calaca",
                MaxHp = 22, Teaches = "Same lesson, higher stakes",
                Pattern = new List<IntentStep> {
                    new IntentStep { Kind = IntentKind.Attack, Amount = 7 },
                    new IntentStep { Kind = IntentKind.Attack, Amount = 7 },
                } },

            new EnemyData { Id = "perro_xolo", ArtId = "perro_xolo", NameEs = "Perro Xolo", NameEn = "Xolo Dog",
                MaxHp = 22, Teaches = "Racing a self-healer, burst matters",
                Pattern = new List<IntentStep> {
                    new IntentStep { Kind = IntentKind.Attack, Amount = 5 },
                    new IntentStep { Kind = IntentKind.Attack, Amount = 5 },
                    new IntentStep { Kind = IntentKind.HealSelf, Amount = 5 },
                } },

            new EnemyData { Id = "guardian_ofrenda_1", ArtId = "guardian_de_ofrenda", NameEs = "Guardián de Ofrenda", NameEn = "Offering Guardian",
                MaxHp = 30, Teaches = "Enemy block exists too, don't stall",
                Pattern = new List<IntentStep> {
                    new IntentStep { Kind = IntentKind.BlockSelf, Amount = 8 },
                    new IntentStep { Kind = IntentKind.Attack, Amount = 9 },
                    new IntentStep { Kind = IntentKind.Attack, Amount = 9 },
                } },

            new EnemyData { Id = "alma_en_pena_2", ArtId = "alma_en_pena", NameEs = "Alma en Pena", NameEn = "Wandering Soul",
                MaxHp = 26, Teaches = "Debuff mgmt, higher cost of ignoring it",
                Pattern = new List<IntentStep> {
                    new IntentStep { Kind = IntentKind.Weaken, Amount = 2 },
                    new IntentStep { Kind = IntentKind.Attack, Amount = 9 },
                } },

            new EnemyData { Id = "catrina_menor", ArtId = "catrina_menor", NameEs = "Catrina Menor", NameEn = "Lesser Catrina",
                MaxHp = 28, Teaches = "Snowball threat, must not let it ride",
                Pattern = new List<IntentStep> {
                    new IntentStep { Kind = IntentKind.Attack, Amount = 6 },
                    new IntentStep { Kind = IntentKind.BuffSelfDamage, Amount = 2 },
                    new IntentStep { Kind = IntentKind.Attack, Amount = 6 }, // scales via accrued BuffSelfDamage
                } },

            new EnemyData { Id = "doble_calavera_a", ArtId = "doble_calavera", NameEs = "Calavera Gemela (A)", NameEn = "Twin Skull (A)",
                MaxHp = 15, Teaches = "AoE payoff (Marigold Bomb shines)",
                Pattern = new List<IntentStep> {
                    new IntentStep { Kind = IntentKind.Attack, Amount = 5 },
                } },

            new EnemyData { Id = "doble_calavera_b", ArtId = "doble_calavera", NameEs = "Calavera Gemela (B)", NameEn = "Twin Skull (B)",
                MaxHp = 15, Teaches = "AoE payoff (Marigold Bomb shines)",
                Pattern = new List<IntentStep> {
                    new IntentStep { Kind = IntentKind.BlockSelf, Amount = 6 },
                    new IntentStep { Kind = IntentKind.Attack, Amount = 5 },
                } },

            new EnemyData { Id = "guardian_ofrenda_2", ArtId = "guardian_de_ofrenda", NameEs = "Guardián de Ofrenda", NameEn = "Offering Guardian",
                MaxHp = 38, Teaches = "Hardest regular wall before the boss",
                Pattern = new List<IntentStep> {
                    new IntentStep { Kind = IntentKind.BlockSelf, Amount = 10 },
                    new IntentStep { Kind = IntentKind.Attack, Amount = 11 },
                    new IntentStep { Kind = IntentKind.Attack, Amount = 11 },
                } },

            new EnemyData { Id = "la_catrina", ArtId = "la_catrina", NameEs = "La Catrina", NameEn = "La Catrina",
                MaxHp = 80, Teaches = "Boss: remixes Weaken and self-buff snowball at higher stakes",
                Pattern = new List<IntentStep> {
                    new IntentStep { Kind = IntentKind.Attack, Amount = 12 },                         // Golpe Elegante
                    new IntentStep { Kind = IntentKind.WeakenAttack, Amount = 3, Amount2 = 6 },       // Mirada de Juicio
                    new IntentStep { Kind = IntentKind.BuffSelfDamage, Amount = 4 },                  // Florecer de Cempasúchil
                } },
        };

        public static EnemyData ById(string id)
        {
            foreach (var e in All) if (e.Id == id) return e;
            return null;
        }
    }

    // One row per fight in the run: which enemy id(s) appear together.
    // Separate from EnemyData because enemy *types* repeat across fights
    // (e.g. Calaca Menor appears in fights 1 and 3 with different HP/dmg).
    public class EncounterData
    {
        public int FightNumber; // 1-9, or 0 for the boss
        public bool IsBoss;
        public List<string> EnemyIds;
    }

    public static class EncounterDatabase
    {
        public static readonly List<EncounterData> Run = new List<EncounterData>
        {
            new EncounterData { FightNumber = 1, EnemyIds = new List<string> { "calaca_menor_1" } },
            new EncounterData { FightNumber = 2, EnemyIds = new List<string> { "alma_en_pena_1" } },
            new EncounterData { FightNumber = 3, EnemyIds = new List<string> { "calaca_menor_2" } },
            new EncounterData { FightNumber = 4, EnemyIds = new List<string> { "perro_xolo" } },
            new EncounterData { FightNumber = 5, EnemyIds = new List<string> { "guardian_ofrenda_1" } },
            new EncounterData { FightNumber = 6, EnemyIds = new List<string> { "alma_en_pena_2" } },
            new EncounterData { FightNumber = 7, EnemyIds = new List<string> { "catrina_menor" } },
            new EncounterData { FightNumber = 8, EnemyIds = new List<string> { "doble_calavera_a", "doble_calavera_b" } },
            new EncounterData { FightNumber = 9, EnemyIds = new List<string> { "guardian_ofrenda_2" } },
            new EncounterData { FightNumber = 0, IsBoss = true, EnemyIds = new List<string> { "la_catrina" } },
        };
    }
}

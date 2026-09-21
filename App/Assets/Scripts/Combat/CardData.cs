using System.Collections.Generic;

namespace Baraja.Combat
{
    // One card's full definition. Immutable data, matches design/CARDS.md.
    public class CardData
    {
        public string Id;           // matches the art filename stem, e.g. "filo_de_hueso"
        public string NameEs;
        public string NameEn;
        public int Cost;
        public CardType Type;
        public Suit Suit;
        public EffectType Effect;
        public int Amount;          // primary effect number (damage, block, etc.)
        public int Amount2;         // secondary number (burn stacks, draw count, etc.)
        public int Amount3;         // Suerte de Catrina only: third random-outcome number

        public string DisplayName(bool spanish) => spanish ? NameEs : NameEn;
    }

    public static class CardDatabase
    {
        public static readonly List<CardData> All = new List<CardData>
        {
            new CardData { Id = "filo_de_hueso", NameEs = "Filo de Hueso", NameEn = "Bone Blade",
                Cost = 1, Type = CardType.Attack, Suit = Suit.RedHeart,
                Effect = EffectType.DealDamage, Amount = 6 },

            new CardData { Id = "golpe_doble", NameEs = "Golpe Doble", NameEn = "Double Strike",
                Cost = 1, Type = CardType.Attack, Suit = Suit.RedHeart,
                Effect = EffectType.DealDamageMultiHit, Amount = 4, Amount2 = 2 },

            new CardData { Id = "llama_de_copal", NameEs = "Llama de Copal", NameEn = "Copal Flame",
                Cost = 1, Type = CardType.Attack, Suit = Suit.Candle,
                Effect = EffectType.DealDamageApplyBurn, Amount = 5, Amount2 = 2 },

            new CardData { Id = "bomba_de_cempasuchil", NameEs = "Bomba de Cempasúchil", NameEn = "Marigold Bomb",
                Cost = 2, Type = CardType.Attack, Suit = Suit.MarigoldBomb,
                Effect = EffectType.DealDamageAllEnemies, Amount = 10 },

            new CardData { Id = "corte_final", NameEs = "Corte Final", NameEn = "Final Cut",
                Cost = 2, Type = CardType.Attack, Suit = Suit.SilverDeath,
                Effect = EffectType.DealDamageExecute, Amount = 8, Amount2 = 16 },

            new CardData { Id = "escarcha", NameEs = "Escarcha", NameEn = "Frostbite",
                Cost = 1, Type = CardType.Attack, Suit = Suit.BlueIce,
                Effect = EffectType.DealDamageApplyFreeze, Amount = 5 },

            new CardData { Id = "escudo_de_hueso", NameEs = "Escudo de Hueso", NameEn = "Bone Shield",
                Cost = 1, Type = CardType.Skill, Suit = Suit.BlueIce,
                Effect = EffectType.GainBlock, Amount = 5 },

            new CardData { Id = "tumba_sellada", NameEs = "Tumba Sellada", NameEn = "Sealed Grave",
                Cost = 2, Type = CardType.Skill, Suit = Suit.SilverDeath,
                Effect = EffectType.GainBlockUntargetable, Amount = 10 },

            new CardData { Id = "humo_de_copal", NameEs = "Humo de Copal", NameEn = "Copal Smoke",
                Cost = 1, Type = CardType.Skill, Suit = Suit.Candle,
                Effect = EffectType.GainBlockDraw, Amount = 4, Amount2 = 1 },

            new CardData { Id = "ofrenda_de_oro", NameEs = "Ofrenda de Oro", NameEn = "Golden Offering",
                Cost = 1, Type = CardType.Skill, Suit = Suit.GreenMoney,
                Effect = EffectType.Draw, Amount = 2 },

            new CardData { Id = "bendicion_real", NameEs = "Bendición Real", NameEn = "Royal Blessing",
                Cost = 2, Type = CardType.Skill, Suit = Suit.PurpleRoyalty,
                Effect = EffectType.GainBlockRemoveDebuff, Amount = 8 },

            new CardData { Id = "suerte_de_catrina", NameEs = "Suerte de Catrina", NameEn = "Catrina's Luck",
                Cost = 1, Type = CardType.Skill, Suit = Suit.RainbowCatrina,
                Effect = EffectType.RandomEffect, Amount = 8, Amount2 = 8, Amount3 = 2 }, // dmg / block / draw

            new CardData { Id = "corona_de_espinas", NameEs = "Corona de Espinas", NameEn = "Crown of Thorns",
                Cost = 2, Type = CardType.Power, Suit = Suit.PurpleRoyalty,
                Effect = EffectType.PowerReflect, Amount = 3 },

            new CardData { Id = "vela_eterna", NameEs = "Vela Eterna", NameEn = "Eternal Candle",
                Cost = 1, Type = CardType.Power, Suit = Suit.Candle,
                Effect = EffectType.PowerBurnEnemyEachTurn, Amount = 1 },

            new CardData { Id = "corazon_de_rubi", NameEs = "Corazón de Rubí", NameEn = "Ruby Heart",
                Cost = 2, Type = CardType.Power, Suit = Suit.RedHeart,
                Effect = EffectType.PowerEnergyPerTurn, Amount = 1 },

            new CardData { Id = "diamante_de_hielo", NameEs = "Diamante de Hielo", NameEn = "Ice Diamond",
                Cost = 2, Type = CardType.Power, Suit = Suit.BlueIce,
                Effect = EffectType.PowerBlockPerTurn, Amount = 3 },

            new CardData { Id = "reliquia_dorada", NameEs = "Reliquia Dorada", NameEn = "Golden Relic",
                Cost = 2, Type = CardType.Power, Suit = Suit.GoldGem,
                Effect = EffectType.PowerDamageOnPowerPlayed, Amount = 5 },

            new CardData { Id = "fuego_fatal", NameEs = "Fuego Fatal", NameEn = "Fatal Fire",
                Cost = 1, Type = CardType.Power, Suit = Suit.SilverDeath,
                Effect = EffectType.PowerDoubleBurnEndOfTurn },
        };

        public static CardData ById(string id)
        {
            foreach (var c in All) if (c.Id == id) return c;
            return null;
        }
    }
}

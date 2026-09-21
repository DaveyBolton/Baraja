namespace Baraja.Combat
{
    public enum CardType { Attack, Skill, Power }

    public enum Suit
    {
        RedHeart, BlueIce, GreenMoney, PurpleRoyalty, GoldGem, SilverDeath,
        RainbowCatrina, Candle, MarigoldBomb
    }

    // One entry per distinct effect the 18 cards need. Kept as a flat enum
    // switched on in CombatManager rather than a generic scripting system -
    // 18 known effects doesn't earn an abstraction, per house rules.
    public enum EffectType
    {
        DealDamage,
        DealDamageMultiHit,     // Golpe Doble: Amount per hit, Amount2 = hit count
        DealDamageAllEnemies,   // Bomba de Cempasúchil
        DealDamageApplyBurn,
        DealDamageApplyFreeze,
        DealDamageExecute,      // bonus damage if target below an HP%
        GainBlock,
        GainBlockUntargetable,  // Sealed Grave
        GainBlockDraw,          // Copal Smoke
        Draw,
        GainBlockRemoveDebuff,  // Royal Blessing
        RandomEffect,           // Catrina's Luck
        PowerReflect,            // Crown of Thorns
        PowerBurnEnemyEachTurn, // Eternal Candle
        PowerEnergyPerTurn,     // Ruby Heart
        PowerBlockPerTurn,      // Ice Diamond
        PowerDamageOnPowerPlayed, // Golden Relic
        PowerDoubleBurnEndOfTurn // Fatal Fire
    }

    // Enemy-inflicted status on the player. Freeze isn't specified in
    // COMBAT.md beyond "Escarcha applies Freeze" - defined here as a 50%
    // reduction to the target's next dealt-damage instance, single use.
    // This is a judgment call, not a rule pulled from the design docs.
    public enum StatusType { Weaken, Freeze, Burn, Untargetable }
}

namespace Baraja.Combat
{
    // Shared HP/Block/Status state for anything that can be hit in combat.
    public abstract class Combatant
    {
        public string DisplayName;
        public int MaxHp;
        public int Hp;
        public int Block;
        public readonly StatusEffects Status = new StatusEffects();

        public bool IsDead => Hp <= 0;

        protected Combatant(string displayName, int maxHp)
        {
            DisplayName = displayName;
            MaxHp = maxHp;
            Hp = maxHp;
        }

        public void GainBlock(int amount) => Block += amount;

        public void ClearBlockAtTurnEnd() => Block = 0;

        // Outgoing damage from this combatant, after Weaken and a Freeze charge.
        public int ModifyOutgoingDamage(int rawDamage)
        {
            int dmg = rawDamage - Status.ConsumeWeakenReduction();
            if (dmg < 0) dmg = 0;
            dmg = Status.ApplyFreezeToOutgoingDamage(dmg);
            return dmg;
        }

        // Applies incoming damage to Block first, then Hp. Returns Hp actually lost.
        public int TakeDamage(int amount)
        {
            if (amount <= 0) return 0;
            int absorbed = System.Math.Min(Block, amount);
            Block -= absorbed;
            int toHp = amount - absorbed;
            int before = Hp;
            Hp = System.Math.Max(0, Hp - toHp);
            return before - Hp;
        }

        public void Heal(int amount)
        {
            Hp = System.Math.Min(MaxHp, Hp + amount);
        }
    }

    public class PlayerState : Combatant
    {
        public int Energy;
        public int MaxEnergy;
        public int EnergyPerTurnBonus;   // Ruby Heart power
        public int BlockPerTurnBonus;    // Ice Diamond power
        public int ReflectAmount;        // Crown of Thorns: flat dmg reflected when hit by an attack
        public int BurnEnemyEachTurn;    // Eternal Candle
        public int PowerPlayedDamage;    // Golden Relic: dmg to enemy whenever a Power is played
        public bool DoubleEnemyBurnAtTurnEnd; // Fatal Fire

        public PlayerState(int maxHp, int maxEnergy) : base("Jugador", maxHp)
        {
            MaxEnergy = maxEnergy;
        }
    }

    public class EnemyCombatant : Combatant
    {
        public EnemyData Data;
        public int PatternIndex;
        public int BonusDamage; // accrued from BuffSelfDamage steps (Catrina Menor, La Catrina)

        public EnemyCombatant(EnemyData data) : base(data.NameEs, data.MaxHp)
        {
            Data = data;
        }

        public IntentStep CurrentIntent => Data.Pattern[PatternIndex % Data.Pattern.Count];

        public void AdvancePattern() => PatternIndex = (PatternIndex + 1) % Data.Pattern.Count;
    }
}

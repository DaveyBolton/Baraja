using System.Collections.Generic;

namespace Baraja.Combat
{
    // Stack count per status a combatant is carrying. Burn/Weaken accumulate;
    // Freeze/Untargetable are single-use flags (stack count 0 or 1) per their
    // definitions in CombatEnums.cs.
    public class StatusEffects
    {
        private readonly Dictionary<StatusType, int> _stacks = new Dictionary<StatusType, int>();

        public int Get(StatusType type) => _stacks.TryGetValue(type, out var v) ? v : 0;

        public bool Has(StatusType type) => Get(type) > 0;

        public void Add(StatusType type, int amount)
        {
            _stacks[type] = Get(type) + amount;
        }

        public void SetFlag(StatusType type) => _stacks[type] = 1;

        public void Clear(StatusType type) => _stacks.Remove(type);

        public void ClearAllDebuffs()
        {
            Clear(StatusType.Weaken);
            Clear(StatusType.Freeze);
        }

        // Burn: deals its stack count as damage, then halves (rounded down),
        // called once at end of the burning combatant's turn.
        public int TickBurn()
        {
            int dmg = Get(StatusType.Burn);
            if (dmg <= 0) return 0;
            _stacks[StatusType.Burn] = dmg / 2;
            return dmg;
        }

        // Consumes one Freeze charge if present, halving the given damage.
        public int ApplyFreezeToOutgoingDamage(int damage)
        {
            if (!Has(StatusType.Freeze)) return damage;
            Clear(StatusType.Freeze);
            return damage / 2;
        }

        public int WeakenDamageReduction => Get(StatusType.Weaken);
    }
}

using UnityEngine;

namespace Baraja.Combat
{
    // Starts a fight as soon as the combat scene plays. Stands in for the
    // still-unbuilt run/map screen, which will eventually pass the fight
    // number in instead of this hard-coded default.
    public class CombatBootstrap : MonoBehaviour
    {
        public CombatUI Ui;
        public int FightNumber = 1;

        private void Start()
        {
            Ui.BeginFight(FightNumber);
        }
    }
}

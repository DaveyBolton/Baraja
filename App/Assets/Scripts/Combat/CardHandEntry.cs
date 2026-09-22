using UnityEngine;
using UnityEngine.UI;

namespace Baraja.Combat
{
    // Every card in hand is independently tappable now (a plain scrolling
    // row - see BarajaCombatSceneBuilder.MakeHandScrollContainer - replaced
    // the old single-"focused"-card coverflow), so affordability is the
    // only thing that still needs to change a card's look: CombatUI sets
    // CanPlay once per hand rebuild (from CombatManager.CanPlay), and this
    // drives the card's own Button.interactable and tint directly - no
    // per-frame carousel pass involved in any of it anymore.
    public class CardHandEntry : MonoBehaviour
    {
        private static readonly Color UnaffordableTint = new Color(0.55f, 0.55f, 0.55f, 1f);

        private bool _canPlay;
        public bool CanPlay
        {
            get => _canPlay;
            set
            {
                _canPlay = value;
                var button = GetComponent<Button>();
                if (button != null) button.interactable = value;
                var graphic = GetComponent<Graphic>();
                // Opaque grey, not see-through - "cards are objects" applies
                // to the unaffordable state too, not just the overlap fix.
                if (graphic != null) graphic.color = value ? Color.white : UnaffordableTint;
            }
        }
    }
}

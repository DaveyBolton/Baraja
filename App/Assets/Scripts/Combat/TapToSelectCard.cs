using UnityEngine;
using UnityEngine.EventSystems;

namespace Baraja.Combat
{
    // Replaces the old DoubleTapToPlay (which just read Unity's native
    // clickCount>=2 timing/distance threshold). Cards now need two taps to
    // play, but as an explicit two-step select-then-confirm instead of a
    // fast-double-click gesture: a first tap arms a card as the pending
    // play (CombatUI scales it up), and a SECOND tap on that same armed
    // card confirms it - with no timing window, so a slow deliberate
    // second tap works exactly the same as a fast one. Tapping a
    // different card instead swaps the pending selection over to that one
    // rather than playing anything - Dave: pulling back a card you armed
    // by mistake and playing a different one instead should just work,
    // no penalty. This component itself doesn't know or care which state
    // it's in - clickCount is ignored entirely, every tap is just "a tap
    // on this card," and CombatUI.OnCardTapped owns the actual state
    // machine (it has to: swapping the pending card means one card's tap
    // handler needs to affect another card's visual state).
    public class TapToSelectCard : MonoBehaviour, IPointerClickHandler
    {
        public System.Action OnTap;

        public void OnPointerClick(PointerEventData eventData) => OnTap?.Invoke();
    }
}

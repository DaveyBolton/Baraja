using UnityEngine;
using UnityEngine.EventSystems;

namespace Baraja.Combat
{
    // Cards play on a double-tap, not a single tap - in a carousel where
    // swiping and playing happen in the same area, a single tap is too
    // easy to trigger by accident while just browsing the hand. Unity's
    // EventSystem already tracks clickCount for a real double-click/tap
    // (within the platform's own timing/distance threshold), so this just
    // reads it rather than hand-rolling timing logic.
    public class DoubleTapToPlay : MonoBehaviour, IPointerClickHandler
    {
        public System.Action OnDoubleTap;

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.clickCount >= 2) OnDoubleTap?.Invoke();
        }
    }
}

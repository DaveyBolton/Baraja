using UnityEngine;
using UnityEngine.UI;

namespace Baraja.Combat
{
    // Shows a left/right chevron over the hand row whenever there are more
    // cards scrolled off that edge - Dave: "put some arrows over on the
    // edges of the playing surface if there are usable cards off screen."
    // Polls in Update rather than only reacting to ScrollRect.onValueChanged
    // because the hand's own content width changes as cards are drawn/played
    // (a hand that used to fit the screen can start overflowing it, and vice
    // versa), not just when the player drags.
    public class HandScrollArrows : MonoBehaviour
    {
        public ScrollRect ScrollRect;
        public GameObject LeftArrow;
        public GameObject RightArrow;

        const float Epsilon = 0.02f;

        void Update()
        {
            if (ScrollRect == null || ScrollRect.content == null || ScrollRect.viewport == null) return;

            bool scrollable = ScrollRect.content.rect.width > ScrollRect.viewport.rect.width + 1f;
            bool showLeft = false, showRight = false;
            if (scrollable)
            {
                float pos = ScrollRect.horizontalNormalizedPosition;
                showLeft = pos > Epsilon;
                showRight = pos < 1f - Epsilon;
            }

            if (LeftArrow.activeSelf != showLeft) LeftArrow.SetActive(showLeft);
            if (RightArrow.activeSelf != showRight) RightArrow.SetActive(showRight);
        }
    }
}

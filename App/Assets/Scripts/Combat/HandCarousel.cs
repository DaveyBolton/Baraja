using UnityEngine;
using UnityEngine.UI;

namespace Baraja.Combat
{
    // Coverflow-style hand: whichever card is nearest the viewport's
    // horizontal center scales up as the focused card, and renders in
    // front of its neighbors; farther cards shrink toward the edges.
    // Driven purely by the ScrollRect's own scroll position (drag,
    // momentum, elastic bounce all just work), not a custom gesture.
    public class HandCarousel : MonoBehaviour
    {
        public ScrollRect ScrollRect;
        public RectTransform Viewport;

        private const float MaxScale = 1.15f;
        private const float MinScale = 0.85f;
        private const float FalloffDistance = 260f; // roughly one card-width + gap

        private void LateUpdate() => Refresh();

        public void Refresh()
        {
            if (ScrollRect == null || ScrollRect.content == null) return;
            float viewportCenterX = Viewport.rect.center.x;

            var content = ScrollRect.content;
            int count = content.childCount;
            float bestDistance = float.MaxValue;
            int bestIndex = -1;

            for (int i = 0; i < count; i++)
            {
                var card = (RectTransform)content.GetChild(i);
                Vector3 worldCenter = card.TransformPoint(card.rect.center);
                float localX = Viewport.InverseTransformPoint(worldCenter).x;
                float distance = Mathf.Abs(localX - viewportCenterX);
                float t = Mathf.Clamp01(distance / FalloffDistance);
                card.localScale = Vector3.one * Mathf.Lerp(MaxScale, MinScale, t);

                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    bestIndex = i;
                }
            }

            // Focused card renders in front of its overlapping neighbors -
            // sibling order otherwise stays untouched so this doesn't fight
            // the hand's own left-to-right ordering.
            if (bestIndex >= 0) content.GetChild(bestIndex).SetAsLastSibling();
        }
    }
}

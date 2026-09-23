using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Baraja.Combat
{
    // Visible "just played" pile - direct answer to "you just click cards
    // and they disappear": instead of a played card vanishing into
    // nothing, a thumbnail of it lands here so the last move stays visible.
    // Was a cascade of up to 3 thumbnails peeking out from behind each
    // other - Dave's call after seeing it live: overlapping cards, even
    // cards deliberately stacked like this, don't read as separate design
    // objects, they read as one smeared mess. Only ever showing the single
    // latest play removes the question entirely: there is never more than
    // one card here to overlap with. Cleared at the start of every new fight.
    public class PlayedCardStack : MonoBehaviour
    {
        private const int MaxVisible = 1;
        // Sized to nearly fill the pile's own 185x230 slot
        // (BarajaCombatSceneBuilder's pileWidth/pileHeight) at the card's
        // native 736:1040 aspect. Shrunk from 205x290 when growing the hand
        // and enemy cards (Dave: "make the player cards and enemy card
        // larger") left only 250px between the hand's top edge and the
        // enemy row's bottom edge - not enough room for a 290-tall
        // thumbnail plus real margin on both sides; grown back up slightly
        // per Dave once the pile margin was tightened to free up room.
        private static readonly Vector2 ThumbSize = new Vector2(163, 230);

        private readonly List<RectTransform> _entries = new List<RectTransform>();

        public void Push(Texture2D texture)
        {
            var go = new GameObject("PlayedCard", typeof(RectTransform));
            go.transform.SetParent(transform, false);
            var rect = (RectTransform)go.transform;
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.sizeDelta = ThumbSize;

            var image = go.AddComponent<RawImage>();
            image.texture = texture;

            var outline = go.AddComponent<Outline>();
            outline.effectColor = new Color(0f, 0f, 0f, 0.6f);
            outline.effectDistance = new Vector2(1.5f, -1.5f);

            _entries.Add(rect);
            rect.SetAsLastSibling();

            while (_entries.Count > MaxVisible)
            {
                if (_entries[0] != null) Destroy(_entries[0].gameObject);
                _entries.RemoveAt(0);
            }

            Relayout();
            StartCoroutine(PunchIn(rect));
        }

        public void Clear()
        {
            foreach (var e in _entries) if (e != null) Destroy(e.gameObject);
            _entries.Clear();
        }

        // End-of-round "clear the table": every card currently in the pile
        // converges on screen center, spinning and shrinking to nothing.
        // Reparents each card into convergeLayer (a full-canvas-stretch
        // layer, e.g. CombatUI's FloatingTextLayer) so "the middle" means
        // screen center regardless of which side this particular pile
        // lives on (the player's pile is on the right, the enemy's on the
        // left - both sweep to the same point).
        public void SweepAway(Transform convergeLayer)
        {
            var toSweep = new List<RectTransform>(_entries);
            _entries.Clear();
            foreach (var e in toSweep)
            {
                if (e != null) StartCoroutine(SweepEntry(e, convergeLayer));
            }
        }

        private static IEnumerator SweepEntry(RectTransform rect, Transform convergeLayer)
        {
            const float duration = 0.6f;
            const float spinDegrees = 720f;

            Vector3 worldPos = rect.position;
            rect.SetParent(convergeLayer, true);
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.position = worldPos;

            Vector2 start = rect.anchoredPosition;
            Vector3 startScale = rect.localScale;
            float t = 0f;
            while (t < duration)
            {
                t += Time.unscaledDeltaTime;
                if (rect == null) yield break;
                float f = Mathf.Clamp01(t / duration);
                float eased = f * f; // ease-in - accelerates toward the vanish
                rect.anchoredPosition = Vector2.Lerp(start, Vector2.zero, eased);
                rect.localRotation = Quaternion.Euler(0f, 0f, Mathf.Lerp(0f, spinDegrees, f));
                rect.localScale = Vector3.Lerp(startScale, Vector3.zero, eased);
                yield return null;
            }
            if (rect != null) Destroy(rect.gameObject);
        }

        // MaxVisible=1 means this only ever has one entry to place - dead
        // centered in the slot, no cascade math needed anymore.
        private void Relayout()
        {
            for (int i = 0; i < _entries.Count; i++)
            {
                _entries[i].anchoredPosition = Vector2.zero;
            }
        }

        // A card that's still mid-punch-in can get bumped off the pile
        // (MaxVisible cap) and destroyed by a later Push() before this
        // finishes - cards can be played faster than the 0.18s animation,
        // e.g. several double-taps within the same handful of frames - so
        // every iteration has to confirm rect is still alive first.
        private static IEnumerator PunchIn(RectTransform rect)
        {
            const float duration = 0.18f;
            float t = 0f;
            rect.localScale = Vector3.one * 1.3f;
            while (t < duration)
            {
                t += Time.unscaledDeltaTime;
                if (rect == null) yield break;
                rect.localScale = Vector3.Lerp(Vector3.one * 1.3f, Vector3.one, t / duration);
                yield return null;
            }
            if (rect != null) rect.localScale = Vector3.one;
        }
    }
}

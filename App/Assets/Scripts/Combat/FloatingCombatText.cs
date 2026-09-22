using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Baraja.Combat
{
    // A single "-12" / "+8" popup that punches in, rises, and fades, then
    // destroys itself. This (plus PlayedCardStack) is the direct fix for
    // "you just click cards and they disappear, opponents cards do
    // nothing" - CombatManager already changed HP/block correctly under
    // the hood, nothing ever showed it happening on screen.
    public class FloatingCombatText : MonoBehaviour
    {
        private const float LifeTime = 0.9f;
        private const float RiseDistance = 90f;

        public static void Spawn(Transform layer, Vector3 worldPos, string text, Color color)
        {
            if (layer == null) return;

            var go = new GameObject("FloatingCombatText", typeof(RectTransform));
            go.transform.SetParent(layer, false);
            var rect = (RectTransform)go.transform;
            rect.sizeDelta = new Vector2(420, 140);
            rect.position = worldPos;
            // Multiple hits landing on the same target in quick succession
            // (a multi-hit card, or several enemies' attacks resolving back
            // to back) used to spawn every popup at the exact same worldPos -
            // with identical rise/fade timing they stayed perfectly stacked
            // their whole lifetime, reading as one blurred number instead of
            // two separate events. A small random scatter per spawn is the
            // standard fix (WoW-style combat text jitter) - each popup is
            // still readable on its own, they just don't perfectly coincide.
            rect.anchoredPosition += new Vector2(Random.Range(-50f, 50f), Random.Range(-20f, 20f));

            var label = go.AddComponent<Text>();
            label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            label.fontSize = 76;
            label.fontStyle = FontStyle.Bold;
            label.alignment = TextAnchor.MiddleCenter;
            label.color = color;
            label.text = text;

            var outline = go.AddComponent<Outline>();
            outline.effectColor = new Color(0f, 0f, 0f, 0.85f);
            outline.effectDistance = new Vector2(3f, -3f);

            go.AddComponent<FloatingCombatText>().StartCoroutine(Animate(rect, label));
        }

        private static IEnumerator Animate(RectTransform rect, Text label)
        {
            Vector2 start = rect.anchoredPosition;
            Color baseColor = label.color;
            float t = 0f;

            while (t < LifeTime)
            {
                t += Time.unscaledDeltaTime;
                float f = Mathf.Clamp01(t / LifeTime);

                rect.anchoredPosition = start + Vector2.up * (RiseDistance * f);
                rect.localScale = Vector3.one * Mathf.Lerp(0.6f, 1f, Mathf.Clamp01(f / 0.2f));
                label.color = new Color(baseColor.r, baseColor.g, baseColor.b,
                    Mathf.Lerp(1f, 0f, Mathf.Clamp01((f - 0.45f) / 0.55f)));

                yield return null;
            }

            Destroy(rect.gameObject);
        }
    }
}

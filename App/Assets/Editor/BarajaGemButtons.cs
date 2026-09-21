using UnityEditor;
using UnityEngine;

/// <summary>
/// Rectangular emerald-cut gem button textures (Assets/Art/Buttons/), one
/// color per semantic role, reusing the same palette the card suits already
/// use so button color carries meaning players have already learned. Labels
/// are a native Unity Text child laid over the gem at scene-build time, not
/// pre-baked into the image - all graphics/layout work stays in Unity.
/// </summary>
public enum GemColor { Ruby, Gold, Blue, Purple, Green, Silver, Rainbow }

public static class BarajaGemButtons
{
    // The gems' own natural content aspect (measured from the generated art,
    // not the 4:1 canvas originally requested - forcing that would have
    // stretched every facet). Every button should be sized from this so the
    // gem never looks squashed or stretched.
    public const float Aspect = 700f / 315f;

    private static readonly System.Collections.Generic.Dictionary<GemColor, Texture2D> Cache = new();

    public static Texture2D Get(GemColor color)
    {
        if (Cache.TryGetValue(color, out var cached)) return cached;
        string path = $"Assets/Art/Buttons/gem_button_{color.ToString().ToLowerInvariant()}.png";
        var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        if (tex == null) Debug.LogError($"BarajaGemButtons: could not load {path}");
        Cache[color] = tex;
        return tex;
    }
}

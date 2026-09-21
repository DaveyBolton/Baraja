using UnityEditor;
using UnityEngine;

/// <summary>
/// Shared font loading for both scene builders. Cinzel Decorative (ornate,
/// engraved-looking caps) for titles/buttons/headers - it echoes the gold and
/// silver filigree on the card frames instead of clashing with it the way
/// Unity's default LegacyRuntime.ttf did. Crimson Text (warm, readable serif)
/// for body copy - the combat log, tutorial paragraphs, store descriptions -
/// where an all-caps decorative face would be unreadable at small sizes.
/// Both are OFL-licensed (Assets/Fonts/OFL-*.txt), free for commercial use.
/// </summary>
public static class BarajaFonts
{
    private static Font _title;
    private static Font _titleBlack;
    private static Font _body;
    private static Font _bodySemiBold;

    public static Font Title => _title ??= Load("CinzelDecorative-Bold");
    public static Font TitleBlack => _titleBlack ??= Load("CinzelDecorative-Black");
    public static Font Body => _body ??= Load("CrimsonText-Regular");
    public static Font BodySemiBold => _bodySemiBold ??= Load("CrimsonText-SemiBold");

    private static Font Load(string name)
    {
        string path = $"Assets/Fonts/{name}.ttf";
        var font = AssetDatabase.LoadAssetAtPath<Font>(path);
        if (font == null) Debug.LogError($"BarajaFonts: could not load {path}");
        return font;
    }
}

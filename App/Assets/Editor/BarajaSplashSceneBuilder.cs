using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Baraja.Core;

/// <summary>
/// One-shot scene assembler for the splash screen, run via:
/// Unity -batchmode -executeMethod BarajaSplashSceneBuilder.Build -quit
/// No manual editor authoring - rebuild any time the layout changes.
/// Must be scene index 0 in the player build (see BarajaFullGameBuilder)
/// so it's what the game actually boots into.
/// </summary>
public static class BarajaSplashSceneBuilder
{
    public static void Build()
    {
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        GameObject camGO = new GameObject("Main Camera");
        camGO.tag = "MainCamera";
        Camera cam = camGO.AddComponent<Camera>();
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = Color.black;
        cam.orthographic = true;
        camGO.AddComponent<AudioListener>();

        GameObject canvasGO = new GameObject("Splash Canvas");
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        scaler.matchWidthOrHeight = 1f; // match height - see BarajaCombatSceneBuilder for why

        // --- Solid black backdrop: first child so it's always behind the
        // title/portrait, and a belt-and-suspenders guarantee of black
        // even if the camera's own clear color is ever bypassed. ---
        GameObject backdropGO = new GameObject("Backdrop");
        backdropGO.transform.SetParent(canvasGO.transform, false);
        Image backdrop = backdropGO.AddComponent<Image>();
        backdrop.color = Color.black;
        RectTransform backdropRT = backdrop.rectTransform;
        backdropRT.anchorMin = Vector2.zero;
        backdropRT.anchorMax = Vector2.one;
        backdropRT.offsetMin = Vector2.zero;
        backdropRT.offsetMax = Vector2.zero;

        // --- Title, upper band - moved up from -220 to make room for the
        // portrait sitting dead center on screen below, per Dave. ---
        Text title = MakeTitleText(canvasGO.transform, "TitleText", new Vector2(0, -160), TextAnchor.MiddleCenter, 84);
        title.rectTransform.anchorMin = new Vector2(0.5f, 1f);
        title.rectTransform.anchorMax = new Vector2(0.5f, 1f);
        title.rectTransform.pivot = new Vector2(0.5f, 1f);
        title.rectTransform.sizeDelta = new Vector2(1000, 260);
        title.text = "Baraja de los Muertos";

        // --- Character portrait, centered below the title. Texture is
        // picked at RUNTIME by SplashScreen.cs, not here - a build-time
        // random pick would just bake in the same character forever. Art
        // is the raw 1024x1024 bust (Art/Enemies), not the composited
        // card (Art/EnemyCards) - square, no frame/name/cost baked in. ---
        GameObject charGO = new GameObject("CharacterImage");
        charGO.transform.SetParent(canvasGO.transform, false);
        RawImage charImage = charGO.AddComponent<RawImage>();
        RectTransform charRT = charImage.rectTransform;
        charRT.anchorMin = new Vector2(0.5f, 0.5f);
        charRT.anchorMax = new Vector2(0.5f, 0.5f);
        charRT.pivot = new Vector2(0.5f, 0.5f);
        // Was 760x760, centered near the middle - left real unused space
        // above the bottom edge and below the title untouched. Dave's call:
        // use all of it, since bust detail is exactly what gets lost first
        // on a small screen. Dead center of the canvas (0,0 on a center
        // anchor) rather than nudged down - title moved up above instead
        // of the portrait being offset to clear it.
        charRT.anchoredPosition = new Vector2(0, 0);
        charRT.sizeDelta = new Vector2(1000, 1000);

        GameObject controllerGO = new GameObject("SplashScreen");
        SplashScreen splash = controllerGO.AddComponent<SplashScreen>();
        splash.CharacterImage = charImage;
        splash.Duration = 5f;

        string scenePath = "Assets/Scenes/Splash.unity";
        System.IO.Directory.CreateDirectory("Assets/Scenes");
        EditorSceneManager.SaveScene(scene, scenePath);
        Debug.Log("Baraja splash scene built successfully at " + scenePath);
    }

    static Text MakeTitleText(Transform parent, string name, Vector2 pos, TextAnchor anchor, int size)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        Text t = go.AddComponent<Text>();
        t.font = BarajaFonts.Title;
        t.fontSize = size;
        t.alignment = anchor;
        t.color = Color.white;
        t.verticalOverflow = VerticalWrapMode.Overflow;
        t.rectTransform.sizeDelta = new Vector2(1000, 260);
        t.rectTransform.anchoredPosition = pos;

        Outline outline = go.AddComponent<Outline>();
        outline.effectColor = new Color(0f, 0f, 0f, 0.9f);
        outline.effectDistance = new Vector2(2f, -2f);
        return t;
    }
}

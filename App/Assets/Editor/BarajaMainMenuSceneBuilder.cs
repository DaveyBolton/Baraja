using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Baraja.Menu;

/// <summary>
/// One-shot scene assembler for the main menu, run via:
/// Unity -batchmode -executeMethod BarajaMainMenuSceneBuilder.Build -quit
/// Title screen with Play / How to Play / Options / Store, each opening as an
/// overlay panel on the same canvas rather than a separate scene, so nothing
/// needs a scene transition just to check the volume sliders.
/// </summary>
public static class BarajaMainMenuSceneBuilder
{
    // One button size for the whole game, wide enough to fit the longest
    // label anywhere ("How to Play") at a legible size - measured with
    // PIL against CinzelDecorative-Bold at 36pt: 269px text width, so 460px
    // leaves generous padding inside the gem on every side.
    const float StandardButtonWidth = 460f;
    const int StandardButtonFontSize = 40;

    public static void Build()
    {
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        GameObject esGO = new GameObject("EventSystem");
        esGO.AddComponent<UnityEngine.EventSystems.EventSystem>();
        esGO.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();

        GameObject camGO = new GameObject("Main Camera");
        camGO.tag = "MainCamera";
        Camera cam = camGO.AddComponent<Camera>();
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.05f, 0.04f, 0.08f);
        cam.orthographic = true;
        camGO.AddComponent<AudioListener>();

        GameObject canvasGO = new GameObject("Menu Canvas");
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        canvasGO.AddComponent<GraphicRaycaster>();

        // --- Title panel: always visible underneath whichever overlay is open ---
        Text title = MakeTitleText(canvasGO.transform, "TitleText", new Vector2(0, -260), TextAnchor.MiddleCenter, 72);
        AnchorTop(title.rectTransform);
        title.rectTransform.sizeDelta = new Vector2(1020, 160);
        title.text = "Baraja de los Muertos";

        // Vertical spacing between stacked gem buttons has to be derived from
        // the gem's own height, not a leftover offset sized for the old flat
        // rectangle buttons - those were ~90-115px tall, these are 207px, so
        // the old spacing had every button overlapping the next one below it.
        float navButtonHeight = StandardButtonWidth / BarajaGemButtons.Aspect;
        float navSpacing = navButtonHeight + 24f;

        // Three nav buttons now - How to Play moved into the combat scene's
        // tutorial banner (CombatTutorialHint), read in context next to the
        // board it describes instead of a menu-only parchment overlay.
        Button playBtn = MakeButton(canvasGO.transform, "PlayButton", StandardButtonWidth, "Play", StandardButtonFontSize, GemColor.Gold);
        CenterAnchor(playBtn.GetComponent<RectTransform>());
        playBtn.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, navSpacing);

        Button optionsBtn = MakeButton(canvasGO.transform, "OptionsButton", StandardButtonWidth, "Options", StandardButtonFontSize, GemColor.Purple);
        CenterAnchor(optionsBtn.GetComponent<RectTransform>());
        optionsBtn.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 0);

        Button storeBtn = MakeButton(canvasGO.transform, "StoreButton", StandardButtonWidth, "Store", StandardButtonFontSize, GemColor.Green);
        CenterAnchor(storeBtn.GetComponent<RectTransform>());
        storeBtn.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -navSpacing);

        // --- Options overlay ---
        GameObject optionsPanel = MakeOverlayPanel(canvasGO.transform, "OptionsPanel");
        Text optionsTitle = MakeTitleText(optionsPanel.transform, "OptionsTitle", new Vector2(0, -80), TextAnchor.MiddleCenter, 54);
        AnchorTop(optionsTitle.rectTransform);
        optionsTitle.text = "Options";

        // Sliders are 720 wide, centered (anchor 0.5,1, so they span screen x
        // 180 to 900). Labels anchor to the screen's top-left corner directly
        // (not AnchorTop, which centers the anchor point) and sit at x=180 so
        // their left edge lines up with the slider's left edge below it -
        // AnchorTop + MiddleLeft alignment previously put the label's whole
        // 800-wide box centered on screen, so left-aligned text rendered
        // starting near x=-220 and never appeared on screen at all.
        Text fxLabel = MakeText(optionsPanel.transform, "FxLabel", new Vector2(180, -260), TextAnchor.MiddleLeft, 42);
        AnchorTopLeft(fxLabel.rectTransform);
        fxLabel.rectTransform.sizeDelta = new Vector2(400, 50);
        fxLabel.text = "FX Volume";
        Slider fxSlider = MakeSlider(optionsPanel.transform, "FxSlider", new Vector2(0, -330));

        Text musicLabel = MakeText(optionsPanel.transform, "MusicLabel", new Vector2(180, -420), TextAnchor.MiddleLeft, 42);
        AnchorTopLeft(musicLabel.rectTransform);
        musicLabel.rectTransform.sizeDelta = new Vector2(400, 50);
        musicLabel.text = "Music Volume";
        Slider musicSlider = MakeSlider(optionsPanel.transform, "MusicSlider", new Vector2(0, -490));

        // Buttons below here are top-anchored (matching the sliders/labels
        // above) rather than center-anchored, so their vertical stacking can
        // be computed the same way instead of mixing two coordinate systems.
        float panelButtonHeight = StandardButtonWidth / BarajaGemButtons.Aspect;

        Button langBtn = MakeButton(optionsPanel.transform, "LanguageButton", StandardButtonWidth, "Español", StandardButtonFontSize, GemColor.Rainbow);
        AnchorTop(langBtn.GetComponent<RectTransform>());
        langBtn.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -590);
        Text langLabel = langBtn.GetComponentInChildren<Text>();

        Button optionsCloseBtn = MakeButton(optionsPanel.transform, "OptionsCloseButton", StandardButtonWidth, "Close", StandardButtonFontSize, GemColor.Silver);
        AnchorTop(optionsCloseBtn.GetComponent<RectTransform>());
        optionsCloseBtn.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -590 - panelButtonHeight - 30f);

        // --- Store overlay ---
        GameObject storePanel = MakeOverlayPanel(canvasGO.transform, "StorePanel");
        Text storeTitle = MakeTitleText(storePanel.transform, "StoreTitle", new Vector2(0, -70), TextAnchor.MiddleCenter, 54);
        AnchorTop(storeTitle.rectTransform);
        storeTitle.text = "Store";

        Text storeBalance = MakeText(storePanel.transform, "StoreBalance", new Vector2(0, -150), TextAnchor.MiddleCenter, 32);
        AnchorTop(storeBalance.rectTransform);
        storeBalance.color = new Color(1f, 0.85f, 0.4f);

        Text storeMessage = MakeText(storePanel.transform, "StoreMessage", new Vector2(0, -200), TextAnchor.MiddleCenter, 28);
        AnchorTop(storeMessage.rectTransform);
        storeMessage.color = new Color(0.6f, 1f, 0.6f);

        GameObject storeScrollContent = MakeScrollList(storePanel.transform, "StoreScroll",
            new Vector2(0, -260), new Vector2(1000, 1300));

        Button storeCloseBtn = MakeButton(storePanel.transform, "StoreCloseButton", StandardButtonWidth, "Close", StandardButtonFontSize, GemColor.Silver);
        storeCloseBtn.GetComponent<RectTransform>().anchorMin = new Vector2(0.5f, 0f);
        storeCloseBtn.GetComponent<RectTransform>().anchorMax = new Vector2(0.5f, 0f);
        storeCloseBtn.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 0f);
        storeCloseBtn.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 40);

        GameObject storeRowPrefab = MakeStoreRowPrefab(canvasGO.transform);

        optionsPanel.SetActive(false);
        storePanel.SetActive(false);

        // --- Wiring ---
        MainMenuUI ui = canvasGO.AddComponent<MainMenuUI>();
        ui.OptionsPanel = optionsPanel;
        ui.StorePanel = storePanel;

        ui.PlayButton = playBtn;
        ui.OptionsButton = optionsBtn;
        ui.StoreButton = storeBtn;

        ui.OptionsCloseButton = optionsCloseBtn;
        ui.StoreCloseButton = storeCloseBtn;

        ui.FxSlider = fxSlider;
        ui.MusicSlider = musicSlider;
        ui.LanguageButton = langBtn;
        ui.LanguageButtonLabel = langLabel;

        ui.StoreListContainer = storeScrollContent.transform;
        ui.StoreRowPrefab = storeRowPrefab;
        ui.StoreBalanceText = storeBalance;
        ui.StoreMessageText = storeMessage;

        string scenePath = "Assets/Scenes/MainMenu.unity";
        System.IO.Directory.CreateDirectory("Assets/Scenes");
        EditorSceneManager.SaveScene(scene, scenePath);
        BarajaBuildScenes.Register(scenePath, 0);

        Debug.Log("Baraja main menu scene built successfully at " + scenePath);
    }

    // Full-screen dark backdrop that the caller populates and toggles active.
    static GameObject MakeOverlayPanel(Transform parent, string name)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        Image img = go.AddComponent<Image>();
        img.color = new Color(0.03f, 0.02f, 0.05f, 0.97f);
        RectTransform rt = img.rectTransform;
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        return go;
    }

    // A vertically scrolling list: returns the Content object callers should
    // parent rows into (a VerticalLayoutGroup + ContentSizeFitter stacks and
    // grows it; the ScrollRect/Viewport/Mask clip it to the visible window).
    static GameObject MakeScrollList(Transform parent, string name, Vector2 anchoredPos, Vector2 size)
    {
        GameObject scrollGO = new GameObject(name);
        scrollGO.transform.SetParent(parent, false);
        RectTransform scrollRT = scrollGO.AddComponent<RectTransform>();
        scrollRT.anchorMin = new Vector2(0.5f, 1f);
        scrollRT.anchorMax = new Vector2(0.5f, 1f);
        scrollRT.pivot = new Vector2(0.5f, 1f);
        scrollRT.anchoredPosition = anchoredPos;
        scrollRT.sizeDelta = size;
        ScrollRect scrollRect = scrollGO.AddComponent<ScrollRect>();
        scrollRect.horizontal = false;
        scrollRect.vertical = true;

        GameObject viewportGO = new GameObject("Viewport");
        viewportGO.transform.SetParent(scrollGO.transform, false);
        RectTransform viewportRT = viewportGO.AddComponent<RectTransform>();
        viewportRT.anchorMin = Vector2.zero;
        viewportRT.anchorMax = Vector2.one;
        viewportRT.offsetMin = Vector2.zero;
        viewportRT.offsetMax = Vector2.zero;
        viewportGO.AddComponent<Image>().color = new Color(1, 1, 1, 0.02f);
        viewportGO.AddComponent<RectMask2D>();
        scrollRect.viewport = viewportRT;

        GameObject contentGO = new GameObject("Content");
        contentGO.transform.SetParent(viewportGO.transform, false);
        RectTransform contentRT = contentGO.AddComponent<RectTransform>();
        contentRT.anchorMin = new Vector2(0f, 1f);
        contentRT.anchorMax = new Vector2(1f, 1f);
        contentRT.pivot = new Vector2(0.5f, 1f);
        contentRT.anchoredPosition = Vector2.zero;
        VerticalLayoutGroup layout = contentGO.AddComponent<VerticalLayoutGroup>();
        layout.spacing = 14f;
        // Rows keep their own authored width/anchors (see MakeStoreRowPrefab)
        // rather than being stretched by the group - childControlWidth=true
        // was overriding row width in a way that pushed text off both edges
        // of the screen instead of staying inside the scroll viewport.
        layout.childControlWidth = false;
        layout.childControlHeight = false;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = false;
        layout.childAlignment = TextAnchor.UpperCenter;
        layout.padding = new RectOffset(10, 10, 10, 10);
        ContentSizeFitter fitter = contentGO.AddComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        scrollRect.content = contentRT;

        return contentGO;
    }

    // Inactive template instantiated per-item by MainMenuUI.RefreshStoreList.
    // Backed by a carved-stone tablet texture instead of the previous
    // barely-visible flat tint. The stone's own natural aspect is ~4.2:1,
    // not the 6:1 a plain row would've used - forcing that would stretch
    // its facets, so the row is sized to the texture instead of the other
    // way around (960x229, not 960x160).
    static GameObject MakeStoreRowPrefab(Transform parent)
    {
        const float rowWidth = 960f;
        float rowHeight = rowWidth * 229f / 960f; // panel_stone.png's own measured aspect

        GameObject go = new GameObject("StoreRowPrefab");
        go.transform.SetParent(parent, false);
        RectTransform rt = go.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 1f);
        rt.anchorMax = new Vector2(0.5f, 1f);
        rt.pivot = new Vector2(0.5f, 1f);
        rt.sizeDelta = new Vector2(rowWidth, rowHeight);
        LayoutElement layoutElement = go.AddComponent<LayoutElement>();
        layoutElement.preferredHeight = rowHeight;
        layoutElement.preferredWidth = rowWidth;
        RawImage bg = go.AddComponent<RawImage>();
        bg.texture = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Art/Panels/panel_stone.png");

        // Text is dark now (was white/amber, tuned for the old near-black
        // tint) since it has to read against light grey stone instead.
        // Name+Desc are vertically centered as a block on the row's own
        // center (matching the Buy button's centering) rather than pinned
        // near the top, which left the bottom ~40% of the slab as dead
        // empty space and read as unbalanced.
        Color darkText = new Color(0.16f, 0.15f, 0.15f);

        // Name/Desc get their own column (x:50-460) strictly separate from
        // the Price column (x:470-660) and Buy (x:680-910) - Best Fit auto-
        // shrinks only the names/descriptions long enough to need it
        // ("Reverso Catrina Arcoíris" etc.) instead of one hand-picked size
        // that either overflows on the longest string or is needlessly
        // small on every shorter one.
        Text nameText = MakeTitleText(go.transform, "NameText", Vector2.zero, TextAnchor.MiddleLeft, 38);
        nameText.color = darkText;
        nameText.horizontalOverflow = HorizontalWrapMode.Overflow; // force single line so Best Fit shrinks the font instead of wrapping to a second line
        nameText.resizeTextForBestFit = true;
        nameText.resizeTextMinSize = 20;
        nameText.resizeTextMaxSize = 38;
        nameText.rectTransform.anchorMin = new Vector2(0f, 0.5f);
        nameText.rectTransform.anchorMax = new Vector2(0f, 0.5f);
        nameText.rectTransform.pivot = new Vector2(0f, 0.5f);
        nameText.rectTransform.anchoredPosition = new Vector2(50, 28);
        nameText.rectTransform.sizeDelta = new Vector2(410, 50);

        Text descText = MakeText(go.transform, "DescText", Vector2.zero, TextAnchor.MiddleLeft, 34);
        descText.color = new Color(0.3f, 0.28f, 0.28f);
        descText.horizontalOverflow = HorizontalWrapMode.Overflow;
        descText.resizeTextForBestFit = true;
        descText.resizeTextMinSize = 18;
        descText.resizeTextMaxSize = 34;
        descText.rectTransform.anchorMin = new Vector2(0f, 0.5f);
        descText.rectTransform.anchorMax = new Vector2(0f, 0.5f);
        descText.rectTransform.pivot = new Vector2(0f, 0.5f);
        descText.rectTransform.anchoredPosition = new Vector2(50, -32);
        descText.rectTransform.sizeDelta = new Vector2(410, 42);

        // Price sits immediately left of the Buy gem, paired with it on the
        // same vertical center - the old above-the-gem placement read as
        // "lost" (too small/low-contrast against the stone, easy to miss
        // next to the much bigger gem). White instead of amber for contrast
        // against the mid-grey stone.
        Text priceText = MakeText(go.transform, "PriceText", Vector2.zero, TextAnchor.MiddleRight, 30);
        priceText.horizontalOverflow = HorizontalWrapMode.Overflow;
        priceText.resizeTextForBestFit = true;
        priceText.resizeTextMinSize = 18;
        priceText.resizeTextMaxSize = 30;
        priceText.rectTransform.anchorMin = new Vector2(1f, 0.5f);
        priceText.rectTransform.anchorMax = new Vector2(1f, 0.5f);
        priceText.rectTransform.pivot = new Vector2(1f, 0.5f);
        priceText.rectTransform.anchoredPosition = new Vector2(-300, 0);
        priceText.rectTransform.sizeDelta = new Vector2(190, 40);
        priceText.color = Color.white;

        // Diamond white, bigger, and centered on the row's own vertical
        // middle (not paired below the price text) - a real "buy" action
        // reads better sitting on its own than sharing a stack with price.
        Button buyBtn = MakeButton(go.transform, "BuyButton", 230f, "Buy", 26, GemColor.Silver);
        buyBtn.GetComponent<RectTransform>().anchorMin = new Vector2(1f, 0.5f);
        buyBtn.GetComponent<RectTransform>().anchorMax = new Vector2(1f, 0.5f);
        buyBtn.GetComponent<RectTransform>().pivot = new Vector2(1f, 0.5f);
        buyBtn.GetComponent<RectTransform>().anchoredPosition = new Vector2(-50, 0);

        go.SetActive(false);
        return go;
    }

    static Slider MakeSlider(Transform parent, string name, Vector2 anchoredPos)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        RectTransform rt = go.AddComponent<RectTransform>();
        AnchorTop(rt);
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = new Vector2(720, 40);
        Slider slider = go.AddComponent<Slider>();
        slider.minValue = 0f;
        slider.maxValue = 1f;

        GameObject bg = new GameObject("Background");
        bg.transform.SetParent(go.transform, false);
        Image bgImg = bg.AddComponent<Image>();
        bgImg.color = new Color(1f, 1f, 1f, 0.15f);
        RectTransform bgRT = bgImg.rectTransform;
        bgRT.anchorMin = Vector2.zero;
        bgRT.anchorMax = Vector2.one;
        bgRT.offsetMin = Vector2.zero;
        bgRT.offsetMax = Vector2.zero;

        GameObject fillArea = new GameObject("Fill Area");
        fillArea.transform.SetParent(go.transform, false);
        RectTransform fillAreaRT = fillArea.AddComponent<RectTransform>();
        fillAreaRT.anchorMin = new Vector2(0f, 0f);
        fillAreaRT.anchorMax = new Vector2(1f, 1f);
        fillAreaRT.offsetMin = Vector2.zero;
        fillAreaRT.offsetMax = Vector2.zero;

        GameObject fill = new GameObject("Fill");
        fill.transform.SetParent(fillArea.transform, false);
        Image fillImg = fill.AddComponent<Image>();
        fillImg.color = new Color(0.8f, 0.2f, 0.2f, 1f);
        RectTransform fillRT = fillImg.rectTransform;
        fillRT.anchorMin = Vector2.zero;
        fillRT.anchorMax = Vector2.one;
        fillRT.offsetMin = Vector2.zero;
        fillRT.offsetMax = Vector2.zero;
        slider.fillRect = fillRT;
        slider.targetGraphic = fillImg;

        return slider;
    }

    // Body font (Crimson Text) by default - MakeTitleText/MakeButton switch
    // to the decorative Cinzel Decorative face for headers and button labels,
    // matching the treatment in BarajaCombatSceneBuilder.
    static Text MakeText(Transform parent, string name, Vector2 pos, TextAnchor anchor, int size)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        Text t = go.AddComponent<Text>();
        t.font = BarajaFonts.Body;
        t.fontSize = size;
        t.alignment = anchor;
        t.color = Color.white;
        t.verticalOverflow = VerticalWrapMode.Overflow;
        t.rectTransform.sizeDelta = new Vector2(800, 100);
        t.rectTransform.anchoredPosition = pos;
        return t;
    }

    static Text MakeTitleText(Transform parent, string name, Vector2 pos, TextAnchor anchor, int size)
    {
        Text t = MakeText(parent, name, pos, anchor, size);
        t.font = BarajaFonts.Title;
        return t;
    }

    // Brilliant-cut gem button, sized from the caller but always drawn from
    // BarajaGemButtons.Aspect so the gem itself is never stretched or
    // squashed - callers pick width, height is derived to match.
    static Button MakeButton(Transform parent, string name, float width, string label, int fontSize, GemColor gem)
    {
        float height = width / BarajaGemButtons.Aspect;
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        RawImage img = go.AddComponent<RawImage>();
        img.texture = BarajaGemButtons.Get(gem);
        img.rectTransform.sizeDelta = new Vector2(width, height);
        Button btn = go.AddComponent<Button>();
        btn.targetGraphic = img;

        Text t = MakeTitleText(go.transform, "Label", Vector2.zero, TextAnchor.MiddleCenter, fontSize);
        t.rectTransform.anchorMin = Vector2.zero;
        t.rectTransform.anchorMax = Vector2.one;
        t.rectTransform.offsetMin = Vector2.zero;
        t.rectTransform.offsetMax = Vector2.zero;
        t.text = label;

        // Black outline so the label stays legible across the gem's bright
        // and shadowed facets alike, matching the design mockup.
        Outline outline = t.gameObject.AddComponent<Outline>();
        outline.effectColor = new Color(0f, 0f, 0f, 0.9f);
        outline.effectDistance = new Vector2(2f, -2f);

        return btn;
    }

    static void AnchorTop(RectTransform rt)
    {
        rt.anchorMin = new Vector2(0.5f, 1f);
        rt.anchorMax = new Vector2(0.5f, 1f);
        rt.pivot = new Vector2(0.5f, 1f);
    }

    static void AnchorTopLeft(RectTransform rt)
    {
        rt.anchorMin = new Vector2(0f, 1f);
        rt.anchorMax = new Vector2(0f, 1f);
        rt.pivot = new Vector2(0f, 1f);
    }

    static void CenterAnchor(RectTransform rt)
    {
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
    }
}

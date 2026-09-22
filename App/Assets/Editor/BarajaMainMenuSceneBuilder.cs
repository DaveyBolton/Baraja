using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Baraja.Menu;
using Baraja.Core;

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
        // Match height, not width - see BarajaCombatSceneBuilder for why:
        // matching width shrinks the canvas's effective height whenever the
        // runtime window isn't exactly 9:16, which can balloon bottom/top
        // anchored elements past their intended footprint.
        scaler.matchWidthOrHeight = 1f;
        canvasGO.AddComponent<GraphicRaycaster>();

        // --- Backdrop: same marigold-path art the combat board and the
        // Options/Store overlays use, so the title screen isn't the only
        // flat-color screen left in the game. First child so everything
        // else (title, nav buttons) draws on top of it. ---
        GameObject backdropGO = new GameObject("Backdrop");
        backdropGO.transform.SetParent(canvasGO.transform, false);
        RawImage backdrop = backdropGO.AddComponent<RawImage>();
        backdrop.texture = Resources.Load<Texture2D>("Art/Backdrops/zone1_marigold_path");
        RectTransform backdropRT = backdrop.rectTransform;
        backdropRT.anchorMin = Vector2.zero;
        backdropRT.anchorMax = Vector2.one;
        backdropRT.offsetMin = Vector2.zero;
        backdropRT.offsetMax = Vector2.zero;

        GameObject backdropTintGO = new GameObject("BackdropTint");
        backdropTintGO.transform.SetParent(canvasGO.transform, false);
        Image backdropTint = backdropTintGO.AddComponent<Image>();
        backdropTint.color = new Color(0.03f, 0.02f, 0.05f, 0.55f);
        backdropTint.raycastTarget = false;
        RectTransform backdropTintRT = backdropTint.rectTransform;
        backdropTintRT.anchorMin = Vector2.zero;
        backdropTintRT.anchorMax = Vector2.one;
        backdropTintRT.offsetMin = Vector2.zero;
        backdropTintRT.offsetMax = Vector2.zero;

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

        // Wired into MainMenuUI so Options title/FX/Music labels and the
        // Store title translate when the language toggles too - these used
        // to be hardcoded English forever, which is what made the language
        // toggle look broken (other things switched, these never did).
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

        ui.OptionsTitleText = optionsTitle;
        ui.FxLabelText = fxLabel;
        ui.MusicLabelText = musicLabel;
        ui.StoreTitleText = storeTitle;

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

        // Same marigold-path art the combat board uses, so Options/Store
        // don't look like a flat black modal dropped on top of the game.
        // Also doubles as the modal's click-blocker (raycastTarget stays
        // true by default), same job the old solid Image did.
        RawImage backdrop = go.AddComponent<RawImage>();
        backdrop.texture = Resources.Load<Texture2D>("Art/Backdrops/zone1_marigold_path");
        RectTransform backdropRT = backdrop.rectTransform;
        backdropRT.anchorMin = Vector2.zero;
        backdropRT.anchorMax = Vector2.one;
        backdropRT.offsetMin = Vector2.zero;
        backdropRT.offsetMax = Vector2.zero;

        // Dark tint alpha-blended on top of the photo (not multiplied into
        // it) so labels/sliders stay legible no matter how bright any given
        // patch of the backdrop is - same approach as the combat tutorial
        // hint banner's own background.
        GameObject tintGO = new GameObject("Tint");
        tintGO.transform.SetParent(go.transform, false);
        Image tint = tintGO.AddComponent<Image>();
        tint.color = new Color(0.03f, 0.02f, 0.05f, 0.7f);
        tint.raycastTarget = false;
        RectTransform tintRT = tint.rectTransform;
        tintRT.anchorMin = Vector2.zero;
        tintRT.anchorMax = Vector2.one;
        tintRT.offsetMin = Vector2.zero;
        tintRT.offsetMax = Vector2.zero;

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
        // Taller than the stone's own natural aspect (960x229) would give -
        // that height forced Name/Description to share a horizontal band
        // with Price+Buy, capping every font at a small size to avoid
        // collisions. Name and Description now each get the row's full
        // width on their own line; only Price+Buy share the bottom line.
        const float rowWidth = 960f;
        const float rowHeight = 300f;

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

        // Dark text straight on the mottled stone texture read as if it were
        // CARVED INTO the graphic rather than a separate UI text layer
        // floating on top of it. A flat rectangular plate behind it fixed
        // legibility but its own hard edge showed up as a visible box seam
        // against the stone - swapped for the same trick the rest of this
        // game already uses to float text over busy art (button labels,
        // the tutorial hint's close X): a black outline halo around the
        // glyphs themselves, no separate background shape at all.
        Color plateText = new Color(0.96f, 0.93f, 0.85f);

        // Name and Description each get the row's FULL width on their own
        // line - measured against the longest strings in the catalog
        // ("Reverso Catrina Arcoíris", "Elimina todos los anuncios para
        // siempre.") at 860px available width, size 40/32 clear with a lot
        // of margin (worst case measured 628px/544px). Still Best Fit as a
        // safety net, not as the primary sizing mechanism - only true
        // outliers would ever need to shrink now.
        Text nameText = MakeTitleText(go.transform, "NameText", Vector2.zero, TextAnchor.UpperLeft, 40);
        nameText.color = plateText;
        nameText.horizontalOverflow = HorizontalWrapMode.Overflow;
        nameText.resizeTextForBestFit = true;
        nameText.resizeTextMinSize = 24;
        nameText.resizeTextMaxSize = 40;
        nameText.rectTransform.anchorMin = new Vector2(0f, 1f);
        nameText.rectTransform.anchorMax = new Vector2(0f, 1f);
        nameText.rectTransform.pivot = new Vector2(0f, 1f);
        // panel_stone.png is a 960x229 texture stretched to fill this
        // 960x300 row (its RawImage IS the row's own RectTransform, no
        // separate child) - that's a 1.31x vertical stretch, and its own
        // carved/vine border occupies the top ~40px of the SOURCE image
        // (measured by sampling pixel color down the center column: it
        // doesn't settle into flat stone until y~40), which becomes ~52px
        // after the stretch. -24 put the name text's own top edge, plus
        // CinzelDecorative-Bold's ascenders/accents (É in "PÉTALOS"),
        // squarely on top of that carved border - visibly clipped by it.
        // Pushed below the safe ~52px line with margin for the glyph
        // overshoot.
        nameText.rectTransform.anchoredPosition = new Vector2(50, -66);
        nameText.rectTransform.sizeDelta = new Vector2(860, 54);
        nameText.GetComponent<MinScreenFontSize>().MinPixelSize = 32f;
        Outline nameOutline = nameText.gameObject.AddComponent<Outline>();
        nameOutline.effectColor = new Color(0f, 0f, 0f, 0.85f);
        nameOutline.effectDistance = new Vector2(2f, -2f);

        Text descText = MakeText(go.transform, "DescText", Vector2.zero, TextAnchor.UpperLeft, 32);
        descText.color = new Color(plateText.r, plateText.g, plateText.b, 0.85f);
        descText.horizontalOverflow = HorizontalWrapMode.Overflow;
        descText.resizeTextForBestFit = true;
        descText.resizeTextMinSize = 20;
        descText.resizeTextMaxSize = 32;
        descText.rectTransform.anchorMin = new Vector2(0f, 1f);
        descText.rectTransform.anchorMax = new Vector2(0f, 1f);
        descText.rectTransform.pivot = new Vector2(0f, 1f);
        // Kept the same 62px gap below the (now lower) name line.
        descText.rectTransform.anchoredPosition = new Vector2(50, -128);
        descText.rectTransform.sizeDelta = new Vector2(860, 48);
        Outline descOutline = descText.gameObject.AddComponent<Outline>();
        descOutline.effectColor = new Color(0f, 0f, 0f, 0.75f);
        descOutline.effectDistance = new Vector2(1.5f, -1.5f);

        // Bottom line: Price (left of the gem, white) + Buy, both centered
        // on the same row so they read as a pair without competing with
        // Name/Description above for width anymore.
        // Price+Buy read as pinned to the row's bottom edge when centered
        // only within the leftover strip below Description - the row's
        // description text is always short enough that it never actually
        // reaches this far right (worst case measured ~544px against 860
        // available), so there's no real collision risk in centering this
        // pair on the FULL row height (150 = half of the 300-tall row)
        // instead - reads as centered on the plate's right side, which is
        // what it's actually sitting next to.
        const float rightColumnCenterY = 150f;

        Text priceText = MakeText(go.transform, "PriceText", Vector2.zero, TextAnchor.MiddleRight, 34);
        priceText.horizontalOverflow = HorizontalWrapMode.Overflow;
        priceText.resizeTextForBestFit = true;
        priceText.resizeTextMinSize = 20;
        priceText.resizeTextMaxSize = 34;
        priceText.rectTransform.anchorMin = new Vector2(1f, 0f);
        priceText.rectTransform.anchorMax = new Vector2(1f, 0f);
        priceText.rectTransform.pivot = new Vector2(1f, 0f);
        priceText.rectTransform.sizeDelta = new Vector2(190, 48);
        priceText.rectTransform.anchoredPosition = new Vector2(-300, rightColumnCenterY - priceText.rectTransform.sizeDelta.y / 2f);
        priceText.color = Color.white;
        priceText.GetComponent<MinScreenFontSize>().MinPixelSize = 30f;

        // Diamond white, bigger, and centered (with Price) on the row's
        // full vertical middle - see rightColumnCenterY above.
        Button buyBtn = MakeButton(go.transform, "BuyButton", 230f, "Buy", 26, GemColor.Silver);
        RectTransform buyBtnRT = buyBtn.GetComponent<RectTransform>();
        buyBtnRT.anchorMin = new Vector2(1f, 0f);
        buyBtnRT.anchorMax = new Vector2(1f, 0f);
        buyBtnRT.pivot = new Vector2(1f, 0f);
        float buyBtnHeight = 230f / BarajaGemButtons.Aspect;
        buyBtnRT.anchoredPosition = new Vector2(-50, rightColumnCenterY - buyBtnHeight / 2f);

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
        // Default legibility floor - see MinScreenFontSize (BarajaCombatSceneBuilder
        // uses the same helper pattern). Handles resizeTextForBestFit fields
        // (store name/desc/price) by raising their min/max bounds instead of
        // fontSize directly, since BestFit ignores fontSize once enabled.
        go.AddComponent<MinScreenFontSize>().MinPixelSize = 28f;
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

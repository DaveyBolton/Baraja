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

        Button playBtn = MakeButton(canvasGO.transform, "PlayButton", new Vector2(440, 116), "Play", 44);
        CenterAnchor(playBtn.GetComponent<RectTransform>());
        playBtn.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 120);

        Button howToPlayBtn = MakeButton(canvasGO.transform, "HowToPlayButton", new Vector2(440, 96), "How to Play", 34);
        CenterAnchor(howToPlayBtn.GetComponent<RectTransform>());
        howToPlayBtn.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -10);

        Button optionsBtn = MakeButton(canvasGO.transform, "OptionsButton", new Vector2(440, 96), "Options", 34);
        CenterAnchor(optionsBtn.GetComponent<RectTransform>());
        optionsBtn.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -130);

        Button storeBtn = MakeButton(canvasGO.transform, "StoreButton", new Vector2(440, 96), "Store", 34);
        CenterAnchor(storeBtn.GetComponent<RectTransform>());
        storeBtn.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -250);

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
        Text fxLabel = MakeText(optionsPanel.transform, "FxLabel", new Vector2(180, -260), TextAnchor.MiddleLeft, 34);
        AnchorTopLeft(fxLabel.rectTransform);
        fxLabel.rectTransform.sizeDelta = new Vector2(400, 50);
        fxLabel.text = "FX Volume";
        Slider fxSlider = MakeSlider(optionsPanel.transform, "FxSlider", new Vector2(0, -330));

        Text musicLabel = MakeText(optionsPanel.transform, "MusicLabel", new Vector2(180, -420), TextAnchor.MiddleLeft, 34);
        AnchorTopLeft(musicLabel.rectTransform);
        musicLabel.rectTransform.sizeDelta = new Vector2(400, 50);
        musicLabel.text = "Music Volume";
        Slider musicSlider = MakeSlider(optionsPanel.transform, "MusicSlider", new Vector2(0, -490));

        Button langBtn = MakeButton(optionsPanel.transform, "LanguageButton", new Vector2(480, 96), "Idioma: Español", 32);
        CenterAnchor(langBtn.GetComponent<RectTransform>());
        langBtn.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -650);
        Text langLabel = langBtn.GetComponentInChildren<Text>();

        Button optionsCloseBtn = MakeButton(optionsPanel.transform, "OptionsCloseButton", new Vector2(250, 84), "Close", 32);
        CenterAnchor(optionsCloseBtn.GetComponent<RectTransform>());
        optionsCloseBtn.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -840);

        // --- Tutorial overlay ---
        GameObject tutorialPanel = MakeOverlayPanel(canvasGO.transform, "TutorialPanel");
        Text tutorialTitle = MakeTitleText(tutorialPanel.transform, "TutorialTitle", new Vector2(0, -100), TextAnchor.MiddleCenter, 50);
        AnchorTop(tutorialTitle.rectTransform);
        tutorialTitle.rectTransform.sizeDelta = new Vector2(900, 80);

        Text tutorialBody = MakeText(tutorialPanel.transform, "TutorialBody", new Vector2(0, -260), TextAnchor.UpperCenter, 34);
        AnchorTop(tutorialBody.rectTransform);
        tutorialBody.rectTransform.sizeDelta = new Vector2(900, 700);

        Text tutorialPageIndex = MakeText(tutorialPanel.transform, "TutorialPageIndex", new Vector2(0, -1080), TextAnchor.MiddleCenter, 28);
        AnchorTop(tutorialPageIndex.rectTransform);
        tutorialPageIndex.color = new Color(1f, 1f, 1f, 0.6f);

        Button tutorialBackBtn = MakeButton(tutorialPanel.transform, "TutorialBackButton", new Vector2(230, 94), "< Back", 32);
        tutorialBackBtn.GetComponent<RectTransform>().anchorMin = new Vector2(0f, 0f);
        tutorialBackBtn.GetComponent<RectTransform>().anchorMax = new Vector2(0f, 0f);
        tutorialBackBtn.GetComponent<RectTransform>().pivot = new Vector2(0f, 0f);
        tutorialBackBtn.GetComponent<RectTransform>().anchoredPosition = new Vector2(40, 260);

        Button tutorialNextBtn = MakeButton(tutorialPanel.transform, "TutorialNextButton", new Vector2(230, 94), "Next >", 32);
        tutorialNextBtn.GetComponent<RectTransform>().anchorMin = new Vector2(1f, 0f);
        tutorialNextBtn.GetComponent<RectTransform>().anchorMax = new Vector2(1f, 0f);
        tutorialNextBtn.GetComponent<RectTransform>().pivot = new Vector2(1f, 0f);
        tutorialNextBtn.GetComponent<RectTransform>().anchoredPosition = new Vector2(-40, 260);

        Button tutorialCloseBtn = MakeButton(tutorialPanel.transform, "TutorialCloseButton", new Vector2(250, 84), "Close", 32);
        CenterAnchor(tutorialCloseBtn.GetComponent<RectTransform>());
        tutorialCloseBtn.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 130);

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

        Button storeCloseBtn = MakeButton(storePanel.transform, "StoreCloseButton", new Vector2(250, 84), "Close", 32);
        storeCloseBtn.GetComponent<RectTransform>().anchorMin = new Vector2(0.5f, 0f);
        storeCloseBtn.GetComponent<RectTransform>().anchorMax = new Vector2(0.5f, 0f);
        storeCloseBtn.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 0f);
        storeCloseBtn.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 40);

        GameObject storeRowPrefab = MakeStoreRowPrefab(canvasGO.transform);

        optionsPanel.SetActive(false);
        tutorialPanel.SetActive(false);
        storePanel.SetActive(false);

        // --- Wiring ---
        MainMenuUI ui = canvasGO.AddComponent<MainMenuUI>();
        ui.OptionsPanel = optionsPanel;
        ui.StorePanel = storePanel;
        ui.TutorialPanel = tutorialPanel;

        ui.PlayButton = playBtn;
        ui.HowToPlayButton = howToPlayBtn;
        ui.OptionsButton = optionsBtn;
        ui.StoreButton = storeBtn;

        ui.OptionsCloseButton = optionsCloseBtn;
        ui.TutorialCloseButton = tutorialCloseBtn;
        ui.StoreCloseButton = storeCloseBtn;

        ui.FxSlider = fxSlider;
        ui.MusicSlider = musicSlider;
        ui.LanguageButton = langBtn;
        ui.LanguageButtonLabel = langLabel;

        ui.TutorialTitleText = tutorialTitle;
        ui.TutorialBodyText = tutorialBody;
        ui.TutorialPageIndexText = tutorialPageIndex;
        ui.TutorialNextButton = tutorialNextBtn;
        ui.TutorialBackButton = tutorialBackBtn;

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
    static GameObject MakeStoreRowPrefab(Transform parent)
    {
        GameObject go = new GameObject("StoreRowPrefab");
        go.transform.SetParent(parent, false);
        RectTransform rt = go.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 1f);
        rt.anchorMax = new Vector2(0.5f, 1f);
        rt.pivot = new Vector2(0.5f, 1f);
        rt.sizeDelta = new Vector2(960, 160);
        LayoutElement layoutElement = go.AddComponent<LayoutElement>();
        layoutElement.preferredHeight = 160;
        layoutElement.preferredWidth = 960;
        Image bg = go.AddComponent<Image>();
        bg.color = new Color(1f, 1f, 1f, 0.06f);

        Text nameText = MakeTitleText(go.transform, "NameText", Vector2.zero, TextAnchor.UpperLeft, 30);
        nameText.rectTransform.anchorMin = new Vector2(0f, 1f);
        nameText.rectTransform.anchorMax = new Vector2(0f, 1f);
        nameText.rectTransform.pivot = new Vector2(0f, 1f);
        nameText.rectTransform.anchoredPosition = new Vector2(24, -14);
        nameText.rectTransform.sizeDelta = new Vector2(620, 40);

        Text descText = MakeText(go.transform, "DescText", Vector2.zero, TextAnchor.UpperLeft, 22);
        descText.rectTransform.anchorMin = new Vector2(0f, 1f);
        descText.rectTransform.anchorMax = new Vector2(0f, 1f);
        descText.rectTransform.pivot = new Vector2(0f, 1f);
        descText.rectTransform.anchoredPosition = new Vector2(24, -60);
        descText.rectTransform.sizeDelta = new Vector2(620, 80);
        descText.color = new Color(1f, 1f, 1f, 0.75f);

        Text priceText = MakeText(go.transform, "PriceText", Vector2.zero, TextAnchor.UpperRight, 24);
        priceText.rectTransform.anchorMin = new Vector2(1f, 1f);
        priceText.rectTransform.anchorMax = new Vector2(1f, 1f);
        priceText.rectTransform.pivot = new Vector2(1f, 1f);
        priceText.rectTransform.anchoredPosition = new Vector2(-24, -14);
        priceText.rectTransform.sizeDelta = new Vector2(260, 34);
        priceText.color = new Color(1f, 0.85f, 0.4f);

        Button buyBtn = MakeButton(go.transform, "BuyButton", new Vector2(180, 64), "Buy", 26);
        buyBtn.GetComponent<RectTransform>().anchorMin = new Vector2(1f, 1f);
        buyBtn.GetComponent<RectTransform>().anchorMax = new Vector2(1f, 1f);
        buyBtn.GetComponent<RectTransform>().pivot = new Vector2(1f, 1f);
        buyBtn.GetComponent<RectTransform>().anchoredPosition = new Vector2(-24, -60);

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

    static Button MakeButton(Transform parent, string name, Vector2 size, string label, int fontSize)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        Image img = go.AddComponent<Image>();
        img.color = new Color(0.6f, 0.15f, 0.15f, 0.9f);
        img.rectTransform.sizeDelta = size;
        Button btn = go.AddComponent<Button>();
        btn.targetGraphic = img;

        Text t = MakeTitleText(go.transform, "Label", Vector2.zero, TextAnchor.MiddleCenter, fontSize);
        t.rectTransform.anchorMin = Vector2.zero;
        t.rectTransform.anchorMax = Vector2.one;
        t.rectTransform.offsetMin = Vector2.zero;
        t.rectTransform.offsetMax = Vector2.zero;
        t.text = label;

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

using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Baraja.Combat;

/// <summary>
/// One-shot scene assembler for the combat scene, run via:
/// Unity -batchmode -executeMethod BarajaCombatSceneBuilder.Build -quit
/// No manual editor authoring — rebuild any time the layout or wiring changes.
/// </summary>
public static class BarajaCombatSceneBuilder
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

        GameObject canvasGO = new GameObject("Combat Canvas");
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        canvasGO.AddComponent<GraphicRaycaster>();

        // --- Backdrop: first child so everything else draws on top of it ---
        GameObject backdropGO = new GameObject("Backdrop");
        backdropGO.transform.SetParent(canvasGO.transform, false);
        RawImage backdrop = backdropGO.AddComponent<RawImage>();
        backdrop.texture = Resources.Load<Texture2D>("Art/Backdrops/zone1_marigold_path");
        RectTransform backdropRT = backdrop.rectTransform;
        backdropRT.anchorMin = Vector2.zero;
        backdropRT.anchorMax = Vector2.one;
        backdropRT.offsetMin = Vector2.zero;
        backdropRT.offsetMax = Vector2.zero;

        // --- Top bar: player HP / energy ---
        Text playerHp = MakeText(canvasGO.transform, "PlayerHpText", new Vector2(0, -30), TextAnchor.UpperCenter, 44);
        AnchorTop(playerHp.rectTransform);
        playerHp.rectTransform.sizeDelta = new Vector2(900, 70);

        Text playerEnergy = MakeText(canvasGO.transform, "PlayerEnergyText", new Vector2(0, -100), TextAnchor.UpperCenter, 40);
        AnchorTop(playerEnergy.rectTransform);
        playerEnergy.rectTransform.sizeDelta = new Vector2(900, 60);

        // --- Enemy row, upper-middle ---
        GameObject enemyContainer = new GameObject("EnemyContainer");
        enemyContainer.transform.SetParent(canvasGO.transform, false);
        RectTransform enemyContainerRT = enemyContainer.AddComponent<RectTransform>();
        enemyContainerRT.anchorMin = new Vector2(0.5f, 0.62f);
        enemyContainerRT.anchorMax = new Vector2(0.5f, 0.62f);
        enemyContainerRT.pivot = new Vector2(0.5f, 0.5f);
        enemyContainerRT.sizeDelta = new Vector2(1000, 600);
        HorizontalLayoutGroup enemyLayout = enemyContainer.AddComponent<HorizontalLayoutGroup>();
        enemyLayout.spacing = 40f;
        enemyLayout.childAlignment = TextAnchor.MiddleCenter;
        enemyLayout.childForceExpandWidth = false;
        enemyLayout.childForceExpandHeight = false;

        GameObject enemyPanelPrefab = MakeEnemyPanelPrefab(canvasGO.transform);

        // --- Log: its own band between the enemy row and the hand, so it never
        // shares screen space with the cards or the End Turn button. Top-anchored
        // with Upper alignment so overflow grows downward into that band, rather
        // than bottom-anchored Overflow, which stacks new lines back over old
        // ones once content exceeds the rect (the cause of the overlap Dave saw).
        Text logText = MakeText(canvasGO.transform, "LogText", new Vector2(0, -1000), TextAnchor.UpperCenter, 30);
        logText.rectTransform.anchorMin = new Vector2(0.5f, 1f);
        logText.rectTransform.anchorMax = new Vector2(0.5f, 1f);
        logText.rectTransform.pivot = new Vector2(0.5f, 1f);
        logText.rectTransform.sizeDelta = new Vector2(1000, 380);
        logText.color = new Color(1f, 1f, 1f, 0.9f);

        // --- End Turn button, top-right. Deliberately kept off the bottom band
        // entirely (rather than beside the hand) so it can never collide with the
        // cards - a bottom-right placement overlapped whichever card ended up
        // rightmost once the hand had 4+ cards in it. ---
        Button endTurnBtn = MakeButton(canvasGO.transform, "EndTurnButton", new Vector2(240, 90), "End Turn", 32);
        RectTransform endTurnRT = endTurnBtn.GetComponent<RectTransform>();
        endTurnRT.anchorMin = new Vector2(1f, 1f);
        endTurnRT.anchorMax = new Vector2(1f, 1f);
        endTurnRT.pivot = new Vector2(1f, 1f);
        endTurnRT.anchoredPosition = new Vector2(-30, -30);

        // --- Hand, bottom - a horizontally scrolling row so cards can be
        // drawn big enough to actually read (3 fit on screen at once; swipe
        // for the rest) instead of shrinking every card to fit a fixed row. ---
        GameObject handContent = MakeHorizontalScrollList(canvasGO.transform, "HandScroll",
            new Vector2(0, 10), new Vector2(1080, 500));

        GameObject cardButtonPrefab = MakeCardButtonPrefab(canvasGO.transform);

        // --- First-time tutorial banner, between the top bar and the enemy row ---
        GameObject hintPanel = new GameObject("TutorialHintPanel");
        hintPanel.transform.SetParent(canvasGO.transform, false);
        Image hintBg = hintPanel.AddComponent<Image>();
        hintBg.color = new Color(0.15f, 0.1f, 0.05f, 0.92f);
        RectTransform hintRT = hintBg.rectTransform;
        hintRT.anchorMin = new Vector2(0.5f, 1f);
        hintRT.anchorMax = new Vector2(0.5f, 1f);
        hintRT.pivot = new Vector2(0.5f, 1f);
        hintRT.anchoredPosition = new Vector2(0, -180);
        hintRT.sizeDelta = new Vector2(980, 220);

        Text hintBody = MakeText(hintPanel.transform, "HintBodyText", new Vector2(0, 25), TextAnchor.MiddleCenter, 32);
        hintBody.rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        hintBody.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        hintBody.rectTransform.pivot = new Vector2(0.5f, 0.5f);
        hintBody.rectTransform.sizeDelta = new Vector2(900, 140);

        Button hintGotIt = MakeButton(hintPanel.transform, "GotItButton", new Vector2(200, 64), "Got it", 28);
        RectTransform hintBtnRT = hintGotIt.GetComponent<RectTransform>();
        hintBtnRT.anchorMin = new Vector2(0.5f, 0f);
        hintBtnRT.anchorMax = new Vector2(0.5f, 0f);
        hintBtnRT.pivot = new Vector2(0.5f, 0f);
        hintBtnRT.anchoredPosition = new Vector2(0, 20);

        CombatTutorialHint hint = canvasGO.AddComponent<CombatTutorialHint>();
        hint.Panel = hintPanel;
        hint.BodyText = hintBody;
        hint.GotItButton = hintGotIt;

        // --- Manager + UI wiring ---
        GameObject managerGO = new GameObject("CombatManager");
        CombatManager manager = managerGO.AddComponent<CombatManager>();

        CombatUI ui = canvasGO.AddComponent<CombatUI>();
        ui.Manager = manager;
        ui.Spanish = true;
        ui.PlayerHpText = playerHp;
        ui.PlayerEnergyText = playerEnergy;
        ui.LogText = logText;
        ui.EndTurnButton = endTurnBtn;
        ui.EnemyContainer = enemyContainer.transform;
        ui.EnemyPanelPrefab = enemyPanelPrefab;
        ui.HandContainer = handContent.transform;
        ui.CardButtonPrefab = cardButtonPrefab;

        CombatBootstrap bootstrap = canvasGO.AddComponent<CombatBootstrap>();
        bootstrap.Ui = ui;
        bootstrap.FightNumber = 1;

        string scenePath = "Assets/Scenes/Combat.unity";
        System.IO.Directory.CreateDirectory("Assets/Scenes");
        EditorSceneManager.SaveScene(scene, scenePath);
        BarajaBuildScenes.Register(scenePath, 1);

        Debug.Log("Baraja combat scene built successfully at " + scenePath);
    }

    // enemy_frame_silver.png is a 736x680 crop of the same locked card frame
    // used for player cards, recolored to silver, chroma-keyed transparent
    // only inside its art window. These fractions are the ACTUAL measured
    // bounds of the transparent pixels in that file (checked with
    // numpy: alpha==0 spans x=158-600, y=96-638) - not the original gold
    // frame's own geometry() measurement (158,95)-(577,638). The right edge
    // in particular is 23px further out here because the eligible-keying
    // zone used to build this file was intentionally extended to x=600 to
    // clear a residual gradient strip, and the black backing behind the
    // frame has to match wherever the file is ACTUALLY transparent, not
    // wherever the unrelated original frame's border happened to measure -
    // that mismatch is exactly what let the backdrop show through a sliver
    // on the right edge of the window.
    const float FrameWindowLeft = 158f / 736f;
    const float FrameWindowRight = 600f / 736f;
    const float FrameWindowTop = 96f / 680f;
    const float FrameWindowBottom = 638f / 680f;

    // Inactive template instantiated per-enemy by CombatUI; never itself part of the live layout.
    static GameObject MakeEnemyPanelPrefab(Transform parent)
    {
        GameObject go = new GameObject("EnemyPanelPrefab");
        go.transform.SetParent(parent, false);
        RectTransform rt = go.AddComponent<RectTransform>();
        rt.sizeDelta = new Vector2(340, 480);
        Button btn = go.AddComponent<Button>();

        // Frame's own footprint: matches the crop's 736:680 aspect, sized to
        // the panel's width, and centered horizontally within the (slightly
        // wider) panel.
        float frameW = 320f;
        float frameH = frameW * 680f / 736f;
        float frameLeft = (340f - frameW) / 2f;

        // All three layers below use the SAME top-left anchor/pivot (0,1),
        // so anchoredPosition is a plain (x from panel's left, -y from
        // panel's top) offset with no half-width correction to get wrong -
        // that correction is exactly what put the window and the frame's
        // actual hole in two different places the first time this was built.
        float winLeft = frameLeft + FrameWindowLeft * frameW;
        float winRight = frameLeft + FrameWindowRight * frameW;
        float winTop = FrameWindowTop * frameH;
        float winBottom = FrameWindowBottom * frameH;
        float winW = winRight - winLeft;
        float winH = winBottom - winTop;

        // Solid black plate behind the whole window, matching the card
        // builder's DARK_PLATE convention - without it the backdrop image
        // would show through the frame's transparent window instead of the
        // letterboxed black margin around the (square) bust art.
        GameObject windowBgGO = new GameObject("WindowBg");
        windowBgGO.transform.SetParent(go.transform, false);
        Image windowBg = windowBgGO.AddComponent<Image>();
        windowBg.color = Color.black;
        RectTransform windowBgRT = windowBg.rectTransform;
        windowBgRT.anchorMin = new Vector2(0f, 1f);
        windowBgRT.anchorMax = new Vector2(0f, 1f);
        windowBgRT.pivot = new Vector2(0f, 1f);
        windowBgRT.anchoredPosition = new Vector2(winLeft, -winTop);
        windowBgRT.sizeDelta = new Vector2(winW, winH);
        btn.targetGraphic = windowBg;

        // Bust art is square and already has a true-black background baked
        // in (clean_card_backgrounds.py), so it letterboxes onto WindowBg
        // exactly like the card builder's art panel - scaled to fit inside
        // both window dimensions, not just one, since this window is
        // narrower than it is tall.
        GameObject imgGO = new GameObject("Image");
        imgGO.transform.SetParent(go.transform, false);
        RawImage img = imgGO.AddComponent<RawImage>();
        RectTransform imgRT = img.rectTransform;
        imgRT.anchorMin = new Vector2(0f, 1f);
        imgRT.anchorMax = new Vector2(0f, 1f);
        imgRT.pivot = new Vector2(0f, 1f);
        float bustSize = Mathf.Min(winW, winH) * 0.85f;
        imgRT.anchoredPosition = new Vector2(
            winLeft + (winW - bustSize) / 2f,
            -(winTop + (winH - bustSize) / 2f));
        imgRT.sizeDelta = new Vector2(bustSize, bustSize);

        // Frame overlay last, on top of both, so its opaque scrollwork draws
        // over the letterboxed edges and only its window shows the art below.
        GameObject frameGO = new GameObject("FrameOverlay");
        frameGO.transform.SetParent(go.transform, false);
        RawImage frameImg = frameGO.AddComponent<RawImage>();
        frameImg.texture = Resources.Load<Texture2D>("Art/Enemies/enemy_frame_silver");
        frameImg.raycastTarget = false;
        RectTransform frameRT = frameImg.rectTransform;
        frameRT.anchorMin = new Vector2(0f, 1f);
        frameRT.anchorMax = new Vector2(0f, 1f);
        frameRT.pivot = new Vector2(0f, 1f);
        frameRT.anchoredPosition = new Vector2(frameLeft, 0);
        frameRT.sizeDelta = new Vector2(frameW, frameH);

        Text hpText = MakeText(go.transform, "HpText", new Vector2(0, -(frameH + 20)), TextAnchor.UpperCenter, 26);
        hpText.rectTransform.anchorMin = new Vector2(0.5f, 1f);
        hpText.rectTransform.anchorMax = new Vector2(0.5f, 1f);
        hpText.rectTransform.pivot = new Vector2(0.5f, 1f);
        hpText.rectTransform.sizeDelta = new Vector2(300, 80);

        Text intentText = MakeText(go.transform, "IntentText", new Vector2(0, -(frameH + 20 + 80 + 10)), TextAnchor.UpperCenter, 24);
        intentText.color = new Color(1f, 0.75f, 0.3f);
        intentText.rectTransform.anchorMin = new Vector2(0.5f, 1f);
        intentText.rectTransform.anchorMax = new Vector2(0.5f, 1f);
        intentText.rectTransform.pivot = new Vector2(0.5f, 1f);
        intentText.rectTransform.sizeDelta = new Vector2(300, 60);

        go.SetActive(false);
        return go;
    }

    // Inactive template instantiated per-card-in-hand by CombatUI.
    static GameObject MakeCardButtonPrefab(Transform parent)
    {
        GameObject go = new GameObject("CardButtonPrefab");
        go.transform.SetParent(parent, false);
        RectTransform rt = go.AddComponent<RectTransform>();
        // 340x480 matches the card art's real 736x1040 aspect ratio (RawImage
        // has no preserveAspect option, unlike Image, so this has to be exact
        // or the art stretches). Big enough to actually read the card text;
        // the hand scrolls horizontally now instead of shrinking cards to
        // force a fixed number of them into one screen width.
        rt.sizeDelta = new Vector2(340, 480);
        rt.anchorMin = new Vector2(0f, 0.5f);
        rt.anchorMax = new Vector2(0f, 0.5f);
        rt.pivot = new Vector2(0f, 0.5f);
        RawImage img = go.AddComponent<RawImage>();
        Button btn = go.AddComponent<Button>();
        btn.targetGraphic = img;

        ColorBlock colors = btn.colors;
        colors.disabledColor = new Color(0.4f, 0.4f, 0.4f, 0.6f);
        btn.colors = colors;

        go.SetActive(false);
        return go;
    }

    static Text MakeText(Transform parent, string name, Vector2 pos, TextAnchor anchor, int size)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        Text t = go.AddComponent<Text>();
        t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        t.fontSize = size;
        t.alignment = anchor;
        t.color = Color.white;
        t.verticalOverflow = VerticalWrapMode.Overflow;
        t.rectTransform.sizeDelta = new Vector2(800, 100);
        t.rectTransform.anchoredPosition = pos;
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

        Text t = MakeText(go.transform, "Label", Vector2.zero, TextAnchor.MiddleCenter, fontSize);
        t.rectTransform.anchorMin = Vector2.zero;
        t.rectTransform.anchorMax = Vector2.one;
        t.rectTransform.offsetMin = Vector2.zero;
        t.rectTransform.offsetMax = Vector2.zero;
        t.text = label;

        return btn;
    }

    // A horizontally scrolling row: returns the Content object callers should
    // parent cards into. A HorizontalLayoutGroup + ContentSizeFitter stacks
    // and grows it sideways; the ScrollRect/Viewport/Mask clip it to the
    // visible window and let the player swipe for the rest of the hand.
    static GameObject MakeHorizontalScrollList(Transform parent, string name, Vector2 anchoredPos, Vector2 size)
    {
        GameObject scrollGO = new GameObject(name);
        scrollGO.transform.SetParent(parent, false);
        RectTransform scrollRT = scrollGO.AddComponent<RectTransform>();
        scrollRT.anchorMin = new Vector2(0.5f, 0f);
        scrollRT.anchorMax = new Vector2(0.5f, 0f);
        scrollRT.pivot = new Vector2(0.5f, 0f);
        scrollRT.anchoredPosition = anchoredPos;
        scrollRT.sizeDelta = size;
        ScrollRect scrollRect = scrollGO.AddComponent<ScrollRect>();
        scrollRect.horizontal = true;
        scrollRect.vertical = false;

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
        contentRT.anchorMin = new Vector2(0f, 0f);
        contentRT.anchorMax = new Vector2(0f, 1f);
        contentRT.pivot = new Vector2(0f, 0.5f);
        contentRT.anchoredPosition = Vector2.zero;
        HorizontalLayoutGroup layout = contentGO.AddComponent<HorizontalLayoutGroup>();
        layout.spacing = 20f;
        layout.childControlWidth = false;
        layout.childControlHeight = false;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = false;
        layout.childAlignment = TextAnchor.MiddleLeft;
        layout.padding = new RectOffset(20, 20, 10, 10);
        ContentSizeFitter fitter = contentGO.AddComponent<ContentSizeFitter>();
        fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
        scrollRect.content = contentRT;

        return contentGO;
    }

    static void AnchorTop(RectTransform rt)
    {
        rt.anchorMin = new Vector2(0.5f, 1f);
        rt.anchorMax = new Vector2(0.5f, 1f);
        rt.pivot = new Vector2(0.5f, 1f);
    }
}

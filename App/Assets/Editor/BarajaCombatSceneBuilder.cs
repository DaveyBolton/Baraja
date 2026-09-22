using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Baraja.Combat;
using Baraja.Core;

/// <summary>
/// One-shot scene assembler for the combat scene, run via:
/// Unity -batchmode -executeMethod BarajaCombatSceneBuilder.Build -quit
/// No manual editor authoring — rebuild any time the layout or wiring changes.
/// </summary>
public static class BarajaCombatSceneBuilder
{
    // One button size for the whole scene, wide enough to fit the longest
    // label anywhere in the game ("How to Play", on the main menu) at a
    // legible size - see BarajaMainMenuSceneBuilder for the measurement.
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

        GameObject canvasGO = new GameObject("Combat Canvas");
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        // Match HEIGHT, not width (CanvasScaler's default). WindowAspectLock's
        // whole design is "maximize height, derive width to keep 9:16" - if the
        // runtime window (or, worse, a batchmode/off-aspect environment) is ever
        // NOT exactly 9:16, matching by width shrinks the canvas's effective
        // HEIGHT below the 1920 every anchored element assumes, so bottom-anchored
        // elements sized in absolute units (like the hand carousel's drag-catcher)
        // balloon past their intended footprint and can cover unrelated UI - this
        // is exactly how a raycast test proved the hand's invisible drag-catcher
        // was stealing clicks meant for End Turn. Matching height instead only
        // ever leaves unused HORIZONTAL canvas space on an off-aspect screen,
        // never a vertical overflow, so top/bottom UI can't collide this way.
        scaler.matchWidthOrHeight = 1f;
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

        // --- Top bar: player HP / energy, left-aligned so the much bigger
        // gem End Turn button (added below) has the right side of the bar
        // to itself instead of overlapping centered text - a standard
        // stats-left/action-right HUD split. ---
        Text playerHp = MakeTitleText(canvasGO.transform, "PlayerHpText", new Vector2(30, -30), TextAnchor.UpperLeft, 46);
        playerHp.fontStyle = FontStyle.Bold;
        AnchorTopLeft(playerHp.rectTransform);
        playerHp.rectTransform.sizeDelta = new Vector2(540, 70);
        playerHp.GetComponent<MinScreenFontSize>().MinPixelSize = 38f;

        Text playerEnergy = MakeTitleText(canvasGO.transform, "PlayerEnergyText", new Vector2(30, -100), TextAnchor.UpperLeft, 40);
        playerEnergy.fontStyle = FontStyle.Bold;
        AnchorTopLeft(playerEnergy.rectTransform);
        playerEnergy.rectTransform.sizeDelta = new Vector2(540, 60);
        playerEnergy.GetComponent<MinScreenFontSize>().MinPixelSize = 34f;

        // --- Vertical layout, stacked bottom-up so every zone's position is
        // derived from the one below it (never independently guessed) - the
        // old version shifted each zone by an independently-chosen amount
        // and that let the enemy row, log and hand overlap each other in
        // three different ways once the new bottom button (below) ate into
        // the space the old corner button never needed. Each zone here is
        // (bottom edge = previous top edge + gap), so gaps can never go
        // negative without it being obvious in the math.
        const float gap = 30f;
        const float endTurnButtonWidth = 440f;
        const float endTurnBottomMargin = 16f;
        float endTurnButtonHeight = endTurnButtonWidth / BarajaGemButtons.Aspect;
        float buttonTop = endTurnBottomMargin + endTurnButtonHeight;

        const float handHeight = 600f; // unchanged - the focused card needs its full scaled-up size
        float handBottom = buttonTop + gap;

        const float logHeight = 280f; // there was real unused vertical margin left over - use it
        // Narrower each time the played-card piles beside it grow to fit
        // bigger, more legible thumbnails (see PlayedCardStack.ThumbSize):
        // 640 -> 570 -> 430.
        const float logWidth = 430f;
        float logBottom = handBottom + handHeight + gap;
        float logTop = logBottom + logHeight;

        const float enemyHeight = 560f; // grown back to fit the bigger enemy card below (frameW 340 -> 380)
        float enemyBottom = logTop + gap;
        float enemyCenterY = enemyBottom + enemyHeight / 2f;

        // --- Enemy row, upper-middle ---
        GameObject enemyContainer = new GameObject("EnemyContainer");
        enemyContainer.transform.SetParent(canvasGO.transform, false);
        RectTransform enemyContainerRT = enemyContainer.AddComponent<RectTransform>();
        enemyContainerRT.anchorMin = new Vector2(0.5f, 0f);
        enemyContainerRT.anchorMax = new Vector2(0.5f, 0f);
        enemyContainerRT.pivot = new Vector2(0.5f, 0.5f);
        enemyContainerRT.anchoredPosition = new Vector2(0, enemyCenterY);
        enemyContainerRT.sizeDelta = new Vector2(1000, enemyHeight);
        HorizontalLayoutGroup enemyLayout = enemyContainer.AddComponent<HorizontalLayoutGroup>();
        enemyLayout.spacing = 40f;
        enemyLayout.childAlignment = TextAnchor.MiddleCenter;
        enemyLayout.childForceExpandWidth = false;
        enemyLayout.childForceExpandHeight = false;

        GameObject enemyPanelPrefab = MakeEnemyPanelPrefab(canvasGO.transform);

        // --- Log: its own band between the enemy row and the hand, so it never
        // shares screen space with the cards or the End Turn button. Bottom-
        // anchored now (was top-anchored with a negative offset) so it slots
        // into the same bottom-up stack as everything else - UpperCenter text
        // alignment still grows downward from the top of this box regardless
        // of how the box itself is anchored in its parent.
        // Narrower than it used to be (1000 -> 640) so the played-card piles
        // (below) have room to flank it left and right at the same band -
        // "player plays stack right of the action text, enemy plays stack
        // left of it" per Dave's spec.
        Text logText = MakeLogScrollView(canvasGO.transform, new Vector2(0, logBottom),
            new Vector2(logWidth, logHeight), out ScrollRect logScrollRect);
        logText.color = new Color(1f, 1f, 1f, 0.9f);
        logText.GetComponent<MinScreenFontSize>().MinPixelSize = 32f;

        // The piles used to anchor from the log's own top edge (logBandTop)
        // at a leftover 420-tall footprint sized for the OLD 3-card cascade
        // - PlayedCardStack only ever shows one card now (see its own
        // comments), but nothing shrank the box to match, so it quietly
        // overlapped down into the hand the entire time; bigger hand cards
        // just made it obvious. Sized and positioned from first principles
        // instead: centered in the actual gap between the hand's top edge
        // and the enemy row's bottom edge, with real margin on both sides.
        float handTop = handBottom + handHeight;
        const float pileMargin = 15f;
        const float pileHeight = 300f;
        const float pileWidth = 225f;
        float pileBottomY = handTop + pileMargin;
        float pileTopY = pileBottomY + pileHeight;
        float pileBandTop = -(1920f - pileTopY);
        if (pileTopY + pileMargin > enemyBottom)
            Debug.LogWarning($"Played-card pile top ({pileTopY:F0}) is too close to the enemy row " +
                              $"({enemyBottom:F0}) - shrink pileHeight or open up more vertical gap.");

        // --- End Turn button, a wide banner across the very bottom of the
        // screen - was a small gem tucked in the top-right corner (Dave:
        // "instead of the turd in the upper right corner"). Still the same
        // emerald-cut gem art (BarajaGemButtons.Aspect), just sized up -
        // the aspect has to stay fixed or the facets stretch/squash, so
        // "wide" necessarily means "taller too," not a thin strip. Kept
        // modest (440, not full-width) after an earlier pass came back
        // "HUGE" - this is close to StandardButtonWidth (460, used for
        // full-width menu buttons), not a screen-spanning bar.
        Button endTurnBtn = MakeButton(canvasGO.transform, "EndTurnButton", endTurnButtonWidth, "End Turn", 28, GemColor.Ruby);
        RectTransform endTurnRT = endTurnBtn.GetComponent<RectTransform>();
        endTurnRT.anchorMin = new Vector2(0.5f, 0f);
        endTurnRT.anchorMax = new Vector2(0.5f, 0f);
        endTurnRT.pivot = new Vector2(0.5f, 0f);
        endTurnRT.anchoredPosition = new Vector2(0, endTurnBottomMargin);

        // MakeButton's own child "Label" Text is exactly what this button
        // no longer wants - the gem and its label are now ONE baked
        // texture (BarajaButtonBaker), not an image with a separate text
        // object floating on top of it. Strip the throwaway label, swap in
        // the baked art, and let CombatUI pick EN/ES by swapping the
        // RawImage's texture (BeginFight()) instead of setting Text.text.
        Transform endTurnLabel = endTurnBtn.transform.Find("Label");
        if (endTurnLabel != null) Object.DestroyImmediate(endTurnLabel.gameObject);
        RawImage endTurnImage = endTurnBtn.GetComponent<RawImage>();
        Texture2D endTurnTextureEN = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Art/Buttons/end_turn_en.png");
        Texture2D endTurnTextureES = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Art/Buttons/end_turn_es.png");
        endTurnImage.texture = endTurnTextureEN;
        // Same fix as the hand cards: EndTurnSequence (CombatUI) now leaves
        // this non-interactable for a beat every turn while the enemy's
        // moves reveal - Unity's default disabled tint would fade the gem
        // toward transparent for that whole beat, which is the same "looks
        // overlaid, not solid" problem. It still blocks clicks while
        // disabled; it just no longer LOOKS different doing it.
        endTurnBtn.transition = Selectable.Transition.None;

        // --- Hand, bottom - a plain scrolling row. Was a coverflow
        // (HandCarousel): one continuously-scaling "focused" card, others
        // shrinking toward the edges, positions driven every frame by drag
        // delta. Went through three rounds of tuning it (overlap, drag
        // sensitivity, "cards should never leave the screen") and each one
        // either didn't fix the complaint or made it worse - the mechanism
        // itself was the problem, not the tuning. Every card is now the
        // same fixed size, laid out left to right with real gaps by a
        // HorizontalLayoutGroup, and independently tappable - no "focused"
        // card concept left to get subtly wrong. ---
        GameObject handContent = MakeHandScrollContainer(canvasGO.transform, "HandScroll",
            new Vector2(0, handBottom), new Vector2(1080, handHeight));

        GameObject cardButtonPrefab = MakeCardButtonPrefab(canvasGO.transform);

        // --- First-time tutorial, shown as a click-through banner over the
        // actual board (the full 8-page TutorialPages content, not just a
        // single message) - reading it here, next to the thing it's
        // describing, beats the old main-menu-only parchment overlay that
        // was divorced from any board to look at.
        const float hintButtonWidth = 400f;
        float hintButtonHeight = hintButtonWidth / BarajaGemButtons.Aspect;
        float hintPanelHeight = 20f + 56f + 14f + 220f + 14f + 40f + 14f + hintButtonHeight + 20f;
        GameObject hintPanel = new GameObject("TutorialHintPanel");
        hintPanel.transform.SetParent(canvasGO.transform, false);
        Image hintBg = hintPanel.AddComponent<Image>();
        hintBg.color = new Color(0.15f, 0.1f, 0.05f, 0.92f);
        RectTransform hintRT = hintBg.rectTransform;
        hintRT.anchorMin = new Vector2(0.5f, 1f);
        hintRT.anchorMax = new Vector2(0.5f, 1f);
        hintRT.pivot = new Vector2(0.5f, 1f);
        hintRT.anchoredPosition = new Vector2(0, -250); // clears the HP/Energy text below the title
        hintRT.sizeDelta = new Vector2(980, hintPanelHeight);

        Text hintTitle = MakeTitleText(hintPanel.transform, "HintTitleText", new Vector2(0, -20), TextAnchor.UpperCenter, 44);
        hintTitle.rectTransform.anchorMin = new Vector2(0.5f, 1f);
        hintTitle.rectTransform.anchorMax = new Vector2(0.5f, 1f);
        hintTitle.rectTransform.pivot = new Vector2(0.5f, 1f);
        hintTitle.rectTransform.sizeDelta = new Vector2(900, 56);
        hintTitle.GetComponent<MinScreenFontSize>().MinPixelSize = 36f;

        Text hintBody = MakeText(hintPanel.transform, "HintBodyText", new Vector2(0, -90), TextAnchor.UpperCenter, 36);
        hintBody.rectTransform.anchorMin = new Vector2(0.5f, 1f);
        hintBody.rectTransform.anchorMax = new Vector2(0.5f, 1f);
        hintBody.rectTransform.pivot = new Vector2(0.5f, 1f);
        hintBody.rectTransform.sizeDelta = new Vector2(900, 220);
        hintBody.GetComponent<MinScreenFontSize>().MinPixelSize = 30f;

        Text hintPageIndex = MakeText(hintPanel.transform, "HintPageIndexText", new Vector2(0, -338), TextAnchor.UpperCenter, 28);
        hintPageIndex.rectTransform.anchorMin = new Vector2(0.5f, 1f);
        hintPageIndex.rectTransform.anchorMax = new Vector2(0.5f, 1f);
        hintPageIndex.rectTransform.pivot = new Vector2(0.5f, 1f);
        hintPageIndex.rectTransform.sizeDelta = new Vector2(900, 40);
        hintPageIndex.color = new Color(1f, 1f, 1f, 0.6f);
        hintPageIndex.GetComponent<MinScreenFontSize>().MinPixelSize = 24f;

        // Small "X" in the corner - dismisses immediately from any page,
        // rather than requiring a click through all 8 pages via Next to
        // leave. Deliberately plain (not a gem) so it doesn't compete with
        // Back/Next for attention; it's a quick exit, not a primary action.
        GameObject hintCloseGO = new GameObject("HintCloseButton");
        hintCloseGO.transform.SetParent(hintPanel.transform, false);
        RectTransform hintCloseRT = hintCloseGO.AddComponent<RectTransform>();
        hintCloseRT.anchorMin = new Vector2(1f, 1f);
        hintCloseRT.anchorMax = new Vector2(1f, 1f);
        hintCloseRT.pivot = new Vector2(1f, 1f);
        hintCloseRT.anchoredPosition = new Vector2(-16, -16);
        hintCloseRT.sizeDelta = new Vector2(56, 56);
        Image hintCloseBg = hintCloseGO.AddComponent<Image>();
        hintCloseBg.color = new Color(1f, 1f, 1f, 0.12f);
        Button hintClose = hintCloseGO.AddComponent<Button>();
        hintClose.targetGraphic = hintCloseBg;

        Text hintCloseLabel = MakeTitleText(hintCloseGO.transform, "Label", Vector2.zero, TextAnchor.MiddleCenter, 32);
        hintCloseLabel.rectTransform.anchorMin = Vector2.zero;
        hintCloseLabel.rectTransform.anchorMax = Vector2.one;
        hintCloseLabel.rectTransform.offsetMin = Vector2.zero;
        hintCloseLabel.rectTransform.offsetMax = Vector2.zero;
        hintCloseLabel.text = "X";

        Button hintBack = MakeButton(hintPanel.transform, "HintBackButton", hintButtonWidth, "< Back", StandardButtonFontSize, GemColor.Silver);
        RectTransform hintBackRT = hintBack.GetComponent<RectTransform>();
        hintBackRT.anchorMin = new Vector2(0f, 0f);
        hintBackRT.anchorMax = new Vector2(0f, 0f);
        hintBackRT.pivot = new Vector2(0f, 0f);
        hintBackRT.anchoredPosition = new Vector2(20, 20);

        Button hintNext = MakeButton(hintPanel.transform, "HintNextButton", hintButtonWidth, "Next >", StandardButtonFontSize, GemColor.Ruby);
        RectTransform hintNextRT = hintNext.GetComponent<RectTransform>();
        hintNextRT.anchorMin = new Vector2(1f, 0f);
        hintNextRT.anchorMax = new Vector2(1f, 0f);
        hintNextRT.pivot = new Vector2(1f, 0f);
        hintNextRT.anchoredPosition = new Vector2(-20, 20);

        // --- Played-card piles: flank the log band, right for the player's
        // plays and left for the enemy's (mirrored) - two people across a
        // table turning cards face-up on their own side. pileBandTop/
        // pileWidth/pileHeight (above) size this to the actual available
        // gap, not a leftover from the old design. ---
        GameObject playedPileGO = new GameObject("PlayedCardPile");
        playedPileGO.transform.SetParent(canvasGO.transform, false);
        RectTransform playedPileRT = playedPileGO.AddComponent<RectTransform>();
        playedPileRT.anchorMin = new Vector2(1f, 1f);
        playedPileRT.anchorMax = new Vector2(1f, 1f);
        playedPileRT.pivot = new Vector2(1f, 1f);
        playedPileRT.anchoredPosition = new Vector2(-20, pileBandTop);
        playedPileRT.sizeDelta = new Vector2(pileWidth, pileHeight);
        PlayedCardStack playedPile = playedPileGO.AddComponent<PlayedCardStack>();

        GameObject enemyPlayedPileGO = new GameObject("EnemyPlayedCardPile");
        enemyPlayedPileGO.transform.SetParent(canvasGO.transform, false);
        RectTransform enemyPlayedPileRT = enemyPlayedPileGO.AddComponent<RectTransform>();
        enemyPlayedPileRT.anchorMin = new Vector2(0f, 1f);
        enemyPlayedPileRT.anchorMax = new Vector2(0f, 1f);
        enemyPlayedPileRT.pivot = new Vector2(0f, 1f);
        enemyPlayedPileRT.anchoredPosition = new Vector2(20, pileBandTop);
        enemyPlayedPileRT.sizeDelta = new Vector2(pileWidth, pileHeight);
        PlayedCardStack enemyPlayedPile = enemyPlayedPileGO.AddComponent<PlayedCardStack>();

        CombatTutorialHint hint = canvasGO.AddComponent<CombatTutorialHint>();
        hint.Panel = hintPanel;
        hint.TitleText = hintTitle;
        hint.BodyText = hintBody;
        hint.PageIndexText = hintPageIndex;
        hint.BackButton = hintBack;
        hint.BackButtonLabel = hintBack.GetComponentInChildren<Text>();
        hint.NextButton = hintNext;
        hint.NextButtonLabel = hintNext.GetComponentInChildren<Text>();
        hint.CloseButton = hintClose;

        // --- Floating combat text layer: added LAST so it's the topmost
        // sibling on the canvas and damage/block popups always render over
        // everything else (cards, enemy panels, the pile). Just a parent
        // transform - FloatingCombatText builds its own Text/Outline per
        // popup and self-destructs, nothing lives here between hits. ---
        GameObject floatingTextLayerGO = new GameObject("FloatingTextLayer");
        floatingTextLayerGO.transform.SetParent(canvasGO.transform, false);
        RectTransform floatingTextLayerRT = floatingTextLayerGO.AddComponent<RectTransform>();
        floatingTextLayerRT.anchorMin = Vector2.zero;
        floatingTextLayerRT.anchorMax = Vector2.one;
        floatingTextLayerRT.offsetMin = Vector2.zero;
        floatingTextLayerRT.offsetMax = Vector2.zero;

        // --- Manager + UI wiring ---
        GameObject managerGO = new GameObject("CombatManager");
        CombatManager manager = managerGO.AddComponent<CombatManager>();

        CombatUI ui = canvasGO.AddComponent<CombatUI>();
        ui.Manager = manager;
        ui.Spanish = true;
        ui.PlayerHpText = playerHp;
        ui.PlayerEnergyText = playerEnergy;
        ui.LogText = logText;
        ui.LogScrollRect = logScrollRect;
        ui.EndTurnButton = endTurnBtn;
        ui.EndTurnButtonImage = endTurnImage;
        ui.EndTurnTextureEN = endTurnTextureEN;
        ui.EndTurnTextureES = endTurnTextureES;
        ui.EnemyContainer = enemyContainer.transform;
        ui.EnemyPanelPrefab = enemyPanelPrefab;
        ui.HandContainer = handContent.transform;
        ui.HandScrollRect = handContent.GetComponentInParent<ScrollRect>();
        ui.CardButtonPrefab = cardButtonPrefab;
        ui.FloatingTextLayer = floatingTextLayerGO.transform;
        ui.PlayedPile = playedPile;
        ui.EnemyPlayedPile = enemyPlayedPile;

        CombatBootstrap bootstrap = canvasGO.AddComponent<CombatBootstrap>();
        bootstrap.Ui = ui;
        bootstrap.FightNumber = 1;

        string scenePath = "Assets/Scenes/Combat.unity";
        System.IO.Directory.CreateDirectory("Assets/Scenes");
        EditorSceneManager.SaveScene(scene, scenePath);
        BarajaBuildScenes.Register(scenePath, 1);

        Debug.Log("Baraja combat scene built successfully at " + scenePath);
    }

    // Enemy cards are now the same committed silver-bordered frame as the
    // player deck (card_frame_v14_silver_zero_margin.png via
    // build_all_enemy_cards.py), on a 736x1040 canvas. That script bakes the
    // frame, bust, name, cost-gem, and medallion, but intentionally leaves
    // the band below the title blank - HP, Block, and Intent are all
    // per-turn dynamic, so they're drawn live here instead of baked.
    // Fractions below are measured directly against that frame: title block
    // is centered on y=624 and can run 2 lines (e.g. "Guardian de
    // Ofrenda"/"Offering Guardian" both wrap at the title's fixed 50pt size),
    // worst-case bottom ~681 - TextTop starts comfortably after that. The
    // medallion ring's outer rim starts at y=897 (measured the same way
    // build_all_enemy_cards.py measures MEDALLION_CY/SOCKET_R) - TextTop and
    // MedallionTop together give this text ~90px of vertical room, not the
    // ~1px margin the old frame's geometry left.
    const float TextLeft = 108f / 736f, TextRight = 628f / 736f;
    const float TextTop = 720f / 1040f;
    const float MedallionTop = 897f / 1040f;

    // Inactive template instantiated per-enemy by CombatUI; a full card, the
    // same size as a hand card.
    static GameObject MakeEnemyPanelPrefab(Transform parent)
    {
        GameObject go = new GameObject("EnemyPanelPrefab");
        go.transform.SetParent(parent, false);
        RectTransform rt = go.AddComponent<RectTransform>();
        // Was 340 (used to match the hand card's own size before the hand
        // grew to 408) - grown again now that enemyHeight has the room,
        // same "there's unused space, use it" call as the hand and the log.
        float frameW = 380f;
        float frameH = frameW * 1040f / 736f; // matches the card art's real aspect ratio exactly
        rt.sizeDelta = new Vector2(frameW, frameH);
        Button btn = go.AddComponent<Button>();

        GameObject imgGO = new GameObject("Image");
        imgGO.transform.SetParent(go.transform, false);
        RawImage img = imgGO.AddComponent<RawImage>();
        RectTransform imgRT = img.rectTransform;
        imgRT.anchorMin = Vector2.zero;
        imgRT.anchorMax = Vector2.one;
        imgRT.offsetMin = Vector2.zero;
        imgRT.offsetMax = Vector2.zero;
        btn.targetGraphic = img;

        // HP/Block and Intent stacked inside the baked card's blank text
        // band, sized to leave real clearance above the medallion ring
        // rather than just filling the band's nominal height - the two
        // lines together must end comfortably before MedallionTop, with a
        // margin, or the text visually crowds the ring/skull below it.
        float textBandTop = TextTop * frameH;
        float medallionTopY = MedallionTop * frameH;
        // The new frame's blank band between the title and the medallion is
        // far roomier than the old one (~90px vs. the old ~51px total), so
        // this is comfortable rather than squeezed to the ceiling.
        const float topPad = 2f, safetyMargin = 1f, lineH = 27f;

        Text hpText = MakeText(go.transform, "HpText", Vector2.zero, TextAnchor.UpperCenter, 21);
        hpText.font = BarajaFonts.BodySemiBold;
        hpText.fontStyle = FontStyle.Bold;
        hpText.rectTransform.anchorMin = new Vector2(0f, 1f);
        hpText.rectTransform.anchorMax = new Vector2(0f, 1f);
        hpText.rectTransform.pivot = new Vector2(0f, 1f);
        hpText.rectTransform.anchoredPosition = new Vector2(TextLeft * frameW, -(textBandTop + topPad));
        hpText.rectTransform.sizeDelta = new Vector2((TextRight - TextLeft) * frameW, lineH);
        hpText.GetComponent<MinScreenFontSize>().MinPixelSize = 20f;

        Text intentText = MakeText(go.transform, "IntentText", Vector2.zero, TextAnchor.UpperCenter, 20);
        intentText.font = BarajaFonts.BodySemiBold;
        intentText.fontStyle = FontStyle.Bold;
        intentText.color = new Color(1f, 0.75f, 0.3f);
        intentText.rectTransform.anchorMin = new Vector2(0f, 1f);
        intentText.rectTransform.anchorMax = new Vector2(0f, 1f);
        intentText.rectTransform.pivot = new Vector2(0f, 1f);
        intentText.rectTransform.anchoredPosition = new Vector2(TextLeft * frameW, -(textBandTop + topPad + lineH));
        intentText.rectTransform.sizeDelta = new Vector2((TextRight - TextLeft) * frameW, lineH);
        intentText.GetComponent<MinScreenFontSize>().MinPixelSize = 19f;

        // Guard rail: fail loudly at build time rather than silently
        // shipping overlap if the frame size or geometry ever changes.
        float textBottom = textBandTop + topPad + lineH * 2f;
        if (textBottom > medallionTopY - safetyMargin)
            Debug.LogWarning($"Enemy card HP/Intent text ({textBottom:F0}px) is too close to the " +
                              $"medallion ({medallionTopY:F0}px) - shrink lineH or the font sizes.");

        go.SetActive(false);
        return go;
    }

    // Inactive template instantiated per-card-in-hand by CombatUI.
    static GameObject MakeCardButtonPrefab(Transform parent)
    {
        GameObject go = new GameObject("CardButtonPrefab");
        go.transform.SetParent(parent, false);
        RectTransform rt = go.AddComponent<RectTransform>();
        // 419x592 matches the card art's real 736:1040 aspect ratio exactly
        // (RawImage has no preserveAspect option, unlike Image, so this has
        // to be exact or the art stretches) - was 340x480, then 408x576,
        // now nearly filling the hand's own 600-tall band edge to edge
        // (Dave's repeated call: bigger, and the baked-in card text grows
        // right along with the card since it's part of the same texture).
        // The hand scrolls horizontally for the rest rather than shrinking
        // cards to force more into one screen.
        rt.sizeDelta = new Vector2(419, 592);
        // Point anchor (center/center) - the standard, simplest anchor
        // setup for a HorizontalLayoutGroup child; the layout group drives
        // anchoredPosition itself every rebuild, so this only matters for
        // where the RectTransform's own origin sits within its own bounds.
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        RawImage img = go.AddComponent<RawImage>();
        Button btn = go.AddComponent<Button>();
        btn.targetGraphic = img;

        // Unity's default ColorBlock.disabledColor is ~60% alpha - cards
        // need to read as solid objects even when unaffordable, not fade
        // toward see-through. Turning off automatic color tinting entirely
        // avoids that (also a harder guarantee against any color-fade
        // flicker than fadeDuration ever was, since no tint ever applies at
        // all now). CardHandEntry.CanPlay drives the one visual state that
        // still needs to show - "you can't afford this one" - directly via
        // Graphic.color, opaque grey, not through Button's transition.
        btn.transition = Selectable.Transition.None;

        go.SetActive(false);
        return go;
    }

    // Body font (Crimson Text) by default - MakeTitleText/MakeButton switch
    // to the decorative Cinzel Decorative face for headers and button labels.
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
        // Default legibility floor for every Text this game creates - see
        // MinScreenFontSize. Callers that want a different minimum grab the
        // component back off the returned Text and override MinPixelSize;
        // nothing needs to opt in by hand.
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

        // Buttons always use the decorative title face - short, high-emphasis
        // labels are exactly where an engraved-plaque look reads best.
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

    // Combat log as a vertical ScrollRect instead of a plain Text with a
    // fixed box: the old version just kept growing past its own box height
    // as more lines wrapped, spilling text down behind the hand cards -
    // clamped only by an entry COUNT cap (3), which said nothing about how
    // many actual wrapped visual lines that turned into at this font/width.
    // A masked viewport can't overflow onto anything else no matter how
    // much text there is; CombatUI scrolls it to the bottom every time a
    // line is added, so the newest text is always the one in view and
    // older lines scroll up out of sight - and clears it back to empty at
    // the start of every round (Dave's call - a log that resets per turn,
    // not one that accumulates the whole fight).
    static Text MakeLogScrollView(Transform parent, Vector2 anchoredPos, Vector2 size, out ScrollRect scrollRect)
    {
        GameObject scrollGO = new GameObject("LogScroll");
        scrollGO.transform.SetParent(parent, false);
        RectTransform scrollRT = scrollGO.AddComponent<RectTransform>();
        scrollRT.anchorMin = new Vector2(0.5f, 0f);
        scrollRT.anchorMax = new Vector2(0.5f, 0f);
        scrollRT.pivot = new Vector2(0.5f, 0f);
        scrollRT.anchoredPosition = anchoredPos;
        scrollRT.sizeDelta = size;
        scrollRect = scrollGO.AddComponent<ScrollRect>();
        scrollRect.horizontal = false;
        scrollRect.vertical = true;
        scrollRect.movementType = ScrollRect.MovementType.Clamped;
        scrollRect.inertia = false; // a log, not a hand of cards - no momentum feel needed

        GameObject viewportGO = new GameObject("Viewport");
        viewportGO.transform.SetParent(scrollGO.transform, false);
        RectTransform viewportRT = viewportGO.AddComponent<RectTransform>();
        viewportRT.anchorMin = Vector2.zero;
        viewportRT.anchorMax = Vector2.one;
        viewportRT.offsetMin = Vector2.zero;
        viewportRT.offsetMax = Vector2.zero;
        viewportGO.AddComponent<RectMask2D>();

        Text logText = MakeText(viewportGO.transform, "LogText", Vector2.zero, TextAnchor.UpperCenter, 44);
        logText.rectTransform.anchorMin = new Vector2(0.5f, 1f);
        logText.rectTransform.anchorMax = new Vector2(0.5f, 1f);
        logText.rectTransform.pivot = new Vector2(0.5f, 1f);
        logText.rectTransform.sizeDelta = new Vector2(size.x, 0f); // height driven by the fitter below
        logText.lineSpacing = 0.85f; // "too spaced out" per Dave - default line height read as too loose
        ContentSizeFitter fitter = logText.gameObject.AddComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        scrollRect.viewport = viewportRT;
        scrollRect.content = logText.rectTransform;

        return logText;
    }

    // A plain horizontally scrolling row: returns the Content object callers
    // should parent cards into. Standard Unity ScrollRect/Viewport/Content -
    // a HorizontalLayoutGroup + ContentSizeFitter stacks cards left to right
    // with real gaps and grows Content sideways as more are added; the
    // Viewport's RectMask2D clips whatever doesn't fit, and ScrollRect
    // itself handles dragging to see the rest (clamped - no rubber-band
    // past either end, no wrap). Replaced HandCarousel (a hand-rolled
    // coverflow: one continuously-scaling "focused" card, positions driven
    // every frame from a drag delta) after three rounds of tuning it still
    // couldn't satisfy "no overlap" and "cards never leave the screen" at
    // the same time - every card here is simply the same fixed size and
    // independently tappable, so neither question can even come up.
    static GameObject MakeHandScrollContainer(Transform parent, string name, Vector2 anchoredPos, Vector2 size)
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
        scrollRect.movementType = ScrollRect.MovementType.Clamped;
        scrollRect.inertia = true;

        GameObject viewportGO = new GameObject("Viewport");
        viewportGO.transform.SetParent(scrollGO.transform, false);
        RectTransform viewportRT = viewportGO.AddComponent<RectTransform>();
        viewportRT.anchorMin = Vector2.zero;
        viewportRT.anchorMax = Vector2.one;
        viewportRT.offsetMin = Vector2.zero;
        viewportRT.offsetMax = Vector2.zero;
        // RectMask2D (unlike Mask) needs no Graphic of its own to clip -
        // nothing to see here, just a clip boundary.
        viewportGO.AddComponent<RectMask2D>();

        GameObject contentGO = new GameObject("Content");
        contentGO.transform.SetParent(viewportGO.transform, false);
        RectTransform contentRT = contentGO.AddComponent<RectTransform>();
        contentRT.anchorMin = new Vector2(0f, 0.5f);
        contentRT.anchorMax = new Vector2(0f, 0.5f);
        contentRT.pivot = new Vector2(0f, 0.5f);
        contentRT.sizeDelta = new Vector2(0f, size.y); // width driven by ContentSizeFitter below
        HorizontalLayoutGroup layout = contentGO.AddComponent<HorizontalLayoutGroup>();
        layout.childAlignment = TextAnchor.MiddleLeft;
        layout.spacing = 24f; // real gap - cards never touch, let alone overlap
        layout.padding = new RectOffset(20, 20, 0, 0);
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = false;
        layout.childControlWidth = false;
        layout.childControlHeight = false;
        ContentSizeFitter fitter = contentGO.AddComponent<ContentSizeFitter>();
        fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;

        scrollRect.viewport = viewportRT;
        scrollRect.content = contentRT;

        return contentGO;
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
}

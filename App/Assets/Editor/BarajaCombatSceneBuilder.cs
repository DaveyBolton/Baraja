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

        // --- Hand, bottom ---
        GameObject handContainer = new GameObject("HandContainer");
        handContainer.transform.SetParent(canvasGO.transform, false);
        RectTransform handRT = handContainer.AddComponent<RectTransform>();
        handRT.anchorMin = new Vector2(0.5f, 0f);
        handRT.anchorMax = new Vector2(0.5f, 0f);
        handRT.pivot = new Vector2(0.5f, 0f);
        handRT.anchoredPosition = new Vector2(0, 20);
        handRT.sizeDelta = new Vector2(1080, 300);
        HorizontalLayoutGroup handLayout = handContainer.AddComponent<HorizontalLayoutGroup>();
        handLayout.spacing = 16f;
        handLayout.childAlignment = TextAnchor.LowerCenter;
        handLayout.childForceExpandWidth = false;
        handLayout.childForceExpandHeight = false;

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
        ui.HandContainer = handContainer.transform;
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

    // Inactive template instantiated per-enemy by CombatUI; never itself part of the live layout.
    static GameObject MakeEnemyPanelPrefab(Transform parent)
    {
        GameObject go = new GameObject("EnemyPanelPrefab");
        go.transform.SetParent(parent, false);
        RectTransform rt = go.AddComponent<RectTransform>();
        rt.sizeDelta = new Vector2(320, 460);
        Image bg = go.AddComponent<Image>();
        bg.color = new Color(0, 0, 0, 0.25f);
        Button btn = go.AddComponent<Button>();
        btn.targetGraphic = bg;

        GameObject imgGO = new GameObject("Image");
        imgGO.transform.SetParent(go.transform, false);
        RawImage img = imgGO.AddComponent<RawImage>();
        RectTransform imgRT = img.rectTransform;
        imgRT.anchorMin = new Vector2(0.5f, 1f);
        imgRT.anchorMax = new Vector2(0.5f, 1f);
        imgRT.pivot = new Vector2(0.5f, 1f);
        imgRT.anchoredPosition = new Vector2(0, 0);
        imgRT.sizeDelta = new Vector2(300, 300);

        Text hpText = MakeText(go.transform, "HpText", new Vector2(0, -310), TextAnchor.UpperCenter, 26);
        hpText.rectTransform.anchorMin = new Vector2(0.5f, 1f);
        hpText.rectTransform.anchorMax = new Vector2(0.5f, 1f);
        hpText.rectTransform.pivot = new Vector2(0.5f, 1f);
        hpText.rectTransform.sizeDelta = new Vector2(300, 80);

        Text intentText = MakeText(go.transform, "IntentText", new Vector2(0, -395), TextAnchor.UpperCenter, 24);
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
        // 198x280 matches the card art's real 736x1040 aspect ratio (RawImage has
        // no preserveAspect option, unlike Image, so this has to be exact or the
        // art stretches). Sized so a full 5-card hand (198*5 + 16*4 spacing =
        // 1054px) fits inside the 1080-wide reference resolution.
        rt.sizeDelta = new Vector2(198, 280);
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

    static void AnchorTop(RectTransform rt)
    {
        rt.anchorMin = new Vector2(0.5f, 1f);
        rt.anchorMax = new Vector2(0.5f, 1f);
        rt.pivot = new Vector2(0.5f, 1f);
    }
}

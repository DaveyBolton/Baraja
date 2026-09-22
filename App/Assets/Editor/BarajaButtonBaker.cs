using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Bakes a gem button + its label into ONE flattened texture - for End Turn,
/// which used to be a RawImage (gem) with a separate child Text floating on
/// top of it, two independent objects rather than a single graphic. Renders
/// with Unity's own UI/Canvas/Camera pipeline (a temporary World Space
/// canvas photographed by an orthographic camera into a RenderTexture), not
/// an external image tool. Run via:
/// Unity -batchmode -executeMethod BarajaButtonBaker.BakeEndTurnButtons -quit
/// Needs REAL rendering (no -nographics) - Camera.Render() produces nothing
/// useful with the graphics device disabled.
/// </summary>
public static class BarajaButtonBaker
{
    public static void BakeEndTurnButtons()
    {
        // Native resolution of the gem art itself (BarajaGemButtons.Aspect
        // = 700/315) so the bake is never upscaling past source quality.
        Bake("End Turn", "Assets/Art/Buttons/end_turn_en.png");
        Bake("Fin de Turno", "Assets/Art/Buttons/end_turn_es.png");
    }

    private static void Bake(string label, string outputPath)
    {
        const int texWidth = 700, texHeight = 315;

        var canvasGO = new GameObject("BakeCanvas");
        var canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        var canvasRT = canvasGO.GetComponent<RectTransform>();
        canvasRT.sizeDelta = new Vector2(texWidth, texHeight);
        canvasGO.transform.position = Vector3.zero;
        canvasGO.transform.rotation = Quaternion.identity;
        canvasGO.transform.localScale = Vector3.one;

        var camGO = new GameObject("BakeCamera");
        var cam = camGO.AddComponent<Camera>();
        cam.orthographic = true;
        cam.orthographicSize = texHeight / 2f;
        cam.aspect = (float)texWidth / texHeight;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0f, 0f, 0f, 0f);
        cam.nearClipPlane = 0.1f;
        cam.farClipPlane = 20f;
        camGO.transform.position = new Vector3(0f, 0f, -10f);
        camGO.transform.LookAt(Vector3.zero, Vector3.up);
        canvas.worldCamera = cam;

        var rtex = new RenderTexture(texWidth, texHeight, 24, RenderTextureFormat.ARGB32)
        {
            antiAliasing = 4
        };
        cam.targetTexture = rtex;

        // Gem background, filling the whole bake canvas.
        var gemGO = new GameObject("Gem");
        gemGO.transform.SetParent(canvasGO.transform, false);
        var gemImg = gemGO.AddComponent<RawImage>();
        gemImg.texture = BarajaGemButtons.Get(GemColor.Ruby);
        gemImg.rectTransform.sizeDelta = new Vector2(texWidth, texHeight);
        gemImg.rectTransform.anchoredPosition = Vector2.zero;

        // Label - roughly DOUBLE the old live-overlay's relative size (it
        // was 28pt against a 440-wide button, ~0.064x the width; baked here
        // at ~0.13x this 700-wide canvas, i.e. ~90pt).
        var textGO = new GameObject("Label");
        textGO.transform.SetParent(canvasGO.transform, false);
        var text = textGO.AddComponent<Text>();
        text.font = BarajaFonts.Title;
        text.fontSize = 90;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = Color.white;
        text.horizontalOverflow = HorizontalWrapMode.Wrap;
        text.verticalOverflow = VerticalWrapMode.Truncate;
        text.resizeTextForBestFit = true;
        text.resizeTextMinSize = 40;
        text.resizeTextMaxSize = 90;
        text.text = label;
        // Inset from the canvas edges - "FIN DE TURNO" (longer than "End
        // Turn") was rendering right to the pixel edge, clipped by the
        // gem's own beveled corners. Best Fit shrinks to whatever fits
        // this narrower box instead of overflowing past it.
        text.rectTransform.sizeDelta = new Vector2(texWidth - 140, texHeight - 60);
        text.rectTransform.anchoredPosition = Vector2.zero;
        var outline = textGO.AddComponent<Outline>();
        outline.effectColor = new Color(0f, 0f, 0f, 0.9f);
        outline.effectDistance = new Vector2(3f, -3f);

        Canvas.ForceUpdateCanvases();
        cam.Render();

        var prevActive = RenderTexture.active;
        RenderTexture.active = rtex;
        var outTex = new Texture2D(texWidth, texHeight, TextureFormat.RGBA32, false);
        outTex.ReadPixels(new Rect(0, 0, texWidth, texHeight), 0, 0);
        outTex.Apply();
        RenderTexture.active = prevActive;

        System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(outputPath) ?? ".");
        System.IO.File.WriteAllBytes(outputPath, outTex.EncodeToPNG());
        AssetDatabase.ImportAsset(outputPath);

        cam.targetTexture = null;
        rtex.Release();
        Object.DestroyImmediate(canvasGO);
        Object.DestroyImmediate(camGO);
        Object.DestroyImmediate(rtex);
        Object.DestroyImmediate(outTex);

        Debug.Log("BarajaButtonBaker: baked " + outputPath);
    }
}

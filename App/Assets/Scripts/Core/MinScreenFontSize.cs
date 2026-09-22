using UnityEngine;
using UnityEngine.UI;

namespace Baraja.Core
{
    // CanvasScaler (ScaleWithScreenSize, match-height, 1080x1920 reference)
    // shrinks EVERYTHING - art, layout, and text - in lockstep with the
    // window. On a normal-height window that's fine, but on a short window
    // (a small monitor, or just a small test screen - confirmed on Dave's
    // own machine: a 720px-tall screen gives a 0.375 scale factor, so a
    // "19pt" label renders at 7 actual pixels) text becomes unreadable
    // well before anything else does, because there is no minimum -
    // scale-with-screen has no floor.
    //
    // This keeps a Text's REAL on-screen pixel size from dropping below
    // MinPixelSize regardless of canvas scale, while leaving it fully
    // proportional (normal designed size) on any screen tall enough that
    // the designed size already clears the floor. It never shrinks text
    // below its designed size, only grows it when the canvas has been
    // scaled down hard enough to make it go blurry-small.
    [RequireComponent(typeof(Text))]
    public class MinScreenFontSize : MonoBehaviour
    {
        public float MinPixelSize = 22f;

        private Text _text;
        private int _designFontSize;
        private int _designMinSize;
        private int _designMaxSize;
        private Canvas _rootCanvas;
        private float _lastScale = -1f;

        private void Awake()
        {
            _text = GetComponent<Text>();
            _designFontSize = _text.fontSize;
            _designMinSize = _text.resizeTextMinSize;
            _designMaxSize = _text.resizeTextMaxSize;
            _rootCanvas = _text.canvas != null ? _text.canvas.rootCanvas : null;
        }

        // Cheap to poll every frame - just a scale compare, no allocation -
        // and resizing the window live (this build supports that) has to
        // be caught, not just the initial launch size.
        private void LateUpdate()
        {
            if (_rootCanvas == null) return;
            float scale = _rootCanvas.scaleFactor;
            if (Mathf.Approximately(scale, _lastScale)) return;
            _lastScale = scale;

            int floorSize = Mathf.CeilToInt(MinPixelSize / Mathf.Max(scale, 0.0001f));

            // resizeTextForBestFit picks its own rendered size every layout
            // pass from resizeTextMinSize/MaxSize - writing fontSize directly
            // (the non-BestFit path below) has no effect on it at all, so
            // the floor has to raise the MIN bound it fits within instead.
            if (_text.resizeTextForBestFit)
            {
                _text.resizeTextMinSize = Mathf.Max(_designMinSize, floorSize);
                _text.resizeTextMaxSize = Mathf.Max(_designMaxSize, _text.resizeTextMinSize);
            }
            else
            {
                _text.fontSize = Mathf.Max(_designFontSize, floorSize);
            }
        }
    }
}

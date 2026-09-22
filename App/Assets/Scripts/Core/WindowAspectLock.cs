using System;
using System.Collections;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Baraja.Core
{
    // The standalone Windows build is resizable for desktop testing (mobile
    // builds are always fullscreen at device resolution, so this never runs
    // there). Two things this owns:
    //
    // 1. Launch size: NOT the old fixed 540x960 - opens windowed (never
    //    exclusive fullscreen) sized to the tallest height that fits the
    //    user's screen (their monitor's work area, i.e. excluding the
    //    taskbar), with width computed to keep the same 9:16 the portrait
    //    CanvasScaler (1080x1920) already assumes. Centered on screen.
    //
    // 2. Live resize lock: a first version corrected Screen.width/height
    //    AFTER the fact (Screen.SetResolution in Update once a size change
    //    was noticed). That cannot actually hold a live drag: Windows
    //    keeps sending new sizes based on the mouse position every
    //    message, faster than a once-per-frame script correction can catch
    //    up to, and Screen.SetResolution in windowed mode doesn't reliably
    //    move the actual OS window frame in the first place. Genuinely
    //    locking a live drag means intercepting WM_SIZING - the message
    //    Windows sends WHILE the user is dragging an edge, with the
    //    proposed rectangle still mutable - and clamping it before Windows
    //    ever finalizes that size. That needs subclassing the window's own
    //    WndProc via SetWindowLongPtr; there is no Unity API for either of
    //    these, hence the raw Win32 interop below. Falls back to
    //    after-the-fact correction if the hook can't be installed for any
    //    reason, so a resize is never left completely uncorrected.
    public static class WindowAspectLock
    {
        private const float TargetAspect = 9f / 16f; // width / height

        [RuntimeInitializeOnLoadMethod]
        static void Install()
        {
#if UNITY_STANDALONE_WIN
            var go = new GameObject("WindowAspectLock");
            go.AddComponent<Hook>();
            UnityEngine.Object.DontDestroyOnLoad(go);
#endif
        }

#if UNITY_STANDALONE_WIN
        private class Hook : MonoBehaviour
        {
            private const int GWLP_WNDPROC = -4;
            private const uint WM_SIZING = 0x0214;
            private const int WMSZ_LEFT = 1, WMSZ_RIGHT = 2, WMSZ_TOP = 3,
                              WMSZ_TOPLEFT = 4, WMSZ_TOPRIGHT = 5,
                              WMSZ_BOTTOM = 6, WMSZ_BOTTOMLEFT = 7, WMSZ_BOTTOMRIGHT = 8;
            private const uint SPI_GETWORKAREA = 0x0030;
            private const uint SWP_NOZORDER = 0x0004, SWP_NOACTIVATE = 0x0010;

            [DllImport("user32.dll", EntryPoint = "SetWindowLongPtrW")]
            private static extern IntPtr SetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

            [DllImport("user32.dll", EntryPoint = "CallWindowProcW")]
            private static extern IntPtr CallWindowProc(IntPtr lpPrevWndFunc, IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

            [DllImport("user32.dll")]
            private static extern IntPtr GetActiveWindow();

            [DllImport("user32.dll")]
            private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

            [DllImport("user32.dll")]
            private static extern bool GetClientRect(IntPtr hWnd, out RECT lpRect);

            [DllImport("user32.dll", EntryPoint = "SystemParametersInfoW")]
            private static extern bool SystemParametersInfo(uint uiAction, uint uiParam, ref RECT pvParam, uint fWinIni);

            [DllImport("user32.dll")]
            private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

            [StructLayout(LayoutKind.Sequential)]
            private struct RECT { public int Left, Top, Right, Bottom; }

            private delegate IntPtr WndProcDelegate(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

            private WndProcDelegate _newProc; // kept as a field - GC must never collect this, it's a live native callback
            private IntPtr _oldProc;
            private IntPtr _hwnd;
            private bool _hooked;

            // Fallback watcher state, only used if the hook never installs.
            private int _lastWidth, _lastHeight;
            private float _retryTimer;

            private void Start()
            {
                _lastWidth = Screen.width;
                _lastHeight = Screen.height;
                TryInstallHook();
            }

            private void TryInstallHook()
            {
                _hwnd = GetActiveWindow();
                if (_hwnd == IntPtr.Zero) return; // window not created yet - retried from Update

                MaximizeHeightKeepingAspect();

                _newProc = WndProc;
                IntPtr newProcPtr = Marshal.GetFunctionPointerForDelegate(_newProc);
                _oldProc = SetWindowLongPtr(_hwnd, GWLP_WNDPROC, newProcPtr);
                _hooked = _oldProc != IntPtr.Zero;
                Debug.Log(_hooked
                    ? "WindowAspectLock: WM_SIZING hook installed - resize is aspect-locked live"
                    : "WindowAspectLock: hook install FAILED, falling back to after-the-fact correction");
            }

            // Opens as tall as the screen's own work area (monitor minus
            // taskbar) allows, width derived from that height to keep
            // 9:16 - not the old fixed 540x960. Runs once, every launch,
            // so it overrides whatever size Unity itself remembered from
            // a previous session (Unity persists window size to
            // PlayerPrefs automatically).
            private void MaximizeHeightKeepingAspect()
            {
                var workArea = new RECT();
                if (!SystemParametersInfo(SPI_GETWORKAREA, 0, ref workArea, 0)) return;
                int workWidth = workArea.Right - workArea.Left;
                int workHeight = workArea.Bottom - workArea.Top;
                if (workWidth <= 0 || workHeight <= 0) return;

                // Screen.SetResolution sets the CLIENT area, not the outer
                // window-with-chrome rect - without subtracting the title
                // bar/border size, the full window would be taller than
                // the work area and get clipped behind the taskbar.
                GetWindowRect(_hwnd, out RECT outer);
                GetClientRect(_hwnd, out RECT client);
                int chromeWidth = (outer.Right - outer.Left) - (client.Right - client.Left);
                int chromeHeight = (outer.Bottom - outer.Top) - (client.Bottom - client.Top);

                int targetClientHeight = workHeight - chromeHeight;
                int targetClientWidth = Mathf.RoundToInt(targetClientHeight * TargetAspect);
                if (targetClientWidth + chromeWidth > workWidth)
                {
                    targetClientWidth = workWidth - chromeWidth;
                    targetClientHeight = Mathf.RoundToInt(targetClientWidth / TargetAspect);
                }

                Screen.SetResolution(targetClientWidth, targetClientHeight, FullScreenMode.Windowed);

                int outerWidth = targetClientWidth + chromeWidth;
                int outerHeight = targetClientHeight + chromeHeight;
                int x = workArea.Left + (workWidth - outerWidth) / 2;
                int y = workArea.Top + Mathf.Max(0, (workHeight - outerHeight) / 2);
                StartCoroutine(CenterAfterResizeApplies(x, y, outerWidth, outerHeight));
            }

            // Screen.SetResolution doesn't take effect until a frame or
            // two later, so centering has to wait until Screen.width
            // actually reflects the requested size, or SetWindowPos would
            // just reposition the window at its OLD size.
            private IEnumerator CenterAfterResizeApplies(int x, int y, int expectedOuterWidth, int expectedOuterHeight)
            {
                for (int i = 0; i < 10; i++)
                {
                    yield return null;
                    GetWindowRect(_hwnd, out RECT outer);
                    int currentOuterWidth = outer.Right - outer.Left;
                    if (Mathf.Abs(currentOuterWidth - expectedOuterWidth) <= 2)
                    {
                        SetWindowPos(_hwnd, IntPtr.Zero, x, y, expectedOuterWidth, expectedOuterHeight, SWP_NOZORDER | SWP_NOACTIVATE);
                        yield break;
                    }
                }
            }

            private IntPtr WndProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam)
            {
                if (msg == WM_SIZING)
                {
                    var rect = Marshal.PtrToStructure<RECT>(lParam);
                    int edge = wParam.ToInt32();
                    int width = rect.Right - rect.Left;
                    int height = rect.Bottom - rect.Top;

                    switch (edge)
                    {
                        case WMSZ_LEFT:
                        case WMSZ_RIGHT:
                            rect.Bottom = rect.Top + Mathf.RoundToInt(width / TargetAspect);
                            break;
                        case WMSZ_TOP:
                        case WMSZ_BOTTOM:
                            rect.Right = rect.Left + Mathf.RoundToInt(height * TargetAspect);
                            break;
                        case WMSZ_TOPLEFT:
                        case WMSZ_TOPRIGHT:
                            rect.Top = rect.Bottom - Mathf.RoundToInt(width / TargetAspect);
                            break;
                        case WMSZ_BOTTOMLEFT:
                        case WMSZ_BOTTOMRIGHT:
                            rect.Bottom = rect.Top + Mathf.RoundToInt(width / TargetAspect);
                            break;
                    }

                    Marshal.StructureToPtr(rect, lParam, true);
                    return new IntPtr(1); // non-zero: "I adjusted the rect, use it as-is"
                }

                return CallWindowProc(_oldProc, hWnd, msg, wParam, lParam);
            }

            private void Update()
            {
                if (!_hooked)
                {
                    // Retry installing every ~0.5s in case the window
                    // handle wasn't ready on the very first frame, and run
                    // the old after-the-fact correction meanwhile so a
                    // resize is never left completely unconstrained.
                    _retryTimer += Time.unscaledDeltaTime;
                    if (_retryTimer > 0.5f) { _retryTimer = 0f; TryInstallHook(); }
                    RunFallbackCorrection();
                }
            }

            private void RunFallbackCorrection()
            {
                if (Screen.fullScreen) return;
                if (Screen.width == _lastWidth && Screen.height == _lastHeight) return;

                int width = Screen.width;
                int height = Mathf.Max(1, Mathf.RoundToInt(width / TargetAspect));
                if (height != Screen.height) Screen.SetResolution(width, height, FullScreenMode.Windowed);
                _lastWidth = width;
                _lastHeight = height;
            }

            private void OnDestroy()
            {
                if (_hooked && _hwnd != IntPtr.Zero)
                    SetWindowLongPtr(_hwnd, GWLP_WNDPROC, _oldProc);
            }
        }
#endif
    }
}

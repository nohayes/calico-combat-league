using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

public static class GameBootstrap
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Initialize()
    {
        Application.targetFrameRate = 60;
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        var gmGo = new GameObject("GameManager");
        gmGo.AddComponent<GameManager>();

        var audioGo = new GameObject("AudioManager");
        audioGo.AddComponent<AudioManager>();

        var esGo = new GameObject("EventSystem");
        esGo.AddComponent<EventSystem>();
        esGo.AddComponent<InputSystemUIInputModule>();

        var canvasGo = new GameObject("Canvas", typeof(RectTransform));
        var canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        // Landscape Conversion (Milestone 26): 1920x1080 is now the primary
        // design reference (was 1080x1920 portrait). Every screen's anchors are
        // normalized 0-1 fractions, so this swap alone re-targets the whole game
        // without touching any screen file.
        var scaler = canvasGo.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        canvasGo.AddComponent<GraphicRaycaster>();

        // On a landscape desktop window, fill the margins with a branded
        // backdrop instead of leaving them blank (mainly matters for ultra-wide
        // monitors now that 16:9 fills natively - see DesktopFrameFitter).
        var backdropGo = new GameObject("DesktopBackdrop", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        backdropGo.transform.SetParent(canvasGo.transform, false);
        var backdropRt = backdropGo.GetComponent<RectTransform>();
        backdropRt.anchorMin = Vector2.zero;
        backdropRt.anchorMax = Vector2.one;
        backdropRt.offsetMin = Vector2.zero;
        backdropRt.offsetMax = Vector2.zero;
        var backdrop = backdropGo.GetComponent<Image>();
        backdrop.color = VisualTheme.BackgroundColor;
        backdrop.raycastTarget = false;

        // The actual game frame: full-bleed on portrait windows (phones - zero
        // behavior change), centered and width-capped on landscape desktop windows
        // so the portrait-tuned screens below don't stretch into an unreadable
        // ultra-wide shape. Every existing screen mounts under here unchanged.
        var frameGo = new GameObject("GameFrame", typeof(RectTransform));
        frameGo.transform.SetParent(canvasGo.transform, false);
        frameGo.AddComponent<DesktopFrameFitter>();

        // All screens mount under SafeArea so they automatically avoid notches/home indicators.
        var safeAreaGo = new GameObject("SafeArea", typeof(RectTransform));
        safeAreaGo.transform.SetParent(frameGo.transform, false);
        var safeAreaRt = safeAreaGo.GetComponent<RectTransform>();
        safeAreaRt.anchorMin = Vector2.zero;
        safeAreaRt.anchorMax = Vector2.one;
        safeAreaRt.offsetMin = Vector2.zero;
        safeAreaRt.offsetMax = Vector2.zero;
        safeAreaGo.AddComponent<SafeAreaFitter>();

        safeAreaGo.AddComponent<UIManager>();
    }
}

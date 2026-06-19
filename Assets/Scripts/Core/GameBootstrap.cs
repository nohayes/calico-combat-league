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

        var scaler = canvasGo.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        canvasGo.AddComponent<GraphicRaycaster>();

        // All screens mount under SafeArea so they automatically avoid notches/home indicators.
        var safeAreaGo = new GameObject("SafeArea", typeof(RectTransform));
        safeAreaGo.transform.SetParent(canvasGo.transform, false);
        var safeAreaRt = safeAreaGo.GetComponent<RectTransform>();
        safeAreaRt.anchorMin = Vector2.zero;
        safeAreaRt.anchorMax = Vector2.one;
        safeAreaRt.offsetMin = Vector2.zero;
        safeAreaRt.offsetMax = Vector2.zero;
        safeAreaGo.AddComponent<SafeAreaFitter>();

        safeAreaGo.AddComponent<UIManager>();
    }
}

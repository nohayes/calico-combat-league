using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MainMenuScreen : UIScreen
{
    readonly Button continueButton;
    readonly TitleButtonFeedback continueFeedback;
    readonly Image avatarImage;
    readonly PlayerAvatarVisual avatarVisual;

    public MainMenuScreen(Transform parent, GameManager gm) : base(parent, gm, "MainMenuScreen")
    {
        // The title art is the full composition (logo, fighter, and visible labels).
        // Keep UI here to invisible hit areas so nothing duplicates the painted screen.
        UIFactory.ApplyScreenBackground(Root, "title_screen", addReadabilityTint: false);

        var avatarMarker = UIFactory.CreateAvatarMarker(Root.transform, "TitlePlayer",
            new Vector2(0.055f, -0.02f), new Vector2(0.405f, 0.82f), out avatarImage);
        avatarVisual = avatarMarker.gameObject.AddComponent<PlayerAvatarVisual>();

        CreateTitleHitArea("NEW GAME", new Vector2(0.44f, 0.29f), new Vector2(0.67f, 0.43f),
            () => GM.StartFreshGame());

        continueButton = CreateTitleHitArea("CONTINUE", new Vector2(0.45f, 0.17f), new Vector2(0.66f, 0.30f),
            () => GM.ContinueGame(), out continueFeedback);
    }

    public void Refresh()
    {
        bool hasPlayer = GM.Player != null;
        continueButton.interactable = hasPlayer;
        continueFeedback.SetDisabled(!hasPlayer);

        ArchetypeType archetype = hasPlayer ? GM.Player.Archetype : ArchetypeType.Boxer;
        Color theme = IconFactory.GetArchetypeThemeColor(archetype);
        avatarVisual.Initialize(avatarImage, archetype, theme, faceRight: true);
    }

    Button CreateTitleHitArea(string name, Vector2 anchorMin, Vector2 anchorMax, UnityEngine.Events.UnityAction onClick) =>
        CreateTitleHitArea(name, anchorMin, anchorMax, onClick, out _);

    Button CreateTitleHitArea(string name, Vector2 anchorMin, Vector2 anchorMax, UnityEngine.Events.UnityAction onClick,
        out TitleButtonFeedback feedback)
    {
        var go = new GameObject("TitleHitArea_" + name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        go.transform.SetParent(Root.transform, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        var image = go.GetComponent<Image>();
        image.color = Color.clear;

        var markerGo = new GameObject("GoldPointer", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        markerGo.transform.SetParent(go.transform, false);
        var markerRt = markerGo.GetComponent<RectTransform>();
        markerRt.anchorMin = new Vector2(-0.18f, 0.34f);
        markerRt.anchorMax = new Vector2(-0.06f, 0.66f);
        markerRt.offsetMin = Vector2.zero;
        markerRt.offsetMax = Vector2.zero;
        markerRt.localRotation = Quaternion.Euler(0f, 0f, -90f);
        var markerImage = markerGo.GetComponent<Image>();
        markerImage.sprite = IconFactory.GetShapeSprite(IconShape.Triangle);
        markerImage.color = UIFactory.GoldColor;
        markerImage.raycastTarget = false;
        markerGo.SetActive(false);

        var button = go.GetComponent<Button>();
        button.transition = Selectable.Transition.None;
        button.targetGraphic = image;
        button.onClick.AddListener(() =>
        {
            if (!button.interactable) return;
            AudioManager.Instance?.PlayClick();
            onClick?.Invoke();
        });

        feedback = go.AddComponent<TitleButtonFeedback>();
        feedback.Initialize(button, markerImage);
        return button;
    }
}

public class TitleButtonFeedback : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    static readonly Color HoverColor = new Color(UIFactory.GoldColor.r, UIFactory.GoldColor.g, UIFactory.GoldColor.b, 0.92f);
    static readonly Color PressedColor = new Color(UIFactory.CreamColor.r, UIFactory.CreamColor.g, UIFactory.CreamColor.b, 1f);

    const float NormalScale = 1f;
    const float PressedScale = 1.18f;

    Button button;
    Image marker;
    bool pointerOver;
    bool disabled;

    public void Initialize(Button targetButton, Image targetMarker)
    {
        button = targetButton;
        marker = targetMarker;
        ApplyState(false, HoverColor, NormalScale);
    }

    public void SetDisabled(bool isDisabled)
    {
        disabled = isDisabled;
        pointerOver = false;
        ApplyState(false, HoverColor, NormalScale);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!IsInteractive()) return;
        pointerOver = true;
        ApplyState(true, HoverColor, NormalScale);
        AudioManager.Instance?.PlayHover();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        pointerOver = false;
        ApplyState(false, HoverColor, NormalScale);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!IsInteractive()) return;
        ApplyState(true, PressedColor, PressedScale);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!IsInteractive()) return;
        ApplyState(pointerOver, HoverColor, NormalScale);
    }

    bool IsInteractive() => !disabled && button != null && button.interactable;

    void ApplyState(bool visible, Color color, float scale)
    {
        if (marker == null) return;
        marker.gameObject.SetActive(visible);
        marker.color = color;
        marker.rectTransform.localScale = Vector3.one * scale;
    }
}

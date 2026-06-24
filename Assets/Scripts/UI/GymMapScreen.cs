using System.Collections;
using UnityEngine;
using UnityEngine.UI;

// Home Screen: the player hub - logo, avatar, FIGHT (primary action), and three
// secondary actions (Profile / Shop / Stats). Moves and Hall of Fame moved into
// Profile (the management hub) to keep this screen's button group small. The
// gym list itself lives on the dedicated GymSelectionScreen.
// Landscape Conversion (Milestone 26): the tall avatar now fills the full-height
// left column - "my fighter's gym" - with FIGHT and the secondary actions
// organized in a column on the right, making full use of 16:9 width instead of
// stacking everything vertically.
public class GymMapScreen : UIScreen
{
    readonly Image avatarImage;
    readonly PlayerAvatarVisual avatarVisual;
    readonly RivalDialogueBox rivalDialogue;

    public GymMapScreen(Transform parent, GameManager gm) : base(parent, gm, "GymMapScreen")
    {
        UIFactory.ApplyScreenBackground(Root, "gym_map_new", addReadabilityTint: false);

        var marker = UIFactory.CreateAvatarMarker(Root.transform, "Player",
            new Vector2(0.055f, 0.02f), new Vector2(0.43f, 0.72f), out avatarImage);
        avatarVisual = marker.gameObject.AddComponent<PlayerAvatarVisual>();

        CreateMapHitArea("FIGHT", new Vector2(0.565f, 0.225f), new Vector2(0.93f, 0.35f),
            () => GM.ChangeState(GameState.GymSelection));
        CreateMapHitArea("PROFILE", new Vector2(0.555f, 0.09f), new Vector2(0.675f, 0.22f),
            () => GM.ChangeState(GameState.ProfileScreen), markerBelow: true);
        CreateMapHitArea("SHOP", new Vector2(0.685f, 0.09f), new Vector2(0.805f, 0.22f),
            () => GM.ChangeState(GameState.ShopScreen), markerBelow: true);
        CreateMapHitArea("STATS", new Vector2(0.81f, 0.09f), new Vector2(0.935f, 0.22f),
            () => GM.ChangeState(GameState.StatsScreen), markerBelow: true);

        // Milestone 29, Part 2: the rival's one-time first-appearance greeting.
        rivalDialogue = UIFactory.CreateRivalDialogue(Root.transform);
    }

    public void Refresh()
    {
        avatarImage.transform.parent.gameObject.SetActive(GM.Player != null);
        if (GM.Player == null) return;

        Color theme = IconFactory.GetArchetypeThemeColor(GM.Player.Archetype);
        avatarVisual.Initialize(avatarImage, GM.Player.Archetype, theme, faceRight: true);

        if (!GM.HasSeenRivalIntro)
        {
            GM.MarkRivalIntroSeen();
            RunAnimation(ShowRivalIntroDelayed());
        }
    }

    // A short beat after the screen's own enter transition finishes, rather
    // than popping in instantly alongside it.
    IEnumerator ShowRivalIntroDelayed()
    {
        yield return new WaitForSecondsRealtime(0.4f);
        rivalDialogue.Show(RivalDatabase.RivalName, RivalDatabase.FirstAppearanceLines);
    }

    Button CreateMapHitArea(string name, Vector2 anchorMin, Vector2 anchorMax, UnityEngine.Events.UnityAction onClick,
        bool markerBelow = false)
    {
        var go = new GameObject("MapHitArea_" + name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
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
        markerRt.anchorMin = markerBelow ? new Vector2(0.40f, -0.30f) : new Vector2(-0.10f, 0.38f);
        markerRt.anchorMax = markerBelow ? new Vector2(0.60f, -0.08f) : new Vector2(-0.035f, 0.62f);
        markerRt.offsetMin = Vector2.zero;
        markerRt.offsetMax = Vector2.zero;
        markerRt.localRotation = markerBelow ? Quaternion.Euler(0f, 0f, 180f) : Quaternion.Euler(0f, 0f, 90f);
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
            AudioManager.Instance?.PlayClick();
            onClick?.Invoke();
        });

        var feedback = go.AddComponent<TitleButtonFeedback>();
        feedback.Initialize(button, markerImage);
        return button;
    }
}

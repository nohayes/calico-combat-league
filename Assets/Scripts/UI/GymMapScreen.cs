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
    readonly RectTransform avatarFrame;
    readonly Image avatarImage;
    readonly PlayerAvatarVisual avatarVisual;
    readonly RectTransform travelCard;
    readonly Text travelText;
    readonly Button fightButton;
    readonly RivalDialogueBox rivalDialogue;
    bool traveling;
    bool travelSkipRequested;

    // Tall portrait avatar art: marker fills its frame edge-to-edge (no inset)
    // so the full vertical height of the sprite is available.
    static readonly Vector2 AvatarRestMin = Vector2.zero;
    static readonly Vector2 AvatarRestMax = Vector2.one;

    public GymMapScreen(Transform parent, GameManager gm) : base(parent, gm, "GymMapScreen", "gym_map")
    {
        UIFactory.CreateBrandHeader(Root.transform, new Vector2(0.30f, 0.86f), new Vector2(0.70f, 0.99f), 1.2f);

        avatarFrame = UIFactory.CreateCard(Root.transform, "HomeAvatar", new Vector2(0.04f, 0.06f), new Vector2(0.42f, 0.84f), Color.clear);
        var marker = UIFactory.CreateAvatarMarker(avatarFrame, "Player", AvatarRestMin, AvatarRestMax, out avatarImage);
        avatarVisual = marker.gameObject.AddComponent<PlayerAvatarVisual>();

        // Clean 1+3 layout: FIGHT stays largest/primary up top, with PROFILE /
        // SHOP / STATS as one uniform row beneath it, the whole group centered
        // a little lower in the right column. (Milestone 30: Street Fight moved
        // to the Gym Selection screen, alongside the gyms it trains you for.)
        fightButton = UIFactory.CreateButton(Root.transform, "FIGHT", new Vector2(0.50f, 0.56f), new Vector2(0.96f, 0.74f),
            OnFightPressed, UIFactory.DangerColor);

        UIFactory.CreateButton(Root.transform, "PROFILE", new Vector2(0.50f, 0.36f), new Vector2(0.64f, 0.48f),
            () => GM.ChangeState(GameState.ProfileScreen), UIFactory.SecondaryColor);
        UIFactory.CreateButton(Root.transform, "SHOP", new Vector2(0.66f, 0.36f), new Vector2(0.80f, 0.48f),
            () => GM.ChangeState(GameState.ShopScreen), UIFactory.SecondaryColor);
        UIFactory.CreateButton(Root.transform, "STATS", new Vector2(0.82f, 0.36f), new Vector2(0.96f, 0.48f),
            () => GM.ChangeState(GameState.StatsScreen), UIFactory.SecondaryColor);

        travelCard = UIFactory.CreateCard(Root.transform, "TravelCard", new Vector2(0.25f, 0.35f), new Vector2(0.75f, 0.65f),
            new Color(0.08f, 0.07f, 0.07f, 0.96f));
        var travelSkip = travelCard.gameObject.AddComponent<Button>();
        travelSkip.transition = Selectable.Transition.None;
        travelSkip.targetGraphic = travelCard.GetComponent<Image>();
        travelSkip.onClick.AddListener(() => travelSkipRequested = true);
        travelText = UIFactory.CreateText(travelCard, "HEADING TO THE GYM DISTRICT...", UIFactory.BodySize, UIFactory.GoldColor,
            TextAnchor.MiddleCenter, new Vector2(0.04f, 0.55f), new Vector2(0.96f, 0.9f), FontStyle.Bold);
        travelText.raycastTarget = false;
        var travelHint = UIFactory.CreateCaption(travelCard, "(tap to skip)", new Vector2(0.04f, 0.12f), new Vector2(0.96f, 0.45f), TextAnchor.MiddleCenter);
        travelHint.raycastTarget = false;
        travelCard.gameObject.SetActive(false);

        // Milestone 29, Part 2: the rival's one-time first-appearance greeting.
        rivalDialogue = UIFactory.CreateRivalDialogue(Root.transform);
    }

    public void Refresh()
    {
        avatarFrame.gameObject.SetActive(GM.Player != null);
        traveling = false;
        travelCard.gameObject.SetActive(false);
        fightButton.interactable = true;
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

    void OnFightPressed()
    {
        if (traveling || GM.Player == null) return;
        traveling = true;
        fightButton.interactable = false;
        travelSkipRequested = false;
        travelCard.gameObject.SetActive(true);
        // Reusing the avatar's existing walk-in-place presentation (same sprite
        // swap + step bounce used for real gym-to-gym travel) instead of any new
        // animation system - it just targets the position it's already at.
        avatarVisual.MoveToAnchor(AvatarRestMin, AvatarRestMax, 1.2f, null);
        RunAnimation(TravelRoutine());
    }

    IEnumerator TravelRoutine()
    {
        float t = 0f;
        const float duration = 1.2f;
        while (t < duration && !travelSkipRequested)
        {
            t += Time.unscaledDeltaTime;
            yield return null;
        }

        travelCard.gameObject.SetActive(false);
        GM.ChangeState(GameState.GymSelection);
    }
}

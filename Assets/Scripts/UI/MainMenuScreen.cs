using UnityEngine;
using UnityEngine.UI;

public class MainMenuScreen : UIScreen
{
    readonly Button continueButton;
    readonly RectTransform avatarFrame;
    readonly Image avatarImage;
    readonly PlayerAvatarVisual avatarVisual;

    public MainMenuScreen(Transform parent, GameManager gm) : base(parent, gm, "MainMenuScreen", "main_menu")
    {
        // Landscape Conversion (Milestone 26): the tall portrait avatar now owns
        // the full-height left column - a layout 16:9 makes natural, where the
        // old portrait design had to fight the avatar for vertical space. The
        // compact logo sits top-center; actions stack on the right.
        UIFactory.CreateBrandHeader(Root.transform, new Vector2(0.30f, 0.83f), new Vector2(0.70f, 0.98f), 1.3f);

        avatarFrame = UIFactory.CreateCard(Root.transform, "HomeAvatar", new Vector2(0.04f, 0.06f), new Vector2(0.46f, 0.82f), Color.clear);
        var marker = UIFactory.CreateAvatarMarker(avatarFrame, "Player", Vector2.zero, Vector2.one, out avatarImage);
        avatarVisual = marker.gameObject.AddComponent<PlayerAvatarVisual>();

        continueButton = UIFactory.CreateButton(Root.transform, "CONTINUE", new Vector2(0.54f, 0.58f), new Vector2(0.96f, 0.72f),
            () => GM.ContinueGame());

        UIFactory.CreateButton(Root.transform, "NEW GAME", new Vector2(0.54f, 0.40f), new Vector2(0.96f, 0.54f),
            () => GM.StartFreshGame(), UIFactory.DangerColor);

        UIFactory.CreateButton(Root.transform, "AUDIO SETTINGS", new Vector2(0.54f, 0.22f), new Vector2(0.96f, 0.34f),
            () => GM.ChangeState(GameState.Settings), UIFactory.SecondaryColor);
    }

    public void Refresh()
    {
        bool hasPlayer = GM.Player != null;
        continueButton.gameObject.SetActive(hasPlayer);

        // The avatar frame must never be empty, even before any profile/save
        // exists: fall back to the default Boxer archetype (reusing the exact
        // same avatar pipeline/art - just no player-specific stats yet).
        ArchetypeType archetype = hasPlayer ? GM.Player.Archetype : ArchetypeType.Boxer;
        Color theme = IconFactory.GetArchetypeThemeColor(archetype);
        avatarVisual.Initialize(avatarImage, archetype, theme, faceRight: true);
    }
}

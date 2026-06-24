using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    MainMenuScreen mainMenu;
    FighterCreationScreen fighterCreation;
    GymMapScreen gymMap;
    GymSelectionScreen gymSelection;
    GymScreen gymScreen;
    BattleScreen battleScreen;
    VictoryScreen victoryScreen;
    DefeatScreen defeatScreen;
    MovesScreen movesScreen;
    StatsScreen statsScreen;
    ShopScreen shopScreen;
    ChampionshipScreen championshipScreen;
    AchievementsScreen achievementsScreen;
    ProfileScreen profileScreen;
    HallOfChampionsScreen hallOfChampionsScreen;
    SettingsScreen settingsScreen;
    StreetFightScreen streetFightScreen;
    CareerScreen careerScreen;

    void Start()
    {
        var gm = GameManager.Instance;

        mainMenu = new MainMenuScreen(transform, gm);
        fighterCreation = new FighterCreationScreen(transform, gm);
        gymMap = new GymMapScreen(transform, gm);
        gymSelection = new GymSelectionScreen(transform, gm);
        gymScreen = new GymScreen(transform, gm);
        battleScreen = new BattleScreen(transform, gm);
        victoryScreen = new VictoryScreen(transform, gm);
        defeatScreen = new DefeatScreen(transform, gm);
        movesScreen = new MovesScreen(transform, gm);
        statsScreen = new StatsScreen(transform, gm);
        shopScreen = new ShopScreen(transform, gm);
        championshipScreen = new ChampionshipScreen(transform, gm);
        achievementsScreen = new AchievementsScreen(transform, gm);
        profileScreen = new ProfileScreen(transform, gm);
        hallOfChampionsScreen = new HallOfChampionsScreen(transform, gm);
        settingsScreen = new SettingsScreen(transform, gm);
        streetFightScreen = new StreetFightScreen(transform, gm);
        careerScreen = new CareerScreen(transform, gm);

        CreateGlobalAudioButton();

        gm.OnStateChanged += HandleStateChanged;
        HandleStateChanged(gm.State);
    }

    // Quick Fix (Global Audio Settings Button): one small, always-available
    // corner button that opens the existing SettingsScreen as an overlay,
    // added once here rather than duplicated on every individual screen.
    // Created last so it's the last sibling under UIManager - rendering above
    // every screen above. Tucked into the corner past every screen's existing
    // header/title margin (none of them use the 0.955-1.0 x/y pocket), so it
    // never overlaps the Fight Night intro card, billing text, or move
    // buttons on any screen, Battle included.
    void CreateGlobalAudioButton()
    {
        var button = UIFactory.CreateCardButton(transform, "AudioSettings", new Vector2(0.955f, 0.945f), new Vector2(0.995f, 0.985f),
            () => settingsScreen.ShowAsOverlay(), new Color(0.08f, 0.07f, 0.07f, 0.4f));

        var iconGo = new GameObject("Icon", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        iconGo.transform.SetParent(button.transform, false);
        var iconRt = iconGo.GetComponent<RectTransform>();
        iconRt.anchorMin = new Vector2(0.2f, 0.2f);
        iconRt.anchorMax = new Vector2(0.8f, 0.8f);
        iconRt.offsetMin = Vector2.zero;
        iconRt.offsetMax = Vector2.zero;
        var icon = iconGo.GetComponent<Image>();
        icon.sprite = IconFactory.GetShapeSprite(IconShape.Circle);
        icon.color = new Color(1f, 1f, 1f, 0.55f);
        icon.preserveAspect = true;
        icon.raycastTarget = false;
    }

    void HandleStateChanged(GameState state)
    {
        AudioManager.Instance?.PlayForState(state, GameManager.Instance.CurrentGym);

        mainMenu.SetVisible(state == GameState.MainMenu);
        fighterCreation.SetVisible(state == GameState.FighterCreation);
        gymMap.SetVisible(state == GameState.GymMap);
        gymSelection.SetVisible(state == GameState.GymSelection);
        gymScreen.SetVisible(state == GameState.GymScreen);
        battleScreen.SetVisible(state == GameState.Battle);
        victoryScreen.SetVisible(state == GameState.Victory);
        defeatScreen.SetVisible(state == GameState.Defeat);
        movesScreen.SetVisible(state == GameState.MovesScreen);
        statsScreen.SetVisible(state == GameState.StatsScreen);
        shopScreen.SetVisible(state == GameState.ShopScreen);
        championshipScreen.SetVisible(state == GameState.Championship);
        achievementsScreen.SetVisible(state == GameState.AchievementsScreen);
        profileScreen.SetVisible(state == GameState.ProfileScreen);
        hallOfChampionsScreen.SetVisible(state == GameState.HallOfChampionsScreen);
        settingsScreen.SetVisible(state == GameState.Settings);
        streetFightScreen.SetVisible(state == GameState.StreetFight);
        careerScreen.SetVisible(state == GameState.CareerScreen);

        switch (state)
        {
            case GameState.MainMenu:
                mainMenu.Refresh();
                break;
            case GameState.FighterCreation:
                fighterCreation.Refresh();
                break;
            case GameState.GymMap:
                gymMap.Refresh();
                break;
            case GameState.GymSelection:
                gymSelection.Refresh();
                break;
            case GameState.GymScreen:
                gymScreen.Refresh();
                break;
            case GameState.Battle:
                battleScreen.Refresh();
                break;
            case GameState.Victory:
                victoryScreen.Refresh();
                break;
            case GameState.Defeat:
                defeatScreen.Refresh();
                break;
            case GameState.MovesScreen:
                movesScreen.Refresh();
                break;
            case GameState.StatsScreen:
                statsScreen.Refresh();
                break;
            case GameState.ShopScreen:
                shopScreen.Refresh();
                break;
            case GameState.Championship:
                championshipScreen.Refresh();
                break;
            case GameState.AchievementsScreen:
                achievementsScreen.Refresh();
                break;
            case GameState.ProfileScreen:
                profileScreen.Refresh();
                break;
            case GameState.HallOfChampionsScreen:
                hallOfChampionsScreen.Refresh();
                break;
            case GameState.Settings:
                settingsScreen.Refresh();
                break;
            case GameState.StreetFight:
                streetFightScreen.Refresh();
                break;
            case GameState.CareerScreen:
                careerScreen.Refresh();
                break;
        }
    }
}

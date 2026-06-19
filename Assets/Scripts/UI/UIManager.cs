using UnityEngine;

public class UIManager : MonoBehaviour
{
    MainMenuScreen mainMenu;
    FighterCreationScreen fighterCreation;
    GymMapScreen gymMap;
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

    void Start()
    {
        var gm = GameManager.Instance;

        mainMenu = new MainMenuScreen(transform, gm);
        fighterCreation = new FighterCreationScreen(transform, gm);
        gymMap = new GymMapScreen(transform, gm);
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

        gm.OnStateChanged += HandleStateChanged;
        HandleStateChanged(gm.State);
    }

    void HandleStateChanged(GameState state)
    {
        mainMenu.SetVisible(state == GameState.MainMenu);
        fighterCreation.SetVisible(state == GameState.FighterCreation);
        gymMap.SetVisible(state == GameState.GymMap);
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

        switch (state)
        {
            case GameState.MainMenu:
                mainMenu.Refresh();
                break;
            case GameState.GymMap:
                gymMap.Refresh();
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
        }
    }
}

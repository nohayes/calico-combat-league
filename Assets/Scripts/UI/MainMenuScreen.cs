using UnityEngine;
using UnityEngine.UI;

public class MainMenuScreen : UIScreen
{
    readonly Button continueButton;

    public MainMenuScreen(Transform parent, GameManager gm) : base(parent, gm, "MainMenuScreen", "main_menu")
    {
        UIFactory.CreateBrandHeader(Root.transform, new Vector2(0.06f, 0.55f), new Vector2(0.94f, 0.92f));

        continueButton = UIFactory.CreateButton(Root.transform, "CONTINUE", new Vector2(0.2f, 0.32f), new Vector2(0.8f, 0.42f),
            () => GM.ContinueGame());

        UIFactory.CreateButton(Root.transform, "NEW GAME", new Vector2(0.2f, 0.18f), new Vector2(0.8f, 0.28f),
            () => GM.StartFreshGame(), UIFactory.DangerColor);
    }

    public void Refresh()
    {
        continueButton.gameObject.SetActive(GM.Player != null);
    }
}

using UnityEngine;
using UnityEngine.UI;

public class DefeatScreen : UIScreen
{
    public DefeatScreen(Transform parent, GameManager gm) : base(parent, gm, "DefeatScreen")
    {
        UIFactory.CreateText(Root.transform, "DEFEATED", UIFactory.HeadingSize, UIFactory.DangerColor, TextAnchor.MiddleCenter,
            new Vector2(0.1f, 0.62f), new Vector2(0.9f, 0.78f), FontStyle.Bold);

        UIFactory.CreateBody(Root.transform, "Not this time. Heal up and try again.",
            new Vector2(0.1f, 0.5f), new Vector2(0.9f, 0.58f), TextAnchor.MiddleCenter);

        UIFactory.CreateCaption(Root.transform, "No penalty - your fighter is fully rested back at the map.",
            new Vector2(0.1f, 0.42f), new Vector2(0.9f, 0.49f), TextAnchor.MiddleCenter);

        UIFactory.CreateButton(Root.transform, "RETURN TO MAP", new Vector2(0.22f, 0.2f), new Vector2(0.78f, 0.31f),
            () => GM.ReturnToMap(), UIFactory.SecondaryColor);
    }

    public void Refresh()
    {
        AudioManager.Instance?.PlayDefeat();
    }
}

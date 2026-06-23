using UnityEngine;
using UnityEngine.UI;

public class DefeatScreen : UIScreen
{
    readonly RectTransform defeatHeading;
    readonly RectTransform messageRect;
    readonly CanvasGroup messageGroup;
    readonly Image defeatedSprite;
    readonly BattleFighterVisual defeatedVisual;
    readonly Text opponentLineText;

    public DefeatScreen(Transform parent, GameManager gm) : base(parent, gm, "DefeatScreen", "defeat")
    {
        // Landscape Conversion: defeated fighter full-height in a left column;
        // heading/messages/button form a right column.
        var fighterRoot = UIFactory.CreateBattleFighter(Root.transform, "DefeatedFighter",
            new Vector2(0.03f, 0.08f), new Vector2(0.38f, 0.92f), out defeatedSprite);
        defeatedVisual = fighterRoot.gameObject.AddComponent<BattleFighterVisual>();

        UIFactory.CreateCaption(Root.transform, "CALICO COMBAT LEAGUE | OFFICIAL RESULT",
            new Vector2(0.42f, 0.80f), new Vector2(0.97f, 0.86f), TextAnchor.MiddleCenter);
        defeatHeading = UIFactory.CreateText(Root.transform, "DEFEATED", UIFactory.HeadingSize, UIFactory.DangerColor, TextAnchor.MiddleCenter,
            new Vector2(0.42f, 0.64f), new Vector2(0.97f, 0.80f), FontStyle.Bold).rectTransform;

        var message = UIFactory.CreateBody(Root.transform, "Not this time. Heal up and try again.",
            new Vector2(0.42f, 0.50f), new Vector2(0.97f, 0.60f), TextAnchor.MiddleCenter);
        messageRect = message.rectTransform;
        messageGroup = message.gameObject.AddComponent<CanvasGroup>();

        UIFactory.CreateCaption(Root.transform, "No penalty - your fighter is fully rested back at the map.",
            new Vector2(0.42f, 0.40f), new Vector2(0.97f, 0.49f), TextAnchor.MiddleCenter);

        // Milestone 22: the opponent's reaction to beating the player.
        opponentLineText = UIFactory.CreateCaption(Root.transform, "", new Vector2(0.42f, 0.30f), new Vector2(0.97f, 0.38f), TextAnchor.MiddleCenter);
        opponentLineText.color = UIFactory.GoldColor;

        UIFactory.CreateButton(Root.transform, "RETURN TO MAP", new Vector2(0.50f, 0.10f), new Vector2(0.89f, 0.22f),
            () => GM.ReturnToMap(), UIFactory.SecondaryColor);
    }

    public void Refresh()
    {
        AudioManager.Instance?.PlayDefeat();
        PlayExhausted(defeatHeading);
        PlayReveal(messageGroup, messageRect, 0.2f, 0.35f);

        string winLine = GM.CurrentOpponentInfo?.WinLine;
        opponentLineText.text = !string.IsNullOrEmpty(winLine) && GM.CurrentOpponent != null
            ? $"\"{winLine}\" - {GM.CurrentOpponent.Name}"
            : "";

        if (GM.Player != null)
        {
            Color theme = IconFactory.GetArchetypeThemeColor(GM.Player.Archetype);
            defeatedVisual.Initialize(defeatedSprite, "player", GM.Player.Archetype, theme, faceRight: true);
            defeatedVisual.PlayDefeatPose();
        }
    }
}

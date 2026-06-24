using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Milestone 27: fighter lineup redesign. The old gym-banner + vertical trainer
// list is replaced with a horizontal fighter select screen (gym background
// visible behind it, no banner/logo/quote) generated from the current gym's
// existing Trainers + Leader data - nothing here is hardcoded per gym, so any
// future gym with a different roster size works automatically.
public class GymScreen : UIScreen
{
    readonly Text gymHeading;
    readonly Transform fighterRow;
    readonly List<GameObject> dynamicEntries = new List<GameObject>();

    public GymScreen(Transform parent, GameManager gm) : base(parent, gm, "GymScreen")
    {
        gymHeading = UIFactory.CreateHeading(Root.transform, "", new Vector2(0.2f, 0.88f), new Vector2(0.8f, 0.98f));

        fighterRow = UIFactory.CreateContainer(Root.transform, new Vector2(0.04f, 0.18f), new Vector2(0.96f, 0.84f));

        UIFactory.CreateButton(Root.transform, "BACK TO MAP", new Vector2(0.35f, 0.03f), new Vector2(0.65f, 0.14f),
            () => GM.ChangeState(GameState.GymSelection), UIFactory.SecondaryColor, isBackAction: true);
    }

    public void Refresh()
    {
        foreach (var entry in dynamicEntries) Object.Destroy(entry);
        dynamicEntries.Clear();

        var gym = GM.CurrentGym;
        if (gym == null || gym.Trainers == null || gym.Leader == null)
        {
            gymHeading.text = "NO GYM SELECTED";
            Debug.LogWarning("GymScreen.Refresh: current gym is missing or incomplete.");
            return;
        }

        // Gym background stays visible behind the lineup - no banner/panel covers it.
        UIFactory.ApplyScreenBackground(Root, $"{gym.GymId}_background");
        gymHeading.text = gym.GymName.ToUpper();

        // Lineup order matches the existing roster order (trainers, then the
        // leader last) - reuses GymInfo as-is, no new trainer data or database.
        var lineup = new List<OpponentInfo>(gym.Trainers) { gym.Leader };
        bool leaderUnlocked = GM.IsLeaderUnlocked(gym);

        for (int i = 0; i < lineup.Count; i++)
        {
            bool isLeader = i == lineup.Count - 1;
            bool unlocked = !isLeader || leaderUnlocked;
            BuildFighterSlot(lineup[i], i, lineup.Count, unlocked);
        }
    }

    // One slot = a nameplate (white/black-bordered, per spec) above a clickable
    // fighter portrait that uses the exact same portrait pipeline (ArtRegistry /
    // SetFighterPortrait) every other screen already uses - no new sprites.
    void BuildFighterSlot(OpponentInfo opponent, int index, int total, bool unlocked)
    {
        float slotWidth = 1f / total;
        float gap = slotWidth * 0.05f;
        float xMin = index * slotWidth + gap;
        float xMax = (index + 1) * slotWidth - gap;

        var slot = UIFactory.CreateContainer(fighterRow, new Vector2(xMin, 0f), new Vector2(xMax, 1f));
        dynamicEntries.Add(slot.gameObject);

        // Nameplate: a high-contrast "name tag" card above the fighter -
        // Milestone 48A: the original was a stark black/white plate sitting
        // completely outside the game's dark gold/cream palette; recolored to
        // a Secondary-bordered, Cream-filled plate so it still reads as a
        // bold name tag without breaking visual cohesion with every other
        // screen's warm tones.
        var borderGo = new GameObject("NameplateBorder", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        borderGo.transform.SetParent(slot, false);
        var borderRt = borderGo.GetComponent<RectTransform>();
        borderRt.anchorMin = new Vector2(0.06f, 0.89f);
        borderRt.anchorMax = new Vector2(0.94f, 1f);
        borderRt.offsetMin = Vector2.zero;
        borderRt.offsetMax = Vector2.zero;
        borderGo.GetComponent<Image>().color = UIFactory.SecondaryColor;

        var plateGo = new GameObject("Nameplate", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        plateGo.transform.SetParent(borderGo.transform, false);
        var plateRt = plateGo.GetComponent<RectTransform>();
        plateRt.anchorMin = Vector2.zero;
        plateRt.anchorMax = Vector2.one;
        plateRt.offsetMin = new Vector2(3f, 3f);
        plateRt.offsetMax = new Vector2(-3f, -3f);
        plateGo.GetComponent<Image>().color = UIFactory.CreamColor;

        var nameText = UIFactory.CreateText(plateGo.transform, opponent.Name, UIFactory.CaptionSize, UIFactory.BackgroundColor,
            TextAnchor.MiddleCenter, Vector2.zero, Vector2.one, FontStyle.Bold);
        nameText.resizeTextForBestFit = true;
        nameText.resizeTextMinSize = 10;
        nameText.resizeTextMaxSize = UIFactory.CaptionSize;
        nameText.raycastTarget = false;

        // The fighter sprite itself is the button.
        var fighterGo = new GameObject("Fighter_" + opponent.OpponentId, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        fighterGo.transform.SetParent(slot, false);
        var fighterRt = fighterGo.GetComponent<RectTransform>();
        fighterRt.anchorMin = new Vector2(0.04f, 0.06f);
        fighterRt.anchorMax = new Vector2(0.96f, 0.85f);
        fighterRt.offsetMin = Vector2.zero;
        fighterRt.offsetMax = Vector2.zero;

        var gym = GM.CurrentGym;
        ArchetypeType archetype = IconFactory.GetPortraitArchetype(gym.GymType);
        Color theme = IconFactory.GetGymThemeColor(gym.GymType);
        var fighterImage = fighterGo.GetComponent<Image>();
        fighterImage.preserveAspect = true;
        UIFactory.SetFighterPortrait(fighterImage, opponent.OpponentId, archetype, theme);
        if (!unlocked)
        {
            var c = fighterImage.color;
            fighterImage.color = new Color(c.r * 0.45f, c.g * 0.45f, c.b * 0.45f, c.a);
        }
        else
        {
            // Milestone 28: subtle hover feedback on selectable fighters.
            fighterGo.AddComponent<HoverGlow>();
        }

        var button = fighterGo.GetComponent<Button>();
        button.targetGraphic = fighterImage;
        button.interactable = unlocked;
        fighterGo.AddComponent<ButtonPunch>();
        button.onClick.AddListener(() =>
        {
            AudioManager.Instance?.PlayClick();
            PlayPulse(fighterRt, 1.15f, 0.2f);
            GM.StartBattle(opponent);
        });

        if (!unlocked)
        {
            var lockedText = UIFactory.CreateCaption(slot, "LOCKED", new Vector2(0.04f, 0.0f), new Vector2(0.96f, 0.07f), TextAnchor.MiddleCenter);
            lockedText.color = UIFactory.LockedColor;
            lockedText.fontStyle = FontStyle.Bold;
            lockedText.raycastTarget = false;
        }
    }
}

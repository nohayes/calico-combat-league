using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Home Screen Redesign: the gym list + avatar-travel logic that used to live on
// the Home/Map screen now lives here, reached via the Home hub's FIGHT button.
// Logic is unchanged from before, just relocated to its own "world map" screen.
public class GymSelectionScreen : UIScreen
{
    readonly Transform listContainer;
    readonly List<GameObject> dynamicEntries = new List<GameObject>();
    readonly Text rivalText;
    // Milestone 25, Part 5: ephemeral (not saved) tracking so a gym that just
    // became unlocked gets a one-time "reveal" pulse instead of silently
    // appearing as "Available" - pure UI flourish, GameManager's unlock logic
    // (IsGymUnlocked) is the single source of truth and is never touched.
    readonly HashSet<string> seenUnlockedGymIds = new HashSet<string>();

    readonly RectTransform avatarMarker;
    readonly Image avatarImage;
    readonly PlayerAvatarVisual avatarVisual;
    int avatarGymIndex = -1;
    bool traveling;

    // Milestone 33, Part 4: a brief rival intercept the first time a gym other
    // than the very first becomes newly unlocked. Set during the row-build
    // loop, consumed once at the end of Refresh() so it can't pop up mid-layout.
    readonly RivalDialogueBox rivalDialogue;
    GymInfo pendingInterceptGym;

    const float TravelDuration = 0.4f;
    const float ListMinY = 0.14f;
    const float ListMaxY = 0.78f;
    const float RailXMin = 0.935f;
    const float RailXMax = 0.985f;

    public GymSelectionScreen(Transform parent, GameManager gm) : base(parent, gm, "GymSelectionScreen", "gym_map")
    {
        UIFactory.CreateHeading(Root.transform, "SELECT YOUR GYM", new Vector2(0.06f, 0.88f), new Vector2(0.94f, 0.98f));

        // Milestone 22, Part 7: a small recurring rival comments on your progress
        // while you pick your next gym. Reuses existing GameManager stats only.
        rivalText = UIFactory.CreateCaption(Root.transform, "", new Vector2(0.06f, 0.8f), new Vector2(0.94f, 0.87f), TextAnchor.MiddleCenter);
        rivalText.color = UIFactory.MutedTextColor;

        // Landscape Conversion: a touch wider now that 16:9 has the room.
        listContainer = UIFactory.CreateContainer(Root.transform, new Vector2(0.05f, ListMinY), new Vector2(0.93f, ListMaxY));

        avatarMarker = UIFactory.CreateAvatarMarker(Root.transform, "MapTraveler",
            new Vector2(RailXMin, 0.6f), new Vector2(RailXMax, 0.68f), out avatarImage);
        avatarVisual = avatarMarker.gameObject.AddComponent<PlayerAvatarVisual>();

        UIFactory.CreateButton(Root.transform, "BACK TO HOME", new Vector2(0.3f, 0.03f), new Vector2(0.7f, 0.12f),
            () => GM.ChangeState(GameState.GymMap), UIFactory.SecondaryColor);

        rivalDialogue = UIFactory.CreateRivalDialogue(Root.transform);
    }

    public void Refresh()
    {
        foreach (var entry in dynamicEntries) Object.Destroy(entry);
        dynamicEntries.Clear();
        pendingInterceptGym = null;

        var gyms = GymDatabase.AllGyms;
        if (gyms == null || gyms.Count == 0)
        {
            Debug.LogWarning("GymSelectionScreen.Refresh: no gyms found in GymDatabase.");
            return;
        }

        // Milestone 26: a secret extra row appears only after the Championship
        // Gym is cleared. Folding it into the same row-math as the real gyms
        // (rather than a fixed overlay) means it never overlaps them.
        bool shadowUnlocked = GM.HasBecomeChampion();
        // Milestone 30 (relocation): Street Fight moved here from the Home
        // screen as a first-class progression option, always last in the list.
        int totalRows = gyms.Count + (shadowUnlocked ? 1 : 0) + 1;

        for (int i = 0; i < gyms.Count; i++)
        {
            BuildGymRow(gyms[i], i, totalRows);
        }
        if (shadowUnlocked) BuildShadowRow(gyms.Count, totalRows);
        BuildStreetFightRow(totalRows - 1, totalRows);

        // Milestone 33, Part 2/5: the rival's existing progress quip plus the
        // Rival Tracker status, so this screen doubles as "world presence."
        rivalText.text = $"{RivalDatabase.RivalName}: \"{RivalDatabase.GetLine(GM)}\"\n{RivalDatabase.GetRivalStatus(GM)}";

        if (pendingInterceptGym != null)
        {
            var gym = pendingInterceptGym;
            pendingInterceptGym = null;
            RunAnimation(ShowGymInterceptDelayed(gym));
        }

        avatarMarker.gameObject.SetActive(GM.Player != null);
        if (GM.Player == null) return;

        traveling = false;
        if (avatarGymIndex < 0) avatarGymIndex = Mathf.Clamp(GM.TotalGymsCleared, 0, gyms.Count - 1);
        avatarGymIndex = Mathf.Clamp(avatarGymIndex, 0, gyms.Count - 1);

        Color theme = IconFactory.GetArchetypeThemeColor(GM.Player.Archetype);
        avatarVisual.Initialize(avatarImage, GM.Player.Archetype, theme, faceRight: true);
        SnapAvatarToRow(avatarGymIndex, totalRows);
    }

    // A mirror of BuildGymRow's visuals, styled to feel mysterious rather than
    // like a normal gym - same button/icon helpers, no new UI primitives.
    void BuildShadowRow(int index, int totalRows)
    {
        bool defeated = GM.HasDefeatedShadowChampion;
        string tagline = defeated ? "Defeated - \"Shadow Slayer\" earned" : "A reflection awaits...";
        string label = $"THE SHADOW GYM\n{tagline}";

        GetRowAnchors(index, totalRows, out float yMin, out float yMax);

        Color color = defeated ? UIFactory.PositiveColor : new Color(0.22f, 0.05f, 0.3f, 1f);
        var button = UIFactory.CreateButton(listContainer, label, new Vector2(0.06f, yMin), new Vector2(0.94f, yMax),
            () => TravelToShadowChampion(index, totalRows), color);
        dynamicEntries.Add(button.gameObject);

        if (!defeated) PlayPulse((RectTransform)button.transform, 1.06f, 0.7f);

        var labelText = button.GetComponentInChildren<Text>();
        labelText.rectTransform.anchorMin = new Vector2(0.24f, 0f);
        labelText.rectTransform.anchorMax = new Vector2(0.97f, 1f);
        labelText.alignment = TextAnchor.MiddleLeft;

        var iconGo = new GameObject("Icon", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        iconGo.transform.SetParent(button.transform, false);
        var iconRt = iconGo.GetComponent<RectTransform>();
        iconRt.anchorMin = new Vector2(0.04f, 0.18f);
        iconRt.anchorMax = new Vector2(0.2f, 0.82f);
        iconRt.offsetMin = Vector2.zero;
        iconRt.offsetMax = Vector2.zero;
        var iconImage = iconGo.GetComponent<Image>();
        iconImage.sprite = IconFactory.GetShapeSprite(IconShape.Star);
        iconImage.color = UIFactory.GoldColor;
    }

    void TravelToShadowChampion(int index, int totalRows)
    {
        if (traveling) return;
        traveling = true;

        GetRowAnchors(index, totalRows, out float yMin, out float yMax);
        Vector2 targetMin = new Vector2(RailXMin, RootYFromListY(yMin));
        Vector2 targetMax = new Vector2(RailXMax, RootYFromListY(yMax));

        avatarVisual.MoveToAnchor(targetMin, targetMax, TravelDuration, () =>
        {
            traveling = false;
            GM.StartShadowChampionBattle();
        });
    }

    // Milestone 30 (relocation): Street Fight as a first-class progression
    // option alongside the gyms - same row/button/icon pattern as BuildGymRow,
    // just always available (no lock state) and not tied to GymDatabase.
    void BuildStreetFightRow(int index, int totalRows)
    {
        string label = "STREET FIGHT\nRandom opponents.\nRisk and reward.\nTrain outside the gym system.";

        GetRowAnchors(index, totalRows, out float yMin, out float yMax);

        var button = UIFactory.CreateButton(listContainer, label, new Vector2(0.06f, yMin), new Vector2(0.94f, yMax),
            () => GM.ChangeState(GameState.StreetFight), UIFactory.AccentOrange);
        dynamicEntries.Add(button.gameObject);

        var labelText = button.GetComponentInChildren<Text>();
        labelText.rectTransform.anchorMin = new Vector2(0.24f, 0f);
        labelText.rectTransform.anchorMax = new Vector2(0.97f, 1f);
        labelText.alignment = TextAnchor.MiddleLeft;

        var iconGo = new GameObject("Icon", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        iconGo.transform.SetParent(button.transform, false);
        var iconRt = iconGo.GetComponent<RectTransform>();
        iconRt.anchorMin = new Vector2(0.04f, 0.18f);
        iconRt.anchorMax = new Vector2(0.2f, 0.82f);
        iconRt.offsetMin = Vector2.zero;
        iconRt.offsetMax = Vector2.zero;
        var iconImage = iconGo.GetComponent<Image>();
        iconImage.sprite = IconFactory.GetShapeSprite(IconShape.Diamond);
        iconImage.color = UIFactory.CreamColor;
    }

    void BuildGymRow(GymInfo gym, int index, int totalGyms)
    {
        bool unlocked = GM.IsGymUnlocked(gym);
        bool completed = GM.IsGymCompleted(gym);
        string tagline = completed ? "Cleared" : (unlocked && !string.IsNullOrEmpty(gym.Motto) ? gym.Motto : (unlocked ? "Available" : "Locked"));
        string label = $"{gym.GymName}\n{tagline}";

        GetRowAnchors(index, totalGyms, out float yMin, out float yMax);

        Color color = completed ? UIFactory.PositiveColor : (unlocked ? IconFactory.GetGymThemeColor(gym.GymType) : UIFactory.LockedColor);
        var button = UIFactory.CreateButton(listContainer, label, new Vector2(0.06f, yMin), new Vector2(0.94f, yMax),
            () => TravelToGym(gym, index, totalGyms), color);
        button.interactable = unlocked;
        dynamicEntries.Add(button.gameObject);

        if (unlocked && seenUnlockedGymIds.Add(gym.GymId))
        {
            PlayPulse((RectTransform)button.transform, 1.08f, 0.5f);
            // Milestone 33, Part 4: skip the very first gym - the rival's
            // FirstAppearanceLines greeting on the Home screen already covers
            // the start of the run.
            if (index > 0) pendingInterceptGym = gym;
        }

        // Make room for an icon on the left of the auto-generated label.
        var labelText = button.GetComponentInChildren<Text>();
        labelText.rectTransform.anchorMin = new Vector2(0.24f, 0f);
        labelText.rectTransform.anchorMax = new Vector2(0.97f, 1f);
        labelText.alignment = TextAnchor.MiddleLeft;

        var iconGo = new GameObject("Icon", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        iconGo.transform.SetParent(button.transform, false);
        var iconRt = iconGo.GetComponent<RectTransform>();
        iconRt.anchorMin = new Vector2(0.04f, 0.18f);
        iconRt.anchorMax = new Vector2(0.2f, 0.82f);
        iconRt.offsetMin = Vector2.zero;
        iconRt.offsetMax = Vector2.zero;
        var iconImage = iconGo.GetComponent<Image>();
        var realIcon = ArtRegistry.GetGymIcon(gym.GymId);
        iconImage.sprite = realIcon != null ? realIcon : IconFactory.GetShapeSprite(IconFactory.GetGymIconShape(gym.GymType));
        iconImage.color = unlocked ? Color.white : new Color(1f, 1f, 1f, 0.5f);
    }

    void TravelToGym(GymInfo gym, int index, int totalGyms)
    {
        if (traveling) return;
        traveling = true;

        GetRowAnchors(index, totalGyms, out float yMin, out float yMax);
        Vector2 targetMin = new Vector2(RailXMin, RootYFromListY(yMin));
        Vector2 targetMax = new Vector2(RailXMax, RootYFromListY(yMax));

        avatarVisual.MoveToAnchor(targetMin, targetMax, TravelDuration, () =>
        {
            traveling = false;
            avatarGymIndex = index;
            GM.EnterGym(gym);
        });
    }

    // Milestone 33, Part 4: a short pause after the screen settles, same
    // pattern GymMapScreen's first-appearance greeting already uses.
    IEnumerator ShowGymInterceptDelayed(GymInfo gym)
    {
        yield return new WaitForSecondsRealtime(0.5f);
        rivalDialogue.Show(RivalDatabase.RivalName, RivalDatabase.GetGymInterceptLines(gym));
    }

    void SnapAvatarToRow(int index, int totalGyms)
    {
        GetRowAnchors(index, totalGyms, out float yMin, out float yMax);
        avatarMarker.anchorMin = new Vector2(RailXMin, RootYFromListY(yMin));
        avatarMarker.anchorMax = new Vector2(RailXMax, RootYFromListY(yMax));
    }

    static void GetRowAnchors(int index, int total, out float yMin, out float yMax)
    {
        float slotHeight = 1f / total;
        float padding = slotHeight * 0.14f;
        yMax = 1f - index * slotHeight - padding;
        yMin = 1f - (index + 1) * slotHeight + padding;
    }

    static float RootYFromListY(float listY) => ListMinY + listY * (ListMaxY - ListMinY);
}

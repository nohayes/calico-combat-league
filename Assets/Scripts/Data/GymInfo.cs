using System.Collections.Generic;

public class GymInfo
{
    public string GymId;
    public string GymName;
    public GymType GymType;
    public string Description;

    // Presentation-only flavor (Milestone 12).
    public string Motto;
    public string History;

    public List<OpponentInfo> Trainers;
    public OpponentInfo Leader;

    // Null/empty means the gym is unlocked from the start.
    public string RequiredGymId;

    // Move ID granted to the player once the leader is defeated. Null/empty means no reward move.
    public string UnlockMoveId;

    // Milestone 52, Part 1: a short "what you learned" line shown on the
    // Victory screen the moment this gym is cleared - presentation-only,
    // never saved (GymInfo itself is static data, not part of SaveData).
    // Null/empty (Championship's GymInfo leaves this unset) simply means no
    // lesson callout - Championship already has its own dedicated screen
    // and celebratory beat, so it doesn't need one here.
    public string LessonText;
}

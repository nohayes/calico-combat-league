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
}

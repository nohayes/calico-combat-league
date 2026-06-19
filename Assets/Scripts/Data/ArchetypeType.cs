// Unspecified is intentionally value 0 so older saves (which never recorded an
// archetype) deserialize safely to a sensible default with no migration code.
public enum ArchetypeType
{
    Unspecified,
    Boxer,
    Wrestler,
    BjjSpecialist,
    MuayThaiFighter
}

namespace MinimalImplementation.Combat.Npcs;

/// <summary>An explicit, complete NPC-type immunity list from migrated Version4 facts.</summary>
public sealed class NpcBuffImmunityProfile
{
  private readonly HashSet<ushort> _immuneTypes;
  private readonly IReadOnlyList<ushort> _immuneTypeList;

  public NpcBuffImmunityProfile(short npcType, IReadOnlyCollection<ushort> immuneTypes)
  {
    ArgumentOutOfRangeException.ThrowIfNegativeOrZero(npcType);
    ArgumentNullException.ThrowIfNull(immuneTypes);
    if (immuneTypes.Any(static type => type == 0) || immuneTypes.Distinct().Count() != immuneTypes.Count)
    {
      throw new ArgumentException("NPC buff immunity types must be nonzero and unique.", nameof(immuneTypes));
    }

    NpcType = npcType;
    _immuneTypes = new HashSet<ushort>(immuneTypes);
    _immuneTypeList = Array.AsReadOnly(immuneTypes.ToArray());
  }

  public short NpcType { get; }

  public IReadOnlyList<ushort> ImmuneTypes => _immuneTypeList;

  public bool IsImmune(ushort buffType) => _immuneTypes.Contains(buffType);
}

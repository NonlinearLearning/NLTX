namespace MinimalImplementation.Combat.Npcs;

/// <summary>Only contains buff and immunity facts explicitly migrated from Version4.</summary>
public sealed class NpcBuffCatalog
{
  private readonly Dictionary<ushort, NpcBuffDefinition> _buffs;
  private readonly Dictionary<short, NpcBuffImmunityProfile> _immunities;

  public NpcBuffCatalog(
    IReadOnlyCollection<NpcBuffDefinition> buffs,
    IReadOnlyCollection<NpcBuffImmunityProfile> immunities)
  {
    ArgumentNullException.ThrowIfNull(buffs);
    ArgumentNullException.ThrowIfNull(immunities);
    _buffs = new Dictionary<ushort, NpcBuffDefinition>(buffs.Count);
    foreach (NpcBuffDefinition buff in buffs)
    {
      ArgumentNullException.ThrowIfNull(buff);
      if (!_buffs.TryAdd(buff.Type, buff))
      {
        throw new ArgumentException($"NPC buff type {buff.Type} is duplicated.", nameof(buffs));
      }
    }

    _immunities = new Dictionary<short, NpcBuffImmunityProfile>(immunities.Count);
    foreach (NpcBuffImmunityProfile profile in immunities)
    {
      ArgumentNullException.ThrowIfNull(profile);
      foreach (ushort buffType in profile.ImmuneTypes)
      {
        if (!_buffs.ContainsKey(buffType))
        {
          throw new ArgumentException(
            $"NPC type {profile.NpcType} immunity references undefined buff type {buffType}.",
            nameof(immunities));
        }
      }

      if (!_immunities.TryAdd(profile.NpcType, profile))
      {
        throw new ArgumentException($"NPC type {profile.NpcType} has duplicate buff immunity profiles.", nameof(immunities));
      }
    }
  }

  public bool TryGetBuff(ushort type, out NpcBuffDefinition definition) =>
    _buffs.TryGetValue(type, out definition!);

  public bool TryGetImmunity(short npcType, out NpcBuffImmunityProfile profile) =>
    _immunities.TryGetValue(npcType, out profile!);
}

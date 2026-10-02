using System.Collections.ObjectModel;

namespace Terraria.NpcTownBestiary;

public sealed class TownNpcProfileCatalog
{
  private readonly Dictionary<NpcTypeId, TownNpcProfileDefinition> _profiles = new();

  public IReadOnlyDictionary<NpcTypeId, TownNpcProfileDefinition> Profiles =>
    new ReadOnlyDictionary<NpcTypeId, TownNpcProfileDefinition>(_profiles);

  public void Register(TownNpcProfileDefinition profile)
  {
    ArgumentNullException.ThrowIfNull(profile);
    _profiles[profile.NpcType] = profile;
  }

  public bool TryGet(NpcTypeId npcType, out TownNpcProfileDefinition profile)
  {
    return _profiles.TryGetValue(npcType, out profile!);
  }
}

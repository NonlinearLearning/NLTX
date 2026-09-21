using System.Collections.ObjectModel;

namespace Terraria.NpcTownBestiary;

public sealed class NpcPersonalityDefinition
{
  public NpcPersonalityDefinition(
    IEnumerable<NpcBiomePreference> biomePreferences,
    IEnumerable<string>? shopTraits = null)
  {
    ArgumentNullException.ThrowIfNull(biomePreferences);
    BiomePreferences = biomePreferences.ToArray();
    ShopTraits = (shopTraits ?? Array.Empty<string>()).ToArray();
  }

  public IReadOnlyList<NpcBiomePreference> BiomePreferences { get; }

  public IReadOnlyList<string> ShopTraits { get; }
}

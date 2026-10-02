namespace Terraria.NpcTownBestiary;

public sealed class TownNpcProfileAssetAdapter
{
  public string GetRoot(
    TownNpcProfileDefinition profile,
    bool shimmered)
  {
    ArgumentNullException.ThrowIfNull(profile);
    return shimmered ? profile.ShimmeredRoot : profile.DefaultRoot;
  }

  public string GetVariantAssetKey(
    TownNpcProfileDefinition profile,
    bool shimmered,
    string variantName)
  {
    if (string.IsNullOrWhiteSpace(variantName))
    {
      throw new ArgumentException("Variant name is required.", nameof(variantName));
    }

    return $"{GetRoot(profile, shimmered)}_{variantName}";
  }
}

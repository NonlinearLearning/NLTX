namespace Terraria.NonAuthoritative.ContentDefinitions;

public readonly record struct ColorSlidersSet(
  float Hue,
  float Saturation,
  float Luminance,
  float Alpha);

public sealed record FontDefinition
{
  public FontDefinition(string nameKey)
  {
    if (string.IsNullOrWhiteSpace(nameKey))
    {
      throw new ArgumentException("Font definitions require a stable name key.", nameof(nameKey));
    }

    NameKey = nameKey;
  }

  public string NameKey { get; }
}

public sealed record HairstyleUnlockDefinition
{
  public HairstyleUnlockDefinition(int hairstyleId, string configKey)
  {
    if (hairstyleId < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(hairstyleId));
    }

    if (string.IsNullOrWhiteSpace(configKey))
    {
      throw new ArgumentException("Hairstyles require a stable config key.", nameof(configKey));
    }

    HairstyleId = hairstyleId;
    ConfigKey = configKey;
  }

  public int HairstyleId { get; }

  public string ConfigKey { get; }
}

public sealed record NpcProfileVariantDefinition
{
  public NpcProfileVariantDefinition(string profileKey, string variantKey)
  {
    if (string.IsNullOrWhiteSpace(profileKey))
    {
      throw new ArgumentException("Profiles require a stable profile key.", nameof(profileKey));
    }

    if (string.IsNullOrWhiteSpace(variantKey))
    {
      throw new ArgumentException("Profiles require a stable variant key.", nameof(variantKey));
    }

    ProfileKey = profileKey;
    VariantKey = variantKey;
  }

  public string ProfileKey { get; }

  public string VariantKey { get; }
}

public sealed class ContentPresentationCatalog
{
  private readonly Dictionary<string, FontDefinition> _fonts =
    new(StringComparer.Ordinal);
  private readonly Dictionary<int, HairstyleUnlockDefinition> _hairstyles = new();
  private readonly Dictionary<string, NpcProfileVariantDefinition> _profiles =
    new(StringComparer.Ordinal);

  public bool IsLoaded { get; internal set; }

  public IReadOnlyDictionary<string, FontDefinition> Fonts => _fonts;

  public IReadOnlyDictionary<int, HairstyleUnlockDefinition> Hairstyles => _hairstyles;

  public IReadOnlyDictionary<string, NpcProfileVariantDefinition> Profiles => _profiles;

  public bool TryGetFont(string nameKey, out FontDefinition? definition)
  {
    return _fonts.TryGetValue(nameKey, out definition);
  }

  public bool TryGetHairstyle(
    int hairstyleId,
    out HairstyleUnlockDefinition definition)
  {
    return _hairstyles.TryGetValue(hairstyleId, out definition!);
  }

  public bool TryGetProfile(
    string profileKey,
    out NpcProfileVariantDefinition? definition)
  {
    return _profiles.TryGetValue(profileKey, out definition);
  }

  internal void AddFont(FontDefinition definition)
  {
    _fonts[definition.NameKey] = definition;
    IsLoaded = true;
  }

  internal void AddHairstyle(HairstyleUnlockDefinition definition)
  {
    _hairstyles[definition.HairstyleId] = definition;
    IsLoaded = true;
  }

  internal void AddProfile(NpcProfileVariantDefinition definition)
  {
    _profiles[definition.ProfileKey] = definition;
    IsLoaded = true;
  }

  internal void Clear()
  {
    _fonts.Clear();
    _hairstyles.Clear();
    _profiles.Clear();
    IsLoaded = false;
  }
}

public static class PresentationCatalogLoadSystem
{
  public static void RegisterFont(
    ContentPresentationCatalog catalog,
    FontDefinition definition)
  {
    ArgumentNullException.ThrowIfNull(catalog);
    ArgumentNullException.ThrowIfNull(definition);
    catalog.AddFont(definition);
  }

  public static void RegisterHairstyle(
    ContentPresentationCatalog catalog,
    HairstyleUnlockDefinition definition)
  {
    ArgumentNullException.ThrowIfNull(catalog);
    ArgumentNullException.ThrowIfNull(definition);
    catalog.AddHairstyle(definition);
  }

  public static void RegisterProfile(
    ContentPresentationCatalog catalog,
    NpcProfileVariantDefinition definition)
  {
    ArgumentNullException.ThrowIfNull(catalog);
    ArgumentNullException.ThrowIfNull(definition);
    catalog.AddProfile(definition);
  }

  public static void Clear(ContentPresentationCatalog catalog)
  {
    ArgumentNullException.ThrowIfNull(catalog);
    catalog.Clear();
  }
}

public interface IPresentationAssetPort
{
  bool TryLoad(string assetKey, out object? asset);
}

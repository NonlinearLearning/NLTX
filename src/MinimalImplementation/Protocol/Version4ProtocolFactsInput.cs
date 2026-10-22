using Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

namespace MinimalImplementation.Server.Protocol;

public sealed class Version4ProtocolFactsInput
{
  public Version4ProtocolFactsInput(
    IReadOnlyList<bool> frameImportant,
    IReadOnlyList<bool> allowsSaveCompressionBatching,
    PacketTileEntityCodecs tileEntityCodecs,
    IReadOnlyList<bool> catchableTypes,
    Func<short, byte?, float?, byte> lifeWidthResolver,
    Func<short, bool> needsUuid,
    int travelMerchantSlotCount,
    int tagEffectNpcSlotCount,
    Func<short, bool> tagEffectUsesProcTimes,
    string contentFactsSha256)
  {
    ArgumentNullException.ThrowIfNull(frameImportant);
    ArgumentNullException.ThrowIfNull(allowsSaveCompressionBatching);
    ArgumentNullException.ThrowIfNull(tileEntityCodecs);
    ArgumentNullException.ThrowIfNull(catchableTypes);
    ArgumentNullException.ThrowIfNull(lifeWidthResolver);
    ArgumentNullException.ThrowIfNull(needsUuid);
    ArgumentNullException.ThrowIfNull(tagEffectUsesProcTimes);
    ArgumentException.ThrowIfNullOrWhiteSpace(contentFactsSha256);
    if (!IsSha256(contentFactsSha256))
    {
      throw new ArgumentException(
        "Content facts must provide a deterministic SHA-256 fingerprint.",
        nameof(contentFactsSha256));
    }

    FrameImportant = Array.AsReadOnly(frameImportant.ToArray());
    AllowsSaveCompressionBatching = Array.AsReadOnly(allowsSaveCompressionBatching.ToArray());
    TileEntityCodecs = tileEntityCodecs;
    CatchableTypes = Array.AsReadOnly(catchableTypes.ToArray());
    LifeWidthResolver = lifeWidthResolver;
    NeedsUuid = needsUuid;
    TravelMerchantSlotCount = travelMerchantSlotCount;
    TagEffectNpcSlotCount = tagEffectNpcSlotCount;
    TagEffectUsesProcTimes = tagEffectUsesProcTimes;
    ContentFactsSha256 = contentFactsSha256.ToUpperInvariant();
  }

  public IReadOnlyList<bool> FrameImportant { get; }

  public IReadOnlyList<bool> AllowsSaveCompressionBatching { get; }

  public PacketTileEntityCodecs TileEntityCodecs { get; }

  public IReadOnlyList<bool> CatchableTypes { get; }

  public Func<short, byte?, float?, byte> LifeWidthResolver { get; }

  public Func<short, bool> NeedsUuid { get; }

  public int TravelMerchantSlotCount { get; }

  public int TagEffectNpcSlotCount { get; }

  public Func<short, bool> TagEffectUsesProcTimes { get; }

  public string ContentFactsSha256 { get; }

  private static bool IsSha256(string value)
  {
    if (value.Length != 64)
    {
      return false;
    }

    foreach (char character in value)
    {
      if (!Uri.IsHexDigit(character))
      {
        return false;
      }
    }

    return true;
  }
}

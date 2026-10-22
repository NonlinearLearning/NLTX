using Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

namespace MinimalImplementation.Server.Protocol;

public static class Version4ProtocolFactsFactory
{
  private const int _version4TileTypeCount = 753;
  private const int _version4NpcTypeCount = 697;
  private const int _version4NpcSlotCount = 200;
  private static readonly object _gate = new();
  private static string? _publishedContentFactsSha256;
  private static Version4ProtocolFactsInput? _publishedInput;

  public static string? PublishedContentFactsSha256 => Volatile.Read(ref _publishedContentFactsSha256);

  internal static bool IsPublished(Version4ProtocolFactsInput input)
  {
    ArgumentNullException.ThrowIfNull(input);
    lock (_gate)
    {
      return ReferenceEquals(_publishedInput, input);
    }
  }

  /// <summary>Checks the complete Terraria319 directory shapes before publishing server facts.</summary>
  public static ProtocolInputs CreateProduction(Version4ProtocolFactsInput input) {
    ArgumentNullException.ThrowIfNull(input);
    if (input.FrameImportant.Count != _version4TileTypeCount
        || input.AllowsSaveCompressionBatching.Count != _version4TileTypeCount
        || input.CatchableTypes.Count != _version4NpcTypeCount
        || input.TagEffectNpcSlotCount != _version4NpcSlotCount) {
      throw new ArgumentException(
          "Production Terraria319 facts require 753 tile types, 697 NPC types and 200 NPC slots.",
          nameof(input));
    }

    return Create(input);
  }

  /// <summary>
  /// Publishes size-compatible facts, including reduced directories for isolated wire fixtures.
  /// Server composition must use <see cref="CreateProduction"/> for complete directory checks.
  /// </summary>
  public static ProtocolInputs Create(Version4ProtocolFactsInput input)
  {
    ArgumentNullException.ThrowIfNull(input);
    lock (_gate)
    {
      if (ProtocolInputs.IsInitialized)
      {
        throw new InvalidOperationException(
          "Version4 protocol facts were already initialized for this process.");
      }

      return CreateCore(input);
    }
  }

  private static ProtocolInputs CreateCore(Version4ProtocolFactsInput input)
  {
    bool[] frameImportant = input.FrameImportant.ToArray();
    bool[] allowsSaveCompressionBatching = input.AllowsSaveCompressionBatching.ToArray();
    bool[] catchableTypes = input.CatchableTypes.ToArray();
    if (frameImportant.Length == 0
        || frameImportant.Length != allowsSaveCompressionBatching.Length
        || catchableTypes.Length == 0)
    {
      throw new ArgumentException(
        "Complete, size-compatible Version4 content directories are required.",
        nameof(input));
    }

    if (input.TravelMerchantSlotCount != 40)
    {
      throw new ArgumentException(
        "The Version4 travel merchant table contains exactly 40 slots.",
        nameof(input));
    }

    if (input.TagEffectNpcSlotCount is < 0 or > byte.MaxValue)
    {
      throw new ArgumentException(
        "The TagEffect NPC directory must fit its byte index and terminator.",
        nameof(input));
    }

    IReadOnlyDictionary<ushort, IPacket82ModuleWireCodec> moduleCodecs =
      Version4ModuleCodecs.Create(isServer: true);
    var protocolInputs = new ProtocolInputs(
      frameImportant,
      allowsSaveCompressionBatching,
      input.TileEntityCodecs,
      isServer: true,
      catchableTypes,
      input.LifeWidthResolver,
      input.NeedsUuid,
      input.TravelMerchantSlotCount,
      moduleCodecs,
      input.TagEffectNpcSlotCount,
      input.TagEffectUsesProcTimes);
    Volatile.Write(ref _publishedContentFactsSha256, input.ContentFactsSha256);
    _publishedInput = input;
    return protocolInputs;
  }
}

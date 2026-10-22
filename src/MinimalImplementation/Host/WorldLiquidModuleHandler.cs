using MinimalImplementation.Contracts;
using MinimalImplementation.Server.Modules;
using MinimalImplementation.Server.Protocol;
using MinimalImplementation.Server.World;
using Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

namespace MinimalImplementation.Server.Host;

/// <summary>Commits authenticated Version4 module 0 liquid assignments without immediate dirtying.</summary>
public sealed class WorldLiquidModuleHandler : IModuleHandler
{
  public const ushort LiquidModuleId = 0;

  private const int EntryBytes = sizeof(int) + sizeof(byte) + sizeof(byte);
  private const int ModulePayloadHeaderBytes = sizeof(ushort) + sizeof(ushort);
  private const int MaximumLiquidEntries =
    (global::MinimalImplementation.Server.Protocol.PacketFrameCodec.MaximumPayloadBytes -
      ModulePayloadHeaderBytes) / EntryBytes;

  private readonly DelegatedModuleRejectionHandler _rejectionHandler =
    new(LiquidModuleId);
  private readonly WorldOwner _worldOwner;
  private readonly Func<InboundPacketEnvelope, ulong?> _resolveWorldEpoch;
  private readonly int _maximumEntries;

  public WorldLiquidModuleHandler(
    WorldOwner worldOwner,
    Func<InboundPacketEnvelope, ulong?> resolveWorldEpoch,
    int maximumEntries = MaximumLiquidEntries)
  {
    ArgumentNullException.ThrowIfNull(worldOwner);
    ArgumentNullException.ThrowIfNull(resolveWorldEpoch);
    if (maximumEntries <= 0 || maximumEntries > MaximumLiquidEntries)
    {
      throw new ArgumentOutOfRangeException(nameof(maximumEntries));
    }

    _worldOwner = worldOwner;
    _resolveWorldEpoch = resolveWorldEpoch;
    _maximumEntries = maximumEntries;
  }

  public ushort ModuleId => LiquidModuleId;

  public ModuleDispatchResult Handle(
    InboundPacketEnvelope envelope,
    ModuleDispatchContext context,
    object? decodedPayload)
  {
    ArgumentNullException.ThrowIfNull(envelope);
    ArgumentNullException.ThrowIfNull(context);
    if (envelope.MessageId != 82 || envelope.ModuleId != ModuleId ||
        envelope.Epoch != context.Epoch || !context.Sender.IsValid)
    {
      return ModuleDispatchResult.Rejected(
        ProtocolErrorCode.StaleEpoch,
        "The module 0 packet does not match its authenticated session envelope.");
    }

    if (context.Stage != ModuleSessionStage.Active)
    {
      return ModuleDispatchResult.Rejected(
        ProtocolErrorCode.WrongStage,
        "Packet 82 module 0 requires an active authenticated session.");
    }

    if (decodedPayload is not Packet82LiquidData request || request.Entries is null)
    {
      return ModuleDispatchResult.Rejected(
        ProtocolErrorCode.MalformedFrame,
        "Packet 82 module 0 requires a decoded liquid entry list.");
    }

    if (request.Entries.Count > _maximumEntries)
    {
      return ModuleDispatchResult.Rejected(
        ProtocolErrorCode.InvalidLength,
        "Packet 82 module 0 exceeds the bounded liquid update budget.");
    }

    ulong? worldEpoch = _resolveWorldEpoch(envelope);
    if (worldEpoch is not ulong epoch || epoch == 0 || !_worldOwner.IsCurrentWorldEpoch(epoch))
    {
      return ModuleDispatchResult.Rejected(
        ProtocolErrorCode.NotReady,
        "Packet 82 module 0 has no current world epoch.");
    }

    var updates = new List<LiquidUpdate>(request.Entries.Count);
    foreach (Packet82LiquidEntry? entry in request.Entries)
    {
      if (entry is null)
      {
        return ModuleDispatchResult.Rejected(
          ProtocolErrorCode.MalformedFrame,
          "Packet 82 module 0 contains a null liquid entry.");
      }

      int x = (entry.PackedTileCoordinate >> 16) & 0xffff;
      int y = entry.PackedTileCoordinate & 0xffff;
      if (entry.LiquidType > (byte)LiquidType.Shimmer)
      {
        return ModuleDispatchResult.Rejected(
          ProtocolErrorCode.MalformedFrame,
          "Packet 82 module 0 contains an unknown liquid type.");
      }

      updates.Add(new LiquidUpdate(
        new TileCoordinate(x, y),
        entry.Amount,
        (LiquidType)entry.LiquidType));
    }

    WorldMutationResult result = _worldOwner.ApplyLiquidModuleValues(epoch, updates, _maximumEntries);
    return result.Applied
      ? ModuleDispatchResult.Handled(result.Version)
      : ModuleDispatchResult.Rejected(ProtocolErrorCode.Conflict, result.Rejection!);
  }
}

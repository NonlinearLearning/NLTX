using Terraria.Relationships;
using Terraria.WorldGeneration.Systems;

namespace NSSLC.Infrastructure.Network;

/// <summary>
/// Connects one completed prototype framing effect to section-scoped packet 11 publication.
/// </summary>
public sealed class WorldGenerationTileFrameRepairAdapter {
  private readonly PacketGateway _gateway;
  private readonly WorldSynchronizationPacketHandlers _worldSections;
  private readonly EntityRuntimeId _worldRuntimeId;

  public WorldGenerationTileFrameRepairAdapter(PacketGateway gateway,
      WorldSynchronizationPacketHandlers worldSections, EntityRuntimeId worldRuntimeId) {
    _gateway = gateway ?? throw new ArgumentNullException(nameof(gateway));
    _worldSections = worldSections
        ?? throw new ArgumentNullException(nameof(worldSections));
    if (!worldRuntimeId.IsAssigned) {
      throw new ArgumentException("A frame repair needs a current world runtime.",
          nameof(worldRuntimeId));
    }
    _worldRuntimeId = worldRuntimeId;
  }

  /// <summary>
  /// Publishes only successful SetFrames results with the exact range returned by the effect
  /// port. Debug actions and failed or empty framing results have no network effect.
  /// </summary>
  public ValueTask<TileFrameRepairPublicationResult> PublishAsync(
      WorldGenerationTileFramingAndDebugSystem.Result framingResult,
      CancellationToken cancellationToken = default) {
    if (!framingResult.Accepted || !framingResult.FramesApplied
        || framingResult.AffectedRegion is not WorldGenerationTileFramingAndDebugSystem.TileFrameRegion region) {
      return ValueTask.FromResult(TileFrameRepairPublicationResult.Rejected(
          "FrameRepairNotApplied", 0));
    }

    region.Validate();
    return _worldSections.PublishFrameRepairAsync(
        _gateway,
        _worldRuntimeId,
        region.StartX,
        region.StartY,
        region.EndXInclusive,
        region.EndYInclusive,
        cancellationToken);
  }
}

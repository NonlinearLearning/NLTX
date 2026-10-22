using MinimalImplementation.Contracts;
using MinimalImplementation.Server.World;

namespace MinimalImplementation.Server.Host;

/// <summary>Detached section bytes captured on the commit loop for one transfer token.</summary>
internal sealed class WorldSectionSendBatch {
  internal sealed record Section(
    WorldSectionCoordinate Coordinate,
    CommitVersion Version,
    IReadOnlyList<OutboundPacketIntent> Fragments);

  public WorldSectionSendBatch(
      WorldSectionTransferToken token, IReadOnlyCollection<Section> sections) {
    Token = token;
    Sections = Array.AsReadOnly(sections.ToArray());
  }

  public WorldSectionTransferToken Token { get; }

  public IReadOnlyList<Section> Sections { get; }
}

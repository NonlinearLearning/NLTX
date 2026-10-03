namespace Terraria.Network;

public sealed class OutboundDispatch {
  public object Packet { get; }
  public PacketDispatchKind Kind { get; }
  public IReadOnlyList<ConnectionIdentity> Targets { get; }
  public NetworkSessionStage AllowedStages { get; }
  public Guid WorldKey { get; }
  public long WorldGeneration { get; }
  public SectionCoordinate Section { get; }
  public long? SnapshotRevision { get; }
  public string SnapshotVariant { get; }

  public OutboundDispatch(object packet, PacketDispatchKind kind,
      IEnumerable<ConnectionIdentity>? targets = null,
      NetworkSessionStage allowedStages = NetworkSessionStage.Active,
      Guid worldKey = default, long worldGeneration = 0, SectionCoordinate section = default,
      long? snapshotRevision = null, string snapshotVariant = "default") {
    ArgumentNullException.ThrowIfNull(packet);
    if (!Enum.IsDefined(kind)) {
      throw new ArgumentOutOfRangeException(nameof(kind));
    }
    ConnectionIdentity[] targetArray = targets?.Distinct().ToArray() ?? [];
    if ((kind == PacketDispatchKind.Single && targetArray.Length != 1)
        || (kind == PacketDispatchKind.ExplicitTargets && targetArray.Length == 0)) {
      throw new ArgumentException("Explicit routing requires current connection identities.");
    }
    Packet = packet;
    Kind = kind;
    Targets = Array.AsReadOnly(targetArray);
    AllowedStages = allowedStages;
    WorldKey = worldKey;
    WorldGeneration = worldGeneration;
    Section = section;
    if (snapshotRevision is < 0
        || (snapshotRevision.HasValue && kind != PacketDispatchKind.SectionSubscribers)) {
      throw new ArgumentException("Shared snapshot revision requires section-subscriber routing.");
    }
    SnapshotRevision = snapshotRevision;
    SnapshotVariant = snapshotVariant;
  }
}

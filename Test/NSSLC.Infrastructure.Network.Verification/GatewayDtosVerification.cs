using Terraria.Network;

namespace NSSLC.NetworkVerification;

internal static class GatewayDtosVerification {
  public static Task RunAsync() {
    var targets = new List<ConnectionIdentity> {
      new(Guid.NewGuid(), 1), new(Guid.NewGuid(), 2)
    };
    var dispatch = new OutboundDispatch(new BindingVerification.BytePacket(1),
        PacketDispatchKind.ExplicitTargets, targets);
    targets.Clear();
    Verify.That(dispatch.Targets.Count == 2,
        "An application dispatch must own its target projection against later producer mutation.");
    Verify.Throws<ArgumentException>(() => new OutboundDispatch(new BindingVerification.BytePacket(),
        PacketDispatchKind.Single));
    Verify.Throws<ArgumentException>(() => new OutboundDispatch(new BindingVerification.BytePacket(),
        PacketDispatchKind.ExplicitTargets));
    Verify.Throws<ArgumentOutOfRangeException>(() => new OutboundDispatch(
        new BindingVerification.BytePacket(), (PacketDispatchKind)99));

    var outbound = new List<OutboundDispatch> { dispatch };
    var result = new PacketHandlingResult(true, outbound);
    outbound.Clear();
    Verify.That(result.Outbound.Count == 1,
        "A committed result must preserve its outbound intent after the owner's list is reused.");

    var sections = new List<SectionCoordinate> { new(2, 3), new(4, 5) };
    Guid world = Guid.NewGuid();
    var interest = new SectionInterestProjection(world, 3, 7, sections);
    sections.Clear();
    Verify.That(interest.WorldKey == world && interest.WorldGeneration == 3 && interest.Revision == 7
        && interest.Sections.Contains(new(2, 3)) && interest.Sections.Count == 2,
        "Section subscription projection must freeze membership and carry world versions.");
    Verify.Throws<ArgumentOutOfRangeException>(() => new SectionInterestProjection(world,
        -1, 7, Array.Empty<SectionCoordinate>()));
    Verify.Throws<ArgumentOutOfRangeException>(() => new SectionInterestProjection(world,
        3, -1, Array.Empty<SectionCoordinate>()));
    return Task.CompletedTask;
  }
}

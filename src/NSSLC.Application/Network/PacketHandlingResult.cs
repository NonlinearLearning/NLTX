namespace Terraria.Network;

public sealed class PacketHandlingResult {
  public bool Accepted { get; }
  public string? RejectionCode { get; }
  public NetworkSessionStage? NextStage { get; }
  public SectionInterestProjection? Interest { get; }
  public IReadOnlyList<OutboundDispatch> Outbound { get; }

  public PacketHandlingResult(bool accepted, IEnumerable<OutboundDispatch>? outbound = null,
      NetworkSessionStage? nextStage = null, SectionInterestProjection? interest = null,
      string? rejectionCode = null) {
    Accepted = accepted;
    RejectionCode = rejectionCode;
    NextStage = nextStage;
    Interest = interest;
    Outbound = Array.AsReadOnly(outbound?.ToArray() ?? []);
  }
}

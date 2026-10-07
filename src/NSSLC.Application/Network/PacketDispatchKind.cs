namespace Terraria.Network;

public enum PacketDispatchKind {
  Single,
  AllActiveExceptSender,
  AllActive,
  ExplicitTargets,
  SectionSubscribers
}

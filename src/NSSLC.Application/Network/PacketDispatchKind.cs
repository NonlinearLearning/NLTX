namespace Terraria.Network;

public enum PacketDispatchKind {
  Single,
  AllActiveExceptSender,
  ExplicitTargets,
  SectionSubscribers
}

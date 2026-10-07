using Terraria.Network;

namespace NSSLC.Infrastructure.Network;

/// <summary>Describes an explicit ingress policy and its registered handler.</summary>
public sealed record PacketRegistrationSnapshot(PacketPolicy Policy, string HandlerType);

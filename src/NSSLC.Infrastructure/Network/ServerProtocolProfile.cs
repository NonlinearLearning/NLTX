using Terraria.Network;

namespace NSSLC.Infrastructure.Network;

/// <summary>Restricts production wire formats to supported message directions.</summary>
public static class ServerProtocolProfile {
  public static ProtocolProfile Create(ProtocolProfile wireProfile) {
    ArgumentNullException.ThrowIfNull(wireProfile);
    PacketBinding[] bindings = wireProfile.Bindings
        .Where(binding => binding.MessageId != 93
            && ReservedPacketRegistration.IsProductionDirectionEnabled(
                binding.MessageId, binding.Direction)
            && IsWorldDirectionEnabled(binding.MessageId, binding.Direction)).ToArray();
    return new ProtocolProfile(wireProfile.Key + ":server-reserved-session-world-v3",
        wireProfile.HelloVersion, bindings);
  }

  private static bool IsWorldDirectionEnabled(byte messageId, PacketDirection direction) {
    return messageId switch {
      7 or 10 or 11 or 18 or 57 or 146 => direction == PacketDirection.ServerToClient,
      8 => direction == PacketDirection.ClientToServer,
      _ => true
    };
  }
}

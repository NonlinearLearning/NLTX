using Terraria.Network;

namespace NSSLC.Infrastructure.Network;

/// <summary>Selects the confirmed Steam 326 wire differences over the generated profile.</summary>
public static class SteamProtocolProfile {
  public static ProtocolProfile Create(ProtocolFacts facts) {
    ProtocolProfile generated = TerrariaProtocolProfile.Create(facts);
    var itemCodec = new SteamItemPacketCodec();
    var bindings = itemCodec.ReplaceBindings(generated.Bindings)
        .Where(binding => binding.MessageId is not (27 or 29))
        .Select(binding => binding.MessageId == 7
            ? new SteamWorldDataPacketBinding(binding) : binding)
        .ToList();
    var projectiles = new SteamProjectilePacketCodec();
    foreach (PacketDirection direction in new[] {
        PacketDirection.ClientToServer, PacketDirection.ServerToClient }) {
      bindings.Add(projectiles.CreateSyncBinding(direction));
      bindings.Add(projectiles.CreateKillBinding(direction));
    }
    return new ProtocolProfile(generated.Key + ":Steam326-items-projectiles-world-v3",
        "Terraria326", bindings);
  }
}

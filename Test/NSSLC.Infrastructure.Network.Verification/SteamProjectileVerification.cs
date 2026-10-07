using System.Buffers.Binary;
using NSSLC.Infrastructure.Network;
using Terraria.Network;
using Terraria.Projectile;

namespace NSSLC.NetworkVerification;

internal static class SteamProjectileVerification {
  public static async Task RunAsync(ProtocolProfile profile) {
    PacketBinding syncBinding = profile.Find(PacketDirection.ClientToServer, (byte)27);
    PacketBinding killBinding = profile.Find(PacketDirection.ClientToServer, (byte)29);
    byte[] syncFrame = Convert.FromHexString(
        "1A001B00110C000000C8420000C8420000000000000000010000");
    byte[] killFrame = Convert.FromHexString("0F001D00110C000000C8420000C842");
    var sync = (SteamProjectileSyncPacket)syncBinding.Decode(syncFrame.AsMemory(3));
    var kill = (SteamProjectileKillPacket)killBinding.Decode(killFrame.AsMemory(3));
    Verify.That(sync.Key == 0x000c1100 && sync.State.Identity == 17
        && sync.State.OwnerSlot == 0 && sync.State.ProjectileType == 1
        && sync.State.Position == new System.Numerics.Vector2(100, 100)
        && sync.State.ProjectileUuid == -1 && kill.Key == sync.Key,
        "Steam 27/29 must decode the packed key and preserve its generation bits.");
    Verify.That(syncBinding.Encode(sync).SequenceEqual(syncFrame)
        && killBinding.Encode(kill).SequenceEqual(killFrame),
        "The Steam projectile fixtures must encode back to the exact frames.");

    uint maximumGenerationKey = (0x3fffu << 18) | (1000u << 8) | 7u;
    var state = new ProjectileNetworkApplyCommand(7, 1000, 1, new(12.5f, -3.25f),
        new(2, -1), 40, 50, 1.5f, 2, 3, 4, bannerIdToRespondTo: 9);
    var optional = new SteamProjectileSyncPacket(maximumGenerationKey, state);
    byte[] optionalFrame = syncBinding.Encode(optional);
    var optionalDecoded = (SteamProjectileSyncPacket)syncBinding.Decode(optionalFrame.AsMemory(3));
    Verify.That(optionalDecoded == optional
        && (optionalDecoded.Key >> 18) == 0x3fff && optionalFrame[25] == 0x7f,
        "All optional projectile fields must round-trip without losing the 14-bit generation.");
    for (int length = 0; length < 23; length++) {
      int prefixLength = length;
      Verify.Throws<PacketProtocolException>(() => syncBinding.Decode(
          syncFrame.AsMemory(3, prefixLength)));
    }
    Verify.Throws<PacketProtocolException>(() => syncBinding.Decode(
        optionalFrame.AsMemory(3, optionalFrame.Length - 4)));
    Verify.Throws<PacketProtocolException>(() => syncBinding.Decode(
        syncFrame.AsMemory(3).ToArray().Concat(new byte[] { 0 }).ToArray()));
    Verify.Throws<PacketProtocolException>(() => killBinding.Decode(killFrame.AsMemory(3, 11)));
    Verify.Throws<PacketProtocolException>(() => killBinding.Decode(
        killFrame.AsMemory(3).ToArray().Concat(new byte[] { 0 }).ToArray()));

    byte[] invalid = syncFrame.AsMemory(3).ToArray();
    invalid[22] = 0x80;
    Verify.Throws<PacketProtocolException>(() => syncBinding.Decode(invalid));
    invalid = syncFrame.AsMemory(3).ToArray();
    BinaryPrimitives.WriteSingleLittleEndian(invalid.AsSpan(4), float.NaN);
    Verify.Throws<PacketProtocolException>(() => syncBinding.Decode(invalid));
    byte[] invalidKill = killFrame.AsMemory(3).ToArray();
    BinaryPrimitives.WriteSingleLittleEndian(invalidKill.AsSpan(8), float.PositiveInfinity);
    Verify.Throws<PacketProtocolException>(() => killBinding.Decode(invalidKill));
    invalid = syncFrame.AsMemory(3).ToArray();
    BinaryPrimitives.WriteUInt32LittleEndian(invalid, 1001u << 8);
    Verify.Throws<PacketProtocolException>(() => syncBinding.Decode(invalid));
    Verify.Throws<PacketEncodingException>(() => syncBinding.Encode(
        optional with { Key = optional.Key ^ 1 }));

    var authority = new RecordingAuthority();
    var observed = new SteamProjectilePacketHandler();
    await using (var gateway = new PacketGateway(profile, authority,
        new PacketGatewayOptions { IgnoreClientVersion = true, UseSteamModuleIds = true })) {
      GatewayVerification.RegisterProgression(gateway);
      gateway.Register<SteamProjectileSyncPacket>(new PacketPolicy(27, NetworkSessionStage.Active),
          observed);
      gateway.Register<SteamProjectileKillPacket>(new PacketPolicy(29, NetworkSessionStage.Active),
          observed);
      await using var peer = new GatewayPeer(gateway, profile);
      await peer.JoinAsync();
      peer.ReceiveRaw(syncFrame);
      peer.ReceiveRaw(killFrame);
      await Verify.EventuallyAsync(() => observed.Snapshot().TryGetValue(0, out var item)
          && item.SyncCount == 1 && item.KillCount == 1,
          "Steam's projectile sync/kill pair must pass the real gateway after spawning.");
      Verify.That(observed.Snapshot()[0].LastKey == sync.Key
          && peer.Session.Stage == NetworkSessionStage.Active,
          "Projectile traffic must retain generation and leave the session Active.");
      byte[] foreign = syncFrame.ToArray();
      foreign[3] = 201;
      peer.ReceiveRaw(foreign);
      await peer.Run.WaitAsync(TimeSpan.FromSeconds(5));
      Verify.That(observed.Snapshot()[0].SyncCount == 1
          && !observed.Snapshot().ContainsKey(201)
          && GatewayVerification.ReadDiagnostics(gateway).Any(item =>
              item.Code == "ProjectileSenderMismatch"),
          "A foreign projectile spawner must be rejected without changing observations.");
    }
    Verify.That(authority.Releases == 1,
        "Projectile rejection must release the authenticated session binding.");
    Console.WriteLine("PASS Steam projectile key layouts, optional values and sender admission");
  }
}

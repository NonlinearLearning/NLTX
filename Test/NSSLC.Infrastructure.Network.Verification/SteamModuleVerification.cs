using NSSLC.Infrastructure.Network;
using Terraria.Network;
using Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

namespace NSSLC.NetworkVerification;

internal static class SteamModuleVerification {
  public static async Task<int> RunAsync() {
    try {
      ProtocolProfile profile = SteamProtocolProfile.Create(
          GeneratedProfileVerification.CreateFacts(steamModules: true));
      PacketBinding binding = profile.Find(PacketDirection.ClientToServer, (byte)82);
      byte[] sliderFrame = Convert.FromHexString("0C005206000E00C90000003F");
      var slider = (NetModulesPacket)binding.Decode(sliderFrame.AsMemory(3));
      Verify.That(slider.ModuleId == 6 && slider.Data is Packet82CreativePowerData {
          PowerId: 14, Value: Packet82PerPlayerSliderPowerState { Player: 201, Value: 0.5f } },
          "Steam module 6 must decode its personal spawn-rate slider, not a sacrifice report.");
      Verify.That(binding.Encode(slider).SequenceEqual(sliderFrame),
          "The Steam slider fixture must encode back to its exact wire frame.");
      Verify.Throws<PacketProtocolException>(() => binding.Decode(sliderFrame.AsMemory(3, 8)));
      Verify.Throws<PacketProtocolException>(() => binding.Decode(
          sliderFrame.AsMemory(3).ToArray().Concat(new byte[] { 0 }).ToArray()));
      var unlock = (NetModulesPacket)binding.Decode(Convert.FromHexString("050008000200"));
      Verify.That(unlock.ModuleId == 5 && unlock.Data is Packet82CreativeUnlockData {
          ItemId: 8, SacrificeCount: 2 }, "Steam module 5 must use its real item-unlock layout.");

      var authority = new RecordingAuthority();
      var observed = new PlayerSpawnRatePacketHandler();
      await using (var gateway = new PacketGateway(profile, authority,
          new PacketGatewayOptions { UseSteamModuleIds = true, IgnoreClientVersion = true,
            EnablePing = true })) {
        GatewayVerification.RegisterProgression(gateway);
        gateway.Register(new PacketPolicy(82, NetworkSessionStage.Active, ModuleId: 5), observed);
        gateway.Register<TeamChangePacket>(new PacketPolicy(45, NetworkSessionStage.Active),
            new PlayerAdmissionPacketHandlers());
        var statePackets = new PlayerAdmissionPacketHandlers();
        gateway.Register<SyncPlayerZonePacket>(new PacketPolicy(36, NetworkSessionStage.Active),
            statePackets);
        gateway.Register<PlayerLifeManaPacket>(new PacketPolicy(16, NetworkSessionStage.Active),
            statePackets);
        gateway.Register<SyncTalkNPCPacket>(new PacketPolicy(40, NetworkSessionStage.Active),
            statePackets);
        gateway.Register<Unknown42Packet>(new PacketPolicy(42, NetworkSessionStage.Active),
            statePackets);
        gateway.Register<PlayerBuffsPacket>(new PacketPolicy(50, NetworkSessionStage.Active),
            statePackets);
        gateway.Register<ItemRotationAndAnimationPacket>(
            new PacketPolicy(41, NetworkSessionStage.Active,
                MaximumPerWindow: 120, MaximumBytesPerWindow: 840), statePackets);
        bool pylonHandled = false;
        gateway.Register(new PacketPolicy(82, NetworkSessionStage.Active, ModuleId: 7, Action: 1),
            new RecordingHandler<NetModulesPacket>((_, packet, _) => {
              Verify.That(packet.ModuleId == 8
                  && packet.Data is Packet82TeleportPylonData { Action: 1, X: 12, Y: 34 },
                  "Steam module selectors must follow the shifted registration order.");
              pylonHandled = true;
              return ValueTask.FromResult(new PacketHandlingResult(true));
            }));
        await using var peer = new GatewayPeer(gateway, profile);
        await peer.JoinAsync();
        peer.ReceiveRaw(sliderFrame);
        await Verify.EventuallyAsync(() => observed.Snapshot().ContainsKey(0),
            "The Steam slider must reach the registered handler without disconnecting.");
        Verify.That(observed.Snapshot()[0] == 0.5f && !observed.Snapshot().ContainsKey(201)
            && peer.Session.Stage == NetworkSessionStage.Active,
            "The slider must be recorded for the bound sender, not its claimed player slot.");
        int beforeTeam = peer.Transport.FrameCount;
        peer.ReceiveRaw(Convert.FromHexString("05002DC901"));
        await Verify.EventuallyAsync(() => peer.Transport.FrameCount > beforeTeam,
            "The post-spawn team packet must be admitted and acknowledged.");
        var team = (TeamChangePacket)profile.Find(PacketDirection.ServerToClient, (byte)45)
            .Decode(peer.Transport.GetFrame(beforeTeam).AsMemory(3));
        Verify.That(team.Player == 0 && team.Team == 1
            && peer.Session.Stage == NetworkSessionStage.Active,
            "A post-spawn team upload must preserve Active and echo the authenticated slot.");
        int beforePeriodic = peer.Transport.FrameCount;
        peer.ReceiveRaw(Convert.FromHexString("0A0024C9010203040506"));
        peer.ReceiveRaw(Convert.FromHexString("080010C964006400"));
        peer.ReceiveRaw(Convert.FromHexString("060028C9FFFF"));
        peer.ReceiveRaw(Convert.FromHexString("08002AC914001400"));
        peer.ReceiveRaw(Convert.FromHexString("060032C90000"));
        await Verify.EventuallyAsync(() => peer.Transport.FrameCount >= beforePeriodic + 2,
            "Periodic zone and conversation uploads must be admitted after spawning.");
        var zone = (SyncPlayerZonePacket)profile.Find(PacketDirection.ServerToClient, (byte)36)
            .Decode(peer.Transport.GetFrame(beforePeriodic).AsMemory(3));
        var talk = (SyncTalkNPCPacket)profile.Find(PacketDirection.ServerToClient, (byte)40)
            .Decode(peer.Transport.GetFrame(beforePeriodic + 1).AsMemory(3));
        Verify.That(zone.Player == 0 && zone.Zone5 == 5 && talk.Player == 0
            && talk.NpcIndex == -1 && peer.Session.Stage == NetworkSessionStage.Active,
            "Steam's periodic player-state burst must keep the authenticated session Active.");
        int beforePing = peer.Transport.FrameCount;
        peer.ReceiveRaw(Convert.FromHexString("0A0029C90000803E1E00"));
        peer.ReceiveRaw(new byte[] { 3, 0, 154 });
        await Verify.EventuallyAsync(() => peer.Transport.FrameCount > beforePing,
            "Steam's latency ping must receive an empty reply after player-state uploads.");
        Verify.That(peer.Transport.GetFrame(beforePing).SequenceEqual(new byte[] { 3, 0, 154 })
            && peer.Session.Stage == NetworkSessionStage.Active,
            "Steam packet 154 must echo through its selected profile without closing the session.");
        Verify.That(peer.Transport.FrameCount == beforePing + 1,
            "Packet 41 must keep Active without reflecting a claimed actor slot to its sender.");
        PacketBinding animationBinding = profile.Find(PacketDirection.ClientToServer, (byte)41);
        byte[] animationBody = Convert.FromHexString("C90000803E1E00");
        Verify.Throws<PacketProtocolException>(() => animationBinding.Decode(
            animationBody.AsMemory(0, 6)));
        Verify.Throws<PacketProtocolException>(() => animationBinding.Decode(
            animationBody.Concat(new byte[] { 0 }).ToArray()));
        peer.Receive(new NetModulesPacket { ModuleId = 8,
          Data = new Packet82TeleportPylonData(1, 12, 34, 0) });
        await Verify.EventuallyAsync(() => pylonHandled,
            "Steam action admission must use the logical module selector.");
        byte[] malformed = Convert.FromHexString("0C005206000E00000000C07F");
        peer.ReceiveRaw(malformed);
        await peer.Run.WaitAsync(TimeSpan.FromSeconds(5));
        Verify.That(observed.Snapshot()[0] == 0.5f
            && GatewayVerification.ReadDiagnostics(gateway).Any(item =>
                item.Code == "UnsupportedCreativePower"),
            "A non-finite slider must be rejected without altering the previous observation.");
      }
      Verify.That(authority.Releases == 1,
          "The Steam session must release its authority binding after closure.");
      await SteamProjectileVerification.RunAsync(profile);
      Console.WriteLine("PASS Steam module layouts, shifted selectors, bound slider and malformed input");
      return 0;
    } catch (Exception exception) {
      Console.Error.WriteLine(exception);
      return 1;
    }
  }
}

using System.Net;
using NSSLC.Infrastructure.Network;
using Terraria.Network;
using Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

namespace NSSLC.NetworkVerification;

internal static class GatewayLoopbackVerification {
  public static async Task RunAsync() {
    ProtocolProfile profile = GatewayVerification.CreateProfile();
    var authority = new RecordingAuthority();
    await using var gateway = new PacketGateway(profile, authority);
    GatewayVerification.RegisterProgression(gateway);
    gateway.Register(new PacketPolicy(30, NetworkSessionStage.Active),
        new RecordingHandler<Packet30Packet>((context, _, _) => ValueTask.FromResult(
            new PacketHandlingResult(true, new[] {
              new OutboundDispatch(new Packet16Packet { Payload = new(context.Actor.PlayerSlot, 40, 100) },
                  PacketDispatchKind.Single, new[] { context.Connection })
            }))));
    var budget = new PacketByteBudget(2 * 1024 * 1024);
    await using var server = gateway.CreateServer(IPAddress.Loopback, 0, budget: budget);
    server.Start();
    var factory = new PacketConnectionFactory(profile, budget: budget);
    await using (IPacketConnection client = await factory.ConnectAsync("127.0.0.1",
        ((IPEndPoint)server.EndPoint).Port, TimeSpan.FromSeconds(5))) {
      await client.WritePacketAsync(new Packet1Packet { Version = "Terraria319" });
      Packet3Packet admitted = (await client.ReadPacketAsync())!.Get<Packet3Packet>();
      Verify.That(admitted.Payload.Player == 0,
          "The real gateway listener must authenticate generated Hello and return its allocated slot.");
      await client.WritePacketAsync(new Packet6Packet());
      await Verify.EventuallyAsync(() => gateway.Sessions.Single().Stage == NetworkSessionStage.AwaitSectionRequest,
          "Real TCP world-data request was not confirmed by its owner.");
      await client.WritePacketAsync(new Packet8Packet());
      await Verify.EventuallyAsync(() => gateway.Sessions.Single().Stage == NetworkSessionStage.Synchronizing,
          "Real TCP section request was not confirmed by its owner.");
      await client.WritePacketAsync(new Packet12Packet());
      await Verify.EventuallyAsync(() => gateway.Sessions.Single().Stage == NetworkSessionStage.Active,
          "Real TCP spawn request was not confirmed by its owner.");
      await client.WritePacketAsync(new Packet30Packet { Payload = new(201, true) });
      Packet16Packet response = (await client.ReadPacketAsync())!.Get<Packet16Packet>();
      Verify.That(response.Payload.Player == 0 && response.Payload.Life == 40,
          "Real TCP dispatch must use the owner result and trusted actor rather than a forged wire slot.");
    }
    await Verify.EventuallyAsync(() => gateway.Sessions.Count == 0 && authority.Releases == 1
        && budget.Used == 0, "Real gateway disconnect did not release binding and network resources.");
    Verify.That(server.LastError is null, "The complete gateway loopback must not record a server error.");
  }
}

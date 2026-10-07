using System.Net;
using NSSLC.Infrastructure.Network;
using Terraria.Network;
using Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;
using Terraria.NonAuthoritative.Persistence;
using Terraria.Relationships;

namespace NSSLC.NetworkVerification;

internal static class GatewayLoopbackVerification {
  public static async Task RunAsync() {
    ProtocolProfile profile = GatewayVerification.CreateProfile();
    var authority = new RecordingAuthority();
    await using var worldOwner = new NetworkWorldOwner(() => new LoadedWorldSession());
    await worldOwner.Ready.WaitAsync(TimeSpan.FromSeconds(5));
    EntityRuntimeId worldRuntimeId = await worldOwner.InvokeAsync(
        session => session.EntityRuntime.RuntimeId);
    NetworkPlayerOwner? players = null;
    await using var gateway = new PacketGateway(profile, authority,
        worldRuntimeIdProvider: () => worldRuntimeId,
        sessionClosing: async (context, token) => {
          NetworkPlayerBindingStatus result = await (players ?? throw new InvalidOperationException(
              "The player owner must be composed before the gateway starts."))
              .DisconnectAsync(context, token).ConfigureAwait(false);
          if (result is not (NetworkPlayerBindingStatus.Disconnected
              or NetworkPlayerBindingStatus.NotFound)) {
            throw new InvalidOperationException($"Player cleanup rejected: {result}.");
          }
        });
    players = new NetworkPlayerOwner(worldOwner, gateway.IsCurrentSender);
    var lifecycleHandlers = new PlayerLifecyclePacketHandlers(players, 4200, 1200);
    gateway.Register(new PacketPolicy(6, NetworkSessionStage.AwaitPlayerData),
        new RecordingHandler<RequestWorldDataPacket>((_, _, _) => ValueTask.FromResult(
            new PacketHandlingResult(true, nextStage: NetworkSessionStage.AwaitSectionRequest))));
    gateway.Register(new PacketPolicy(8, NetworkSessionStage.AwaitSectionRequest),
        new RecordingHandler<SpawnTileDataPacket>((_, _, _) => ValueTask.FromResult(
            new PacketHandlingResult(true, nextStage: NetworkSessionStage.Synchronizing))));
    PlayerLifecyclePacketRegistration.Register(gateway, lifecycleHandlers);
    gateway.Register(new PacketPolicy(30, NetworkSessionStage.Active),
        new RecordingHandler<TogglePVPPacket>((context, _, _) => ValueTask.FromResult(
            new PacketHandlingResult(true, new[] {
              new OutboundDispatch(new PlayerLifeManaPacket { Player = context.Actor.PlayerSlot, Life = 40, MaximumLife = 100 },
                  PacketDispatchKind.Single, new[] { context.Connection })
            }))));
    var budget = new PacketByteBudget(2 * 1024 * 1024);
    await using var server = gateway.CreateServer(IPAddress.Loopback, 0, budget: budget);
    server.Start();
    var factory = new PacketConnectionFactory(profile, budget: budget);
    await using (IPacketConnection client = await factory.ConnectAsync("127.0.0.1",
        ((IPEndPoint)server.EndPoint).Port, TimeSpan.FromSeconds(5))) {
      await client.WritePacketAsync(new HelloPacket { Version = "Terraria319" });
      PlayerInfoPacket admitted = (await client.ReadPacketAsync())!.Get<PlayerInfoPacket>();
      Verify.That(admitted.Player == 0,
          "The real gateway listener must authenticate generated Hello and return its allocated slot.");
      await client.WritePacketAsync(new RequestWorldDataPacket());
      await Verify.EventuallyAsync(() => gateway.Sessions.Single().Stage == NetworkSessionStage.AwaitSectionRequest,
          "Real TCP world-data request was not confirmed by its owner.");
      await client.WritePacketAsync(new SpawnTileDataPacket());
      await Verify.EventuallyAsync(() => gateway.Sessions.Single().Stage == NetworkSessionStage.Synchronizing,
          "Real TCP section request was not confirmed by its owner.");
      await client.WritePacketAsync(new PlayerSpawnPacket());
      await Verify.EventuallyAsync(() => gateway.Sessions.Single().Stage == NetworkSessionStage.Active,
          "Real TCP spawn request was not confirmed by its owner.");
      _ = (await client.ReadPacketAsync())!.Get<FinishedConnectingToServerPacket>();
      await client.WritePacketAsync(new TogglePVPPacket { Player = 201, Hostile = true });
      PlayerLifeManaPacket response = (await client.ReadPacketAsync())!.Get<PlayerLifeManaPacket>();
      Verify.That(response.Player == 0 && response.Life == 40,
          "Real TCP dispatch must use the owner result and trusted actor rather than a forged wire slot.");

      await using (IPacketConnection departing = await factory.ConnectAsync("127.0.0.1",
          ((IPEndPoint)server.EndPoint).Port, TimeSpan.FromSeconds(5))) {
        await departing.WritePacketAsync(new HelloPacket { Version = "Terraria319" });
        PlayerInfoPacket secondAdmission = (await departing.ReadPacketAsync())!.Get<PlayerInfoPacket>();
        Verify.That(secondAdmission.Player == 1,
            "The disconnect projection fixture must use a second authenticated player slot.");
        await departing.WritePacketAsync(new RequestWorldDataPacket());
        await departing.WritePacketAsync(new SpawnTileDataPacket());
        await departing.WritePacketAsync(new PlayerSpawnPacket { Player = secondAdmission.Player });
        try {
          await Verify.EventuallyAsync(() => gateway.Sessions.Count == 2 &&
              gateway.Sessions.All(session => session.Stage == NetworkSessionStage.Active),
              "Both real TCP players must reach Active before disconnect projection.");
        } catch (InvalidOperationException error) {
          string stages = string.Join(", ", gateway.Sessions.Select(session =>
              $"{session.Identity}:{session.Stage}:{session.Binding}"));
          string diagnostics = string.Join("; ", GatewayVerification.ReadDiagnostics(gateway)
              .Select(item => $"{item.Code}/{item.MessageId}"));
          throw new InvalidOperationException($"{error.Message} stages=[{stages}] " +
              $"diagnostics=[{diagnostics}]", error);
        }
        _ = (await departing.ReadPacketAsync())!.Get<FinishedConnectingToServerPacket>();

        PlayerSpawnPacket joinProjection = (await client.ReadPacketAsync())!.Get<PlayerSpawnPacket>();
        PlayerActivePacket connectProjection = (await client.ReadPacketAsync())!.Get<PlayerActivePacket>();
        Verify.That(joinProjection.Player == 1 && connectProjection.Player == 1 &&
            connectProjection.ActiveState == 1,
            "A real TCP join must project the authenticated slot as active to existing clients.");
      }

      PlayerActivePacket disconnectProjection = (await client.ReadPacketAsync())!
          .Get<PlayerActivePacket>();
      Verify.That(disconnectProjection.Player == 1 && disconnectProjection.ActiveState == 0,
          "A real TCP disconnect must project the authenticated slot as inactive to other active clients.");
    }
    await Verify.EventuallyAsync(() => gateway.Sessions.Count == 0 && authority.Releases == 2
        && budget.Used == 0, "Real gateway disconnect did not release binding and network resources.");
    Verify.That(server.LastError is null, "The complete gateway loopback must not record a server error.");
  }
}

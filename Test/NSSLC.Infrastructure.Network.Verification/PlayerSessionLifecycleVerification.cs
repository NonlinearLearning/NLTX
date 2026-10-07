using System.Net;
using EntityEcs;
using NSSLC.Infrastructure.Network;
using Terraria.Network;
using Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;
using Terraria.NonAuthoritative.Persistence;
using Terraria.Relationships;

namespace NSSLC.NetworkVerification;

internal static class PlayerSessionLifecycleVerification {
  public static async Task RunAsync() {
    await using var world = new NetworkWorldOwner(() => new LoadedWorldSession());
    await world.Ready.WaitAsync(TimeSpan.FromSeconds(5));
    EntityRuntimeId runtimeId = await world.InvokeAsync(session => session.EntityRuntime.RuntimeId);
    var authority = new PlayerSlotSessionAuthority(playerSlotCount: 1);
    var cleanupEntered = new TaskCompletionSource(
        TaskCreationOptions.RunContinuationsAsynchronously);
    var allowCleanup = new TaskCompletionSource(
        TaskCreationOptions.RunContinuationsAsynchronously);
    NetworkPlayerOwner? players = null;
    int cleanedPlayers = 0;
    var created = new TaskCompletionSource<(NetworkSessionContext, NetworkPlayerSnapshot)>(
        TaskCreationOptions.RunContinuationsAsynchronously);
    ProtocolProfile profile = ServerProtocolProfile.Create(
        TerrariaProtocolProfile.Create(GeneratedProfileVerification.CreateFacts()));
    await using var host = new NetworkGatewayHost(IPAddress.Loopback, 0, profile, authority,
        worldRuntimeIdProvider: () => runtimeId,
        sessionClosing: async (context, token) => {
          Verify.That(context.Stage == NetworkSessionStage.Closing && authority.ActiveBindings == 1,
              "Cleanup must receive the closing binding before its slot returns to the pool.");
          cleanupEntered.TrySetResult();
          await allowCleanup.Task.WaitAsync(token);
          NetworkPlayerBindingStatus result = await players!.DisconnectAsync(context, token);
          Verify.That(result == NetworkPlayerBindingStatus.Disconnected &&
              authority.ActiveBindings == 1,
              "The player must be deleted before the authority releases its slot.");
          Interlocked.Increment(ref cleanedPlayers);
        });
    players = new NetworkPlayerOwner(world, host.Gateway.IsCurrentSender);
    host.Gateway.Register(new PacketPolicy(6, NetworkSessionStage.AwaitPlayerData),
        new RecordingHandler<RequestWorldDataPacket>(async (context, _, token) => {
          NetworkPlayerBindingResult result = await players.EnsurePlayerAsync(context, token);
          if (!result.Succeeded || result.Player is not NetworkPlayerSnapshot snapshot) {
            throw new InvalidOperationException("The current TCP sender did not create a player.");
          }
          created.TrySetResult((context, snapshot));
          return new PacketHandlingResult(true);
        }));
    host.Start();
    int port = ((IPEndPoint)host.EndPoint).Port;

    await using IPacketConnection first = await ConnectAsync(host, port);
    await first.WritePacketAsync(new RequestWorldDataPacket());
    (NetworkSessionContext oldContext, NetworkPlayerSnapshot oldPlayer) =
        await created.Task.WaitAsync(TimeSpan.FromSeconds(5));
    Verify.That(host.Gateway.IsCurrentSender(oldContext),
        "The gateway must recognize its authenticated live sender.");
    Verify.That(!host.Gateway.IsCurrentSender(oldContext with {
          Connection = oldContext.Connection with { Epoch = oldContext.Connection.Epoch + 1 }
        }) && !host.Gateway.IsCurrentSender(oldContext with { ProfileKey = "other-profile" }) &&
        !host.Gateway.IsCurrentSender(oldContext with {
          Actor = oldContext.Actor with { GameSessionKey = Guid.NewGuid() }
        }) && !host.Gateway.IsCurrentSender(oldContext with {
          WorldRuntimeId = new EntityRuntimeId(Guid.NewGuid())
        }) && !host.Gateway.IsCurrentSender(oldContext with { Stage = NetworkSessionStage.Active }),
        "Epoch, profile, actor, world and stage changes must invalidate a captured sender.");

    await first.DisposeAsync();
    try {
      await cleanupEntered.Task.WaitAsync(TimeSpan.FromSeconds(5));
      Verify.That(!host.Gateway.IsCurrentSender(oldContext),
          "Closing must invalidate queued sender work while entity cleanup is still waiting.");
      NetworkPlayerBindingResult rejected = await players.EnsurePlayerAsync(oldContext);
      Verify.That(rejected.Status == NetworkPlayerBindingStatus.RejectedSenderBinding &&
          authority.ActiveBindings == 1,
          "A stale command must not recreate a player or release its occupied slot.");
    } finally {
      allowCleanup.TrySetResult();
    }
    await Verify.EventuallyAsync(() => host.Gateway.Sessions.Count == 0 &&
        authority.ActiveBindings == 0 && Volatile.Read(ref cleanedPlayers) == 1,
        "Completed cleanup must remove the network session and free exactly one binding.");
    Verify.That(!await world.InvokeAsync(session =>
        session.EntityRuntime.TryResolve(oldPlayer.Reference, out _)),
        "A disconnected player's reference must no longer resolve in the shared world.");

    created = new TaskCompletionSource<(NetworkSessionContext, NetworkPlayerSnapshot)>(
        TaskCreationOptions.RunContinuationsAsynchronously);
    await using IPacketConnection second = await ConnectAsync(host, port);
    await second.WritePacketAsync(new RequestWorldDataPacket());
    (NetworkSessionContext newContext, NetworkPlayerSnapshot newPlayer) =
        await created.Task.WaitAsync(TimeSpan.FromSeconds(5));
    Verify.That(newPlayer.PlayerSlot == oldPlayer.PlayerSlot &&
        newPlayer.Reference != oldPlayer.Reference && host.Gateway.IsCurrentSender(newContext),
        "A recycled TCP slot must get a distinct live player identity.");
    Verify.That(await players.DisconnectAsync(oldContext) == NetworkPlayerBindingStatus.NotFound &&
        await world.InvokeAsync(session =>
          session.EntityRuntime.TryResolve(newPlayer.Reference, out _)),
        "Repeated old cleanup must preserve the player that now occupies the same slot.");
    await second.DisposeAsync();
    await Verify.EventuallyAsync(() => authority.ActiveBindings == 0 &&
        Volatile.Read(ref cleanedPlayers) == 2,
        "The second player's disconnect must run the same complete cleanup path.");
  }

  private static async Task<IPacketConnection> ConnectAsync(NetworkGatewayHost host, int port) {
    IPacketConnection connection = await host.Connections.ConnectAsync(
        "127.0.0.1", port, TimeSpan.FromSeconds(5));
    try {
      await connection.WritePacketAsync(new HelloPacket { Version = "Terraria319" });
      PacketMessage? response = await connection.ReadPacketAsync().AsTask()
          .WaitAsync(TimeSpan.FromSeconds(5));
      Verify.That(response?.MessageId == 3 && response.Get<PlayerInfoPacket>().Player == 0,
          "The formal authority must assign slot zero over TCP.");
      return connection;
    } catch {
      await connection.DisposeAsync();
      throw;
    }
  }
}

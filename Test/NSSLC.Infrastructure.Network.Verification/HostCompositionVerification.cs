using System.Net;
using NSSLC.Infrastructure.Network;
using Terraria.Network;
using Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

namespace NSSLC.NetworkVerification;

internal static class HostCompositionVerification {
  public static async Task RunAsync() {
    var authority = new RecordingAuthority();
    var budget = new PacketByteBudget(2 * 1024 * 1024);
    await using var host = new NetworkGatewayHost(IPAddress.Loopback, 0,
        GeneratedProfileVerification.CreateFacts(), authority, budget: budget);
    var ownerHandled = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
    host.Gateway.Register(new PacketPolicy(6, NetworkSessionStage.AwaitPlayerData),
        new RecordingHandler<Packet6Packet>((context, _, _) => {
          Verify.That(context.Actor.PlayerSlot == 0 && context.ProfileKey == host.Profile.Key,
              "Host registration must bind its owner to the selected generated profile and actor.");
          ownerHandled.TrySetResult();
          return ValueTask.FromResult(new PacketHandlingResult(true,
              nextStage: NetworkSessionStage.AwaitSectionRequest));
        }));
    var cacheKey = new PacketSnapshotCacheKey(host.Profile.Key, Guid.NewGuid(), 1,
        new(0, 0), 1, "public");
    Verify.That(await host.Snapshots.TryStoreAsync(cacheKey, SnapshotCacheVerification.CreateSection()),
        "Host composition must share its process budget with the snapshot cache.");
    host.Start();
    Verify.Throws<InvalidOperationException>(() => host.Gateway.Register(
        new PacketPolicy(30, NetworkSessionStage.Active),
        new RecordingHandler<Packet30Packet>((_, _, _) => ValueTask.FromResult(new PacketHandlingResult(true)))));
    await using (IPacketConnection client = await host.Connections.ConnectAsync("127.0.0.1",
        ((IPEndPoint)host.EndPoint).Port, TimeSpan.FromSeconds(5))) {
      await client.WritePacketAsync(new Packet1Packet { Version = "Terraria319" });
      Verify.That((await client.ReadPacketAsync())!.Get<Packet3Packet>().Payload.Player == 0,
          "The host's real listener must admit and answer its generated client Hello.");
      await client.WritePacketAsync(new Packet6Packet());
      await ownerHandled.Task.WaitAsync(TimeSpan.FromSeconds(5));
      await Verify.EventuallyAsync(() => host.Gateway.Sessions.Single().Stage
          == NetworkSessionStage.AwaitSectionRequest,
          "Owner registered after host construction must remain active after Start seals registration.");
    }
    await Verify.EventuallyAsync(() => host.Gateway.Sessions.Count == 0 && authority.Releases == 1,
        "Host listener disconnect must release its owner binding.");
    await host.DisposeAsync();
    Verify.That(budget.Used == 0 && host.Snapshots.Count == 0 && host.LastTransportError is null,
        "Host stop must close connections and release all shared cache/process bytes.");
    Verify.Throws<ObjectDisposedException>(host.Start);
  }
}

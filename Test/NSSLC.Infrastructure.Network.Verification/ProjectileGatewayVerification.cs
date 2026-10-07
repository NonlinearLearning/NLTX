using System.Net;
using System.Numerics;
using NSSLC.Infrastructure.Network;
using Terraria.Network;
using Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;
using Terraria.Projectile;

namespace NSSLC.NetworkVerification;

internal static class ProjectileGatewayVerification {
  private sealed class RecordingOwner : IProjectileNetworkCommandOwner {
    public TaskCompletionSource<ProjectileNetworkApplyCommand> Applied { get; } =
        new(TaskCreationOptions.RunContinuationsAsynchronously);
    public TaskCompletionSource<ProjectileNetworkTerminateCommand> Terminated { get; } =
        new(TaskCreationOptions.RunContinuationsAsynchronously);

    public ValueTask<PacketHandlingResult> ApplyProjectileAsync(NetworkSessionContext sender,
        ProjectileNetworkApplyCommand command, CancellationToken cancellationToken) {
      Applied.TrySetResult(command);
      return ValueTask.FromResult(new PacketHandlingResult(true));
    }

    public ValueTask<PacketHandlingResult> TerminateProjectileAsync(NetworkSessionContext sender,
        ProjectileNetworkTerminateCommand command, CancellationToken cancellationToken) {
      Terminated.TrySetResult(command);
      return ValueTask.FromResult(new PacketHandlingResult(true));
    }
  }

  public static async Task RunAsync() {
    var codec27 = new ProjectilePacket27Codec();
    var command = new ProjectileNetworkApplyCommand(201, 17, 1, new(12.5f, -3.25f),
        new(2f, -1f), 40, 50, 1.5f, 2f, 3f, 4f, bannerIdToRespondTo: 9);
    Verify.That(codec27.TryMapToPacket(command, out SyncProjectilePacket packet),
        "A projectile command must map to the direct-member packet.");
    byte[] body = codec27.Encode(command, includeUuid: false);
    Verify.That(codec27.TryDecode(body, out ProjectileNetworkApplyCommand decoded)
        && decoded == command, "Packet 27 must round-trip all optional projectile values.");
    Verify.That(codec27.Decode(body.AsSpan(0, body.Length - 1)).Status
        == ProjectilePacket27DecodeStatus.Truncated,
        "Truncated packet 27 must fail without committing a command.");
    Verify.That(codec27.Decode(body.Concat(new byte[] { 0 }).ToArray()).Status
        == ProjectilePacket27DecodeStatus.TrailingBytes,
        "Packet 27 must reject bytes beyond the complete body.");
    var withUuid = new ProjectileNetworkApplyCommand(201, 17, 1, Vector2.Zero,
        Vector2.Zero, 0, 0, 0f, projectileUuid: 999);
    Verify.That(codec27.TryDecode(codec27.Encode(withUuid, includeUuid: true), out decoded)
        && decoded == withUuid, "The raw adapter must preserve a valid UUID value.");
    Verify.That(!codec27.TryEncode(withUuid, includeUuid: false, out _),
        "UUID presence must agree with the raw adapter's explicit protocol fact.");

    var codec29 = new ProjectilePacket29Codec();
    var termination = new ProjectileNetworkTerminateCommand(201, 17);
    Verify.That(codec29.TryMapToPacket(termination, out KillProjectilePacket kill)
        && codec29.TryDecode(new byte[] { 17, 0, 201 }, out var decodedKill)
        && decodedKill == termination, "Packet 29 must preserve owner and projectile identity.");
    Verify.That(codec29.Decode(new byte[] { 17, 0 }).Status
        == ProjectilePacket29DecodeStatus.Truncated
        && codec29.Decode(new byte[] { 17, 0, 201, 0 }).Status
        == ProjectilePacket29DecodeStatus.TrailingBytes,
        "Packet 29 must reject truncated bodies and trailing bytes.");

    foreach (short projectileType in new short[] { 1, 949 }) {
      var authority = new RecordingAuthority();
      var owner = new RecordingOwner();
      await using var host = new NetworkGatewayHost(IPAddress.Loopback, 0,
          GeneratedProfileVerification.CreateFacts(), authority);
      GatewayVerification.RegisterProgression(host.Gateway);
      ProjectilePacketGatewayRegistration.Register(host.Gateway, owner,
          new PacketPolicy(27, NetworkSessionStage.Active),
          new PacketPolicy(29, NetworkSessionStage.Active));
      host.Start();
      await using (IPacketConnection client = await host.Connections.ConnectAsync("127.0.0.1",
          ((IPEndPoint)host.EndPoint).Port, TimeSpan.FromSeconds(5))) {
        await client.WritePacketAsync(new HelloPacket { Version = "Terraria319" });
        Verify.That((await client.ReadPacketAsync())!.Get<PlayerInfoPacket>().Player == 0,
            "Projectile integration must establish an authenticated TCP connection.");
        await client.WritePacketAsync(new RequestWorldDataPacket());
        await Verify.EventuallyAsync(() => host.Gateway.Sessions.Single().Stage
            == NetworkSessionStage.AwaitSectionRequest, "World request did not advance.");
        await client.WritePacketAsync(new SpawnTileDataPacket());
        await Verify.EventuallyAsync(() => host.Gateway.Sessions.Single().Stage
            == NetworkSessionStage.Synchronizing, "Section request did not advance.");
        await client.WritePacketAsync(new PlayerSpawnPacket());
        await Verify.EventuallyAsync(() => host.Gateway.Sessions.Single().Stage
            == NetworkSessionStage.Active, "Spawn request did not advance.");
        packet.ProjectileType = projectileType;
        await client.WritePacketAsync(packet);
        ProjectileNetworkApplyCommand applied = await owner.Applied.Task.WaitAsync(
            TimeSpan.FromSeconds(5));
        Verify.That(applied.OwnerSlot == (projectileType == 949 ? byte.MaxValue : 0)
            && applied.Identity == 17 && applied.Position == command.Position
            && applied.Damage == 40 && applied.Ai2 == 4f,
            "The owner must receive trusted sender identity and the decoded projectile values.");
        await client.WritePacketAsync(kill);
        ProjectileNetworkTerminateCommand terminated = await owner.Terminated.Task.WaitAsync(
            TimeSpan.FromSeconds(5));
        Verify.That(terminated.OwnerSlot == 0 && terminated.Identity == 17,
            "Termination must use the authenticated owner instead of the forged wire slot.");
      }
      await Verify.EventuallyAsync(() => host.Gateway.Sessions.Count == 0
          && authority.Releases == 1 && host.Budget.Used == 0,
          "Projectile TCP integration must release the session and all network bytes.");
      Verify.That(host.LastTransportError is null, "Projectile TCP transport must complete cleanly.");
    }
  }
}

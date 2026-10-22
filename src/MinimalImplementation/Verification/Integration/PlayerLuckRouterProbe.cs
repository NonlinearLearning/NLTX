using System.Buffers.Binary;
using System.Diagnostics;
using System.Globalization;
using System.Net;
using System.Net.Sockets;
using MinimalImplementation.Contracts;
using MinimalImplementation.Server.Host;
using MinimalImplementation.Server.Player;
using MinimalImplementation.Server.Protocol;
using MinimalImplementation.Server.Session;
using Terraria.NetWork.Prototype.PacketDesignCompiler;
using Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;
using Terraria.NetWork.Prototype.PacketDesignCompiler.Wire;

namespace MinimalImplementation.Verification.Integration;

internal static class PlayerLuckRouterProbe
{
  private const int MaximumWorldFileBytes = 64 * 1024 * 1024;
  private const int ProbeTimeoutSeconds = 10;

  public static async Task<int> RunAsync(string? requestedOutputRoot)
  {
    string outputRoot = Path.GetFullPath(requestedOutputRoot ?? Path.Combine(
      "Build",
      "diagnostics",
      "MinimalImplementation",
      "PlayerLuckRouter",
      DateTime.UtcNow.ToString("yyyyMMdd-HHmmssfff", CultureInfo.InvariantCulture)));
    if (Directory.Exists(outputRoot))
    {
      throw new ArgumentException(
        "The Player luck router probe requires a fresh output directory.");
    }

    Directory.CreateDirectory(outputRoot);
    string resultPath = Path.Combine(outputRoot, "player-luck-router-result.md");
    using var cancellation = new CancellationTokenSource(TimeSpan.FromSeconds(ProbeTimeoutSeconds));
    try
    {
      await VerifyAsync(outputRoot, cancellation.Token).ConfigureAwait(false);
      File.WriteAllText(resultPath, """
        # Packet 134 Player luck router probe

        Status: PASS

        A real loopback listener accepted two connections. Four client-to-server Packet 134 frames
        traversed the production frame assembler, Version4 decoder and inbound router. The first
        two were rejected before authentication; the third refreshed the router's concurrent
        stage snapshot after the explicit Active fixture transition; the fourth committed through
        the authenticated Player owner and returned a decodable Packet 134 projection to the other
        authenticated connection. The submitted player slot was deliberately different from the
        bound slot; the response used the authenticated binding. The server-owned Player snapshot
        and projection matched, and current luck was recalculated from captured Lantern and buff
        facts.

        Boundary: targeted route/transport probe, not a fixed SC scenario. The Active transition
        is an explicit fixture after Hello preparation/commit; this does not prove the complete
        network join/admission flow, the full inbound dispatcher, or P08-B readiness.
        """);
      Console.WriteLine($"PlayerLuckRouter PASS evidence={resultPath}");
      return 0;
    }
    catch (Exception exception)
    {
      File.WriteAllText(
        resultPath,
        $"# Packet 134 Player luck router probe\n\nStatus: FAIL\n\n```text\n{exception}\n```\n");
      Console.Error.WriteLine($"PlayerLuckRouter FAIL {exception}");
      return 1;
    }
  }

  private static async Task VerifyAsync(string outputRoot, CancellationToken cancellationToken)
  {
    using var reservation = new TcpListener(IPAddress.Loopback, 0);
    reservation.Start();
    int port = ((IPEndPoint)reservation.LocalEndpoint).Port;
    reservation.Stop();

    await using var composition = ServerComposition.Create(new ServerOptions(
      "127.0.0.1",
      port,
      Path.Combine(outputRoot, "unused-fixture.wld"),
      MaximumWorldFileBytes,
      commitQueueCapacity: 64));
    await composition.CommitLoop.StartAsync().ConfigureAwait(false);

    var router = new Version4InboundRouter(composition);
    int handledPacketCount = 0;

    async ValueTask RoutePacketAsync(
      InboundVersion4Packet packet,
      CancellationToken token)
    {
      await router.OnPacket(packet, token).ConfigureAwait(false);
      Interlocked.Increment(ref handledPacketCount);
    }

    ServerConnectionHost host = composition.AttachConnectionHost(
      composition.Version4WireProfile,
      router.AllocateEpoch,
      router.CreateEnvelope,
      RoutePacketAsync,
      router.OnClosed,
      router.OnConnectionFailure,
      onConnected: router.OnConnected);
    router.AttachConnectionHost(host);
    await host.StartAsync(cancellationToken).ConfigureAwait(false);

    using var client = new TcpClient();
    using var recipient = new TcpClient();
    await client.ConnectAsync(IPAddress.Loopback, port, cancellationToken).ConfigureAwait(false);
    SessionEpoch epoch = SessionEpoch.Create(1);
    await WaitForOpenConnectionsAsync(composition, host, 1, cancellationToken)
      .ConfigureAwait(false);
    await recipient.ConnectAsync(IPAddress.Loopback, port, cancellationToken).ConfigureAwait(false);
    SessionEpoch recipientEpoch = SessionEpoch.Create(2);
    await WaitForOpenConnectionsAsync(composition, host, 2, cancellationToken)
      .ConfigureAwait(false);

    NetworkStream stream = client.GetStream();
    NetworkStream recipientStream = recipient.GetStream();
    var packet = CreateLuckPacket(player: 7);
    await SendPacketAsync(composition.Version4WireProfile, stream, packet, cancellationToken)
      .ConfigureAwait(false);
    await WaitForHandledPacketCountAsync(
      1,
      () => Volatile.Read(ref handledPacketCount),
      cancellationToken)
      .ConfigureAwait(false);
    Require(
      router.LastPolicyRejection?.Contains("stage", StringComparison.OrdinalIgnoreCase) == true,
      "Packet 134 before session admission was not rejected by the stage gate.");

    await SendPacketAsync(composition.Version4WireProfile, stream, packet, cancellationToken)
      .ConfigureAwait(false);
    await WaitForHandledPacketCountAsync(
      2,
      () => Volatile.Read(ref handledPacketCount),
      cancellationToken)
      .ConfigureAwait(false);
    bool sequenceUnchanged = false;
    await composition.CommitLoop.SubmitAsync(
      () => sequenceUnchanged = composition.SessionAdmission.Capture(epoch)?.LastSequence is null,
      cancellationToken).ConfigureAwait(false);
    Require(sequenceUnchanged,
      "Rejected pre-admission Packet 134 frames advanced the authenticated sequence.");

    Player? activePlayer = null;
    SessionBinding? binding = null;
    PlayerLuck expectedLuck = PlayerLuck.Empty;
    CommitVersion beforeVersion = CommitVersion.Initial;
    await composition.CommitLoop.SubmitAsync(
      () =>
      {
        PrepareActiveFixture(composition, epoch);
        PrepareActiveFixture(composition, recipientEpoch);
        Require(
          composition.SessionAdmission.TryGetStage(recipientEpoch, out SessionStage recipientStage)
            && recipientStage == SessionStage.Active
            && composition.SessionAdmission.TryGetBinding(recipientEpoch, out SessionBinding? _),
          "The Packet 134 projection recipient is not an authenticated Active session.");
        Require(
          composition.SessionAdmission.TryGetBinding(epoch, out binding) && binding is not null,
          "The Active fixture has no authenticated binding.");
        Require(
          composition.Players.TryGetBoundPlayer(
            epoch, composition.SessionAdmission, out activePlayer)
            && activePlayer is not null,
          "The Active fixture has no authoritative Player.");

        beforeVersion = activePlayer!.Version;
        PlayerLuckCalculationContext context = new(
          composition.LanternNightState.CaptureSnapshot().LanternsUp,
          activePlayer.Buffs.HasBuff(Version4PlayerBuffCatalog.StinkyBuffType));
        PlayerLuck factors = new PlayerLuck(
          packet.LadyBugLuckTime,
          packet.TorchLuck,
          packet.LuckPotion,
          packet.HasGardenGnome,
          packet.BrokenMirrorBadLuck,
          packet.EquipmentLuckBonus,
          packet.CoinLuck,
          packet.KiteLuckLevel,
          0f).WithAuthoritativeBuffs(activePlayer.Buffs);
        expectedLuck = factors.WithCurrentLuck(PlayerLuckCalculator.Calculate(
          factors,
          activePlayer.Appearance.HasUsedGalaxyPearl,
          context));
      },
      cancellationToken).ConfigureAwait(false);

    await SendPacketAsync(composition.Version4WireProfile, stream, packet, cancellationToken)
      .ConfigureAwait(false);
    await WaitForHandledPacketCountAsync(
      3,
      () => Volatile.Read(ref handledPacketCount),
      cancellationToken)
      .ConfigureAwait(false);
    Require(
      router.LastPolicyRejection?.Contains("stage", StringComparison.OrdinalIgnoreCase) == true,
      "The stale router stage did not reject the stage-refresh frame.");

    await SendPacketAsync(composition.Version4WireProfile, stream, packet, cancellationToken)
      .ConfigureAwait(false);
    byte[] responseFrame = await ReadFrameAsync(recipientStream, cancellationToken)
      .ConfigureAwait(false);
    Require(
      responseFrame[PacketFrameCodec.LengthPrefixBytes] ==
        SessionAdmissionCoordinator.UpdatePlayerLuckFactorsMessageId,
      "The successful Packet 134 route returned an unexpected message id.");
    object decoded = composition.Version4WireProfile.Decode(
      WireDirection.ServerToClient,
      SessionAdmissionCoordinator.UpdatePlayerLuckFactorsMessageId,
      responseFrame.AsMemory(PacketFrameCodec.HeaderBytes));
    Require(
      decoded is UpdatePlayerLuckFactorsPacket response,
      "The successful Packet 134 route returned an invalid response payload.");

    await WaitForHandledPacketCountAsync(
      4,
      () => Volatile.Read(ref handledPacketCount),
      cancellationToken)
      .ConfigureAwait(false);
    PlayerLuck actualLuck = PlayerLuck.Empty;
    CommitVersion afterVersion = CommitVersion.Initial;
    await composition.CommitLoop.SubmitAsync(
      () =>
      {
        Require(
          composition.Players.TryGetBoundPlayer(
            epoch, composition.SessionAdmission, out Player? current)
            && ReferenceEquals(current, activePlayer),
          "The authenticated Player binding changed while the packet was routed.");
        actualLuck = activePlayer!.Luck;
        afterVersion = activePlayer.Version;
      },
      cancellationToken).ConfigureAwait(false);

    Require(afterVersion.Value > beforeVersion.Value,
      "The accepted Packet 134 did not advance the Player owner version.");
    Require(actualLuck == expectedLuck,
      "The authenticated Player owner did not commit the calculated luck factors.");
    Require(decoded is UpdatePlayerLuckFactorsPacket responsePacket
        && responsePacket.Player == binding!.PlayerSlot
        && responsePacket.Player != packet.Player
        && responsePacket.LadyBugLuckTime == expectedLuck.LadyBugLuckTime
        && responsePacket.TorchLuck == expectedLuck.TorchLuck
        && responsePacket.LuckPotion == expectedLuck.LuckPotion
        && responsePacket.HasGardenGnome == expectedLuck.HasGardenGnome
        && responsePacket.BrokenMirrorBadLuck == expectedLuck.BrokenMirrorBadLuck
        && responsePacket.EquipmentLuckBonus == expectedLuck.EquipmentLuckBonus
        && responsePacket.CoinLuck == expectedLuck.CoinLuck
        && responsePacket.KiteLuckLevel == expectedLuck.KiteLuckLevel,
      "The server Packet 134 projection did not match authoritative Player luck state.");
  }

  private static UpdatePlayerLuckFactorsPacket CreateLuckPacket(byte player)
  {
    return new UpdatePlayerLuckFactorsPacket
    {
      Player = player,
      LadyBugLuckTime = 21_600,
      TorchLuck = 0.5f,
      LuckPotion = 2,
      HasGardenGnome = true,
      BrokenMirrorBadLuck = false,
      EquipmentLuckBonus = 0.2f,
      CoinLuck = 250f,
      KiteLuckLevel = 2
    };
  }

  private static InboundPacketEnvelope CreateEnvelope(
    Version4WireProfile profile,
    SessionEpoch epoch,
    long sequence,
    SessionStage stage,
    object packet)
  {
    Version4EncodedPacket encoded = profile.Encode(WireDirection.ClientToServer, packet);
    byte[] rawFrame = PacketFrameCodec.Encode(encoded.MessageId, encoded.Payload.Span);
    return new InboundPacketEnvelope(
      epoch,
      new PacketSequence(sequence),
      DateTimeOffset.UnixEpoch.AddSeconds(sequence),
      encoded.MessageId,
      moduleId: null,
      rawFrame,
      stage.ToString(),
      worldScope: null,
      entityGeneration: null,
      expectedRevision: null);
  }

  private static void PrepareActiveFixture(ServerComposition composition, SessionEpoch epoch)
  {
    var hello = new HelloPacket { Version = "Terraria319" };
    InboundPacketEnvelope envelope = CreateEnvelope(
      composition.Version4WireProfile,
      epoch,
      sequence: 1,
      SessionStage.AwaitHello,
      hello);
    SessionAdmissionPreparation preparation = composition.SessionPacketHandlers.HandleHello(
      envelope,
      hello,
      isBanned: false);
    Require(preparation.IsReady, "The explicit Hello fixture was rejected.");
    RequireApplied(composition.SessionPacketHandlers.CommitAdmission(
      preparation,
      composition.Version4WireProfile).Result, "Hello fixture commit");
    RequireApplied(composition.SessionAdmission.TryTransition(
      epoch,
      new PacketSequence(2),
      SessionStage.AwaitPlayerData,
      SessionStage.Active), "Active fixture transition");
  }

  private static async Task SendPacketAsync(
    Version4WireProfile profile,
    NetworkStream stream,
    object packet,
    CancellationToken cancellationToken)
  {
    Version4EncodedPacket encoded = profile.Encode(WireDirection.ClientToServer, packet);
    byte[] frame = PacketFrameCodec.Encode(encoded.MessageId, encoded.Payload.Span);
    await stream.WriteAsync(frame, cancellationToken).ConfigureAwait(false);
    await stream.FlushAsync(cancellationToken).ConfigureAwait(false);
  }

  private static async Task<byte[]> ReadFrameAsync(
    NetworkStream stream,
    CancellationToken cancellationToken)
  {
    byte[] prefix = new byte[PacketFrameCodec.LengthPrefixBytes];
    await stream.ReadExactlyAsync(prefix, cancellationToken).ConfigureAwait(false);
    ushort totalLength = BinaryPrimitives.ReadUInt16LittleEndian(prefix);
    Require(
      totalLength >= PacketFrameCodec.HeaderBytes,
      "The Packet 134 response frame is truncated.");
    byte[] frame = new byte[totalLength];
    prefix.CopyTo(frame, 0);
    await stream.ReadExactlyAsync(
      frame.AsMemory(PacketFrameCodec.LengthPrefixBytes),
      cancellationToken).ConfigureAwait(false);
    return frame;
  }

  private static async Task WaitForHandledPacketCountAsync(
    int expected,
    Func<int> captureCount,
    CancellationToken cancellationToken)
  {
    var elapsed = Stopwatch.StartNew();
    while (captureCount.Invoke() < expected)
    {
      Require(elapsed.Elapsed < TimeSpan.FromSeconds(3),
        $"The router did not finish inbound packet {expected}.");
      await Task.Delay(10, cancellationToken).ConfigureAwait(false);
    }
  }

  private static async Task WaitUntilAsync(
    Func<bool> predicate,
    CancellationToken cancellationToken)
  {
    var elapsed = Stopwatch.StartNew();
    while (!predicate.Invoke())
    {
      Require(elapsed.Elapsed < TimeSpan.FromSeconds(3), "The real listener admission timed out.");
      await Task.Delay(10, cancellationToken).ConfigureAwait(false);
    }
  }

  private static async Task WaitForOpenConnectionsAsync(
    ServerComposition composition,
    ServerConnectionHost host,
    int expectedConnectionCount,
    CancellationToken cancellationToken)
  {
    await WaitUntilAsync(
      () => host.ActiveConnectionCount >= expectedConnectionCount,
      cancellationToken).ConfigureAwait(false);

    var elapsed = Stopwatch.StartNew();
    bool sessionsOpen = false;
    while (!sessionsOpen)
    {
      await composition.CommitLoop.SubmitAsync(
        () =>
        {
          sessionsOpen = true;
          for (ulong value = 1; value <= (ulong)expectedConnectionCount; value++)
          {
            if (composition.SessionAdmission.Capture(SessionEpoch.Create(value)) is null)
            {
              sessionsOpen = false;
              break;
            }
          }
        },
        cancellationToken).ConfigureAwait(false);
      Require(
        elapsed.Elapsed < TimeSpan.FromSeconds(3),
        "The session open callback did not complete.");
      if (!sessionsOpen)
      {
        await Task.Delay(10, cancellationToken).ConfigureAwait(false);
      }
    }
  }

  private static void RequireApplied(OwnerCommitResult result, string operation)
  {
    Require(result.Status == OwnerCommitStatus.Applied,
      $"{operation} was not applied: {result.Detail}");
  }

  private static void Require(bool condition, string detail)
  {
    if (!condition)
    {
      throw new InvalidOperationException(detail);
    }
  }
}

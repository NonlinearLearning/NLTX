using System.Buffers.Binary;
using System.Net;
using System.Net.Sockets;
using MinimalImplementation.Contracts;
using MinimalImplementation.Server.Host;
using MinimalImplementation.Server.Identity;
using MinimalImplementation.Server.Inventory.State;
using MinimalImplementation.Server.Player;
using MinimalImplementation.Server.Protocol;
using MinimalImplementation.Server.Session;
using Terraria.NetWork.Prototype.PacketDesignCompiler;
using Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;
using Terraria.NetWork.Prototype.PacketDesignCompiler.Wire;

namespace MinimalImplementation.Verification.Packet85Router;

internal static class Packet85RouterProbe
{
  private const int MaximumWorldFileBytes = 64 * 1024 * 1024;
  private const byte QuickStackPacketMessageId = 85;

  private static async Task<int> Main(string[] args)
  {
    string outputRoot = ReadOption(args, "--output-root") ?? Path.Combine(
      "Build",
      "diagnostics",
      "MinimalImplementation",
      "Packet85Router",
      DateTime.UtcNow.ToString("yyyyMMdd-HHmmssfff"));
    Directory.CreateDirectory(outputRoot);

    try
    {
      string evidence = await RunAsync(outputRoot).ConfigureAwait(false);
      string evidencePath = Path.Combine(outputRoot, "packet85-router-result.md");
      File.WriteAllText(evidencePath, evidence);
      Console.WriteLine($"Packet85Router PASS evidence={Path.GetFullPath(evidencePath)}");
      return 0;
    }
    catch (Exception exception)
    {
      Console.Error.WriteLine(
        $"Packet85Router FAIL {exception.GetType().Name}: {exception.Message}");
      Console.Error.WriteLine(exception.ToString());
      return 1;
    }
  }

  private static async Task<string> RunAsync(string outputRoot)
  {
    int port = ReserveLoopbackPort();
    string unusedWorldPath = Path.Combine(outputRoot, "router-fixture.wld");
    var options = new ServerOptions(
      "127.0.0.1",
      port,
      unusedWorldPath,
      MaximumWorldFileBytes,
      commitQueueCapacity: 64);
    ServerComposition? composition = null;
    TcpClient? client = null;
    try
    {
      composition = ServerComposition.Create(options);
      await composition.CommitLoop.StartAsync().ConfigureAwait(false);

      var router = new Version4InboundRouter(composition);
      ServerConnectionHost host = composition.AttachConnectionHost(
        composition.Version4WireProfile,
        router.AllocateEpoch,
        router.CreateEnvelope,
        router.OnPacket,
        router.OnClosed,
        router.OnConnectionFailure,
        onConnected: router.OnConnected);
      router.AttachConnectionHost(host);
      await host.StartAsync().ConfigureAwait(false);
      await WaitUntilAsync(() => host.IsRunning, TimeSpan.FromSeconds(2)).ConfigureAwait(false);

      client = new TcpClient();
      await client.ConnectAsync(IPAddress.Loopback, port).ConfigureAwait(false);
      await WaitUntilAsync(
        () => host.ActiveConnectionCount == 1,
        TimeSpan.FromSeconds(2)).ConfigureAwait(false);

      SessionEpoch epoch = SessionEpoch.Create(1);
      await WaitUntilAsync(
        () => composition.SessionAdmission.Capture(epoch) is not null,
        TimeSpan.FromSeconds(2)).ConfigureAwait(false);
      PlayerFixture playerFixture = await PrepareActiveFixtureAsync(
        composition,
        epoch).ConfigureAwait(false);

      QuickStackChestsRequestPacket request = new()
      {
        ItemSlots = [(short)0],
        SmartStack = false
      };
      InboundPacketEnvelope envelope = CreateEnvelope(
        composition.Version4WireProfile,
        epoch,
        sequence: 100,
        SessionStage.Active,
        request,
        worldScope: composition.IdentityRegistry.CurrentWorldScope);
      await router.OnPacket(
        new InboundVersion4Packet(
          envelope,
          Version4PacketHandling.Typed,
          request,
          "Packet 85 router probe"),
        CancellationToken.None).ConfigureAwait(false);

      using var readDeadline = new CancellationTokenSource(TimeSpan.FromSeconds(2));
      byte[] responseFrame = await ReadFrameAsync(
        client.GetStream(),
        readDeadline.Token).ConfigureAwait(false);
      Require(
        responseFrame[PacketFrameCodec.LengthPrefixBytes] == QuickStackPacketMessageId,
        "The router did not send a Packet 85 response frame.");
      object decodedResponse = composition.Version4WireProfile.Decode(
        WireDirection.ServerToClient,
        QuickStackPacketMessageId,
        responseFrame.AsMemory(PacketFrameCodec.HeaderBytes));
      Require(
        decodedResponse is QuickStackChestsResponsePacket response
          && response.BlockedChestIndexes.Length == 0,
        "The Packet 85 response did not preserve the committed empty blocked-chest list.");

      Require(
        composition.InventoryStore.TryCapture(
          playerFixture.PlayerId,
          out InventorySnapshot? inventory)
          && inventory is not null
          && inventory.Slots[0].Item.IsEmpty,
        "The routed QuickStack did not consume the authenticated inventory source slot.");
      Require(
        composition.ChestStore.TryCapture(
          playerFixture.ChestId,
          out ChestSnapshot? chest)
          && chest is not null
          && chest.Slots[0].Item.Type == 1
          && chest.Slots[0].Item.Stack == 3,
        "The routed QuickStack did not commit the destination chest mutation.");

      QuickStackChestsRequestPacket unauthorizedRequest = new()
      {
        ItemSlots = [(short)0],
        SmartStack = false
      };
      InboundPacketEnvelope unauthorizedEnvelope = CreateEnvelope(
        composition.Version4WireProfile,
        SessionEpoch.Create(2),
        sequence: 1,
        SessionStage.Active,
        unauthorizedRequest,
        worldScope: composition.IdentityRegistry.CurrentWorldScope);
      await router.OnPacket(
        new InboundVersion4Packet(
          unauthorizedEnvelope,
          Version4PacketHandling.Typed,
          unauthorizedRequest,
          "Packet 85 unauthorized probe"),
        CancellationToken.None).ConfigureAwait(false);
      Require(
        router.LastPolicyRejection?.Contains("active", StringComparison.OrdinalIgnoreCase) == true,
        "The routed Packet 85 request without an authenticated session was not rejected.");

      return $"""
        # Packet 85 router probe

        Status: PASS

        Transport: one real `ServerConnectionHost` accepted loopback epoch `{epoch.Value}`.
        The router sent a typed Packet 85 request through the existing authenticated handler;
        the client received a framed Packet 85 response and decoded an empty blocked-chest list.

        Commit: the authenticated inventory source slot was emptied and chest
        `{playerFixture.ChestId.Value}` changed from stack 1 to stack 3 in one QuickStack
        transaction.

        Admission: a second typed Packet 85 envelope with no active authenticated binding was
        rejected by the router policy path and did not reach the transaction owner.

        Boundary: this is a targeted Packet 85 route/receipt probe. It does not claim complete
        G2 inbound coverage, complete P08-B readiness, or the unresolved non-empty Packet 159
        attachment path.
        """;
    }
    finally
    {
      client?.Dispose();
      if (composition is not null)
      {
        await composition.DisposeAsync().ConfigureAwait(false);
      }
    }
  }

  private static async Task<PlayerFixture> PrepareActiveFixtureAsync(
    ServerComposition composition,
    SessionEpoch epoch)
  {
    PlayerFixture? fixture = null;
    await composition.CommitLoop.SubmitAsync(
      () =>
      {
        HelloPacket hello = new() { Version = "Terraria319" };
        SessionAdmissionPreparation preparation = composition.SessionPacketHandlers.HandleHello(
          CreateEnvelope(
            composition.Version4WireProfile,
            epoch,
            sequence: 1,
            SessionStage.AwaitHello,
            hello,
            worldScope: null),
          hello,
          isBanned: false);
        Require(
          preparation.IsReady,
          "The router probe could not prepare the authenticated player.");
        SessionTransitionCommitResult admission = composition.SessionPacketHandlers.CommitAdmission(
          preparation,
          composition.Version4WireProfile);
        RequireApplied(admission.Result, "router probe player admission");
        RequireApplied(
          composition.SessionAdmission.TryTransition(
            epoch,
            new PacketSequence(2),
            SessionStage.AwaitPlayerData,
            SessionStage.Active),
          "router probe Active transition");

        Require(
          composition.Players.TryGetBoundPlayer(
            epoch,
            composition.SessionAdmission,
            out Player? player)
            && player is not null,
          "The router probe could not resolve the authenticated Player owner.");

        InventoryItemState[] inventoryItems = new InventoryItemState[
          ProtocolInputs.Instance.SlotCount];
        Array.Fill(inventoryItems, InventoryItemState.Empty);
        inventoryItems[0] = new InventoryItemState(1, 2, 0, false);
        RequireApplied(
          composition.InventoryStore.Create(
            player!.Handle.Id,
            inventoryItems,
            out InventorySnapshot? _),
          "router probe inventory creation");

        ChestId chestId = new(0);
        RequireApplied(
          composition.ChestStore.Create(
            chestId,
            new WorldScopeId(composition.IdentityRegistry.CurrentWorldScope),
            new WorldTilePoint(1, 1),
            kindId: 0,
            name: "Packet85Probe",
            [
              new InventoryItemState(1, 1, 0, false),
              InventoryItemState.Empty
            ],
            out ChestSnapshot? _),
          "router probe chest creation");
        fixture = new PlayerFixture(player.Handle.Id, chestId);
      }).ConfigureAwait(false);

    return fixture
      ?? throw new InvalidOperationException("The router probe fixture was not created.");
  }

  private static InboundPacketEnvelope CreateEnvelope(
    Version4WireProfile profile,
    SessionEpoch epoch,
    long sequence,
    SessionStage stage,
    object packet,
    long? worldScope)
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
      worldScope,
      entityGeneration: null,
      expectedRevision: null);
  }

  private static async Task<byte[]> ReadFrameAsync(
    NetworkStream stream,
    CancellationToken cancellationToken)
  {
    byte[] lengthBytes = new byte[PacketFrameCodec.LengthPrefixBytes];
    await stream.ReadExactlyAsync(lengthBytes, cancellationToken).ConfigureAwait(false);
    ushort totalLength = BinaryPrimitives.ReadUInt16LittleEndian(lengthBytes);
    Require(
      totalLength >= PacketFrameCodec.HeaderBytes,
      "The Packet 85 response had an invalid frame length.");

    byte[] frame = new byte[totalLength];
    lengthBytes.CopyTo(frame, 0);
    await stream.ReadExactlyAsync(
      frame.AsMemory(PacketFrameCodec.LengthPrefixBytes),
      cancellationToken).ConfigureAwait(false);
    return frame;
  }

  private static async Task WaitUntilAsync(Func<bool> predicate, TimeSpan timeout)
  {
    DateTime deadline = DateTime.UtcNow + timeout;
    while (!predicate())
    {
      if (DateTime.UtcNow >= deadline)
      {
        throw new TimeoutException("The Packet 85 router probe precondition did not become true.");
      }

      await Task.Delay(TimeSpan.FromMilliseconds(10)).ConfigureAwait(false);
    }
  }

  private static int ReserveLoopbackPort()
  {
    using var probe = new TcpListener(IPAddress.Loopback, 0);
    probe.Start();
    int port = ((IPEndPoint)probe.LocalEndpoint).Port;
    probe.Stop();
    return port;
  }

  private static string? ReadOption(IReadOnlyList<string> args, string option)
  {
    for (int index = 0; index + 1 < args.Count; index++)
    {
      if (string.Equals(args[index], option, StringComparison.Ordinal))
      {
        return args[index + 1];
      }
    }

    return null;
  }

  private static void RequireApplied(OwnerCommitResult result, string operation)
  {
    Require(
      result.Status == OwnerCommitStatus.Applied,
      $"{operation} was not applied: {result.Detail}");
  }

  private static void Require(bool condition, string message)
  {
    if (!condition)
    {
      throw new InvalidOperationException(message);
    }
  }

  private sealed record PlayerFixture(EntityUuid PlayerId, ChestId ChestId);
}

using System.Buffers.Binary;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using MinimalImplementation.Contracts;
using MinimalImplementation.Modules.Creative;
using MinimalImplementation.Server.Host;
using MinimalImplementation.Server.Modules.Protocol;
using MinimalImplementation.Server.Protocol;
using MinimalImplementation.Server.Session;
using Terraria.NetWork.Prototype.PacketDesignCompiler;
using Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;
using Terraria.NetWork.Prototype.PacketDesignCompiler.Wire;

namespace MinimalImplementation.Verification.Integration;

internal static class CreativePermissionProbe {
  private const int _permissionCount = 15;
  private const ushort _permissionModuleId = (ushort)Packet82ModuleId.CreativePowerPermissions;

  public static async Task<int> RunAsync(string? requestedOutputRoot) {
    string outputRoot = Path.GetFullPath(requestedOutputRoot ?? Path.Combine(
        "Build", "diagnostics", "MinimalImplementation", "CreativePermissions",
        DateTime.UtcNow.ToString("yyyyMMdd-HHmmssfff")));
    Directory.CreateDirectory(outputRoot);
    string resultPath = Path.Combine(outputRoot, "creative-permissions-result.md");
    if (File.Exists(resultPath)) {
      throw new ArgumentException("The Creative permission probe requires a fresh result path.");
    }
    using var deadline = new CancellationTokenSource(TimeSpan.FromSeconds(45));
    try {
      await VerifyAsync(outputRoot, deadline.Token).ConfigureAwait(false);
      File.WriteAllText(resultPath, """
          # Creative permission delivery probe

          Status: PASS

          Receipt fixtures: all 15 production module 9 encodings preserve action, power order,
          permission level, version, target and delivery token. Pending captures exclude concurrent
          captures. Reversed/foreign intents, missing/extra receipts, duplicate sequence,
          foreign epoch, failed/unknown/unsubmitted outcomes cannot clean dirty state. Mixed queued
          and locally-sent receipts settle locally. Old results cannot release newer captures;
          registered module-dispatcher retirement invalidates an outstanding capture. Encoding
          failure propagates and releases its reservation. Full and closed pump rejection permit
          fresh captures.

          Transport: a real loopback ServerConnectionHost, Version4InboundRouter, outbound pump and
          ModulePeriodicSyncOwner send 15 framed module 9 entries to an Active fixture.
          A changed permission produces another complete 15-entry snapshot; the clean next phase
          produces none. The client decodes both snapshots with the production Version4WireProfile.
          Pump completion drains owner settlements before connection/commit-loop disposal.

          Boundary: targeted integration probe, not a fixed SC scenario. Active is established by
          a direct fixture transition after Hello preparation/commit. PeriodicSync is invoked on
          the commit loop. This does not prove real join, autonomous nine-phase scheduling, client
          acknowledgement, Creative effect retry ordering, or SC091/P08-B production readiness.
          """);
      Console.WriteLine($"CreativePermissions PASS evidence={resultPath}");
      return 0;
    } catch (Exception exception) {
      Console.Error.WriteLine($"CreativePermissions FAIL {exception}");
      return 1;
    }
  }

  private static async Task VerifyAsync(string outputRoot, CancellationToken cancellation) {
    using var reservation = new TcpListener(IPAddress.Loopback, 0);
    reservation.Start();
    int port = ((IPEndPoint)reservation.LocalEndpoint).Port;
    reservation.Stop();
    await using var composition = ServerComposition.Create(new ServerOptions(
        "127.0.0.1", port, Path.Combine(outputRoot, "unused-fixture.wld"),
        64 * 1024 * 1024, commitQueueCapacity: 64));
    await composition.CommitLoop.StartAsync().ConfigureAwait(false);
    await composition.CommitLoop.SubmitAsync(() => VerifyReceipts(composition), cancellation)
        .ConfigureAwait(false);

    var router = new Version4InboundRouter(composition);
    ServerConnectionHost host = composition.AttachConnectionHost(
        composition.Version4WireProfile, router.AllocateEpoch, router.CreateEnvelope,
        router.OnPacket, router.OnClosed, router.OnConnectionFailure,
        onConnected: router.OnConnected);
    router.AttachConnectionHost(host);
    await VerifyPumpRejectionAsync(composition, host, cancellation).ConfigureAwait(false);
    await host.StartAsync(cancellation).ConfigureAwait(false);
    using var client = new TcpClient();
    await client.ConnectAsync(IPAddress.Loopback, port, cancellation).ConfigureAwait(false);
    SessionEpoch epoch = SessionEpoch.Create(1);
    var elapsed = Stopwatch.StartNew();
    while (true) {
      bool connected = false;
      await composition.CommitLoop.SubmitAsync(
          () => connected = composition.SessionAdmission.Capture(epoch) is not null, cancellation)
          .ConfigureAwait(false);
      if (connected) {
        break;
      }
      Require(elapsed.Elapsed < TimeSpan.FromSeconds(3), "Real listener admission timed out.");
      await Task.Delay(10, cancellation).ConfigureAwait(false);
    }
    await composition.CommitLoop.SubmitAsync(
        () => PrepareActiveFixture(composition, epoch), cancellation).ConfigureAwait(false);
    var pump = new ServerOutboundDeliveryPump(composition.CommitLoop, host, capacity: 64);
    var periodic = new ModulePeriodicSyncOwner(composition, pump);
    try {
      await composition.CommitLoop.SubmitAsync(() => periodic.Advance(1), cancellation)
          .ConfigureAwait(false);
      await ReadPermissionsAsync(composition, client.GetStream(), changed: false, cancellation)
          .ConfigureAwait(false);
      await WaitForSettlementAsync(composition, pump, epoch, null, cancellation)
          .ConfigureAwait(false);
      OutboundDeliverySettlementResult? previous = pump.LastSettlement;
      await composition.CommitLoop.SubmitAsync(() => {
        RequireApplied(composition.CreativePowerStateOwner.SetPermission(
            3, CreativePowerPermissionLevel.CanBeChangedByHostAlone));
        periodic.Advance(2);
      }, cancellation).ConfigureAwait(false);
      await ReadPermissionsAsync(composition, client.GetStream(), changed: true, cancellation)
          .ConfigureAwait(false);
      await WaitForSettlementAsync(composition, pump, epoch, previous, cancellation)
          .ConfigureAwait(false);
      await composition.CommitLoop.SubmitAsync(() => {
        Require(CreativePermissionDeliverySettlement.Capture(
            composition.CreativeModuleHandler, composition.Packet82IntentProducer, epoch) is null,
            "Locally sent permissions remained dirty.");
        periodic.Advance(3);
      }, cancellation).ConfigureAwait(false);
      await pump.CompleteAsync(cancellation).ConfigureAwait(false);
      Require(!client.GetStream().DataAvailable, "A clean periodic phase emitted more output.");
    } finally {
      await pump.CompleteAsync(cancellation).ConfigureAwait(false);
    }
    Console.WriteLine("CreativePermissions real TCP: initial=15 changed=15 clean=0; " +
        "Active admission and periodic tick are explicit fixtures.");
  }

  private static void VerifyReceipts(ServerComposition composition) {
    SessionEpoch epoch = SessionEpoch.Create(100);
    var cases = new (string Name, Action<List<OutboundIntentDelivery>> Corrupt)[] {
      ("reversed intents", deliveries => deliveries.Reverse()),
      ("foreign intent", deliveries => deliveries[0] = new OutboundIntentDelivery(
          deliveries[0].Intent with { }, deliveries[0].Receipts)),
      ("missing delivery", deliveries => deliveries.RemoveAt(0)),
      ("missing receipt", deliveries => deliveries[0] = new OutboundIntentDelivery(
          deliveries[0].Intent, Array.Empty<SendReceipt>())),
      ("extra receipt", deliveries => deliveries[0] = new OutboundIntentDelivery(
          deliveries[0].Intent, [deliveries[0].Receipts[0], deliveries[1].Receipts[0]])),
      ("duplicate sequence", deliveries => deliveries[1] = new OutboundIntentDelivery(
          deliveries[1].Intent, [deliveries[0].Receipts[0]])),
      ("foreign epoch", deliveries => ReplaceReceipt(
          deliveries, TransportSendCertainty.LocallySent, SessionEpoch.Create(101))),
      ("failed", deliveries => ReplaceReceipt(deliveries, TransportSendCertainty.Failed, epoch)),
      ("unknown", deliveries => ReplaceReceipt(
          deliveries, TransportSendCertainty.OutcomeUnknown, epoch)),
      ("unsubmitted", deliveries => ReplaceReceipt(
          deliveries, TransportSendCertainty.NotSubmitted, epoch))
    };
    foreach (var test in cases) {
      CreativePermissionDeliverySettlement batch = Capture(composition, epoch);
      VerifyIntents(composition, batch, epoch);
      Require(CreativePermissionDeliverySettlement.Capture(
          composition.CreativeModuleHandler, composition.Packet82IntentProducer, epoch) is null,
          "A second permission capture bypassed the reservation.");
      List<OutboundIntentDelivery> deliveries = Deliveries(batch);
      test.Corrupt.Invoke(deliveries);
      Require(!batch.Settle(deliveries).IsSettled, $"{test.Name} falsely settled.");
      CreativePermissionDeliverySettlement retry = Capture(composition, epoch);
      Require(!batch.Settle(Deliveries(batch)).IsSettled, "Old success settled a newer capture.");
      Require(!batch.Settle(Array.Empty<OutboundIntentDelivery>()).IsSettled,
          "Old failure settled a newer capture.");
      Require(CreativePermissionDeliverySettlement.Capture(
          composition.CreativeModuleHandler, composition.Packet82IntentProducer, epoch) is null,
          "Old settlement released the newer reservation.");
      Require(retry.Settle(Deliveries(retry)).IsSettled, "A fresh retry did not settle.");
      Require(CreativePermissionDeliverySettlement.Capture(
          composition.CreativeModuleHandler, composition.Packet82IntentProducer, epoch) is null,
          "A complete retry did not clean the target.");
      composition.ModuleDispatcher.RetireEpoch(epoch);
      Console.WriteLine($"CreativePermissions rejected {test.Name}; retry/stale isolation PASS");
    }

    CreativePermissionDeliverySettlement oldVersion = Capture(composition, epoch);
    RequireApplied(composition.CreativePowerStateOwner.SetPermission(
        0, CreativePowerPermissionLevel.LockedForEveryone));
    Require(oldVersion.Settle(Deliveries(oldVersion)).IsSettled, "Old version could not settle.");
    CreativePermissionDeliverySettlement newer = Capture(composition, epoch);
    Require(newer.Intents[0].CommitVersion.Value > oldVersion.Intents[0].CommitVersion.Value,
        "A change during delivery did not retain the newer dirty version.");
    VerifyIntents(composition, newer, epoch);
    List<OutboundIntentDelivery> mixed = Deliveries(newer);
    ReplaceReceipt(mixed, TransportSendCertainty.Queued, epoch);
    Require(newer.Settle(mixed).IsSettled, "Mixed queued/local receipts did not settle locally.");
    composition.ModuleDispatcher.RetireEpoch(epoch);
    CreativePermissionDeliverySettlement retired = Capture(composition, epoch);
    composition.ModuleDispatcher.RetireEpoch(epoch);
    Require(!retired.Settle(Deliveries(retired)).IsSettled,
        "Registered Creative retirement accepted a late receipt.");
    CreativePermissionDeliverySettlement current = Capture(composition, epoch);
    Require(!retired.Settle(Array.Empty<OutboundIntentDelivery>()).IsSettled,
        "Retired failure settled a new capture.");
    Require(current.Settle(Deliveries(current)).IsSettled,
        "A retired batch released a replacement capture.");
    composition.ModuleDispatcher.RetireEpoch(epoch);

    var inputs = ProtocolInputs.Instance;
    var missingCodec = new Packet82ModuleIntentProducer(
        inputs.ModuleCodecs.Where(pair => pair.Key != _permissionModuleId)
            .ToDictionary(pair => pair.Key, pair => pair.Value),
        inputs.TagEffectNpcSlotCount, inputs.TagEffectUsesProcTimes);
    bool threw = false;
    try {
      CreativePermissionDeliverySettlement.Capture(
          composition.CreativeModuleHandler, missingCodec, epoch);
    } catch (PacketWireFormatException) {
      threw = true;
    }
    Require(threw, "Missing permission codec did not propagate its wire exception.");
    CreativePermissionDeliverySettlement recovered = Capture(composition, epoch);
    Require(recovered.Settle(Deliveries(recovered)).IsSettled,
        "Encoding failure leaked the permission reservation.");
    composition.ModuleDispatcher.RetireEpoch(epoch);
    RequireApplied(composition.CreativePowerStateOwner.SetPermission(
        0, CreativePowerPermissionLevel.CanBeChangedByEveryone));
    Console.WriteLine("CreativePermissions version change, mixed certainty, " +
        "registered retirement and encoding failure PASS");
  }

  private static async Task VerifyPumpRejectionAsync(
      ServerComposition composition, ServerConnectionHost host, CancellationToken cancellation) {
    var pump = new ServerOutboundDeliveryPump(composition.CommitLoop, host, capacity: 1);
    try {
      await composition.CommitLoop.SubmitAsync(() => {
        int rejected = 0;
        // With the loop held, the worker can remove at most one batch before its settlement waits.
        for (ulong value = 200; value < 203; value++) {
          SessionEpoch epoch = SessionEpoch.Create(value);
          if (!pump.TryEnqueue(Capture(composition, epoch))) {
            rejected++;
            Capture(composition, epoch).Settle(Array.Empty<OutboundIntentDelivery>());
          }
        }
        Require(rejected > 0, "The bounded pump did not exercise full admission rejection.");
      }, cancellation).ConfigureAwait(false);
    } finally {
      await pump.CompleteAsync(cancellation).ConfigureAwait(false);
    }
    await composition.CommitLoop.SubmitAsync(() => {
      SessionEpoch epoch = SessionEpoch.Create(203);
      Require(!pump.TryEnqueue(Capture(composition, epoch)), "The closed pump admitted output.");
      Capture(composition, epoch).Settle(Array.Empty<OutboundIntentDelivery>());
      for (ulong value = 200; value <= 203; value++) {
        composition.ModuleDispatcher.RetireEpoch(SessionEpoch.Create(value));
      }
    }, cancellation).ConfigureAwait(false);
    Console.WriteLine("CreativePermissions bounded full/closed pump reservation release PASS");
  }

  private static void PrepareActiveFixture(ServerComposition composition, SessionEpoch epoch) {
    var hello = new HelloPacket { Version = "Terraria319" };
    Version4EncodedPacket encoded = composition.Version4WireProfile.Encode(
        WireDirection.ClientToServer, hello);
    var envelope = new InboundPacketEnvelope(
        epoch, new PacketSequence(1), DateTimeOffset.UnixEpoch, encoded.MessageId, null,
        PacketFrameCodec.Encode(encoded.MessageId, encoded.Payload.Span),
        SessionStage.AwaitHello.ToString(), null, null, null);
    SessionAdmissionPreparation preparation = composition.SessionPacketHandlers.HandleHello(
        envelope, hello, isBanned: false);
    Require(preparation.IsReady, "The explicit Hello fixture was rejected.");
    RequireApplied(composition.SessionPacketHandlers.CommitAdmission(
        preparation, composition.Version4WireProfile).Result);
    RequireApplied(composition.SessionAdmission.TryTransition(
        epoch, new PacketSequence(2), SessionStage.AwaitPlayerData, SessionStage.Active));
  }

  private static CreativePermissionDeliverySettlement Capture(
      ServerComposition composition, SessionEpoch epoch) {
    return CreativePermissionDeliverySettlement.Capture(
        composition.CreativeModuleHandler, composition.Packet82IntentProducer, epoch)
        ?? throw new InvalidOperationException("Expected dirty permission capture unavailable.");
  }

  private static void VerifyIntents(
      ServerComposition composition, CreativePermissionDeliverySettlement batch,
      SessionEpoch epoch) {
    Require(batch.Intents.Count == _permissionCount, "Permission snapshot was incomplete.");
    CreativePowerSnapshot snapshot = composition.CreativePowerStateOwner.CaptureSnapshot();
    Guid? batchId = null;
    for (int index = 0; index < batch.Intents.Count; index++) {
      OutboundPacketIntent intent = batch.Intents[index];
      Require(intent.MessageId == 82 && intent.TargetEpochs.SequenceEqual(new[] { epoch }) &&
          intent.IgnoreEpoch is null && intent.CommitVersion == snapshot.Version,
          "Permission intent lost its target or owner version.");
      OutboundDeliveryToken token = intent.DeliveryToken
          ?? throw new InvalidOperationException("Permission intent has no delivery token.");
      batchId ??= token.BatchId;
      Require(token.BatchId == batchId && token.OwnerKey == batch.OwnerKey &&
          token.Target == epoch && token.IntentIndex == index, "Invalid permission token.");
      Packet82PermissionData entry = DecodePermission(composition, intent.Payload);
      Require(entry.Action == 0 && entry.PowerId == index &&
          entry.Level == (byte)snapshot.Permissions[(ushort)index],
          "Permission encoding lost its ordered owner value.");
    }
  }

  private static List<OutboundIntentDelivery> Deliveries(
      CreativePermissionDeliverySettlement batch) {
    return batch.Intents.Select((intent, index) => new OutboundIntentDelivery(intent, [
      new SendReceipt(TransportSendCertainty.LocallySent, intent.TargetEpochs[0],
          new PacketSequence(index + 1), intent.Payload.Length, null)
    ])).ToList();
  }

  private static void ReplaceReceipt(
      List<OutboundIntentDelivery> deliveries, TransportSendCertainty certainty,
      SessionEpoch epoch) {
    deliveries[0] = new OutboundIntentDelivery(deliveries[0].Intent, [
      new SendReceipt(certainty, epoch, new PacketSequence(1), 0,
          certainty == TransportSendCertainty.Failed ? ProtocolErrorCode.TransportFailure : null)
    ]);
  }

  private static Packet82PermissionData DecodePermission(
      ServerComposition composition, ReadOnlyMemory<byte> payload) {
    object decoded = composition.Version4WireProfile.Decode(
        WireDirection.ServerToClient, 82, payload);
    Require(decoded is NetModulesPacket { ModuleId: _permissionModuleId },
        "The permission payload did not decode as module 9.");
    return ((NetModulesPacket)decoded).Data as Packet82PermissionData
        ?? throw new InvalidOperationException("The permission payload has the wrong type.");
  }

  private static async Task ReadPermissionsAsync(
      ServerComposition composition, NetworkStream stream, bool changed,
      CancellationToken cancellation) {
    for (int powerId = 0; powerId < _permissionCount;) {
      byte[] prefix = new byte[PacketFrameCodec.LengthPrefixBytes];
      await stream.ReadExactlyAsync(prefix, cancellation).ConfigureAwait(false);
      ushort length = BinaryPrimitives.ReadUInt16LittleEndian(prefix);
      Require(length >= PacketFrameCodec.HeaderBytes, "Invalid outbound frame length.");
      byte[] frame = new byte[length];
      prefix.CopyTo(frame, 0);
      await stream.ReadExactlyAsync(frame.AsMemory(prefix.Length), cancellation)
          .ConfigureAwait(false);
      Require(frame[PacketFrameCodec.LengthPrefixBytes] == 82, "Unexpected periodic packet id.");
      ReadOnlyMemory<byte> payload = frame.AsMemory(PacketFrameCodec.HeaderBytes);
      ushort moduleId = BinaryPrimitives.ReadUInt16LittleEndian(payload.Span);
      if (moduleId is (ushort)Packet82ModuleId.Bestiary or (ushort)Packet82ModuleId.Banners) {
        continue;
      }
      Packet82PermissionData entry = DecodePermission(composition, payload);
      byte expected = (byte)(changed && powerId == 3
        ? CreativePowerPermissionLevel.CanBeChangedByHostAlone
        : CreativePowerPermissionLevel.CanBeChangedByEveryone);
      Require(entry.Action == 0 && entry.PowerId == powerId && entry.Level == expected,
          "The real client received an incorrect permission snapshot.");
      powerId++;
    }
  }

  private static async Task WaitForSettlementAsync(
      ServerComposition composition, ServerOutboundDeliveryPump pump, SessionEpoch epoch,
      OutboundDeliverySettlementResult? previous,
      CancellationToken cancellation) {
    var elapsed = Stopwatch.StartNew();
    while (true) {
      bool settled = false;
      await composition.CommitLoop.SubmitAsync(() => {
        settled = pump.LastSettlement is { IsSettled: true, OwnerKey: "CreativePermissions" }
          && !ReferenceEquals(previous, pump.LastSettlement);
        if (settled) {
          CreativePermissionDeliverySettlement? pending =
              CreativePermissionDeliverySettlement.Capture(
                  composition.CreativeModuleHandler, composition.Packet82IntentProducer, epoch);
          Require(pending is null, "Pump settled without cleaning the permission owner.");
        }
      }, cancellation).ConfigureAwait(false);
      if (settled) {
        return;
      }
      Require(elapsed.Elapsed < TimeSpan.FromSeconds(3), "Permission pump settlement timed out.");
      await Task.Delay(10, cancellation).ConfigureAwait(false);
    }
  }

  private static void RequireApplied(OwnerCommitResult result) {
    Require(result.Status == OwnerCommitStatus.Applied, result.Detail ?? "Mutation rejected.");
  }

  private static void Require(bool condition, string message) {
    if (!condition) {
      throw new InvalidOperationException(message);
    }
  }
}

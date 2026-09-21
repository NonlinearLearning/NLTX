using Terraria.Network.Session;
using Terraria.Network.Presentation;
using Terraria.Network.Protocol;
using Terraria.Network.Transport;

namespace Terraria.NetworkProtocolSessionVerification;

internal static class Program
{
  private static int Main()
  {
    TestRecentEndpointCapacityAndSlotInvariant();
    TestMapPresentationUsesExplicitRefreshAndTickInputs();
    TestNetworkSessionModeCommitsOnlyExplicitTransitions();
    TestNetworkConfigurationFreezesValidatedExternalSettings();
    TestRemoteEndpointValueSeparatesTransportIdentity();
    TestNetworkBufferPoolOwnsLeaseReturn();
    TestNetworkMessageBufferStateBoundsAndFrameCursor();
    TestInboundNetworkCommandPreservesMetadataAndPayloadIsolation();
    TestNetworkPacketEnvelopeOwnsHeaderLengthAndRecycle();
    TestNetworkModuleRegistryAssignsIdsAndDispatchesImmutablePayload();
    Console.WriteLine("P13 verifier passed.");
    return 0;
  }

  private static void TestRecentEndpointCapacityAndSlotInvariant()
  {
    NetworkSessionEndpointState state = new(capacity: 2);

    Assert(state.MaxEntries == 2, "The endpoint state must expose its fixed capacity.");
    Assert(
      state.Apply(new RecentServerEndpointCommand(0, "world-a", "127.0.0.1", 7777)),
      "A valid endpoint command should be accepted.");

    NetworkSessionEndpointSnapshot snapshot = state.CreateSnapshot();
    Assert(snapshot.Worlds[0] == "world-a", "The world name must stay in the endpoint slot.");
    Assert(snapshot.IpAddresses[0] == "127.0.0.1", "The IP must stay in the endpoint slot.");
    Assert(snapshot.Ports[0] == 7777, "The port must stay in the endpoint slot.");

    Assert(
      !state.Apply(new RecentServerEndpointCommand(2, "invalid", "127.0.0.1", 7778)),
      "An out-of-range slot must be rejected without mutation.");
    Assert(state.CreateSnapshot().Worlds[0] == "world-a", "A rejected command must not mutate state.");

    Assert(
      state.Apply(new RecentServerEndpointCommand(1, "world-b", "localhost", 7778)),
      "A second valid endpoint command should be accepted.");
    NetworkSessionEndpointSnapshot isolated = state.CreateSnapshot();
    state.Clear();
    Assert(isolated.Worlds[1] == "world-b", "Snapshots must be isolated from later state changes.");
    Assert(state.CreateSnapshot().Worlds[0] is null, "Clear must remove the first endpoint.");
  }

  private static void TestMapPresentationUsesExplicitRefreshAndTickInputs()
  {
    NetworkMapPresentationStateComponent state = new();

    state.ApplyBackground(new NetworkBackgroundPresentationCommand(
      InstantTransitionCounter: 3,
      BackgroundDelay: 4,
      BackgroundStyle: 7,
      FrontLayerAlpha: 0.25f,
      FarBackLayerAlpha: 0.75f,
      WallOfFleshNpcIndex: 19,
      DrawAreaTop: 20,
      DrawAreaBottom: 80));
    state.ApplyMapRefresh(new MapRefreshCommand(
      MapTimeMax: 2,
      UpdateMap: true,
      ClearMap: true));

    state.AdvanceMap(ticks: 1);
    NetworkMapPresentationSnapshot pending = state.CreateSnapshot();
    Assert(!pending.MapReady, "A map must remain pending before its explicit tick budget is reached.");
    Assert(pending.MapTime == 1, "Map time must advance by the explicit tick input.");
    Assert(pending.BackgroundStyle == 7, "Background state must be preserved in the snapshot.");
    Assert(pending.ClearMap, "The clear-map request must remain visible while the refresh is pending.");

    state.AdvanceMap(ticks: 1);
    NetworkMapPresentationSnapshot ready = state.CreateSnapshot();
    Assert(ready.MapReady, "The map must become ready at the configured tick boundary.");
    Assert(!ready.RefreshMap, "A completed refresh must clear the pending refresh flag.");
    Assert(ready.MapTime == 2, "Map time must stop at the configured maximum.");

    bool rejected = false;
    try
    {
      state.AdvanceMap(ticks: -1);
    }
    catch (ArgumentOutOfRangeException)
    {
      rejected = true;
    }

    Assert(rejected, "Negative map ticks must be rejected explicitly.");
  }

  private static void TestNetworkSessionModeCommitsOnlyExplicitTransitions()
  {
    NetworkSessionModeStateComponent state =
      new(NetworkSessionMode.SinglePlayerClient, maxItemUpdates: 3);

    Assert(
      state.RequestTransition(new NetworkModeChangeCommand(NetworkSessionMode.Server)),
      "A supported target mode should create a pending transition.");
    NetworkSessionModeSnapshot pending = state.CreateSnapshot();
    Assert(
      pending.CurrentMode == NetworkSessionMode.SinglePlayerClient &&
      pending.TargetMode == NetworkSessionMode.Server &&
      pending.HasPendingTransition,
      "Requesting a mode must not mutate current mode before commit.");

    Assert(state.CommitTransition(), "A pending mode must commit exactly once.");
    NetworkSessionModeSnapshot committed = state.CreateSnapshot();
    Assert(
      committed.CurrentMode == NetworkSessionMode.Server &&
      !committed.HasPendingTransition,
      "Commit must replace current mode and clear the pending transition.");
    Assert(!state.CommitTransition(), "A second commit without a request must be rejected.");
    Assert(
      !state.RequestTransition(new NetworkModeChangeCommand((NetworkSessionMode)99)),
      "An unknown mode value must be rejected.");

    state.RecordItemUpdate(2);
    Assert(state.CreateSnapshot().LastItemUpdate == 2, "Item update throttling must use explicit input.");
  }

  private static void TestNetworkConfigurationFreezesValidatedExternalSettings()
  {
    NetworkSessionConfigurationStateComponent state = new();
    NetworkConfigurationAdapter adapter = new();
    NetworkSessionConfigurationInput input = new(
      MaxConnections: 8,
      NetBufferSize: 4096,
      DefaultPort: 7777,
      BanFilePath: "bans.txt",
      ServerPassword: "secret",
      ServerIp: "127.0.0.1",
      ServerIpText: "localhost",
      IsHostAndPlay: true,
      HostToken: "token",
      UseUpnp: false,
      SaveOnServerExit: true,
      HandshakeLoggingEnabled: false);

    Assert(adapter.TryApply(state, input, out _), "A valid session configuration should be accepted.");
    adapter.Freeze(state);
    NetworkSessionConfigurationSnapshot snapshot = state.CreateSnapshot();
    Assert(snapshot.MaxConnections == 8, "Configuration must preserve connection capacity.");
    Assert(snapshot.ServerIp == "127.0.0.1", "Resolved server IP must stay separate from display text.");
    Assert(snapshot.ServerIpText == "localhost", "Server IP display text must stay separate from resolved IP.");
    Assert(!snapshot.ContainsSecrets, "Configuration snapshots must not expose secret material.");

    NetworkSessionConfigurationInput changed = input with { DefaultPort = 7778 };
    Assert(!adapter.TryApply(state, changed, out _), "Frozen configuration must reject mutation.");
    Assert(state.CreateSnapshot().DefaultPort == 7777, "Rejected frozen updates must not mutate state.");
    Assert(!adapter.TryApply(state, input with { MaxConnections = 0 }, out _),
      "Invalid capacity must be rejected before state mutation.");
  }

  private static void TestRemoteEndpointValueSeparatesTransportIdentity()
  {
    RemoteEndpointValue endpoint = new(
      RemoteEndpointKind.Tcp,
      "127.0.0.1",
      7777);

    Assert(endpoint.IsLocalHost, "Loopback endpoint detection must be explicit.");
    Assert(
      endpoint.GetIdentifier() == "tcp://127.0.0.1:7777",
      "Endpoint identity must include transport and port.");
    Assert(
      endpoint.GetFriendlyName() == "127.0.0.1:7777",
      "Friendly endpoint text must remain separate from protocol identity.");

    bool rejected = false;
    try
    {
      _ = new RemoteEndpointValue(RemoteEndpointKind.Tcp, "127.0.0.1", 0);
    }
    catch (ArgumentOutOfRangeException)
    {
      rejected = true;
    }

    Assert(rejected, "An endpoint with an invalid port must be rejected.");
  }

  private static void TestNetworkBufferPoolOwnsLeaseReturn()
  {
    NetworkBufferPoolAdapter pool = new();

    NetworkBufferLease small = pool.Rent(128);
    Assert(small.Bucket == NetworkBufferBucket.Small, "Small requests must use the small bucket.");
    Assert(small.Buffer.Length >= 128, "A lease must satisfy the requested capacity.");
    Assert(pool.Return(small), "The first lease return must be accepted.");
    Assert(!pool.Return(small), "A lease must not be returned twice.");
    Assert(pool.SmallAvailableCount == 1, "Returned small buffers must be reusable.");

    NetworkBufferLease reused = pool.Rent(128);
    Assert(
      ReferenceEquals(reused.Buffer, small.Buffer),
      "A returned buffer should be reused by the matching bucket.");
    pool.Return(reused);

    NetworkBufferLease custom = pool.Rent(NetworkBufferPoolAdapter.LargeBufferSize + 1);
    Assert(custom.Bucket == NetworkBufferBucket.Custom, "Oversized requests must use a custom lease.");
    Assert(pool.Return(custom), "Custom leases must be returnable exactly once.");
    Assert(pool.CustomBufferCount == 0, "Returned custom leases must not remain owned by the pool.");
  }

  private static void TestNetworkMessageBufferStateBoundsAndFrameCursor()
  {
    NetworkMessageBufferAdapterState state = new(
      connectionSlot: 7,
      readBufferCapacity: 8,
      writeBufferCapacity: 4);

    Assert(state.WhoAmI == 7, "The buffer must preserve its explicit connection slot.");
    Assert(state.RemainingReadBufferLength == 8, "A new buffer must expose its full read capacity.");
    Assert(state.TryAppendReadBytes(new byte[] { 4, 0, 42, 1 }),
      "A frame that fits must be accepted.");
    Assert(state.TotalData == 4 && state.CheckBytes,
      "Receiving bytes must advance data and mark the buffer for an explicit check.");
    Assert(state.TryPeekMessageLength(out int messageLength) && messageLength == 4,
      "A complete little-endian frame length must be observable without consuming it.");
    Assert(state.TryConsumeReadFrame(out byte[] frame) && frame.Length == 4 && frame[2] == 42,
      "A complete frame must be returned as an isolated snapshot.");
    Assert(state.TotalData == 0 && !state.CheckBytes,
      "Consuming the last frame must clear the check marker.");

    Assert(!state.TryAppendReadBytes(new byte[9]),
      "An append beyond the bounded read capacity must be rejected atomically.");
    Assert(state.TotalData == 0, "A rejected append must not change the cursor.");
    Assert(state.TryAcquireWrite(), "The first write lease must be acquired.");
    Assert(!state.TryAcquireWrite(), "A second write lease must be rejected while locked.");
    state.ReleaseWrite();
    Assert(state.TryWriteBytes(new byte[] { 1, 2, 3, 4 }),
      "A write that fits must be accepted after releasing the lease.");
    Assert(!state.TryWriteBytes(new byte[] { 5 }),
      "A write beyond the bounded write capacity must be rejected.");

    state.Reset();
    Assert(state.TotalData == 0 && state.RemainingReadBufferLength == 8 && !state.CheckBytes,
      "Reset must restore the empty receive state.");
  }

  private static void TestInboundNetworkCommandPreservesMetadataAndPayloadIsolation()
  {
    byte[] sourcePayload = new byte[] { 7, 8, 9 };
    InboundNetworkCommand command = new(
      connectionSlot: 4,
      messageId: 42,
      protocolVersion: 3,
      payload: sourcePayload,
      receiveSequence: 12);

    sourcePayload[0] = 99;
    Assert(command.ConnectionSlot == 4, "The command must preserve its connection slot.");
    Assert(command.MessageId == 42, "The command must preserve its protocol message id.");
    Assert(command.ProtocolVersion == 3, "The command must preserve its protocol version.");
    Assert(command.ReceiveSequence == 12, "The command must preserve its receive sequence.");
    Assert(
      command.Payload.Length == 3 && command.Payload.Span[0] == 7,
      "The command must isolate its payload from the caller's mutable array.");

    byte[] exposedPayload = command.Payload.ToArray();
    exposedPayload[0] = 100;
    Assert(
      command.Payload.Span[0] == 7,
      "A payload snapshot must not be mutable through an exposed copy.");

    bool rejected = false;
    try
    {
      _ = new InboundNetworkCommand(-1, 42, 3, Array.Empty<byte>(), 13);
    }
    catch (ArgumentOutOfRangeException)
    {
      rejected = true;
    }

    Assert(rejected, "A negative connection slot must be rejected at the command boundary.");
  }

  private static void TestNetworkPacketEnvelopeOwnsHeaderLengthAndRecycle()
  {
    NetworkBufferPoolAdapter pool = new();
    NetworkPacketEnvelope packet = new(
      id: 7,
      payloadCapacity: 4,
      bufferPool: pool);

    Assert(packet.Id == 7, "The packet must preserve its module id.");
    Assert(packet.Length == NetworkPacketEnvelope.HeaderSize + 4,
      "A new packet must reserve header and payload capacity.");
    Assert(packet.PayloadLength == 0, "A new packet must have an empty payload cursor.");
    Assert(
      packet.Buffer.Span[0] == 9 && packet.Buffer.Span[1] == 0 &&
      packet.Buffer.Span[2] == NetworkPacketEnvelope.PacketMarker &&
      packet.Buffer.Span[3] == 7 && packet.Buffer.Span[4] == 0,
      "The packet must write the Version4-compatible header at construction.");

    Assert(packet.TryWritePayload(new byte[] { 1, 2, 3 }),
      "A payload that fits must be accepted.");
    Assert(!packet.TryWritePayload(new byte[] { 4, 5 }),
      "A payload that exceeds the reserved capacity must be rejected.");
    packet.ShrinkToFit();
    Assert(packet.Length == 8 && packet.PayloadLength == 3,
      "ShrinkToFit must reduce the packet to its written payload length.");
    Assert(packet.Buffer.Span[0] == 8 && packet.Buffer.Span[1] == 0,
      "ShrinkToFit must rewrite the little-endian packet length.");

    Assert(packet.Recycle(), "The packet must return its lease to the pool exactly once.");
    Assert(!packet.Recycle(), "A recycled packet must reject a second recycle.");
    Assert(pool.SmallAvailableCount == 1, "Recycling must return the packet buffer to its bucket.");

    bool rejected = false;
    try
    {
      _ = new NetworkPacketEnvelope(
        id: 1,
        payloadCapacity: ushort.MaxValue,
        bufferPool: pool);
    }
    catch (ArgumentOutOfRangeException)
    {
      rejected = true;
    }

    Assert(rejected, "A packet larger than the ushort wire length must be rejected.");
  }

  private static void TestNetworkModuleRegistryAssignsIdsAndDispatchesImmutablePayload()
  {
    NetworkModuleRegistryState registry = new();
    byte observedFirstByte = 0;
    int observedConnectionSlot = -1;

    ushort assignedId = registry.Register<RegistryProbeModule>(
      (payload, connectionSlot) =>
      {
        observedFirstByte = payload.Span[0];
        observedConnectionSlot = connectionSlot;
        return true;
      });

    Assert(assignedId == 0, "The first module must receive the first available protocol id.");
    Assert(registry.GetId<RegistryProbeModule>() == assignedId,
      "A registered module type must resolve to its assigned protocol id.");

    byte[] sourcePayload = new byte[] { 17, 18 };
    Assert(registry.TryDispatch(assignedId, sourcePayload, connectionSlot: 3, out bool handled) && handled,
      "A registered module must dispatch to its adapter handler.");
    sourcePayload[0] = 99;
    Assert(observedFirstByte == 17 && observedConnectionSlot == 3,
      "Dispatch must isolate the handler payload from the caller's mutable buffer.");

    Assert(!registry.TryDispatch(ushort.MaxValue, Array.Empty<byte>(), 3, out handled) && !handled,
      "An unknown module id must be rejected without invoking a handler.");
    Assert(!registry.TryRegister<RegistryProbeModule>(
      assignedId,
      (_, _) => true),
      "A duplicate module type or id must be rejected.");
  }

  private sealed class RegistryProbeModule
  {
  }

  private static void Assert(bool condition, string message)
  {
    if (!condition)
    {
      throw new InvalidOperationException(message);
    }
  }
}

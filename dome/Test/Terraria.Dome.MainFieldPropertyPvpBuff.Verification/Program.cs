using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Terraria.Dome.Protocol.V1456.Dispatch;
using Terraria.Dome.Protocol.V1456.Packets;
using Terraria.Dome.Protocol.V1456.Protocol;
using Terraria.Dome.Protocol.V1456.Session;
using Terraria.Dome.Server;
using Terraria.Dome.Simulation;
using Terraria.Dome.Simulation.StatusEffects.Definitions;
using Terraria.Dome.Simulation.StatusEffects.Snapshots;
using Terraria.Dome.Simulation.WorldModel;

VerifyLegacyPvpBuffRegistry();
VerifySimulationAuthorityPolicy();
VerifyMessage55CodecAndDispatcher();
await VerifyMessage55ServerAuthorityAsync();

Console.WriteLine("PASS: Main.pvpBuff message-55 authoritative ECS boundary");

static void VerifyLegacyPvpBuffRegistry()
{
  int[] expected = [20, 24, 30, 31, 36, 39, 44, 69, 70, 103, 119, 120, 137, 320, 323, 324];
  IReadOnlySet<int> actual = LegacyPvpBuffRegistry.RegisterDefaults();
  if (actual.Count != expected.Length || !expected.All(actual.Contains) ||
      actual.Any(value => value < 0 || value >= LegacyPvpBuffRegistry.BuffTypeCount) ||
      !LegacyPvpBuffRegistry.IsPvpBuff(20) || !LegacyPvpBuffRegistry.IsPvpBuff(324) ||
      LegacyPvpBuffRegistry.IsPvpBuff(0) || LegacyPvpBuffRegistry.IsPvpBuff(21) ||
      LegacyPvpBuffRegistry.IsPvpBuff(-1) ||
      LegacyPvpBuffRegistry.IsPvpBuff(LegacyPvpBuffRegistry.BuffTypeCount))
  {
    throw new InvalidOperationException("Legacy Main.pvpBuff exact allowlist was not preserved.");
  }
}

static void VerifyMessage55CodecAndDispatcher()
{
  AddPlayerBuffPvpPacket request = new(7, 323, 1800);
  byte[] frame = TerrariaPacketCodec.Encode(request);
  TerrariaFrame decodedFrame = TerrariaFrameCodec.Decode(frame);
  if (decodedFrame.MessageId != TerrariaMessageId.AddPlayerBuffPvp ||
      decodedFrame.Payload.Length != sizeof(byte) + sizeof(ushort) + sizeof(int))
  {
    throw new InvalidOperationException("Message 55 did not encode its exact seven-byte payload.");
  }

  AddPlayerBuffPvpPacket decoded = TerrariaPacketCodec.DecodeAddPlayerBuffPvp(frame);
  if (decoded != request)
  {
    throw new InvalidOperationException("Message 55 codec did not round-trip target, buff, duration.");
  }

  TerrariaMessageDescriptor descriptor = TerrariaMessageCatalog.Get(
    TerrariaMessageId.AddPlayerBuffPvp);
  if (descriptor.Direction != TerrariaPacketDirection.ClientToServer ||
      descriptor.Support != TerrariaPacketSupport.Handled ||
      !string.Equals(descriptor.Name, "AddPlayerBuffPvP", StringComparison.Ordinal))
  {
    throw new InvalidOperationException("Message 55 catalog descriptor is not client-to-server handled.");
  }

  TerrariaSession session = new(3);
  MoveSessionToActive(session, 3);
  TerrariaPacketDispatchResult result = new TerrariaPacketDispatcher().Dispatch(session, frame);
  if (result.AddPlayerBuffPvp != request ||
      result.Outcome != TerrariaPacketDispatchOutcome.ActiveSynchronizationAccepted)
  {
    throw new InvalidOperationException("Active message 55 was not retained by the dispatcher.");
  }

  byte[] truncated = frame[..^1];
  byte[] trailing = [.. frame, 0xFF];
  byte[] invalidDurationPayload = new byte[7];
  invalidDurationPayload[0] = 7;
  BinaryPrimitives.WriteUInt16LittleEndian(invalidDurationPayload.AsSpan(1, 2), 323);
  byte[] invalidDuration = TerrariaFrameCodec.Encode(new TerrariaFrame(
    TerrariaMessageId.AddPlayerBuffPvp,
    invalidDurationPayload));
  ExpectInvalidData(() => TerrariaPacketCodec.DecodeAddPlayerBuffPvp(truncated));
  ExpectInvalidData(() => TerrariaPacketCodec.DecodeAddPlayerBuffPvp(trailing));
  ExpectInvalidData(() => TerrariaPacketCodec.DecodeAddPlayerBuffPvp(invalidDuration));
}

static void VerifySimulationAuthorityPolicy()
{
  using DomeSimulation simulation = new(new WorldGrid(200, 150));
  PlayerHandle source = simulation.CreatePlayer(new SimulationVector(10.0f, 10.0f));
  PlayerHandle target = simulation.CreatePlayer(new SimulationVector(12.0f, 10.0f));
  if (!simulation.TryQueuePlayerPvpBuff(source, target, 323, 1800))
  {
    throw new InvalidOperationException("A valid player PvP buff was not queued.");
  }

  simulation.Tick(new SimulationInputBatch());
  PlayerStatusEffectStateSnapshot targetState = simulation
    .CreatePlayerStatusEffectStateSnapshots()
    .Single(state => state.PlayerId == target.Value);
  if (targetState.Effects.Count != 1 || targetState.Effects[0].Type != 323 ||
      targetState.Effects[0].RemainingTicks != 1800 || targetState.Revision != 1)
  {
    throw new InvalidOperationException(
      "A queued player PvP buff did not commit deterministically to its target snapshot.");
  }

  if (simulation.TryQueuePlayerPvpBuff(source, target, 1, 1800) ||
      simulation.TryQueuePlayerPvpBuff(source, target, 323, 0) ||
      simulation.TryQueuePlayerPvpBuff(source, source, 323, 1800) ||
      simulation.TryQueuePlayerPvpBuff(new PlayerHandle(99), target, 323, 1800))
  {
    throw new InvalidOperationException("Invalid player PvP buff authority input was accepted.");
  }

  if (!simulation.DestroyPlayer(target) ||
      simulation.TryQueuePlayerPvpBuff(source, target, 323, 1800))
  {
    throw new InvalidOperationException("An inactive player remained a valid PvP buff target.");
  }
}

static async Task VerifyMessage55ServerAuthorityAsync()
{
  WorldGrid world = new(4200, 1200);
  using DomeServer server = new(world);
  server.Start();
  using TcpClient senderClient = new();
  using TcpClient targetClient = new();
  await senderClient.ConnectAsync("127.0.0.1", server.Port);
  await targetClient.ConnectAsync("127.0.0.1", server.Port);
  using NetworkStream sender = senderClient.GetStream();
  using NetworkStream target = targetClient.GetStream();
  byte senderSlot = await ActivateAsync(sender, "Sender");
  byte targetSlot = await ActivateAsync(target, "Target");

  await sender.WriteAsync(TerrariaPacketCodec.Encode(
    new AddPlayerBuffPvpPacket(targetSlot, 323, 1800)));
  await WaitForStatusAsync(server, targetSlot, 323, 1800);

  if (server.SimulationFault is not null)
  {
    throw new InvalidOperationException(
      $"Message 55 caused a server simulation fault: {server.SimulationFault.Message}");
  }

  IReadOnlyList<PlayerStatusEffectStateSnapshot> states =
    server.CreatePlayerStatusEffectStateSnapshots();
  PlayerStatusEffectStateSnapshot targetState = states.Single(state => state.PlayerId ==
    server.ResolvePlayerHandleForSlot(targetSlot).Value);
  StatusEffectSnapshot effect = targetState.Effects.Single(status => status.Type == 323);
  if (effect.RemainingTicks != 1800 || effect.TargetId != targetState.PlayerId)
  {
    throw new InvalidOperationException("Accepted message 55 did not reach the target snapshot.");
  }

  await sender.WriteAsync(TerrariaPacketCodec.Encode(
    new AddPlayerBuffPvpPacket(targetSlot, 1, 1800)));
  await sender.WriteAsync(TerrariaPacketCodec.Encode(
    new AddPlayerBuffPvpPacket(250, 323, 1800)));
  await Task.Delay(TimeSpan.FromMilliseconds(80));
  if (server.CreatePlayerStatusEffectStateSnapshots()
      .Single(state => state.PlayerId == targetState.PlayerId)
      .Effects.Count != 1)
  {
    throw new InvalidOperationException(
      "Disallowed or nonexistent message-55 targets changed authoritative state.");
  }

  await sender.WriteAsync(TerrariaPacketCodec.Encode(
    new AddPlayerBuffPvpPacket(senderSlot, 323, 1800)));
  await Task.Delay(TimeSpan.FromMilliseconds(80));
  if (server.CreatePlayerStatusEffectStateSnapshots()
      .Single(state => state.PlayerId == server.ResolvePlayerHandleForSlot(senderSlot).Value)
      .Effects.Any(status => status.Type == 323))
  {
    throw new InvalidOperationException("Message 55 self-target was not rejected by server authority.");
  }
}

static void MoveSessionToActive(TerrariaSession session, byte slot)
{
  _ = session.AcceptHello(TerrariaPacketCodec.Encode(new HelloPacket()));
  _ = session.AcceptPlayerProfile(TerrariaPacketCodec.Encode(CreateProfile(slot)));
  _ = session.AcceptPlayerUuid(TerrariaPacketCodec.Encode(new PlayerUuidPacket(
    $"00000000-0000-0000-0000-{slot:x12}")));
  _ = session.AcceptRequestWorldData(TerrariaPacketCodec.EncodeRequestWorldData());
  _ = session.AcceptSpawnTileData(TerrariaPacketCodec.EncodeSpawnTileData(
    new SpawnTileDataRequestPacket(2100, 300, 0)));
  session.MarkInitialWorldStreamSent();
  _ = session.AcceptPlayerSpawn(TerrariaPacketCodec.Encode(new PlayerSpawnPacket(
    slot, 2100, 300, 0, 0, 0, 0, 0)));
}

static PlayerProfilePacket CreateProfile(byte slot)
{
  TerrariaColor color = new(0, 0, 0);
  return new PlayerProfilePacket(
    slot,
    0,
    0,
    0.0f,
    0,
    $"Player-{slot}",
    0,
    0,
    0,
    color,
    color,
    color,
    color,
    color,
    color,
    color,
    0,
    0,
    0);
}

static async Task<byte> ActivateAsync(NetworkStream stream, string name)
{
  await stream.WriteAsync(TerrariaPacketCodec.Encode(new HelloPacket()));
  TerrariaFrame userSlot = TerrariaFrameCodec.Decode(await ReadFrameAsync(stream));
  byte slot = userSlot.Payload.Span[0];
  await stream.WriteAsync(TerrariaPacketCodec.Encode(CreateProfile(slot) with { Name = name }));
  await stream.WriteAsync(TerrariaPacketCodec.Encode(new PlayerUuidPacket(
    $"00000000-0000-0000-0000-{slot:x12}")));
  await stream.WriteAsync(TerrariaPacketCodec.EncodeRequestWorldData());
  _ = await ReadFrameAsync(stream);
  await stream.WriteAsync(TerrariaPacketCodec.EncodeSpawnTileData(
    new SpawnTileDataRequestPacket(2100, 300, 0)));
  for (int index = 0; index < 17; index++)
  {
    _ = await ReadFrameAsync(stream);
  }

  await stream.WriteAsync(TerrariaPacketCodec.Encode(new PlayerSpawnPacket(
    slot, 10, 10, 0, 0, 0, 0, 0)));
  _ = await ReadFrameAsync(stream);
  _ = await ReadFrameAsync(stream);
  return slot;
}

static async Task WaitForStatusAsync(DomeServer server, byte targetSlot, ushort type, int duration)
{
  using CancellationTokenSource timeout = new(TimeSpan.FromSeconds(3));
  while (!timeout.IsCancellationRequested)
  {
    if (!server.TryResolvePlayerHandleForSlot(targetSlot, out PlayerHandle target))
    {
      await Task.Delay(TimeSpan.FromMilliseconds(16), timeout.Token);
      continue;
    }

    PlayerStatusEffectStateSnapshot state = server.CreatePlayerStatusEffectStateSnapshots()
      .SingleOrDefault(candidate => candidate.PlayerId == target.Value) ??
      throw new InvalidOperationException("Target player snapshot was not created.");
    if (state.Effects.Any(effect => effect.Type == type && effect.RemainingTicks == duration))
    {
      return;
    }

    await Task.Delay(TimeSpan.FromMilliseconds(16), timeout.Token);
  }

  throw new InvalidOperationException("Message 55 did not commit before the timeout.");
}

static async Task<byte[]> ReadFrameAsync(NetworkStream stream)
{
  byte[] length = new byte[2];
  await stream.ReadExactlyAsync(length);
  int frameLength = length[0] | length[1] << 8;
  byte[] frame = new byte[frameLength];
  length.CopyTo(frame, 0);
  await stream.ReadExactlyAsync(frame.AsMemory(2));
  return frame;
}

static void ExpectInvalidData(Action action)
{
  try
  {
    action();
  }
  catch (InvalidDataException)
  {
    return;
  }

  throw new InvalidOperationException("Malformed message 55 input was accepted.");
}

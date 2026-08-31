using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Terraria.Dome.Protocol.V1456.Compatibility;
using Terraria.Dome.Simulation;
using Terraria.Dome.Simulation.Player.Components;
using Terraria.Dome.Simulation.Items;
using Terraria.Dome.Simulation.WorldModel;
using Terraria.Dome.Simulation.WorldObjects;
using Terraria.Dome.Simulation.WorldGeneration;
using Terraria.Dome.Protocol.V1456.Isolation;
using Terraria.Dome.Protocol.V1456.Protocol;
using Terraria.Dome.Server;

List<string> results = new();

Test("Network isolation and projection do not advance an unstarted simulation", () =>
{
  using DomeServer server = new();
  if (server.SimulationTickNumber != 0)
  {
    throw new InvalidOperationException("A new server did not start at simulation Tick zero.");
  }

  if (!server.NetworkIsolation.EnqueueInbound(NetworkInboundEnvelope.FromFrame(
        1,
        TerrariaFrameCodec.Encode(
          new TerrariaFrame(TerrariaMessageId.Ping, Array.Empty<byte>())))))
  {
    throw new InvalidOperationException("The host rejected an inbound isolation envelope.");
  }

  _ = server.CreateWorldDataContext();
  if (server.SimulationTickNumber != 0)
  {
    throw new InvalidOperationException(
      "Protocol isolation or world projection advanced simulation before the host Tick loop.");
  }
});

Test("Inbound envelopes preserve sequence and drain once", () =>
{
  DomeNetworkIsolation isolation = new(maxInboundCount: 4, maxOutboundCount: 4);
  NetworkInboundEnvelope first = NetworkInboundEnvelope.FromFrame(
    3,
    TerrariaFrameCodec.Encode(
      new TerrariaFrame(TerrariaMessageId.Ping, Array.Empty<byte>())));
  NetworkInboundEnvelope second = NetworkInboundEnvelope.FromFrame(
    3,
    TerrariaFrameCodec.Encode(
      new TerrariaFrame(TerrariaMessageId.Ping, new byte[] { 4, 5, 6 })));
  if (!isolation.EnqueueInbound(first) || !isolation.EnqueueInbound(second))
  {
    throw new InvalidOperationException("Inbound enqueue unexpectedly failed.");
  }

  List<NetworkInboundEnvelope> observed = new();
  int processed = isolation.Update(new RecordingProtocolCommandSink(observed));
  if (processed != 2 || observed.Count != 2 || observed[0].Sequence >= observed[1].Sequence)
  {
    throw new InvalidOperationException("Inbound sequence or drain count was incorrect.");
  }

  if (isolation.Update(new RecordingProtocolCommandSink(new())) != 0)
  {
    throw new InvalidOperationException("Inbound envelopes were consumed more than once.");
  }
});

Test("Inbound queue records unsupported messages and closes safely", () =>
{
  DomeNetworkIsolation isolation = new(maxInboundCount: 8, maxOutboundCount: 2);
  NetworkInboundEnvelope envelope = NetworkInboundEnvelope.FromFrame(
    1,
    TerrariaFrameCodec.Encode(
      new TerrariaFrame(TerrariaMessageId.Ping, Array.Empty<byte>())));
  if (!isolation.EnqueueInbound(envelope))
  {
    throw new InvalidOperationException("Unsupported test frame was not enqueued.");
  }

  int processed = isolation.Update(new FixedResultCommandSink(ProtocolCommandResult.Unsupported));
  if (processed != 1 || isolation.UnsupportedMessageIds.Count != 1 ||
      isolation.UnsupportedMessageIds[0] != TerrariaMessageId.Ping)
  {
    throw new InvalidOperationException("Unsupported message diagnostics were not recorded.");
  }

  isolation.Close();
  if (!isolation.IsClosed || isolation.EnqueueInbound(envelope) || isolation.RejectedInboundCount < 1)
  {
    throw new InvalidOperationException("Closed isolation layer accepted an inbound envelope.");
  }
});

Test("Concurrent inbound producers preserve all accepted envelopes", () =>
{
  const int ProducerCount = 4;
  const int EnvelopesPerProducer = 8;
  DomeNetworkIsolation isolation = new(
    maxInboundCount: ProducerCount * EnvelopesPerProducer,
    maxOutboundCount: 1);
  byte[] frame = TerrariaFrameCodec.Encode(
    new TerrariaFrame(TerrariaMessageId.Ping, Array.Empty<byte>()));
  Task[] producers = new Task[ProducerCount];
  for (int producer = 0; producer < ProducerCount; producer++)
  {
    producers[producer] = Task.Run(() =>
    {
      for (int index = 0; index < EnvelopesPerProducer; index++)
      {
        if (!isolation.EnqueueInbound(NetworkInboundEnvelope.FromFrame(1, frame)))
        {
          throw new InvalidOperationException("Concurrent inbound enqueue unexpectedly failed.");
        }
      }
    });
  }

  Task.WaitAll(producers);
  List<NetworkInboundEnvelope> observed = new();
  int processed = isolation.Update(new RecordingProtocolCommandSink(observed));
  if (processed != ProducerCount * EnvelopesPerProducer ||
      observed.Count != ProducerCount * EnvelopesPerProducer)
  {
    throw new InvalidOperationException("Concurrent inbound producers lost envelopes.");
  }
});

Test("DomeServer drains isolated inbound data at a tick boundary", () =>
{
  using DomeServer server = new();
  server.Start();
  NetworkInboundEnvelope envelope = NetworkInboundEnvelope.FromFrame(
    playerSlot: 1,
    TerrariaFrameCodec.Encode(
      new TerrariaFrame(TerrariaMessageId.Ping, Array.Empty<byte>())));
  if (!server.NetworkIsolation.EnqueueInbound(envelope))
  {
    throw new InvalidOperationException("DomeServer isolation queue rejected the diagnostic frame.");
  }

  Stopwatch timeout = Stopwatch.StartNew();
  while (server.NetworkIsolation.UnsupportedMessageIds.Count == 0 &&
         timeout.Elapsed < TimeSpan.FromSeconds(2))
  {
    Thread.Sleep(10);
  }

  if (server.NetworkIsolation.UnsupportedMessageIds.Count != 1 ||
      server.NetworkIsolation.UnsupportedMessageIds[0] != TerrariaMessageId.Ping)
  {
    throw new InvalidOperationException("DomeServer did not drain its isolation queue at a tick.");
  }
});

Test("ChestName payloads remain explicitly isolated at the server boundary", () =>
{
  using DomeServer server = new();
  server.Start();
  IReadOnlyList<ChestSnapshot> beforeChests = server.CreateChestSnapshots();
  byte[] maliciousChestNameFrame = TerrariaFrameCodec.Encode(
    new TerrariaFrame(
      TerrariaMessageId.ChestName,
      new byte[] { 0xFF, 0xFF, 0x00, 0x00, 0x00, 0xFF, 0xFF, 0xFF }));
  if (!server.NetworkIsolation.EnqueueInbound(
        NetworkInboundEnvelope.FromFrame(playerSlot: 2, maliciousChestNameFrame)))
  {
    throw new InvalidOperationException(
      "ChestName fixture was rejected before reaching isolation.");
  }

  Stopwatch timeout = Stopwatch.StartNew();
  while (!server.NetworkIsolation.UnsupportedMessageIds.Contains(TerrariaMessageId.ChestName) &&
         timeout.Elapsed < TimeSpan.FromSeconds(2))
  {
    Thread.Sleep(10);
  }

  IReadOnlyList<ChestSnapshot> afterChests = server.CreateChestSnapshots();
  bool chestStateChanged = afterChests.Count != beforeChests.Count;
  for (int index = 0; !chestStateChanged && index < beforeChests.Count; index++)
  {
    chestStateChanged = beforeChests[index].ChestId != afterChests[index].ChestId ||
      beforeChests[index].Revision != afterChests[index].Revision;
  }

  if (!server.NetworkIsolation.UnsupportedMessageIds.Contains(TerrariaMessageId.ChestName) ||
      chestStateChanged)
  {
    throw new InvalidOperationException(
      "ChestName was not rejected without mutating server-owned chest state.");
  }
});

Test("Outbound envelopes enforce capacity and expose immutable payload copies", () =>
{
  DomeNetworkIsolation isolation = new(maxInboundCount: 1, maxOutboundCount: 1);
  byte[] payload = [10, 20];
  NetworkOutboundEnvelope envelope = NetworkOutboundEnvelope.Frame(
    playerSlot: 4,
    targetClient: 4,
    messageId: TerrariaMessageId.Ping,
    payload);
  if (!isolation.EnqueueOutbound(envelope) || isolation.EnqueueOutbound(envelope))
  {
    throw new InvalidOperationException("Outbound capacity was not enforced.");
  }

  payload[0] = 99;
  IReadOnlyList<NetworkOutboundEnvelope> outbound = isolation.ReadOutbound();
  if (outbound.Count != 1 || outbound[0].Payload.Span[0] != 10)
  {
    throw new InvalidOperationException("Outbound payload ownership was not isolated.");
  }
});

Test("MessageBuffer rejects malformed frame and emits valid inbound frame", () =>
{
  DomeNetworkIsolation isolation = new(maxInboundCount: 4, maxOutboundCount: 4);
  MessageBuffer buffer = new(playerSlot: 2, isolation);
  byte[] validFrame = [4, 0, 1, 2];
  if (buffer.ReceiveBytes(Array.Empty<byte>(), out _) != MessageBufferReceiveResult.Incomplete ||
      !buffer.CheckBytes(validFrame) || buffer.CheckBytes([1, 0]))
  {
    throw new InvalidOperationException("MessageBuffer byte-boundary checks were incorrect.");
  }

  if (buffer.ReceiveBytes(validFrame, out int accepted) != MessageBufferReceiveResult.Accepted ||
      accepted != validFrame.Length)
  {
    throw new InvalidOperationException("Valid frame was not accepted.");
  }

  if (buffer.ReceiveBytes([1, 0], out _) != MessageBufferReceiveResult.Rejected)
  {
    throw new InvalidOperationException("Malformed frame was accepted.");
  }

  if (buffer.ReceiveBytes([2, 0], out _) != MessageBufferReceiveResult.Rejected)
  {
    throw new InvalidOperationException("A below-minimum frame was accepted.");
  }
});

Test("MessageBuffer reassembles a fragmented frame", () =>
{
  DomeNetworkIsolation isolation = new(maxInboundCount: 4, maxOutboundCount: 4);
  MessageBuffer buffer = new(playerSlot: 2, isolation);
  byte[] frame = [4, 0, 1, 2];
  if (buffer.ReceiveBytes(frame.AsSpan(0, 1), out _) != MessageBufferReceiveResult.Incomplete)
  {
    throw new InvalidOperationException("A partial length prefix was not reported as incomplete.");
  }

  if (buffer.ReceiveBytes(frame.AsSpan(1, 2), out _) != MessageBufferReceiveResult.Incomplete)
  {
    throw new InvalidOperationException("A partial frame was not reported as incomplete.");
  }

  if (buffer.ReceiveBytes(frame.AsSpan(3, 1), out int accepted) !=
      MessageBufferReceiveResult.Accepted || accepted != frame.Length)
  {
    throw new InvalidOperationException("A fragmented frame was not reassembled.");
  }
});

Test("NetMessage emits a framed outbound envelope", () =>
{
  DomeNetworkIsolation isolation = new(maxInboundCount: 1, maxOutboundCount: 4);
  NetMessage netMessage = new(isolation);
  if (!netMessage.TrySendData(
        TerrariaMessageId.Ping,
        2,
        targetClient: -1,
        ignoreClient: 5,
        payload: new byte[] { 8 }))
  {
    throw new InvalidOperationException("NetMessage did not enqueue its frame.");
  }

  NetworkOutboundEnvelope envelope = isolation.ReadOutbound()[0];
  TerrariaFrame frame = TerrariaFrameCodec.Decode(envelope.FrameBytes.Span);
  if (envelope.TargetClient != -1 || envelope.IgnoreClient != 5 ||
      frame.MessageId != TerrariaMessageId.Ping ||
      frame.Payload.Span[0] != 8)
  {
    throw new InvalidOperationException("NetMessage emitted an incorrect frame.");
  }
});

Test("Network slices copy Dome snapshot state", () =>
{
  PlayerSnapshot player = new(
    new PlayerHandle(7),
    new SimulationVector(10, 20),
    new SimulationVector(1, 2),
    Facing: 1,
    IsGrounded: false,
    Health: 80,
    IsActive: true,
    RespawnTicks: 14,
    AssignedSlot: 7,
    Mana: 15,
    MaximumMana: 30,
    GravityDirection: -1.0f,
    ColliderWidth: 1.5f,
    ColliderHeight: 2.5f,
    SelectedSlot: 4,
    SelectedLoadout: 2,
    AccessoryVisibility: 3,
    EquipmentStateCount: 5,
    EquipmentRevision: 8,
    EquipmentLoadoutRevision: 2,
    BuffCount: 3,
    BuffRevision: 9,
    WellFedRank: 2,
    WellFedTimeLeft: 600,
    MaximumHealth: 120,
    Defense: 17,
    LifeRegenUnitsPerTick: 75,
    HealthRegenerationDelayTicks: 6,
    HealthRegenerationAccumulator: 3,
    ManaRegenerationDelayTicks: 4,
    ManaRegenerationAccumulator: 2,
    ImmunityRemainingTicks: 5,
    IsImmune: true,
    UseItem: true,
    IsUsingItem: true,
    ItemUseRevision: 6,
    ItemUseCooldownTicks: 3,
    ItemAnimationTicks: 2,
    IsChannelingItem: true,
    FireCooldownTicks: 11,
    Aggro: 8,
    NoAggroNpcTypeCount: 3,
    HasInteractionTarget: true,
    InteractionMode: PlayerInteractionMode.OpenChest,
    Down: true,
    Up: true,
    Fire: true,
    UseTile: true,
    Dash: true,
    MoveLeft: true,
    MoveRight: true,
    Jump: true,
    Stealth: 0.375f,
    IsInvisible: true,
    HasShroomiteStealth: true,
    IsVortexStealthActive: true,
    StealthTimer: 4,
    ItemAnimationJustStarted: true);
  NetworkPlayerSlice playerSlice = NetworkPlayerSlice.From(player);
  if (playerSlice.PlayerSlot != 7 || playerSlice.Health != 80 ||
      playerSlice.RespawnTicks != 14 ||
      playerSlice.PositionX != 10 || playerSlice.VelocityY != 2 ||
      playerSlice.GravityDirection != -1.0f ||
      playerSlice.ColliderWidth != 1.5f || playerSlice.ColliderHeight != 2.5f ||
      playerSlice.SelectedSlot != 4 || playerSlice.SelectedLoadout != 2 ||
      playerSlice.AccessoryVisibility != 3 || playerSlice.MaximumHealth != 120 ||
      playerSlice.EquipmentStateCount != 5 || playerSlice.EquipmentRevision != 8 ||
      playerSlice.EquipmentLoadoutRevision != 2 || playerSlice.BuffCount != 3 ||
      playerSlice.BuffRevision != 9 || playerSlice.WellFedRank != 2 ||
      playerSlice.WellFedTimeLeft != 600 ||
      playerSlice.Defense != 17 || playerSlice.IsGrounded ||
      playerSlice.LifeRegenUnitsPerTick != 75 ||
      playerSlice.HealthRegenerationDelayTicks != 6 ||
      playerSlice.HealthRegenerationAccumulator != 3 ||
      playerSlice.ManaRegenerationDelayTicks != 4 ||
      playerSlice.ManaRegenerationAccumulator != 2 ||
      playerSlice.ImmunityRemainingTicks != 5 || !playerSlice.IsImmune ||
      !playerSlice.IsUsingItem || !playerSlice.UseItem || playerSlice.ItemUseRevision != 6 ||
      playerSlice.ItemUseCooldownTicks != 3 || playerSlice.ItemAnimationTicks != 2 ||
      !playerSlice.IsChannelingItem || playerSlice.FireCooldownTicks != 11 ||
      playerSlice.Aggro != 8 || playerSlice.NoAggroNpcTypeCount != 3 ||
      !playerSlice.HasInteractionTarget ||
      playerSlice.InteractionMode != PlayerInteractionMode.OpenChest ||
      !playerSlice.Down || !playerSlice.Up || !playerSlice.Fire ||
      !playerSlice.UseTile || !playerSlice.Dash || !playerSlice.MoveLeft ||
      !playerSlice.MoveRight || !playerSlice.Jump ||
      playerSlice.Stealth != 0.375f || !playerSlice.IsInvisible ||
      !playerSlice.HasShroomiteStealth || !playerSlice.IsVortexStealthActive ||
      playerSlice.StealthTimer != 4 || !playerSlice.ItemAnimationJustStarted)
  {
    throw new InvalidOperationException("Player slice did not preserve snapshot fields.");
  }

  ItemStack[] slots = [new ItemStack(42, 3)];
  ChestSnapshot chest = new(
    chestId: 2,
    tileX: 4,
    tileY: 5,
    opener: null,
    slots: slots,
    revision: 1,
    section: new WorldSectionCoordinates(0, 0),
    isLocked: false);
NetworkChestSlice chestSlice = NetworkChestSlice.From(chest);
slots[0] = ItemStack.Empty;
if (chestSlice.Slots[0].ItemType != 42 || chestSlice.Slots[0].Quantity != 3)
{
  throw new InvalidOperationException("Chest slice retained mutable source storage.");
}

bool rejectedInvalidChestStackProjection = false;
try
{
  _ = NetworkChestSlice.From(new ChestSnapshot(
    chestId: 2,
    tileX: 4,
    tileY: 5,
    opener: null,
    slots: [new ItemStack(0, 1)],
    revision: 1,
    section: new WorldSectionCoordinates(0, 0),
    isLocked: false));
}
catch (ArgumentOutOfRangeException)
{
  rejectedInvalidChestStackProjection = true;
}

if (!rejectedInvalidChestStackProjection)
{
  throw new InvalidOperationException(
    "Chest network isolation projected a non-canonical empty ItemStack.");
}

WorldTile[,] tiles = new WorldTile[2, 1];
  tiles[0, 0] = new WorldTile(IsActive: true, Type: 5);
  WorldSectionSnapshot section = new(
    new WorldSectionCoordinates(1, 2),
    width: 2,
    height: 1,
    version: 3,
    tiles);
  NetworkTileSectionSlice sectionSlice = NetworkTileSectionSlice.From(section);
  if (sectionSlice.Tiles.Count != 2 || sectionSlice.Tiles[0].Type != 5)
  {
    throw new InvalidOperationException("Tile section slice did not preserve tile state.");
  }

  WorldMetadata metadata = new(
    "slice-world",
    new WorldSeed(99),
    width: 200,
    height: 150,
    spawnX: 50,
    spawnY: 20);
  NetworkWorldSlice worldSlice = NetworkWorldSlice.From(
    metadata,
    WorldJoinStateSnapshot.CreateDefault());
  if (worldSlice.Name != "slice-world" || worldSlice.WorldId != 99 ||
      worldSlice.Width != 200 || worldSlice.SpawnX != 50 || worldSlice.AnglerQuest != 20)
  {
    throw new InvalidOperationException("World slice did not preserve authoritative state.");
  }

  NetworkTileEntitySlice entitySlice = NetworkTileEntitySlice.From(
    new LegacyHatRackTileEntity(
      EntityId: 4,
      TileX: 12,
      TileY: 13,
      Item0: new LegacyTileEntityItem(100, 2, 1),
      Item1: null,
      Dye0: new LegacyTileEntityItem(101, 3, 1),
      Dye1: null));
  if (entitySlice.EntityId != 4 || entitySlice.EntityKind != nameof(LegacyHatRackTileEntity) ||
      entitySlice.Items.Count != 2 || entitySlice.Items[1].Prefix != 3)
  {
    throw new InvalidOperationException("Tile entity slice did not preserve item state.");
  }
});

Console.WriteLine($"PASS: {results.Count} network isolation checks");

void Test(string name, Action action)
{
  action();
  results.Add(name);
}

sealed class RecordingProtocolCommandSink : IProtocolCommandSink
{
  private readonly List<NetworkInboundEnvelope> _observed;

  public RecordingProtocolCommandSink(List<NetworkInboundEnvelope> observed)
  {
    _observed = observed;
  }

  public ProtocolCommandResult Accept(NetworkInboundEnvelope envelope)
  {
    _observed.Add(envelope);
    return ProtocolCommandResult.Accepted;
  }
}

sealed class FixedResultCommandSink : IProtocolCommandSink
{
  private readonly ProtocolCommandResult _result;

  public FixedResultCommandSink(ProtocolCommandResult result)
  {
    _result = result;
  }

  public ProtocolCommandResult Accept(NetworkInboundEnvelope envelope)
  {
    return _result;
  }
}

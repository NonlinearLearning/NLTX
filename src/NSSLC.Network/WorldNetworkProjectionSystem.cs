using System.Buffers.Binary;
using System.Numerics;
using Arch.Core;
using Arch.System;
using ArchWorld = Arch.Core.World;
using Terraria.Entity;
using Terraria.World;

using Terraria.Communication;

namespace Terraria.Network;

/// <summary>
/// Projects committed world mutations into the small server-to-client packets understood by the
/// current Version4 client. The world owner remains responsible for state and drop rules; this
/// system only serializes those committed facts and targets live sessions.
/// </summary>
public sealed class WorldNetworkProjectionSystem : BaseSystem<ArchWorld, EcsServerTickContext>
{
  private const byte AreaTileChangeMessageId = 20;
  private const byte SyncItemMessageId = 21;
  private const byte ItemOwnerMessageId = 22;
  private const byte SyncItemDespawnMessageId = 151;
  private readonly IWorldProjectionSource _world;
  private readonly IWorldItemProjectionSource _items;
  private WorldItemRequestQueueComponent? _itemRequests;
  private readonly IReadOnlyList<bool> _frameImportant;
  private readonly QueryDescription _sessionQuery = new QueryDescription().WithAll<
      CommunicationSessionComponent,
      CommunicationOutboundMessageBufferComponent>();
  public WorldNetworkProjectionSystem(
      ArchWorld ecsWorld,
      IWorldProjectionSource world,
      IWorldItemProjectionSource items,
      IReadOnlyList<bool> frameImportant)
      : base(ecsWorld ?? throw new ArgumentNullException(nameof(ecsWorld)))
  {
    _world = world ?? throw new ArgumentNullException(nameof(world));
    _items = items ?? throw new ArgumentNullException(nameof(items));
    _frameImportant = frameImportant is null
        ? throw new ArgumentNullException(nameof(frameImportant))
        : Array.AsReadOnly(frameImportant.ToArray());
  }

  /// <summary>
  /// World-scoped item request queue, resolved on first use so construction order never decides
  /// which instance a system observes.
  /// </summary>
  private WorldItemRequestQueueComponent ItemRequests =>
      _itemRequests ??= WorldItemRequestQueue.Require(World);

  public override void Update(in EcsServerTickContext context)
  {
    // Drain committed mutations on every tick, even when no client is currently active.
    // A future client receives a full world snapshot, so retaining historical deltas while
    // sessions are connecting or synchronizing only creates an unbounded queue.
    IReadOnlyList<WorldMutation> drained = _world.DrainPendingMutations();
    IReadOnlyList<WorldItemMutation> itemMutations = _items.DrainMutations();

    List<SessionEpoch> activeSessions = [];
    World.Query(
        in _sessionQuery,
        (ref CommunicationSessionComponent session,
            ref CommunicationOutboundMessageBufferComponent _) =>
        {
          if (session.Stage == CommunicationSessionStage.Active)
          {
            activeSessions.Add(session.Epoch);
          }
        });

    if (activeSessions.Count == 0)
    {
      return;
    }

    Dictionary<WorldTileCoordinate, WorldMutation> latest = [];
    foreach (WorldMutation mutation in drained)
    {
      if (latest.TryGetValue(mutation.Coordinate, out WorldMutation existing))
      {
        latest[mutation.Coordinate] = existing with
        {
          Current = mutation.Current,
          DropItemType = existing.DropItemType > 0
              ? existing.DropItemType
              : mutation.DropItemType
        };
      }
      else
      {
        latest.Add(mutation.Coordinate, mutation);
      }
    }

    foreach (WorldMutation mutation in latest.Values)
    {
      if (mutation.DropItemType > 0)
      {
        // 只表达意图：掉落物由 World 域的 WorldItemRequestSystem 生成。
        ItemRequests.Enqueue(new WorldItemRequest(
            WorldItemRequestKind.Create,
            Slot: -1,
            Actor: activeSessions[0],
            ItemType: mutation.DropItemType,
            Stack: mutation.DropStack,
            Prefix: 0,
            Position: new System.Numerics.Vector2(
                mutation.Coordinate.X * _world.TileSize + 8f,
                mutation.Coordinate.Y * _world.TileSize + 8f),
            Velocity: new System.Numerics.Vector2(0f, -1.5f),
            ReservedFor: activeSessions[0]));
      }
    }

    if (latest.Count == 0 && itemMutations.Count == 0)
    {
      return;
    }

    CommitVersion version = new(Math.Max(1, _world.CommitRevision));
    World.Query(
        in _sessionQuery,
        (ref CommunicationSessionComponent session,
            ref CommunicationOutboundMessageBufferComponent outbound) =>
        {
          if (session.Stage != CommunicationSessionStage.Active)
          {
            return;
          }

          foreach (WorldMutation mutation in latest.Values)
          {
            outbound.Enqueue(new OutboundPacketIntent(
                [session.Epoch],
                null,
                AreaTileChangeMessageId,
                CreateTileChangePayload(mutation.Coordinate, mutation.Current),
                version));

          }

          foreach (WorldItemMutation mutation in itemMutations)
          {
            outbound.Enqueue(new OutboundPacketIntent(
                [session.Epoch],
                null,
                mutation.Kind switch
                {
                  WorldItemChangeKind.Retire => SyncItemDespawnMessageId,
                  WorldItemChangeKind.Ownership => ItemOwnerMessageId,
                  _ => SyncItemMessageId
                },
                CreateItemPayload(mutation),
                version));
          }
        });
  }

  private byte[] CreateTileChangePayload(WorldTileCoordinate coordinate, WorldTileState tile)
  {
    using var stream = new MemoryStream(capacity: 32);
    WriteInt16(stream, checked((short)coordinate.X));
    WriteInt16(stream, checked((short)coordinate.Y));
    stream.WriteByte(1);
    stream.WriteByte(1);
    stream.WriteByte(0);
    WriteTile(stream, tile);
    return stream.ToArray();
  }

  private void WriteTile(Stream stream, WorldTileState tile)
  {
    bool hasLiquid = tile.HasLiquid;
    bool hasTileColor = tile.Active && tile.TileColor > 0;
    bool hasWallColor = tile.Wall > 0 && tile.WallColor > 0;
    byte flags1 = 0;
    if (tile.Active) flags1 |= 1;
    if (tile.Wall > 0) flags1 |= 1 << 2;
    if (hasLiquid) flags1 |= 1 << 3;
    if ((tile.Wires & 1) != 0) flags1 |= 1 << 4;
    if (tile.HalfBlock) flags1 |= 1 << 5;
    if (tile.Actuator) flags1 |= 1 << 6;
    if (tile.Inactive) flags1 |= 1 << 7;

    byte flags2 = (byte)((tile.Slope & 7) << 4);
    if ((tile.Wires & 2) != 0) flags2 |= 1;
    if ((tile.Wires & 4) != 0) flags2 |= 1 << 1;
    if (hasTileColor) flags2 |= 1 << 2;
    if (hasWallColor) flags2 |= 1 << 3;
    if ((tile.Wires & 8) != 0) flags2 |= 1 << 7;

    byte flags3 = 0;
    if ((tile.TileHeader3 & 0x80) != 0) flags3 |= 1;
    if ((tile.WallHeader & 0x80) != 0) flags3 |= 1 << 1;
    if ((tile.TileHeader3 & 0x20) != 0) flags3 |= 1 << 2;
    if ((tile.TileHeader3 & 0x40) != 0) flags3 |= 1 << 3;
    stream.WriteByte(flags1);
    stream.WriteByte(flags2);
    stream.WriteByte(flags3);
    if (hasTileColor) stream.WriteByte(tile.TileColor);
    if (hasWallColor) stream.WriteByte(tile.WallColor);
    if (tile.Active)
    {
      WriteUInt16(stream, tile.Type);
      if (tile.Type < _frameImportant.Count && _frameImportant[tile.Type])
      {
        WriteInt16(stream, tile.FrameX);
        WriteInt16(stream, tile.FrameY);
      }
    }

    if (tile.Wall > 0) WriteUInt16(stream, tile.Wall);
    if (hasLiquid)
    {
      stream.WriteByte(tile.LiquidAmount);
      stream.WriteByte(tile.Liquid switch
      {
        Terraria.Liquid.LiquidContactFlags.Water => 0,
        Terraria.Liquid.LiquidContactFlags.Lava => 1,
        Terraria.Liquid.LiquidContactFlags.Honey => 2,
        Terraria.Liquid.LiquidContactFlags.Shimmer => 3,
        _ => 0
      });
    }
  }

  private static byte[] CreateItemPayload(WorldItemMutation mutation)
  {
    if (mutation.Kind == WorldItemChangeKind.Retire)
    {
      using var retired = new MemoryStream(capacity: 2);
      WriteInt16(retired, mutation.Item.Slot);
      return retired.ToArray();
    }

    if (mutation.Kind == WorldItemChangeKind.Ownership)
    {
      using var ownership = new MemoryStream(capacity: 24);
      WriteInt16(ownership, mutation.Item.Slot);
      ownership.WriteByte(mutation.Item.ReservedFor is null ? byte.MaxValue : (byte)0);
      Write7BitEncodedInt(ownership, mutation.Item.ReservedFor is null ? 0 : 15);
      ownership.WriteByte(byte.MaxValue);
      Write7BitEncodedInt(ownership, 0);
      WriteSingle(ownership, mutation.Item.Position.X);
      WriteSingle(ownership, mutation.Item.Position.Y);
      return ownership.ToArray();
    }

    using var stream = new MemoryStream(capacity: 24);
    WriteInt16(stream, mutation.Item.Slot);
    WriteSingle(stream, mutation.Item.Position.X);
    WriteSingle(stream, mutation.Item.Position.Y);
    WriteSingle(stream, mutation.Item.Velocity.X);
    WriteSingle(stream, mutation.Item.Velocity.Y);
    WriteInt16(stream, checked((short)Math.Clamp(mutation.Item.Stack, 1, short.MaxValue)));
    stream.WriteByte(mutation.Item.Prefix);
    stream.WriteByte(0);
    WriteInt16(stream, checked((short)mutation.Item.ItemType));
    return stream.ToArray();
  }

  private static void WriteInt16(Stream stream, short value)
  {
    Span<byte> buffer = stackalloc byte[2];
    BinaryPrimitives.WriteInt16LittleEndian(buffer, value);
    stream.Write(buffer);
  }

  private static void WriteUInt16(Stream stream, ushort value)
  {
    Span<byte> buffer = stackalloc byte[2];
    BinaryPrimitives.WriteUInt16LittleEndian(buffer, value);
    stream.Write(buffer);
  }

  private static void WriteSingle(Stream stream, float value)
  {
    Span<byte> buffer = stackalloc byte[4];
    BinaryPrimitives.WriteSingleLittleEndian(buffer, value);
    stream.Write(buffer);
  }

  private static void Write7BitEncodedInt(Stream stream, int value)
  {
    uint remaining = checked((uint)value);
    while (remaining >= 0x80)
    {
      stream.WriteByte((byte)(remaining | 0x80));
      remaining >>= 7;
    }

    stream.WriteByte((byte)remaining);
  }
}

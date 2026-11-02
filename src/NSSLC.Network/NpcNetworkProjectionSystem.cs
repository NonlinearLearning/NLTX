using System.Buffers.Binary;
using System.IO;
using System.Numerics;
using Arch.Core;
using Arch.System;
using ArchEntity = Arch.Core.Entity;
using ArchWorld = Arch.Core.World;
using Terraria.Combat;
using Terraria.Diagnostics;
using Terraria.Entity;
using Terraria.Movement;
using Terraria.Npc;
using Terraria.Physics;
using Terraria.Relationships;
using MovementVelocityComponent = Terraria.Movement.VelocityComponent;

using Terraria.Communication;

namespace Terraria.Network;

/// <summary>
/// Projects authoritative NPC entities into the Version4 packet-23 representation.
/// </summary>
/// <remarks>
/// The projection deliberately owns only wire encoding and per-session initial delivery state.
/// NPC identity, life, location and velocity remain owned by their ECS components.
/// </remarks>
public sealed class NpcNetworkProjectionSystem : BaseSystem<ArchWorld, EcsServerTickContext>
{
  private const byte SyncNpcMessageId = 23;
  private const int NpcAiCount = 4;
  private readonly QueryDescription _sessionQuery = new QueryDescription().WithAll<
      CommunicationSessionComponent,
      CommunicationOutboundMessageBufferComponent>();
  private readonly QueryDescription _npcQuery = new QueryDescription().WithAll<
      NpcNetworkIdentityComponent,
      LifeComponent,
      LocationComponent,
      MovementVelocityComponent,
      ColliderComponent,
      NpcBehaviorStateComponent,
      GroundingComponent>();

  public NpcNetworkProjectionSystem(ArchWorld world)
      : base(world ?? throw new ArgumentNullException(nameof(world)))
  {
  }

  public override void Update(in EcsServerTickContext context)
  {
    List<NpcNetworkSnapshot> npcs = CaptureNpcs();

    if (npcs.Count == 0)
    {
      return;
    }

    World.Query(
        in _sessionQuery,
        (ref CommunicationSessionComponent session,
            ref CommunicationOutboundMessageBufferComponent outbound) =>
        {
          if (session.Stage != CommunicationSessionStage.Active)
          {
            return;
          }

          foreach (NpcNetworkSnapshot npc in npcs)
          {
            bool initial = !session.HasSentNpcSlot(npc.Identity.Slot);
            outbound.Enqueue(new OutboundPacketIntent(
                [session.Epoch],
                null,
                SyncNpcMessageId,
                CreatePayload(npc, initial),
                CommitVersion.Initial));
            if (initial)
            {
              session.MarkNpcSlotSent(npc.Identity.Slot);
              ServerLog.Info(
                  "ecs.npc.network_sync",
                  $"epoch={session.Epoch.Value} slot={npc.Identity.Slot} " +
                  $"generation={npc.Identity.Generation} type={npc.Identity.NetId}");
            }
          }
        });
  }

  private List<NpcNetworkSnapshot> CaptureNpcs()
  {
    List<NpcNetworkSnapshot> npcs = [];
    World.Query(
        in _npcQuery,
        (ArchEntity _,
            ref NpcNetworkIdentityComponent identity,
            ref LifeComponent life,
            ref LocationComponent location,
            ref MovementVelocityComponent velocity,
            ref ColliderComponent collider,
            ref NpcBehaviorStateComponent behavior,
            ref GroundingComponent grounding) =>
        {
          Vector2 linearVelocity = velocity.LinearVelocity;
          bool directionPositive = linearVelocity.X >= 0f;
          bool directionYPositive = linearVelocity.Y >= 0f;

          // The ECS behavior slots are private server state. Terraria clients execute the
          // native AI for packet-23 NPC types, so publishing these values as ai[0]/ai[1] makes
          // the client reinterpret our phase and timer as native Slime/Town NPC state.
          float[] ai = [0f, 0f, 0f, 0f];
          npcs.Add(new NpcNetworkSnapshot(
              identity,
              // Guide (22) and ordinary Slime (1) use the default zero SyncAnchor on the
              // current client. Packet 23 therefore carries the ECS collider's top-left.
              location.Coordinates,
              linearVelocity,
              life.Current,
              life.Maximum,
              directionPositive,
              directionYPositive,
              directionPositive,
              ai,
              behavior,
              grounding.IsGrounded));
        });
    npcs.Sort(static (left, right) => left.Identity.Slot.CompareTo(right.Identity.Slot));
    return npcs;
  }

  private static byte[] CreatePayload(NpcNetworkSnapshot npc, bool initial)
  {
    int lifeWidth = ResolveLifeWidth(npc.MaximumLife);
    using var stream = new MemoryStream(capacity: 64);

    // The current client reads packet 23's first two bytes as slot and generation separately.
    stream.WriteByte(npc.Identity.Slot);
    stream.WriteByte(npc.Identity.Generation);
    WriteSingle(stream, npc.Position.X);
    WriteSingle(stream, npc.Position.Y);
    WriteSingle(stream, npc.Velocity.X);
    WriteSingle(stream, npc.Velocity.Y);
    WriteUInt16(stream, 0);

    byte flags1 = 0;
    SetFlag(ref flags1, 0, npc.DirectionPositive);
    SetFlag(ref flags1, 1, npc.DirectionYPositive);
    for (int index = 0; index < NpcAiCount; index++)
    {
      SetFlag(ref flags1, index + 2, npc.Ai[index] != 0f);
    }

    SetFlag(ref flags1, 6, npc.SpriteDirectionPositive);
    WriteByte(stream, flags1);

    // Bit 5 is unused by the Version4 packet-23 reader. The latest client uses it as an
    // explicit marker for ECS-authoritative NPCs, so it will not run a second local AI/physics
    // simulation between authoritative snapshots. Bit 4 is the native shimmer flag and must
    // remain clear for ordinary NPCs, otherwise the client renders them fully transparent.
    byte flags2 = 1 << 5;
    SetFlag(ref flags2, 3, initial);
    WriteByte(stream, flags2);
    for (int index = 0; index < NpcAiCount; index++)
    {
      if (npc.Ai[index] != 0f)
      {
        WriteSingle(stream, npc.Ai[index]);
      }
    }

    WriteInt16(stream, npc.Identity.NetId);
    WriteByte(stream, checked((byte)lifeWidth));
    switch (lifeWidth)
    {
      case 1:
        WriteSByte(stream, checked((sbyte)npc.Life));
        break;
      case 2:
        WriteInt16(stream, checked((short)npc.Life));
        break;
      default:
        WriteInt32(stream, npc.Life);
        break;
    }

    return stream.ToArray();
  }

  private static int ResolveLifeWidth(int maximumLife)
  {
    return maximumLife > short.MaxValue ? 4 : maximumLife > sbyte.MaxValue ? 2 : 1;
  }

  private static void SetFlag(ref byte flags, int bit, bool value)
  {
    if (value)
    {
      flags |= checked((byte)(1 << bit));
    }
  }

  private static void WriteByte(Stream stream, byte value)
  {
    stream.WriteByte(value);
  }

  private static void WriteSByte(Stream stream, sbyte value)
  {
    stream.WriteByte(unchecked((byte)value));
  }

  private static void WriteInt16(Stream stream, short value)
  {
    Span<byte> buffer = stackalloc byte[sizeof(short)];
    BinaryPrimitives.WriteInt16LittleEndian(buffer, value);
    stream.Write(buffer);
  }

  private static void WriteUInt16(Stream stream, ushort value)
  {
    Span<byte> buffer = stackalloc byte[sizeof(ushort)];
    BinaryPrimitives.WriteUInt16LittleEndian(buffer, value);
    stream.Write(buffer);
  }

  private static void WriteInt32(Stream stream, int value)
  {
    Span<byte> buffer = stackalloc byte[sizeof(int)];
    BinaryPrimitives.WriteInt32LittleEndian(buffer, value);
    stream.Write(buffer);
  }

  private static void WriteSingle(Stream stream, float value)
  {
    Span<byte> buffer = stackalloc byte[sizeof(float)];
    BinaryPrimitives.WriteSingleLittleEndian(buffer, value);
    stream.Write(buffer);
  }

  private sealed record NpcNetworkSnapshot(
      NpcNetworkIdentityComponent Identity,
      Vector2 Position,
      Vector2 Velocity,
      int Life,
      int MaximumLife,
      bool DirectionPositive,
      bool DirectionYPositive,
      bool SpriteDirectionPositive,
      IReadOnlyList<float> Ai,
      NpcBehaviorStateComponent Behavior,
      bool IsGrounded);
}

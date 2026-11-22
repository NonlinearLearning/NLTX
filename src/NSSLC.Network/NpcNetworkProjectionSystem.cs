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
    List<NpcNetworkSnapshot> npcs = CaptureNpcs(context.DeltaTime);

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
            bool initial = !session.HasSentNpcSlot(
                npc.Identity.Slot,
                npc.Identity.Generation);
            outbound.Enqueue(new OutboundPacketIntent(
                [session.Epoch],
                null,
                SyncNpcMessageId,
                CreatePayload(npc, initial),
                CommitVersion.Initial,
                OutboundPacketPriority.Realtime));
            if (initial)
            {
              session.MarkNpcSlotSent(
                  npc.Identity.Slot,
                  npc.Identity.Generation);
              ServerLog.Info(
                  "ecs.npc.network_sync",
                  $"epoch={session.Epoch.Value} slot={npc.Identity.Slot} " +
                  $"generation={npc.Identity.Generation} type={npc.Identity.NetId}");
            }
          }
        });
  }

  private List<NpcNetworkSnapshot> CaptureNpcs(float deltaTime)
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
          Vector2 wireVelocity = velocity.LinearVelocity * deltaTime;
          bool directionPositive = behavior.StateSlot0 == 0f
              ? wireVelocity.X >= 0f
              : behavior.StateSlot0 > 0f;
          bool directionYPositive = wireVelocity.Y >= 0f;

          // Ordinary type-1 Slime uses state slot 1 as its reference ai[0] jump timer. The
          // remaining native values select the default body, initialize ai[2], and leave ai[3]
          // unused. Other NPC types retain their existing zero AI projection.
          float[] ai = CreateAiState(identity.NetId, behavior);
          npcs.Add(new NpcNetworkSnapshot(
              identity,
              // Guide (22) and ordinary Slime (1) use the default zero SyncAnchor on the
              // current client. Packet 23 therefore carries the ECS collider's top-left.
              location.Coordinates,
              wireVelocity,
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

    // Bit 5 is a custom server-authority extension. The supplied Version4 client ignores this
    // reserved bit and still runs native NPC AI; packet AI state above keeps the Slime prediction
    // aligned with the authoritative timer. Bit 4 is the native shimmer flag and stays clear.
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

  private static float[] CreateAiState(
      short networkType,
      NpcBehaviorStateComponent behavior)
  {
    return networkType == 1
        ? [behavior.StateSlot1, -1f, behavior.ActionPhase > 0 ? 1f : 0f, 0f]
        : [0f, 0f, 0f, 0f];
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

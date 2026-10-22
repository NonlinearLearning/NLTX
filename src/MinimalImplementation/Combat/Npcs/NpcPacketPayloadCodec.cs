using System;
using System.IO;
using Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;
using Terraria.NetWork.Prototype.PacketDesignCompiler.Wire;

namespace MinimalImplementation.Combat.Npcs;

/// <summary>Encodes a packet-23 body; packet framing and target selection belong to P01/P03.</summary>
public sealed class NpcPacketPayloadCodec
{
  private readonly NpcPacket23ProtocolFacts _facts;

  public NpcPacketPayloadCodec(NpcPacket23ProtocolFacts facts)
  {
    ArgumentNullException.ThrowIfNull(facts);
    _facts = facts;
  }

  public byte[] EncodeSyncBody(
      NpcView npc, NpcSyncPacketProducer producer,
      byte generation = 0, bool? spawnNeedsSyncing = null)
  {
    ArgumentNullException.ThrowIfNull(npc);
    ArgumentNullException.ThrowIfNull(producer);
    SyncNPCPacket packet = producer.Create(npc);
    using var stream = new MemoryStream();
    var writer = new PacketWireWriter(stream);
    SyncNPCPacket.Write(
      writer,
      // Current client reads byte slot followed by byte generation in the same two bytes.
      unchecked((short)(checked((byte)packet.NpcSlot) | (generation << 8))),
      packet.PositionX,
      packet.PositionY,
      packet.VelocityX,
      packet.VelocityY,
      packet.Target,
      packet.DirectionPositive,
      packet.DirectionYPositive,
      packet.SpriteDirectionPositive,
      packet.FullLife,
      packet.SpawnedFromStatue,
      spawnNeedsSyncing ?? packet.SpawnNeedsSyncing,
      packet.Shimmering,
      packet.Ai,
      packet.NetId,
      packet.PlayerCount,
      packet.Difficulty,
      packet.Life,
      packet.EncodedLifeWidth,
      packet.CatchableReleaseOwner,
      _facts.CatchableTypes,
      _facts.LifeWidthResolver);
    return stream.ToArray();
  }
}

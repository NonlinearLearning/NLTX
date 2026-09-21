using System;
using System.IO;
using Terraria.Dome.Protocol.V1456.Compatibility;
using Terraria.Dome.Protocol.V1456.Protocol;
using Terraria.Dome.Simulation;

namespace Terraria.Dome.Protocol.V1456.Npc;

public static class NpcSyncPacketCodec
{
  private const byte DirectionRightBit = 1 << 0;
  private const byte DirectionYDownBit = 1 << 1;
  private const byte Ai0PresentBit = 1 << 2;
  private const byte Ai1PresentBit = 1 << 3;
  private const byte Ai2PresentBit = 1 << 4;
  private const byte Ai3PresentBit = 1 << 5;
  private const byte SpriteDirectionRightBit = 1 << 6;
  private const byte FullLifeBit = 1 << 7;
  private const byte PlayersForScalingBit = 1 << 0;
  private const byte SpawnedFromStatueBit = 1 << 1;
  private const byte DifficultyBit = 1 << 2;
  private const byte SpawnNeedsSyncingBit = 1 << 3;
  private const byte ShimmerBit = 1 << 4;

  public static byte[] Encode(NpcSyncPacket packet)
  {
    Validate(packet);
    NpcReplicationSnapshot snapshot = new(
      packet.Identity,
      packet.NpcType,
      packet.Position,
      packet.Velocity,
      packet.Life,
      packet.Life > 0,
      packet.Revision,
      default);
    LegacyNpcWireState state = new(
      snapshot,
      packet.Target,
      packet.DirectionRight,
      packet.DirectionYDown,
      packet.Ai0 ?? 0.0f,
      packet.Ai1 ?? 0.0f,
      packet.Ai2 ?? 0.0f,
      packet.Ai3 ?? 0.0f,
      packet.SpriteDirectionRight,
      packet.LifeMaximum,
      packet.PlayersForScaling,
      packet.SpawnedFromStatue,
      packet.Difficulty,
      packet.SpawnNeedsSyncing,
      packet.ShimmerTransparency,
      packet.ReleaseOwner,
      packet.IsCatchable);
    return TerrariaV1456Compatibility.EncodeNpcReplication(state);
  }

  public static NpcSyncPacket Decode(ReadOnlySpan<byte> frameBytes)
  {
    TerrariaFrame frame = TerrariaFrameCodec.Decode(frameBytes);
    if (frame.MessageId != TerrariaMessageId.SyncNPC)
    {
      throw new InvalidDataException("Terraria frame is not a SyncNPC packet.");
    }

    using MemoryStream stream = new(frame.Payload.ToArray(), writable: false);
    using BinaryReader reader = new(stream);
    short identity = reader.ReadInt16();
    SimulationVector position = new(
      TerrariaWorldCoordinates.FromPixels(reader.ReadSingle()),
      TerrariaWorldCoordinates.FromPixels(reader.ReadSingle()));
    SimulationVector velocity = new(
      TerrariaWorldCoordinates.FromPixels(reader.ReadSingle()),
      TerrariaWorldCoordinates.FromPixels(reader.ReadSingle()));
    ushort target = reader.ReadUInt16();
    byte flags = reader.ReadByte();
    byte flags2 = reader.ReadByte();
    float? ai0 = ReadOptionalFloat(reader, stream, flags, Ai0PresentBit);
    float? ai1 = ReadOptionalFloat(reader, stream, flags, Ai1PresentBit);
    float? ai2 = ReadOptionalFloat(reader, stream, flags, Ai2PresentBit);
    float? ai3 = ReadOptionalFloat(reader, stream, flags, Ai3PresentBit);
    short npcType = reader.ReadInt16();
    byte playersForScaling = (flags2 & PlayersForScalingBit) != 0
      ? reader.ReadByte()
      : (byte)1;
    float difficulty = (flags2 & DifficultyBit) != 0 ? reader.ReadSingle() : 1.0f;
    bool fullLife = (flags & FullLifeBit) != 0;
    int life;
    int lifeMaximum;
    if (fullLife)
    {
      life = 1;
      lifeMaximum = 1;
    }
    else
    {
      byte lifeWidth = reader.ReadByte();
      (life, lifeMaximum) = lifeWidth switch
      {
        1 => (reader.ReadSByte(), sbyte.MaxValue),
        2 => (reader.ReadInt16(), short.MaxValue),
        4 => (reader.ReadInt32(), int.MaxValue),
        _ => throw new InvalidDataException("SyncNPC life width is invalid.")
      };
    }

    byte releaseOwner = 0;
    bool isCatchable = stream.Position < stream.Length;
    if (isCatchable)
    {
      releaseOwner = reader.ReadByte();
    }

    if (stream.Position != stream.Length)
    {
      throw new InvalidDataException("SyncNPC contains trailing payload bytes.");
    }

    return new NpcSyncPacket(
      identity,
      position,
      velocity,
      target,
      (flags & DirectionRightBit) != 0,
      (flags & DirectionYDownBit) != 0,
      ai0,
      ai1,
      ai2,
      ai3,
      (flags & SpriteDirectionRightBit) != 0,
      npcType,
      playersForScaling,
      (flags2 & SpawnedFromStatueBit) != 0,
      difficulty,
      (flags2 & SpawnNeedsSyncingBit) != 0,
      (flags2 & ShimmerBit) != 0 ? 1.0f : 0.0f,
      life,
      lifeMaximum,
      releaseOwner,
      isCatchable,
      Revision: 0);
  }

  private static float? ReadOptionalFloat(
    BinaryReader reader,
    MemoryStream stream,
    byte flags,
    byte bit)
  {
    if ((flags & bit) == 0)
    {
      return null;
    }

    if (stream.Length - stream.Position < sizeof(float))
    {
      throw new InvalidDataException("SyncNPC contains a truncated AI value.");
    }

    return reader.ReadSingle();
  }

  private static void Validate(NpcSyncPacket packet)
  {
    if (packet.Identity <= 0 || packet.NpcType <= 0 || packet.PlayersForScaling == 0)
    {
      throw new ArgumentOutOfRangeException(nameof(packet));
    }

    if (packet.LifeMaximum <= 0 || packet.Life < 0 || packet.Life > packet.LifeMaximum)
    {
      throw new ArgumentOutOfRangeException(nameof(packet));
    }

    if (!float.IsFinite(packet.Difficulty) || packet.Difficulty <= 0.0f ||
        !float.IsFinite(packet.ShimmerTransparency) || packet.ShimmerTransparency < 0.0f)
    {
      throw new ArgumentOutOfRangeException(nameof(packet));
    }

    if (packet.IsCatchable && packet.ReleaseOwner > byte.MaxValue)
    {
      throw new ArgumentOutOfRangeException(nameof(packet));
    }
  }
}

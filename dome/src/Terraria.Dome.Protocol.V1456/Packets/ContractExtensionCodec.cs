using System;
using System.IO;
using Terraria.Dome.Protocol.V1456.Protocol;
using Terraria.Dome.Simulation;
using Terraria.Dome.Simulation.StatusEffects.Snapshots;
using Terraria.Dome.Simulation.WorldObjects.Sign;

namespace Terraria.Dome.Protocol.V1456.Packets;

public enum ContractExtensionKind : byte
{
  ChestTransferRevision = 1,
  SignDeletion = 2,
  VersionNegotiation = 3,
  CapabilityOffer = 4,
  CapabilityAck = 5,
  NpcProjectileCapabilityOffer = 6,
  NpcProjectileCapabilityAck = 7,
  NpcProjectileReplication = 8,
  NpcProjectileReplicationV2 = 9,
  NpcProjectileReplicationV3 = 10,
  NpcStatusEffect = 11,
  NpcStatusEffectCapabilityOffer = 12,
  NpcStatusEffectCapabilityAck = 13
}

public readonly record struct ChestTransferRevisionEnvelope(int ChestId, long ExpectedRevision);

public readonly record struct SignDeletionFrame(
  int SignId,
  long Revision,
  SignTombstoneReason Reason);

public readonly record struct ContractExtensionNegotiation(ushort SupportedVersions);

public static class ContractExtensionCodec
{
  private const ushort ContractExtensionModuleId = 15;
  private const byte CurrentVersion = 1;
  private const int EnvelopeHeaderLength = sizeof(ushort) + sizeof(byte) + sizeof(byte);
  private const int ChestRevisionBodyLength = sizeof(int) + sizeof(long);
  private const int SignDeletionBodyLength = sizeof(int) + sizeof(long) + sizeof(byte);
  private const int CapabilityBodyLength = sizeof(ushort) + sizeof(ushort);
  private const int NpcProjectileCapabilityBodyLength = sizeof(ushort);
  private const int NpcProjectileReplicationBodyLength =
    sizeof(int) * 4 + sizeof(float) * 4 + sizeof(int) * 2 + sizeof(byte) + sizeof(long) * 2 +
    sizeof(int) * 3 + sizeof(float) + sizeof(byte);
  private const int NpcProjectileReplicationV2BodyLength =
    NpcProjectileReplicationBodyLength + sizeof(float) * 3;
  private const int NpcProjectileReplicationV3BodyLength =
    NpcProjectileReplicationV2BodyLength + sizeof(byte) + 16;
  private const int NpcStatusEffectPrefixLength = sizeof(int) + sizeof(long) + sizeof(byte);
  private const int NpcStatusEffectEntryLength = sizeof(ushort) + sizeof(int);

  public static byte[] EncodeNpcStatusEffect(NpcStatusEffectEnvelope envelope)
  {
    envelope.Validate();
    using MemoryStream payload = new();
    using (BinaryWriter writer = new(payload, System.Text.Encoding.UTF8, leaveOpen: true))
    {
      WriteHeader(writer, ContractExtensionKind.NpcStatusEffect);
      writer.Write(envelope.ReplicationId);
      writer.Write(envelope.Revision);
      writer.Write((byte)envelope.Effects.Count);
      for (int index = 0; index < envelope.Effects.Count; index++)
      {
        StatusEffectSnapshot effect = envelope.Effects[index];
        writer.Write(effect.Type);
        writer.Write(effect.RemainingTicks);
      }
    }

    return TerrariaFrameCodec.Encode(
      new TerrariaFrame(TerrariaMessageId.NetModules, payload.ToArray()));
  }

  public static NpcStatusEffectEnvelope DecodeNpcStatusEffect(ReadOnlySpan<byte> frameBytes)
  {
    TerrariaFrame frame = TerrariaFrameCodec.Decode(frameBytes);
    int minimumLength = EnvelopeHeaderLength + NpcStatusEffectPrefixLength;
    if (frame.MessageId != TerrariaMessageId.NetModules || frame.Payload.Length < minimumLength ||
        frame.Payload.Span[0] != 15 || frame.Payload.Span[1] != 0 || frame.Payload.Span[2] != 1 ||
        frame.Payload.Span[3] != (byte)ContractExtensionKind.NpcStatusEffect)
    {
      throw new InvalidDataException("NPC status-effect extension header is invalid.");
    }

    using MemoryStream stream = new(frame.Payload.ToArray());
    using BinaryReader reader = new(stream);
    _ = reader.ReadUInt16();
    _ = reader.ReadByte();
    _ = reader.ReadByte();
    int replicationId = reader.ReadInt32();
    long revision = reader.ReadInt64();
    int count = reader.ReadByte();
    if (count > NpcStatusEffectEnvelope.MaximumEffectCount ||
        stream.Length != minimumLength + count * NpcStatusEffectEntryLength)
    {
      throw new InvalidDataException("NPC status-effect extension length is invalid.");
    }

    StatusEffectSnapshot[] effects = new StatusEffectSnapshot[count];
    for (int index = 0; index < effects.Length; index++)
    {
      effects[index] = new StatusEffectSnapshot(
        StatusEffectTargetKind.Npc,
        replicationId,
        revision,
        reader.ReadUInt16(),
        reader.ReadInt32());
    }

    NpcStatusEffectEnvelope envelope = new(replicationId, revision, effects);
    envelope.Validate();
    return envelope;
  }

  public static bool IsNpcStatusEffect(ReadOnlySpan<byte> frameBytes)
  {
    TerrariaFrame frame = TerrariaFrameCodec.Decode(frameBytes);
    return frame.MessageId == TerrariaMessageId.NetModules &&
      frame.Payload.Length >= EnvelopeHeaderLength && frame.Payload.Span[0] == 15 &&
      frame.Payload.Span[1] == 0 && frame.Payload.Span[2] == 1 &&
      frame.Payload.Span[3] == (byte)ContractExtensionKind.NpcStatusEffect;
  }

  public static byte[] EncodeChestTransferRevision(
    ChestTransferRevisionEnvelope envelope)
  {
    if (envelope.ChestId <= 0 || envelope.ExpectedRevision < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(envelope));
    }

    using MemoryStream payload = new();
    using (BinaryWriter writer = new(payload, System.Text.Encoding.UTF8, leaveOpen: true))
    {
      WriteHeader(writer, ContractExtensionKind.ChestTransferRevision);
      writer.Write(envelope.ChestId);
      writer.Write(envelope.ExpectedRevision);
    }

    return TerrariaFrameCodec.Encode(
      new TerrariaFrame(TerrariaMessageId.NetModules, payload.ToArray()));
  }

  public static ChestTransferRevisionEnvelope DecodeChestTransferRevision(
    ReadOnlySpan<byte> frameBytes)
  {
    using BinaryReader reader = CreateReader(
      frameBytes,
      ContractExtensionKind.ChestTransferRevision,
      ChestRevisionBodyLength);
    int chestId = reader.ReadInt32();
    long expectedRevision = reader.ReadInt64();
    if (chestId <= 0 || expectedRevision < 0)
    {
      throw new InvalidDataException("Contract chest revision envelope contains an invalid value.");
    }

    return new ChestTransferRevisionEnvelope(chestId, expectedRevision);
  }

  public static byte[] EncodeSignDeletion(SignDeletionFrame deletion)
  {
    if (deletion.SignId < 0 || deletion.SignId > short.MaxValue || deletion.Revision < 0 ||
        !Enum.IsDefined(deletion.Reason))
    {
      throw new ArgumentOutOfRangeException(nameof(deletion));
    }

    using MemoryStream payload = new();
    using (BinaryWriter writer = new(payload, System.Text.Encoding.UTF8, leaveOpen: true))
    {
      WriteHeader(writer, ContractExtensionKind.SignDeletion);
      writer.Write(deletion.SignId);
      writer.Write(deletion.Revision);
      writer.Write((byte)deletion.Reason);
    }

    return TerrariaFrameCodec.Encode(
      new TerrariaFrame(TerrariaMessageId.NetModules, payload.ToArray()));
  }

  public static SignDeletionFrame DecodeSignDeletion(ReadOnlySpan<byte> frameBytes)
  {
    using BinaryReader reader = CreateReader(
      frameBytes,
      ContractExtensionKind.SignDeletion,
      SignDeletionBodyLength);
    int signId = reader.ReadInt32();
    long revision = reader.ReadInt64();
    SignTombstoneReason reason = (SignTombstoneReason)reader.ReadByte();
    if (signId < 0 || signId > short.MaxValue || revision < 0 || !Enum.IsDefined(reason))
    {
      throw new InvalidDataException("Contract sign deletion frame contains an invalid value.");
    }

    return new SignDeletionFrame(signId, revision, reason);
  }

  public static byte[] EncodeVersionNegotiation(ContractExtensionNegotiation negotiation)
  {
    if ((negotiation.SupportedVersions & (1 << (CurrentVersion - 1))) == 0)
    {
      throw new ArgumentOutOfRangeException(nameof(negotiation));
    }

    using MemoryStream payload = new();
    using (BinaryWriter writer = new(payload, System.Text.Encoding.UTF8, leaveOpen: true))
    {
      WriteHeader(writer, ContractExtensionKind.VersionNegotiation);
      writer.Write(negotiation.SupportedVersions);
    }

    return TerrariaFrameCodec.Encode(
      new TerrariaFrame(TerrariaMessageId.NetModules, payload.ToArray()));
  }

  public static ContractExtensionNegotiation DecodeVersionNegotiation(
    ReadOnlySpan<byte> frameBytes)
  {
    using BinaryReader reader = CreateReader(
      frameBytes,
      ContractExtensionKind.VersionNegotiation,
      sizeof(ushort));
    ushort supportedVersions = reader.ReadUInt16();
    if ((supportedVersions & (1 << (CurrentVersion - 1))) == 0)
    {
      throw new InvalidDataException(
        "Contract extension negotiation does not support the current version.");
    }

    return new ContractExtensionNegotiation(supportedVersions);
  }

  public static byte[] EncodeCapabilityOffer(ContractCapabilityOffer offer)
  {
    using MemoryStream payload = new();
    using (BinaryWriter writer = new(payload, System.Text.Encoding.UTF8, leaveOpen: true))
    {
      WriteHeader(writer, ContractExtensionKind.CapabilityOffer);
      writer.Write(offer.SignDeletionVersions);
      writer.Write(offer.ChestTransferRevisionVersions);
    }

    return TerrariaFrameCodec.Encode(
      new TerrariaFrame(TerrariaMessageId.NetModules, payload.ToArray()));
  }

  public static ContractCapabilityOffer DecodeCapabilityOffer(ReadOnlySpan<byte> frameBytes)
  {
    using BinaryReader reader = CreateReader(
      frameBytes,
      ContractExtensionKind.CapabilityOffer,
      CapabilityBodyLength);
    return new ContractCapabilityOffer(reader.ReadUInt16(), reader.ReadUInt16());
  }

  public static byte[] EncodeCapabilityAck(ContractCapabilityAck ack)
  {
    using MemoryStream payload = new();
    using (BinaryWriter writer = new(payload, System.Text.Encoding.UTF8, leaveOpen: true))
    {
      WriteHeader(writer, ContractExtensionKind.CapabilityAck);
      writer.Write(ack.SignDeletionVersions);
      writer.Write(ack.ChestTransferRevisionVersions);
    }

    return TerrariaFrameCodec.Encode(
      new TerrariaFrame(TerrariaMessageId.NetModules, payload.ToArray()));
  }

  public static ContractCapabilityAck DecodeCapabilityAck(ReadOnlySpan<byte> frameBytes)
  {
    using BinaryReader reader = CreateReader(
      frameBytes,
      ContractExtensionKind.CapabilityAck,
      CapabilityBodyLength);
    return new ContractCapabilityAck(reader.ReadUInt16(), reader.ReadUInt16());
  }

  public static byte[] EncodeNpcProjectileCapabilityOffer(ushort versions)
  {
    return EncodeNpcProjectileCapabilityEnvelope(
      ContractExtensionKind.NpcProjectileCapabilityOffer,
      versions);
  }

  public static ushort DecodeNpcProjectileCapabilityOffer(ReadOnlySpan<byte> frameBytes)
  {
    return DecodeNpcProjectileCapabilityEnvelope(
      frameBytes,
      ContractExtensionKind.NpcProjectileCapabilityOffer);
  }

  public static byte[] EncodeNpcProjectileCapabilityAck(ushort versions)
  {
    return EncodeNpcProjectileCapabilityEnvelope(
      ContractExtensionKind.NpcProjectileCapabilityAck,
      versions);
  }

  public static ushort DecodeNpcProjectileCapabilityAck(ReadOnlySpan<byte> frameBytes)
  {
    return DecodeNpcProjectileCapabilityEnvelope(
      frameBytes,
      ContractExtensionKind.NpcProjectileCapabilityAck);
  }

  public static byte[] EncodeNpcStatusEffectCapabilityOffer(ushort versions)
  {
    return EncodeNpcProjectileCapabilityEnvelope(
      ContractExtensionKind.NpcStatusEffectCapabilityOffer,
      versions);
  }

  public static ushort DecodeNpcStatusEffectCapabilityOffer(ReadOnlySpan<byte> frameBytes)
  {
    return DecodeNpcProjectileCapabilityEnvelope(
      frameBytes,
      ContractExtensionKind.NpcStatusEffectCapabilityOffer);
  }

  public static byte[] EncodeNpcStatusEffectCapabilityAck(ushort versions)
  {
    return EncodeNpcProjectileCapabilityEnvelope(
      ContractExtensionKind.NpcStatusEffectCapabilityAck,
      versions);
  }

  public static bool IsNpcStatusEffectCapabilityOffer(ReadOnlySpan<byte> frameBytes)
  {
    TerrariaFrame frame = TerrariaFrameCodec.Decode(frameBytes);
    return frame.MessageId == TerrariaMessageId.NetModules &&
      frame.Payload.Length >= EnvelopeHeaderLength && frame.Payload.Span[0] == 15 &&
      frame.Payload.Span[1] == 0 && frame.Payload.Span[2] == 1 &&
      frame.Payload.Span[3] == (byte)ContractExtensionKind.NpcStatusEffectCapabilityOffer;
  }

  public static bool IsNpcProjectileCapabilityOffer(ReadOnlySpan<byte> frameBytes)
  {
    TerrariaFrame frame = TerrariaFrameCodec.Decode(frameBytes);
    return frame.MessageId == TerrariaMessageId.NetModules &&
      frame.Payload.Length >= EnvelopeHeaderLength &&
      frame.Payload.Span[0] == 15 &&
      frame.Payload.Span[1] == 0 &&
      frame.Payload.Span[2] == 1 &&
      frame.Payload.Span[3] == (byte)ContractExtensionKind.NpcProjectileCapabilityOffer;
  }

  public static byte[] EncodeNpcProjectileReplication(
    NpcProjectileReplicationSnapshot snapshot)
  {
    NpcProjectileReplicationEnvelope envelope =
      NpcProjectileReplicationEnvelope.From(snapshot);
    if (envelope.ReplicationId <= 0 || envelope.Owner <= 0 || envelope.Identity <= 0 ||
        envelope.Revision < 0 || envelope.TombstoneRetainedUntilTick < 0 ||
        !Enum.IsDefined(envelope.TombstoneReason) ||
        !float.IsFinite(envelope.PositionX) || !float.IsFinite(envelope.PositionY) ||
        !float.IsFinite(envelope.VelocityX) || !float.IsFinite(envelope.VelocityY) ||
        (envelope.IsActive && envelope.TombstoneReason != ProjectileTombstoneReason.None) ||
        (!envelope.IsActive && envelope.TombstoneReason == ProjectileTombstoneReason.None))
    {
      throw new ArgumentOutOfRangeException(nameof(snapshot));
    }

    using MemoryStream payload = new();
    using (BinaryWriter writer = new(payload, System.Text.Encoding.UTF8, leaveOpen: true))
    {
      WriteHeader(writer, ContractExtensionKind.NpcProjectileReplication);
      writer.Write(envelope.ReplicationId);
      writer.Write(envelope.ProjectileType);
      writer.Write(envelope.Owner);
      writer.Write(envelope.PositionX);
      writer.Write(envelope.PositionY);
      writer.Write(envelope.VelocityX);
      writer.Write(envelope.VelocityY);
      writer.Write(envelope.Damage);
      writer.Write(envelope.RemainingLifetime);
      writer.Write(envelope.IsActive);
      writer.Write(envelope.Revision);
      writer.Write(envelope.SectionX);
      writer.Write(envelope.SectionY);
      writer.Write(envelope.Identity);
      writer.Write((byte)envelope.TombstoneReason);
      writer.Write(envelope.TombstoneRetainedUntilTick);
      writer.Write(envelope.DefinitionKnockback);
      writer.Write(envelope.DefinitionOriginalDamage);
    }

    return TerrariaFrameCodec.Encode(
      new TerrariaFrame(TerrariaMessageId.NetModules, payload.ToArray()));
  }

  public static NpcProjectileReplicationSnapshot DecodeNpcProjectileReplication(
    ReadOnlySpan<byte> frameBytes)
  {
    using BinaryReader reader = CreateReader(
      frameBytes,
      ContractExtensionKind.NpcProjectileReplication,
      NpcProjectileReplicationBodyLength);
    NpcProjectileReplicationEnvelope envelope = new(
      reader.ReadInt32(),
      reader.ReadInt32(),
      reader.ReadInt32(),
      reader.ReadSingle(),
      reader.ReadSingle(),
      reader.ReadSingle(),
      reader.ReadSingle(),
      reader.ReadInt32(),
      reader.ReadInt32(),
      reader.ReadBoolean(),
      reader.ReadInt64(),
      reader.ReadInt32(),
      reader.ReadInt32(),
      reader.ReadInt32(),
      (ProjectileTombstoneReason)reader.ReadByte(),
      reader.ReadInt64(),
      reader.ReadSingle(),
      reader.ReadInt32());
    NpcProjectileReplicationSnapshot snapshot = envelope.ToSnapshot();
    if (snapshot.ReplicationId <= 0 || !snapshot.Owner.IsValid || snapshot.Identity <= 0 ||
        snapshot.Revision < 0 || snapshot.Damage <= 0 || snapshot.RemainingLifetime < 0 ||
        snapshot.TombstoneRetainedUntilTick < 0 || !Enum.IsDefined(snapshot.TombstoneReason) ||
        !float.IsFinite(snapshot.Position.X) || !float.IsFinite(snapshot.Position.Y) ||
        !float.IsFinite(snapshot.Velocity.X) || !float.IsFinite(snapshot.Velocity.Y) ||
        !float.IsFinite(snapshot.DefinitionKnockback) || snapshot.DefinitionOriginalDamage <= 0 ||
        (snapshot.IsActive && snapshot.TombstoneReason != ProjectileTombstoneReason.None) ||
        (!snapshot.IsActive && snapshot.TombstoneReason == ProjectileTombstoneReason.None))
    {
      throw new InvalidDataException("NPC projectile replication envelope is invalid.");
    }

    return snapshot;
  }

  public static byte[] EncodeNpcProjectileReplicationV2(
    NpcProjectileReplicationSnapshot snapshot)
  {
    NpcProjectileReplicationV2Envelope envelope = NpcProjectileReplicationV2Envelope.From(snapshot);
    if (envelope.ReplicationId <= 0 || envelope.Owner <= 0 || envelope.Identity <= 0 ||
        envelope.Revision < 0 || envelope.Damage <= 0 || envelope.RemainingLifetime < 0 ||
        envelope.TombstoneRetainedUntilTick < 0 || !Enum.IsDefined(envelope.TombstoneReason) ||
        !float.IsFinite(envelope.PositionX) || !float.IsFinite(envelope.PositionY) ||
        !float.IsFinite(envelope.VelocityX) || !float.IsFinite(envelope.VelocityY) ||
        !float.IsFinite(envelope.DefinitionKnockback) || envelope.DefinitionOriginalDamage <= 0 ||
        !float.IsFinite(envelope.Ai0) || !float.IsFinite(envelope.Ai1) ||
        !float.IsFinite(envelope.Ai2) ||
        (envelope.IsActive && envelope.TombstoneReason != ProjectileTombstoneReason.None) ||
        (!envelope.IsActive && envelope.TombstoneReason == ProjectileTombstoneReason.None))
    {
      throw new ArgumentOutOfRangeException(nameof(snapshot));
    }

    using MemoryStream payload = new();
    using (BinaryWriter writer = new(payload, System.Text.Encoding.UTF8, leaveOpen: true))
    {
      WriteHeader(writer, ContractExtensionKind.NpcProjectileReplicationV2);
      writer.Write(envelope.ReplicationId);
      writer.Write(envelope.ProjectileType);
      writer.Write(envelope.Owner);
      writer.Write(envelope.PositionX);
      writer.Write(envelope.PositionY);
      writer.Write(envelope.VelocityX);
      writer.Write(envelope.VelocityY);
      writer.Write(envelope.Damage);
      writer.Write(envelope.RemainingLifetime);
      writer.Write(envelope.IsActive);
      writer.Write(envelope.Revision);
      writer.Write(envelope.SectionX);
      writer.Write(envelope.SectionY);
      writer.Write(envelope.Identity);
      writer.Write((byte)envelope.TombstoneReason);
      writer.Write(envelope.TombstoneRetainedUntilTick);
      writer.Write(envelope.DefinitionKnockback);
      writer.Write(envelope.DefinitionOriginalDamage);
      writer.Write(envelope.Ai0);
      writer.Write(envelope.Ai1);
      writer.Write(envelope.Ai2);
    }

    return TerrariaFrameCodec.Encode(
      new TerrariaFrame(TerrariaMessageId.NetModules, payload.ToArray()));
  }

  public static NpcProjectileReplicationSnapshot DecodeNpcProjectileReplicationV2(
    ReadOnlySpan<byte> frameBytes)
  {
    using BinaryReader reader = CreateReader(
      frameBytes,
      ContractExtensionKind.NpcProjectileReplicationV2,
      NpcProjectileReplicationV2BodyLength);
    NpcProjectileReplicationV2Envelope envelope = new(
      reader.ReadInt32(),
      reader.ReadInt32(),
      reader.ReadInt32(),
      reader.ReadSingle(),
      reader.ReadSingle(),
      reader.ReadSingle(),
      reader.ReadSingle(),
      reader.ReadInt32(),
      reader.ReadInt32(),
      reader.ReadBoolean(),
      reader.ReadInt64(),
      reader.ReadInt32(),
      reader.ReadInt32(),
      reader.ReadInt32(),
      (ProjectileTombstoneReason)reader.ReadByte(),
      reader.ReadInt64(),
      reader.ReadSingle(),
      reader.ReadInt32(),
      reader.ReadSingle(),
      reader.ReadSingle(),
      reader.ReadSingle());
    NpcProjectileReplicationSnapshot snapshot = envelope.ToSnapshot();
    if (snapshot.ReplicationId <= 0 || !snapshot.Owner.IsValid || snapshot.Identity <= 0 ||
        snapshot.Revision < 0 || snapshot.Damage <= 0 || snapshot.RemainingLifetime < 0 ||
        snapshot.TombstoneRetainedUntilTick < 0 || !Enum.IsDefined(snapshot.TombstoneReason) ||
        !float.IsFinite(snapshot.Position.X) || !float.IsFinite(snapshot.Position.Y) ||
        !float.IsFinite(snapshot.Velocity.X) || !float.IsFinite(snapshot.Velocity.Y) ||
        !float.IsFinite(snapshot.DefinitionKnockback) || snapshot.DefinitionOriginalDamage <= 0 ||
        !float.IsFinite(snapshot.Ai0) || !float.IsFinite(snapshot.Ai1) ||
        !float.IsFinite(snapshot.Ai2) ||
        (snapshot.IsActive && snapshot.TombstoneReason != ProjectileTombstoneReason.None) ||
        (!snapshot.IsActive && snapshot.TombstoneReason == ProjectileTombstoneReason.None))
    {
      throw new InvalidDataException("NPC projectile V2 replication envelope is invalid.");
    }

    return snapshot;
  }

  public static bool IsNpcProjectileReplicationV2(ReadOnlySpan<byte> frameBytes)
  {
    TerrariaFrame frame = TerrariaFrameCodec.Decode(frameBytes);
    return frame.MessageId == TerrariaMessageId.NetModules &&
      frame.Payload.Length >= EnvelopeHeaderLength &&
      frame.Payload.Span[0] == 15 &&
      frame.Payload.Span[1] == 0 &&
      frame.Payload.Span[2] == 1 &&
      frame.Payload.Span[3] == (byte)ContractExtensionKind.NpcProjectileReplicationV2;
  }

  public static byte[] EncodeNpcProjectileReplicationV3(
    NpcProjectileReplicationSnapshot snapshot)
  {
    NpcProjectileReplicationV3Envelope envelope = NpcProjectileReplicationV3Envelope.From(snapshot);
    if (!envelope.HasProjectileUuid || envelope.ProjectileUuid == Guid.Empty)
    {
      throw new ArgumentOutOfRangeException(nameof(snapshot));
    }

    byte[] v2Frame = EncodeNpcProjectileReplicationV2(snapshot with { ProjectileUuid = null });
    TerrariaFrame v2 = TerrariaFrameCodec.Decode(v2Frame);
    using MemoryStream payload = new();
    using (BinaryWriter writer = new(payload, System.Text.Encoding.UTF8, leaveOpen: true))
    {
      WriteHeader(writer, ContractExtensionKind.NpcProjectileReplicationV3);
      writer.Write(v2.Payload.Span.Slice(EnvelopeHeaderLength));
      writer.Write(envelope.HasProjectileUuid);
      writer.Write(envelope.ProjectileUuid.ToByteArray());
    }

    return TerrariaFrameCodec.Encode(
      new TerrariaFrame(TerrariaMessageId.NetModules, payload.ToArray()));
  }

  public static NpcProjectileReplicationSnapshot DecodeNpcProjectileReplicationV3(
    ReadOnlySpan<byte> frameBytes)
  {
    using BinaryReader reader = CreateReader(
      frameBytes,
      ContractExtensionKind.NpcProjectileReplicationV3,
      NpcProjectileReplicationV3BodyLength);
    byte[] v2Payload = reader.ReadBytes(NpcProjectileReplicationV2BodyLength);
    bool hasProjectileUuid = reader.ReadBoolean();
    Guid projectileUuid = new(reader.ReadBytes(16));
    using MemoryStream v2Stream = new();
    using (BinaryWriter writer = new(v2Stream, System.Text.Encoding.UTF8, leaveOpen: true))
    {
      WriteHeader(writer, ContractExtensionKind.NpcProjectileReplicationV2);
      writer.Write(v2Payload);
    }

    NpcProjectileReplicationSnapshot snapshot = DecodeNpcProjectileReplicationV2(
      TerrariaFrameCodec.Encode(new TerrariaFrame(TerrariaMessageId.NetModules, v2Stream.ToArray())));
    if (!hasProjectileUuid || projectileUuid == Guid.Empty)
    {
      throw new InvalidDataException("NPC projectile V3 Guid is invalid.");
    }

    return snapshot with { ProjectileUuid = projectileUuid };
  }

  public static bool IsNpcProjectileReplicationV3(ReadOnlySpan<byte> frameBytes)
  {
    TerrariaFrame frame = TerrariaFrameCodec.Decode(frameBytes);
    return frame.MessageId == TerrariaMessageId.NetModules &&
      frame.Payload.Length >= EnvelopeHeaderLength &&
      frame.Payload.Span[0] == 15 &&
      frame.Payload.Span[1] == 0 &&
      frame.Payload.Span[2] == 1 &&
      frame.Payload.Span[3] == (byte)ContractExtensionKind.NpcProjectileReplicationV3;
  }

  private static byte[] EncodeNpcProjectileCapabilityEnvelope(
    ContractExtensionKind kind,
    ushort versions)
  {
    if (versions == 0 || !Enum.IsDefined(kind))
    {
      throw new ArgumentOutOfRangeException(nameof(versions));
    }

    using MemoryStream payload = new();
    using (BinaryWriter writer = new(payload, System.Text.Encoding.UTF8, leaveOpen: true))
    {
      WriteHeader(writer, kind);
      writer.Write(versions);
    }

    return TerrariaFrameCodec.Encode(
      new TerrariaFrame(TerrariaMessageId.NetModules, payload.ToArray()));
  }

  private static ushort DecodeNpcProjectileCapabilityEnvelope(
    ReadOnlySpan<byte> frameBytes,
    ContractExtensionKind kind)
  {
    using BinaryReader reader = CreateReader(frameBytes, kind, NpcProjectileCapabilityBodyLength);
    ushort versions = reader.ReadUInt16();
    if (versions == 0)
    {
      throw new InvalidDataException("NPC projectile capability envelope has no supported version.");
    }

    return versions;
  }

  private static void WriteHeader(BinaryWriter writer, ContractExtensionKind kind)
  {
    writer.Write(ContractExtensionModuleId);
    writer.Write(CurrentVersion);
    writer.Write((byte)kind);
  }

  private static BinaryReader CreateReader(
    ReadOnlySpan<byte> frameBytes,
    ContractExtensionKind expectedKind,
    int bodyLength)
  {
    TerrariaFrame frame = TerrariaFrameCodec.Decode(frameBytes);
    int expectedPayloadLength = EnvelopeHeaderLength + bodyLength;
    if (frame.MessageId != TerrariaMessageId.NetModules ||
        frame.Payload.Length != expectedPayloadLength)
    {
      throw new InvalidDataException("Contract extension envelope has an invalid frame shape.");
    }

    MemoryStream stream = new(frame.Payload.ToArray(), writable: false);
    BinaryReader reader = new(stream, System.Text.Encoding.UTF8, leaveOpen: false);
    if (reader.ReadUInt16() != ContractExtensionModuleId || reader.ReadByte() != CurrentVersion)
    {
      reader.Dispose();
      throw new InvalidDataException(
        "Contract extension envelope has an unsupported module version.");
    }

    if (reader.ReadByte() != (byte)expectedKind)
    {
      reader.Dispose();
      throw new InvalidDataException(
        "Contract extension envelope kind does not match the decoder.");
    }

    return reader;
  }
}

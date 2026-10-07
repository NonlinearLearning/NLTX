using System;
using Terraria.Combat;

namespace Terraria.Npc.Network;

public interface INpcReplicationPacketApi
{
  public sealed record Packet23DecodeResult { }

  public sealed record Packet23EncodeRequest { }

  public sealed record Packet23EncodeResult { }

  public sealed record Packet28DecodeResult { }

  public sealed record Packet28EncodeRequest { }

  public sealed record Packet28EncodeResult { }

  public sealed record PacketRecipientRequest { }

  public sealed record PacketRecipientPlan { }

  public sealed record PacketPublishRequest { }

  public sealed record PacketPublishResult { }

  Packet23DecodeResult DecodePacket23(ReadOnlyMemory<byte> payload);

  Packet23EncodeResult EncodePacket23(Packet23EncodeRequest request);

  Packet28DecodeResult DecodePacket28(ReadOnlyMemory<byte> payload);

  Packet28EncodeResult EncodePacket28(Packet28EncodeRequest request);

  PacketRecipientPlan SelectRecipients(PacketRecipientRequest request);

  PacketPublishResult Publish(PacketPublishRequest request);

  PacketPublishResult PublishPacket54BuffSlots(
    int npcLegacySlot,
    ReadOnlyMemory<StatusEffectSlot> buffSlots);
}

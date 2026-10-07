using Terraria.NetWork.Prototype.PacketDesignCompiler.Wire;

namespace Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

public sealed partial class SyncEmoteBubblePacket
{
    public static void Write(
        PacketWireWriter writer,
        int bubbleId,
        byte anchorKind,
        ushort? anchorId,
        ushort? lifetime,
        byte? emote,
        short? metadata)
    {
        bool removed = anchorKind == byte.MaxValue;
        if (removed != (anchorId is null && lifetime is null && emote is null && metadata is null) || (!removed && (!anchorId.HasValue || !lifetime.HasValue || !emote.HasValue)))
        {
            throw new PacketWireFormatException("Packet 91 removal and update fields do not match the anchor kind.");
        }
        if (!removed && (emote == byte.MaxValue) != metadata.HasValue)
        {
            throw new PacketWireFormatException("Packet 91 signed emotes require metadata, and ordinary emotes cannot carry it.");
        }

        writer.WriteInt32(bubbleId);
        writer.WriteByte(anchorKind);
        if (!removed)
        {
            writer.WriteUInt16(anchorId!.Value);
            writer.WriteUInt16(lifetime!.Value);
            writer.WriteByte(emote!.Value);
            if (metadata is short metadataValue)
            {
                writer.WriteInt16(metadataValue);
            }
        }
    }

    public static (
        int BubbleId,
        byte AnchorKind,
        ushort? AnchorId,
        ushort? Lifetime,
        byte? Emote,
        short? Metadata) Read(PacketWireReader reader)
    {
        int bubbleId = reader.ReadInt32();
        byte anchorKind = reader.ReadByte();
        if (anchorKind == byte.MaxValue)
        {
            if (reader.Remaining != 0)
                throw new PacketWireFormatException("Packet 91 removal variant cannot contain an anchor payload.");
            return (BubbleId: bubbleId, AnchorKind: anchorKind, AnchorId: null, Lifetime: null, Emote: null, Metadata: null);
        }

        ushort anchorId = reader.ReadUInt16();
        ushort lifetime = reader.ReadUInt16();
        byte emote = reader.ReadByte();
        short? metadata = emote == byte.MaxValue
            ? reader.ReadInt16()
            : null;
        if (reader.Remaining != 0)
        {
            throw new PacketWireFormatException("Packet 91 has unexpected bytes after its emote metadata.");
        }
        return (BubbleId: bubbleId, AnchorKind: anchorKind, AnchorId: anchorId, Lifetime: lifetime, Emote: emote, Metadata: metadata);
    }
}

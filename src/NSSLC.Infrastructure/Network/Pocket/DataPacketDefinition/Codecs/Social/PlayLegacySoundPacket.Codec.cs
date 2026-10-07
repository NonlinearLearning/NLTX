using Terraria.NetWork.Prototype.PacketDesignCompiler.Wire;

namespace Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

public sealed partial class PlayLegacySoundPacket
{
    public static void Write(
        PacketWireWriter writer,
        PacketVector2 position,
        ushort soundIndex,
        int? style,
        float? volume,
        float? pitchOffset)
    {
        if (style == -1 || volume == -1f || pitchOffset == -1f)
            throw new PacketWireFormatException("Packet 132 uses -1 as the absent-value sentinel; use null for absent fields.");
        WriteVector2(writer, position);
        writer.WriteUInt16(soundIndex);
        byte flags = 0;
        if (style.HasValue)
            flags |= 1;
        if (volume.HasValue)
            flags |= 2;
        if (pitchOffset.HasValue)
            flags |= 4;
        writer.WriteByte(flags);
        if (style is int styleValue)
            writer.WriteInt32(styleValue);
        if (volume is float volumeValue)
            writer.WriteSingle(volumeValue);
        if (pitchOffset is float pitchOffsetValue)
            writer.WriteSingle(pitchOffsetValue);
    }

    public static (
        PacketVector2 Position,
        ushort SoundIndex,
        int? Style,
        float? Volume,
        float? PitchOffset) Read(PacketWireReader reader)
    {
        PacketVector2 position = ReadVector2(reader);
        ushort soundIndex = reader.ReadUInt16();
        byte flags = reader.ReadByte();
        if ((flags & ~7) != 0)
        {
            throw new PacketWireFormatException("Packet 132 contains unsupported sound flags.");
        }
        int? style = (flags & 1) != 0 ? reader.ReadInt32() : null;
        float? volume = (flags & 2) != 0 ? reader.ReadSingle() : null;
        float? pitch = (flags & 4) != 0 ? reader.ReadSingle() : null;
        return (Position: position, SoundIndex: soundIndex, Style: style, Volume: volume, PitchOffset: pitch);
    }

    private static void WriteVector2(PacketWireWriter writer, PacketVector2 value)
    {
        writer.WriteSingle(value.X);
        writer.WriteSingle(value.Y);
    }

    private static PacketVector2 ReadVector2(PacketWireReader reader) => new(reader.ReadSingle(), reader.ReadSingle());
}

using Terraria.NetWork.Prototype.PacketDesignCompiler.Wire;

namespace Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

public delegate void SyncPlayerWrite(
        PacketWireWriter writer,
        byte player,
        byte skinVariant,
        byte voiceVariant,
        float voicePitchOffset,
        byte hair,
        string name,
        byte hairDye,
        ushort hiddenAccessories,
        byte hideMisc,
        PacketRgb hairColor,
        PacketRgb skinColor,
        PacketRgb eyeColor,
        PacketRgb shirtColor,
        PacketRgb underShirtColor,
        PacketRgb pantsColor,
        PacketRgb shoeColor,
        byte difficultyAndAccessoryFlags,
        byte biomeAndCartFlags,
        byte permanentUpgradeFlags);
public sealed partial class SyncPlayerPacket
{
    public static readonly SyncPlayerWrite WriteFunction = Write;
    public static void Write(
        PacketWireWriter writer,
        byte player,
        byte skinVariant,
        byte voiceVariant,
        float voicePitchOffset,
        byte hair,
        string name,
        byte hairDye,
        ushort hiddenAccessories,
        byte hideMisc,
        PacketRgb hairColor,
        PacketRgb skinColor,
        PacketRgb eyeColor,
        PacketRgb shirtColor,
        PacketRgb underShirtColor,
        PacketRgb pantsColor,
        PacketRgb shoeColor,
        byte difficultyAndAccessoryFlags,
        byte biomeAndCartFlags,
        byte permanentUpgradeFlags)
    {
        writer.WriteByte(player);
        writer.WriteByte(skinVariant);
        writer.WriteByte(voiceVariant);
        writer.WriteSingle(voicePitchOffset);
        writer.WriteByte(hair);
        writer.WriteString(name);
        writer.WriteByte(hairDye);
        writer.WriteUInt16(hiddenAccessories);
        writer.WriteByte(hideMisc);
        WriteRgb(writer, hairColor);
        WriteRgb(writer, skinColor);
        WriteRgb(writer, eyeColor);
        WriteRgb(writer, shirtColor);
        WriteRgb(writer, underShirtColor);
        WriteRgb(writer, pantsColor);
        WriteRgb(writer, shoeColor);
        writer.WriteByte(difficultyAndAccessoryFlags);
        writer.WriteByte(biomeAndCartFlags);
        writer.WriteByte(permanentUpgradeFlags);
    }

    public static (
        byte Player,
        byte SkinVariant,
        byte VoiceVariant,
        float VoicePitchOffset,
        byte Hair,
        string Name,
        byte HairDye,
        ushort HiddenAccessories,
        byte HideMisc,
        PacketRgb HairColor,
        PacketRgb SkinColor,
        PacketRgb EyeColor,
        PacketRgb ShirtColor,
        PacketRgb UnderShirtColor,
        PacketRgb PantsColor,
        PacketRgb ShoeColor,
        byte DifficultyAndAccessoryFlags,
        byte BiomeAndCartFlags,
        byte PermanentUpgradeFlags) Read(PacketWireReader reader)
    {
        byte player = reader.ReadByte();
        byte skin = reader.ReadByte();
        byte voice = reader.ReadByte();
        float pitch = reader.ReadSingle();
        byte hair = reader.ReadByte();
        string name = reader.ReadString();
        byte hairDye = reader.ReadByte();
        ushort hiddenAccessories = reader.ReadUInt16();
        byte hideMisc = reader.ReadByte();
        PacketRgb hairColor = ReadRgb(reader);
        PacketRgb skinColor = ReadRgb(reader);
        PacketRgb eyeColor = ReadRgb(reader);
        PacketRgb shirtColor = ReadRgb(reader);
        PacketRgb underShirtColor = ReadRgb(reader);
        PacketRgb pantsColor = ReadRgb(reader);
        PacketRgb shoeColor = ReadRgb(reader);
        byte difficulty = reader.ReadByte();
        byte biomeAndCart = reader.ReadByte();
        byte upgrades = reader.ReadByte();
        return (Player: player, SkinVariant: skin, VoiceVariant: voice, VoicePitchOffset: pitch, Hair: hair, Name: name, HairDye: hairDye, HiddenAccessories: hiddenAccessories, HideMisc: hideMisc, HairColor: hairColor, SkinColor: skinColor, EyeColor: eyeColor, ShirtColor: shirtColor, UnderShirtColor: underShirtColor, PantsColor: pantsColor, ShoeColor: shoeColor, DifficultyAndAccessoryFlags: difficulty, BiomeAndCartFlags: biomeAndCart, PermanentUpgradeFlags: upgrades);
    }

    private static void WriteRgb(PacketWireWriter writer, PacketRgb color)
    {
        writer.WriteByte(color.Red);
        writer.WriteByte(color.Green);
        writer.WriteByte(color.Blue);
    }

    private static PacketRgb ReadRgb(PacketWireReader reader) => new(reader.ReadByte(), reader.ReadByte(), reader.ReadByte());
}

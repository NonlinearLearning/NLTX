using Terraria.NetWork.Prototype.PacketDesignCompiler.Wire;

namespace Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

public sealed partial class PlayerControlsPacket
{
    public static void Write(
        PacketWireWriter writer,
        byte player,
        byte controlFlags,
        byte movementFlags,
        byte playerFeatureFlags,
        byte actionFlags,
        byte selectedItem,
        PacketVector2 position,
        PacketVector2? velocity,
        ushort? mountType,
        PacketVector2? potionOfReturnUsePosition,
        PacketVector2? potionOfReturnHomePosition,
        PacketVector2? cameraTarget)
    {
        RequirePresence(movementFlags, 2, velocity is not null, "velocity");
        RequirePresence(movementFlags, 7, mountType is not null, "mount type");
        bool hasPotionOfReturn = potionOfReturnUsePosition is not null;
        if (hasPotionOfReturn != (potionOfReturnHomePosition is not null))
            throw new PacketWireFormatException("Packet 13 requires both Potion of Return coordinates or neither.");
        RequirePresence(playerFeatureFlags, 6, hasPotionOfReturn, "Potion of Return coordinates");
        RequirePresence(actionFlags, 5, cameraTarget is not null, "camera target");
        writer.WriteByte(player);
        writer.WriteByte(controlFlags);
        writer.WriteByte(movementFlags);
        writer.WriteByte(playerFeatureFlags);
        writer.WriteByte(actionFlags);
        writer.WriteByte(selectedItem);
        WriteVector2(writer, position);
        if (velocity is PacketVector2 velocityValue)
            WriteVector2(writer, velocityValue);
        if (mountType is ushort mountTypeValue)
            writer.WriteUInt16(mountTypeValue);
        if (potionOfReturnUsePosition is PacketVector2 usePosition)
            WriteVector2(writer, usePosition);
        if (potionOfReturnHomePosition is PacketVector2 homePosition)
            WriteVector2(writer, homePosition);
        if (cameraTarget is PacketVector2 cameraTargetValue)
            WriteVector2(writer, cameraTargetValue);
    }

    public static (
        byte Player,
        byte ControlFlags,
        byte MovementFlags,
        byte PlayerFeatureFlags,
        byte ActionFlags,
        byte SelectedItem,
        PacketVector2 Position,
        PacketVector2? Velocity,
        ushort? MountType,
        PacketVector2? PotionOfReturnUsePosition,
        PacketVector2? PotionOfReturnHomePosition,
        PacketVector2? CameraTarget) Read(PacketWireReader reader)
    {
        byte player = reader.ReadByte();
        byte controlFlags = reader.ReadByte();
        byte movementFlags = reader.ReadByte();
        byte featureFlags = reader.ReadByte();
        byte actionFlags = reader.ReadByte();
        byte selectedItem = reader.ReadByte();
        PacketVector2 position = ReadVector2(reader);
        PacketVector2? velocity = Has(movementFlags, 2) ? ReadVector2(reader) : null;
        ushort? mount = Has(movementFlags, 7) ? reader.ReadUInt16() : null;
        PacketVector2? potionUse = null;
        PacketVector2? potionHome = null;
        if (Has(featureFlags, 6))
        {
            potionUse = ReadVector2(reader);
            potionHome = ReadVector2(reader);
        }

        PacketVector2? cameraTarget = Has(actionFlags, 5) ? ReadVector2(reader) : null;
        return (Player: player, ControlFlags: controlFlags, MovementFlags: movementFlags, PlayerFeatureFlags: featureFlags, ActionFlags: actionFlags, SelectedItem: selectedItem, Position: position, Velocity: velocity, MountType: mount, PotionOfReturnUsePosition: potionUse, PotionOfReturnHomePosition: potionHome, CameraTarget: cameraTarget);
    }

    private static void RequirePresence(byte flags, int bit, bool present, string field)
    {
        if (Has(flags, bit) != present)
            throw new PacketWireFormatException($"Packet 13 presence flag for {field} does not match its value.");
    }

    private static bool Has(byte flags, int bit) => (flags & (1 << bit)) != 0;
    private static void WriteVector2(PacketWireWriter writer, PacketVector2 value)
    {
        writer.WriteSingle(value.X);
        writer.WriteSingle(value.Y);
    }

    private static PacketVector2 ReadVector2(PacketWireReader reader) => new(reader.ReadSingle(), reader.ReadSingle());
}

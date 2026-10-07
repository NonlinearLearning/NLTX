using Terraria.NetWork.Prototype.PacketDesignCompiler.Wire;

namespace Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

[PacketSharedFunction]
public static class PacketTileEntityCodecsV4
{
    public static PacketTileEntityCodecs Create() => new(
    [
        new FixedTileEntityCodec((byte)PacketTileEntityType.TrainingDummy, static (writer, data, _) =>
        {
            writer.WriteInt16(Require<short>(data));
        }, static (reader, _) => reader.ReadInt16()),
        new FixedTileEntityCodec((byte)PacketTileEntityType.ItemFrame, static (writer, data, _) => WriteItem(writer, Require<PacketItemStack>(data)),
            static (reader, _) => ReadItem(reader)),
        new FixedTileEntityCodec((byte)PacketTileEntityType.LogicSensor, static (writer, data, networkSend) =>
        {
            if (networkSend) return;
            var sensor = Require<PacketLogicSensorData>(data);
            writer.WriteByte(sensor.LogicCheck);
            writer.WriteBoolean(sensor.On);
        }, static (reader, networkSend) => networkSend ? null : new PacketLogicSensorData(reader.ReadByte(), reader.ReadBoolean())),
        new FixedTileEntityCodec((byte)PacketTileEntityType.DisplayDoll, static (writer, data, _) => WriteDisplayDoll(writer, Require<PacketDisplayDollData>(data)),
            static (reader, _) => ReadDisplayDoll(reader)),
        new FixedTileEntityCodec((byte)PacketTileEntityType.WeaponsRack, static (writer, data, _) => WriteItem(writer, Require<PacketItemStack>(data)),
            static (reader, _) => ReadItem(reader)),
        new FixedTileEntityCodec((byte)PacketTileEntityType.HatRack, static (writer, data, _) => WriteHatRack(writer, Require<PacketHatRackData>(data)),
            static (reader, _) => ReadHatRack(reader)),
        new FixedTileEntityCodec((byte)PacketTileEntityType.FoodPlatter, static (writer, data, _) => WriteItem(writer, Require<PacketItemStack>(data)),
            static (reader, _) => ReadItem(reader)),
        new FixedTileEntityCodec((byte)PacketTileEntityType.TeleportationPylon, static (_, _, _) => { }, static (_, _) => null),
        new FixedTileEntityCodec((byte)PacketTileEntityType.DeadCellsDisplayJar, static (_, _, _) => { }, static (_, _) => null),
        new FixedTileEntityCodec((byte)PacketTileEntityType.KiteAnchor, static (_, _, _) => { }, static (_, _) => null),
        new FixedTileEntityCodec((byte)PacketTileEntityType.CritterAnchor, static (_, _, _) => { }, static (_, _) => null)
    ]);

    private static T Require<T>(object? data) => data is T value
        ? value
        : throw new PacketWireFormatException($"Tile entity requires {typeof(T).Name} data.");

    private static void WriteItem(PacketWireWriter writer, PacketItemStack item)
    {
        writer.WriteInt16(item.Type);
        writer.WriteByte(item.Prefix);
        writer.WriteInt16(item.Stack);
    }

    private static PacketItemStack ReadItem(PacketWireReader reader) =>
        new(reader.ReadInt16(), reader.ReadByte(), reader.ReadInt16());

    private static void WriteDisplayDoll(PacketWireWriter writer, PacketDisplayDollData data)
    {
        RequireLength(data.Equipment, 9, "display-doll equipment");
        RequireLength(data.Dyes, 9, "display-doll dyes");
        byte equipment = Presence(data.Equipment);
        byte dyes = Presence(data.Dyes);
        byte extra = 0;
        if (data.Misc is not null) extra |= 1;
        if (data.Equipment[8] is not null) extra |= 2;
        if (data.Dyes[8] is not null) extra |= 4;
        writer.WriteByte(equipment);
        writer.WriteByte(dyes);
        writer.WriteByte(data.Pose);
        writer.WriteByte(extra);
        WritePresentItems(writer, data.Equipment);
        WritePresentItems(writer, data.Dyes);
        if (data.Misc is not null) WriteItem(writer, data.Misc);
    }

    private static PacketDisplayDollData ReadDisplayDoll(PacketWireReader reader)
    {
        byte equipmentFlags = reader.ReadByte();
        byte dyeFlags = reader.ReadByte();
        byte pose = reader.ReadByte();
        byte extraFlags = reader.ReadByte();
        var equipment = new PacketItemStack?[9];
        var dyes = new PacketItemStack?[9];
        for (int index = 0; index < 8; index++)
            if ((equipmentFlags & (1 << index)) != 0) equipment[index] = ReadItem(reader);
        if ((extraFlags & 2) != 0) equipment[8] = ReadItem(reader);
        for (int index = 0; index < 8; index++)
            if ((dyeFlags & (1 << index)) != 0) dyes[index] = ReadItem(reader);
        if ((extraFlags & 4) != 0) dyes[8] = ReadItem(reader);
        PacketItemStack? misc = (extraFlags & 1) != 0 ? ReadItem(reader) : null;
        return new PacketDisplayDollData(equipment, dyes, misc, pose);
    }

    private static void WriteHatRack(PacketWireWriter writer, PacketHatRackData data)
    {
        RequireLength(data.Items, 2, "hat-rack items");
        RequireLength(data.Dyes, 2, "hat-rack dyes");
        byte flags = (byte)(Presence(data.Items) | (Presence(data.Dyes) << 2));
        writer.WriteByte(flags);
        WritePresentItems(writer, data.Items);
        WritePresentItems(writer, data.Dyes);
    }

    private static PacketHatRackData ReadHatRack(PacketWireReader reader)
    {
        byte flags = reader.ReadByte();
        var items = new PacketItemStack?[2];
        var dyes = new PacketItemStack?[2];
        for (int index = 0; index < 2; index++)
            if ((flags & (1 << index)) != 0) items[index] = ReadItem(reader);
        for (int index = 0; index < 2; index++)
            if ((flags & (1 << (index + 2))) != 0) dyes[index] = ReadItem(reader);
        return new PacketHatRackData(items, dyes);
    }

    private static void WritePresentItems(PacketWireWriter writer, PacketItemStack?[] items)
    {
        foreach (PacketItemStack? item in items)
            if (item is PacketItemStack value) WriteItem(writer, value);
    }

    private static byte Presence(PacketItemStack?[] items)
    {
        byte flags = 0;
        for (int index = 0; index < items.Length; index++)
            if (items[index] is not null) flags |= (byte)(1 << index);
        return flags;
    }

    private static void RequireLength<T>(T[] values, int expected, string name)
    {
        if (values is null || values.Length != expected)
            throw new PacketWireFormatException($"Packet {name} must contain {expected} slots.");
    }

    private sealed class FixedTileEntityCodec(
        byte type,
        Action<PacketWireWriter, object?, bool> write,
        Func<PacketWireReader, bool, object?> read) : IPacketTileEntityWireCodec
    {
        public byte Type { get; } = type;

        public void WriteExtraData(PacketWireWriter writer, PacketTileEntityRecord entity, bool networkSend) =>
            write(writer, entity.ExtraData, networkSend);

        public object? ReadExtraData(PacketWireReader reader, bool networkSend) => read(reader, networkSend);
    }
}

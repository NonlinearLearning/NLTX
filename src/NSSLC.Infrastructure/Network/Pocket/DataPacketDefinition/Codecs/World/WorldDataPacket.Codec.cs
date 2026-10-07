using Terraria.NetWork.Prototype.PacketDesignCompiler.Wire;

namespace Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

public delegate void WorldDataWrite(
        PacketWireWriter writer,
        int time,
        byte timeFlags,
        byte moonPhase,
        short maxTilesX,
        short maxTilesY,
        short spawnTileX,
        short spawnTileY,
        short worldSurface,
        short rockLayer,
        int worldId,
        string worldName,
        byte gameMode,
        byte[] worldGuid,
        ulong worldGeneratorVersion,
        byte moonType,
        byte[] backgroundTypes,
        byte[] specialBackgroundStyles,
        float windSpeedTarget,
        byte cloudCount,
        int[] treePositions,
        byte[] treeStyles,
        int[] caveBackgroundPositions,
        byte[] caveBackgroundStyles,
        byte[] treeTopStyles,
        float maximumRain,
        byte[] worldFlagGroups,
        byte sundialCooldown,
        byte moondialCooldown,
        short[] savedOreTiers,
        sbyte invasionType,
        ulong lobbyId,
        float sandstormSeverity,
        Packet7ExtraSpawnPoint[] extraSpawnPoints);
public sealed partial class WorldDataPacket
{
    public static readonly WorldDataWrite WriteFunction = Write;
    private const int TreeTopStyleCount = 13;
    private const int WorldFlagGroupCount = 11;
    public static void Write(
        PacketWireWriter writer,
        int time,
        byte timeFlags,
        byte moonPhase,
        short maxTilesX,
        short maxTilesY,
        short spawnTileX,
        short spawnTileY,
        short worldSurface,
        short rockLayer,
        int worldId,
        string worldName,
        byte gameMode,
        byte[] worldGuid,
        ulong worldGeneratorVersion,
        byte moonType,
        byte[] backgroundTypes,
        byte[] specialBackgroundStyles,
        float windSpeedTarget,
        byte cloudCount,
        int[] treePositions,
        byte[] treeStyles,
        int[] caveBackgroundPositions,
        byte[] caveBackgroundStyles,
        byte[] treeTopStyles,
        float maximumRain,
        byte[] worldFlagGroups,
        byte sundialCooldown,
        byte moondialCooldown,
        short[] savedOreTiers,
        sbyte invasionType,
        ulong lobbyId,
        float sandstormSeverity,
        Packet7ExtraSpawnPoint[] extraSpawnPoints)
    {
        Validate(worldName, worldGuid, backgroundTypes, specialBackgroundStyles, treePositions, treeStyles, caveBackgroundPositions, caveBackgroundStyles, treeTopStyles, worldFlagGroups, savedOreTiers, extraSpawnPoints);
        writer.WriteInt32(time);
        writer.WriteByte(timeFlags);
        writer.WriteByte(moonPhase);
        writer.WriteInt16(maxTilesX);
        writer.WriteInt16(maxTilesY);
        writer.WriteInt16(spawnTileX);
        writer.WriteInt16(spawnTileY);
        writer.WriteInt16(worldSurface);
        writer.WriteInt16(rockLayer);
        writer.WriteInt32(worldId);
        writer.WriteString(worldName);
        writer.WriteByte(gameMode);
        writer.WriteBytes(worldGuid);
        writer.WriteUInt64(worldGeneratorVersion);
        writer.WriteByte(moonType);
        writer.WriteBytes(backgroundTypes);
        writer.WriteBytes(specialBackgroundStyles);
        writer.WriteSingle(windSpeedTarget);
        writer.WriteByte(cloudCount);
        WriteInt32Array(writer, treePositions);
        writer.WriteBytes(treeStyles);
        WriteInt32Array(writer, caveBackgroundPositions);
        writer.WriteBytes(caveBackgroundStyles);
        writer.WriteBytes(treeTopStyles);
        writer.WriteSingle(maximumRain);
        writer.WriteBytes(worldFlagGroups);
        writer.WriteByte(sundialCooldown);
        writer.WriteByte(moondialCooldown);
        foreach (short tier in savedOreTiers)
            writer.WriteInt16(tier);
        writer.WriteSByte(invasionType);
        writer.WriteUInt64(lobbyId);
        writer.WriteSingle(sandstormSeverity);
        writer.WriteByte((byte)extraSpawnPoints.Length);
        foreach (Packet7ExtraSpawnPoint point in extraSpawnPoints)
        {
            writer.WriteInt16(point.X);
            writer.WriteInt16(point.Y);
        }
    }

    public static (
        int Time,
        byte TimeFlags,
        byte MoonPhase,
        short MaxTilesX,
        short MaxTilesY,
        short SpawnTileX,
        short SpawnTileY,
        short WorldSurface,
        short RockLayer,
        int WorldId,
        string WorldName,
        byte GameMode,
        byte[] WorldGuid,
        ulong WorldGeneratorVersion,
        byte MoonType,
        byte[] BackgroundTypes,
        byte[] SpecialBackgroundStyles,
        float WindSpeedTarget,
        byte CloudCount,
        int[] TreePositions,
        byte[] TreeStyles,
        int[] CaveBackgroundPositions,
        byte[] CaveBackgroundStyles,
        byte[] TreeTopStyles,
        float MaximumRain,
        byte[] WorldFlagGroups,
        byte SundialCooldown,
        byte MoondialCooldown,
        short[] SavedOreTiers,
        sbyte InvasionType,
        ulong LobbyId,
        float SandstormSeverity,
        Packet7ExtraSpawnPoint[] ExtraSpawnPoints) Read(PacketWireReader reader)
    {
        int time = reader.ReadInt32();
        byte timeFlags = reader.ReadByte();
        byte moonPhase = reader.ReadByte();
        short maxTilesX = reader.ReadInt16();
        short maxTilesY = reader.ReadInt16();
        short spawnX = reader.ReadInt16();
        short spawnY = reader.ReadInt16();
        short surface = reader.ReadInt16();
        short rockLayer = reader.ReadInt16();
        int worldId = reader.ReadInt32();
        string worldName = reader.ReadString();
        byte gameMode = reader.ReadByte();
        byte[] worldGuid = reader.ReadBytes(16).ToArray();
        ulong generatorVersion = reader.ReadUInt64();
        byte moonType = reader.ReadByte();
        byte[] backgroundTypes = ReadBytes(reader, 13);
        byte[] specialBackgroundStyles = ReadBytes(reader, 3);
        float windSpeed = reader.ReadSingle();
        byte cloudCount = reader.ReadByte();
        int[] treePositions = ReadInt32Array(reader, 3);
        byte[] treeStyles = ReadBytes(reader, 4);
        int[] cavePositions = ReadInt32Array(reader, 3);
        byte[] caveStyles = ReadBytes(reader, 4);
        byte[] treeTopStyles = ReadBytes(reader, TreeTopStyleCount);
        float maximumRain = reader.ReadSingle();
        byte[] flags = ReadBytes(reader, WorldFlagGroupCount);
        byte sundial = reader.ReadByte();
        byte moondial = reader.ReadByte();
        short[] oreTiers = new short[7];
        for (int index = 0; index < oreTiers.Length; index++)
            oreTiers[index] = reader.ReadInt16();
        sbyte invasionType = reader.ReadSByte();
        ulong lobbyId = reader.ReadUInt64();
        float severity = reader.ReadSingle();
        int spawnCount = reader.ReadByte();
        var spawnPoints = new Packet7ExtraSpawnPoint[spawnCount];
        for (int index = 0; index < spawnPoints.Length; index++)
            spawnPoints[index] = new Packet7ExtraSpawnPoint(reader.ReadInt16(), reader.ReadInt16());
        return (Time: time, TimeFlags: timeFlags, MoonPhase: moonPhase, MaxTilesX: maxTilesX, MaxTilesY: maxTilesY, SpawnTileX: spawnX, SpawnTileY: spawnY, WorldSurface: surface, RockLayer: rockLayer, WorldId: worldId, WorldName: worldName, GameMode: gameMode, WorldGuid: worldGuid, WorldGeneratorVersion: generatorVersion, MoonType: moonType, BackgroundTypes: backgroundTypes, SpecialBackgroundStyles: specialBackgroundStyles, WindSpeedTarget: windSpeed, CloudCount: cloudCount, TreePositions: treePositions, TreeStyles: treeStyles, CaveBackgroundPositions: cavePositions, CaveBackgroundStyles: caveStyles, TreeTopStyles: treeTopStyles, MaximumRain: maximumRain, WorldFlagGroups: flags, SundialCooldown: sundial, MoondialCooldown: moondial, SavedOreTiers: oreTiers, InvasionType: invasionType, LobbyId: lobbyId, SandstormSeverity: severity, ExtraSpawnPoints: spawnPoints);
    }

    private static void Validate(
        string worldName,
        byte[] worldGuid,
        byte[] backgroundTypes,
        byte[] specialBackgroundStyles,
        int[] treePositions,
        byte[] treeStyles,
        int[] caveBackgroundPositions,
        byte[] caveBackgroundStyles,
        byte[] treeTopStyles,
        byte[] worldFlagGroups,
        short[] savedOreTiers,
        Packet7ExtraSpawnPoint[] extraSpawnPoints)
    {
        RequireLength(worldGuid, 16, nameof(worldGuid));
        RequireLength(backgroundTypes, 13, nameof(backgroundTypes));
        RequireLength(specialBackgroundStyles, 3, nameof(specialBackgroundStyles));
        RequireLength(treePositions, 3, nameof(treePositions));
        RequireLength(treeStyles, 4, nameof(treeStyles));
        RequireLength(caveBackgroundPositions, 3, nameof(caveBackgroundPositions));
        RequireLength(caveBackgroundStyles, 4, nameof(caveBackgroundStyles));
        RequireLength(treeTopStyles, TreeTopStyleCount, nameof(treeTopStyles));
        RequireLength(worldFlagGroups, WorldFlagGroupCount, nameof(worldFlagGroups));
        RequireLength(savedOreTiers, 7, nameof(savedOreTiers));
        if (extraSpawnPoints is null || extraSpawnPoints.Length > byte.MaxValue)
            throw new PacketWireFormatException("Packet 7 extra spawn point count must fit in one byte.");
        if (worldName is null)
            throw new PacketWireFormatException("Packet 7 world name is required.");
    }

    private static void RequireLength<T>(T[] values, int expected, string name)
    {
        if (values is null || values.Length != expected)
            throw new PacketWireFormatException($"Packet 7 {name} must contain exactly {expected} values.");
    }

    private static void WriteInt32Array(PacketWireWriter writer, int[] values)
    {
        foreach (int value in values)
            writer.WriteInt32(value);
    }

    private static int[] ReadInt32Array(PacketWireReader reader, int count)
    {
        var values = new int[count];
        for (int index = 0; index < count; index++)
            values[index] = reader.ReadInt32();
        return values;
    }

    private static byte[] ReadBytes(PacketWireReader reader, int count) => reader.ReadBytes(count).ToArray();
}

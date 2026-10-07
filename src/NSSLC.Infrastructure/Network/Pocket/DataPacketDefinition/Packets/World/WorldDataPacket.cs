namespace Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

public readonly record struct Packet7ExtraSpawnPoint(short X, short Y);

public sealed partial class WorldDataPacket
{
    public int Time { get; set; }
    public byte TimeFlags { get; set; }
    public byte MoonPhase { get; set; }
    public short MaxTilesX { get; set; }
    public short MaxTilesY { get; set; }
    public short SpawnTileX { get; set; }
    public short SpawnTileY { get; set; }
    public short WorldSurface { get; set; }
    public short RockLayer { get; set; }
    public int WorldId { get; set; }
    public string WorldName { get; set; } = string.Empty;
    public byte GameMode { get; set; }
    public byte[] WorldGuid { get; set; } = new byte[16];
    public ulong WorldGeneratorVersion { get; set; }
    public byte MoonType { get; set; }
    public byte[] BackgroundTypes { get; set; } = new byte[13];
    public byte[] SpecialBackgroundStyles { get; set; } = new byte[3];
    public float WindSpeedTarget { get; set; }
    public byte CloudCount { get; set; }
    public int[] TreePositions { get; set; } = new int[3];
    public byte[] TreeStyles { get; set; } = new byte[4];
    public int[] CaveBackgroundPositions { get; set; } = new int[3];
    public byte[] CaveBackgroundStyles { get; set; } = new byte[4];
    public byte[] TreeTopStyles { get; set; } = new byte[13];
    public float MaximumRain { get; set; }
    public byte[] WorldFlagGroups { get; set; } = new byte[11];
    public byte SundialCooldown { get; set; }
    public byte MoondialCooldown { get; set; }
    public short[] SavedOreTiers { get; set; } = new short[7];
    public sbyte InvasionType { get; set; }
    public ulong LobbyId { get; set; }
    public float SandstormSeverity { get; set; }
    public Packet7ExtraSpawnPoint[] ExtraSpawnPoints { get; set; } = [];
    public short DungeonX { get; set; }
    public short DungeonY { get; set; }
}

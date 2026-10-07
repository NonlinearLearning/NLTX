namespace Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

public enum Packet82ModuleId : ushort
{
    Liquid = 0,
    Text = 1,
    Ping = 2,
    Ambience = 3,
    Bestiary = 4,
    CreativePowers = 5,
    CreativeUnlocksPlayerReport = 6,
    TeleportPylon = 7,
    Particles = 8,
    CreativePowerPermissions = 9,
    Banners = 10,
    CraftingRequests = 11,
    TagEffectState = 12,
    LeashedEntity = 13,
    UnbreakableWallScan = 14
}

public sealed record Packet82LiquidEntry(int PackedTileCoordinate, byte Amount, byte LiquidType);

public sealed record Packet82LiquidData(IReadOnlyList<Packet82LiquidEntry> Entries);
public sealed record Packet82PingData(PacketVector2 Position);
public sealed record Packet82TextData(
        byte Author,
        NetworkText Text,
        byte Red,
        byte Green,
        byte Blue);

public sealed record Packet82AmbienceData(byte Player, int Seed, byte SkyEntityType);
public sealed record Packet82BestiaryData(byte Action, short NpcNetId, int? KillCount);

public sealed record Packet82CreativePowerData(ushort PowerId, object? Value);
public sealed record Packet82SharedTogglePowerState(bool Enabled);

public sealed record Packet82PerPlayerTogglePowerState(bool[] EnabledPlayers);
public sealed record Packet82SharedSliderPowerState(float Value);

public sealed record Packet82PerPlayerSliderPowerState(byte Player, float Value);
public sealed record Packet82ParticleSettings(
        float PositionX,
        float PositionY,
        float MovementX,
        float MovementY,
        int UniqueInfoPiece,
        byte InvokingPlayer);

public sealed record Packet82ParticlesData(byte ParticleType, Packet82ParticleSettings Settings);
public sealed record Packet82PermissionData(byte Action, ushort PowerId, byte Level);

public sealed record Packet82LeashedData(
        int Action,
        int EntityId,
        int EntityType,
        short? AnchorX,
        short? AnchorY);

public sealed record Packet82BannerData(
        byte Action,
        IReadOnlyList<int>? KillCounts,
        IReadOnlyList<ushort>? ClaimableCounts,
        short? BannerId,
        int? KillCount,
        ushort? ClaimableCount);

public sealed record Packet82EmptyModuleData;
public sealed record Packet82CreativeUnlockData(short ItemId, ushort SacrificeCount);
public sealed record Packet82TeleportPylonData(byte Action, short X, short Y, byte PylonType);

public sealed record Packet82TagEffectData(
        byte Player,
        byte Action,
        short EffectType,
        int[]? NpcTimes,
        int[]? ProcTimes);

public sealed partial class NetModulesPacket
{
    public ushort ModuleId { get; set; }
    public object? Data { get; set; }
}

using Terraria.NetWork.Prototype.PacketDesignCompiler.Wire;

namespace Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

public sealed partial class PlayerSpawnPacket
{
    public static void Write(
        PacketWireWriter writer,
        byte player,
        short spawnX,
        short spawnY,
        int respawnTimer,
        short pveDeaths,
        short pvpDeaths,
        byte team,
        byte spawnContext)
    {
        writer.WriteByte(player);
        writer.WriteInt16(spawnX);
        writer.WriteInt16(spawnY);
        writer.WriteInt32(respawnTimer);
        writer.WriteInt16(pveDeaths);
        writer.WriteInt16(pvpDeaths);
        writer.WriteByte(team);
        writer.WriteByte(spawnContext);
    }

    public static (
        byte Player,
        short SpawnX,
        short SpawnY,
        int RespawnTimer,
        short PveDeaths,
        short PvpDeaths,
        byte Team,
        byte SpawnContext) Read(PacketWireReader reader)
    {
        byte player = reader.ReadByte();
        short spawnX = reader.ReadInt16();
        short spawnY = reader.ReadInt16();
        int timer = reader.ReadInt32();
        short pveDeaths = reader.ReadInt16();
        short pvpDeaths = reader.ReadInt16();
        byte team = reader.ReadByte();
        byte context = reader.ReadByte();
        return (Player: player, SpawnX: spawnX, SpawnY: spawnY, RespawnTimer: timer, PveDeaths: pveDeaths, PvpDeaths: pvpDeaths, Team: team, SpawnContext: context);
    }
}

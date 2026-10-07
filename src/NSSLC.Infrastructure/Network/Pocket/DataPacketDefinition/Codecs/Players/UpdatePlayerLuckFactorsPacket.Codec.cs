using Terraria.NetWork.Prototype.PacketDesignCompiler.Wire;

namespace Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

public sealed partial class UpdatePlayerLuckFactorsPacket
{
    public static void Write(
        PacketWireWriter writer,
        byte player,
        int ladyBugLuckTime,
        float torchLuck,
        byte luckPotion,
        bool hasGardenGnome,
        bool brokenMirrorBadLuck,
        float equipmentLuckBonus,
        float coinLuck,
        byte kiteLuckLevel)
    {
        writer.WriteByte(player);
        writer.WriteInt32(ladyBugLuckTime);
        writer.WriteSingle(torchLuck);
        writer.WriteByte(luckPotion);
        writer.WriteBoolean(hasGardenGnome);
        writer.WriteBoolean(brokenMirrorBadLuck);
        writer.WriteSingle(equipmentLuckBonus);
        writer.WriteSingle(coinLuck);
        writer.WriteByte(kiteLuckLevel);
    }

    public static (
        byte Player,
        int LadyBugLuckTime,
        float TorchLuck,
        byte LuckPotion,
        bool HasGardenGnome,
        bool BrokenMirrorBadLuck,
        float EquipmentLuckBonus,
        float CoinLuck,
        byte KiteLuckLevel) Read(PacketWireReader reader)
    {
        byte player = reader.ReadByte();
        int ladyBugLuckTime = reader.ReadInt32();
        float torchLuck = reader.ReadSingle();
        byte luckPotion = reader.ReadByte();
        bool gardenGnome = reader.ReadBoolean();
        bool brokenMirror = reader.ReadBoolean();
        float equipmentLuck = reader.ReadSingle();
        float coinLuck = reader.ReadSingle();
        byte kiteLuck = reader.ReadByte();
        return (Player: player, LadyBugLuckTime: ladyBugLuckTime, TorchLuck: torchLuck, LuckPotion: luckPotion, HasGardenGnome: gardenGnome, BrokenMirrorBadLuck: brokenMirror, EquipmentLuckBonus: equipmentLuck, CoinLuck: coinLuck, KiteLuckLevel: kiteLuck);
    }
}

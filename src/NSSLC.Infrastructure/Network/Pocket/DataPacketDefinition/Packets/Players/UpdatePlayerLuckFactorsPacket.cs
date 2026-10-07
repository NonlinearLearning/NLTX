namespace Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

public sealed partial class UpdatePlayerLuckFactorsPacket
{
    public byte Player { get; set; }
    public int LadyBugLuckTime { get; set; }
    public float TorchLuck { get; set; }
    public byte LuckPotion { get; set; }
    public bool HasGardenGnome { get; set; }
    public bool BrokenMirrorBadLuck { get; set; }
    public float EquipmentLuckBonus { get; set; }
    public float CoinLuck { get; set; }
    public byte KiteLuckLevel { get; set; }
}

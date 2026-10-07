namespace Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

public sealed record PacketHatRackData(
    PacketItemStack?[] Items,
    PacketItemStack?[] Dyes);

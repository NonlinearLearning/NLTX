namespace Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

public sealed record PacketDisplayDollData(
    PacketItemStack?[] Equipment,
    PacketItemStack?[] Dyes,
    PacketItemStack? Misc,
    byte Pose);

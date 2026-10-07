namespace Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

public sealed record PacketTileEntityRecord(
    byte Type,
    short X,
    short Y,
    int? Id,
    object? ExtraData);

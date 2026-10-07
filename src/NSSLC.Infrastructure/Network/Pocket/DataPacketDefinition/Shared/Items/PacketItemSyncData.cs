namespace Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

public sealed record PacketItemSyncData(
    short ItemIndex,
    float PositionX,
    float PositionY,
    float VelocityX,
    float VelocityY,
    short Stack,
    byte Prefix,
    byte StateFlags,
    short ItemType);

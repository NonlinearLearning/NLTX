namespace Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

public sealed record PlayerDeathReason
{
    public short? SourcePlayerIndex { get; init; }
    public short? SourceNpcIndex { get; init; }
    public short? SourceProjectileIndex { get; init; }
    public byte? SourceOtherIndex { get; init; }
    public short? SourceProjectileType { get; init; }
    public short? SourceItemType { get; init; }
    public byte? SourceItemPrefix { get; init; }
    public string? CustomReason { get; init; }
}

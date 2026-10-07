namespace Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

public struct JoinWorldPacket
{
    public int RequestedTileX { get; set; }

    public int RequestedTileY { get; set; }
}

public partial struct VelocityPacket
{
    public byte PlayerId { get; set; }

    public byte MSK { get; set; }

    public short? VelocityX { get; set; }

    public short? VelocityY { get; set; }
}

public static class VelocityDefinitionHost
{
    public static CompilationInputBundle CreateInput()
    {
        PacketGraph<VelocityPacket> graph =
            new PacketGraph<VelocityPacket>(messageId: 13);

        graph.Field(VelocityPacket.Members.PlayerId);
        graph.Field(VelocityPacket.Members.MSK);
        graph.VariableField(VelocityPacket.Members.VelocityX).BindMSK(VelocityPacket.Members.MSK, bitIndex: 0);
        graph.VariableField(VelocityPacket.Members.VelocityY).BindMSK(VelocityPacket.Members.MSK, bitIndex: 0);

        return new CompilationInputBundle([graph.Build()]);
    }
}

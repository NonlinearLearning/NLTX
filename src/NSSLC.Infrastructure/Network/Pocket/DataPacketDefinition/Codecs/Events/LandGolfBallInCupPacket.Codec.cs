using Terraria.NetWork.Prototype.PacketDesignCompiler.Wire;

namespace Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

public sealed partial class LandGolfBallInCupPacket
{
    public static void Write(
        PacketWireWriter writer,
        byte player,
        ushort ballX,
        ushort ballY,
        ushort cupX,
        ushort cupY)
    {
        writer.WriteByte(player);
        writer.WriteUInt16(ballX);
        writer.WriteUInt16(ballY);
        writer.WriteUInt16(cupX);
        writer.WriteUInt16(cupY);
    }

    public static (
        byte Player,
        ushort BallX,
        ushort BallY,
        ushort CupX,
        ushort CupY) Read(PacketWireReader reader)
    {
        byte player = reader.ReadByte();
        ushort ballX = reader.ReadUInt16();
        ushort ballY = reader.ReadUInt16();
        ushort cupX = reader.ReadUInt16();
        ushort cupY = reader.ReadUInt16();
        return (Player: player, BallX: ballX, BallY: ballY, CupX: cupX, CupY: cupY);
    }
}

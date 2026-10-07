using Terraria.NetWork.Prototype.PacketDesignCompiler.Wire;

namespace Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

public sealed partial class ShimmerActionsPacket
{
    public static void Write(PacketWireWriter writer, byte action, PacketVector2? position, int? coinAmount)
    {
        bool hasPosition = action is 0 or 1;
        bool hasCoinAmount = action == 1;
        if (hasPosition != position.HasValue || hasCoinAmount != coinAmount.HasValue)
            throw new PacketWireFormatException("Packet 146 fields must match the selected action variant.");
        writer.WriteByte(action);
        if (hasPosition)
        {
            writer.WriteSingle(position!.Value.X);
            writer.WriteSingle(position.Value.Y);
        }

        if (hasCoinAmount)
            writer.WriteInt32(coinAmount!.Value);
    }

    public static (byte Action, PacketVector2? Position, int? CoinAmount) Read(PacketWireReader reader)
    {
        byte action = reader.ReadByte();
        PacketVector2? position = null;
        int? coinAmount = null;
        if (action is 0 or 1)
            position = new PacketVector2(reader.ReadSingle(), reader.ReadSingle());
        if (action == 1)
            coinAmount = reader.ReadInt32();
        return (Action: action, Position: position, CoinAmount: coinAmount);
    }
}

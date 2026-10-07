using Terraria.NetWork.Prototype.PacketDesignCompiler.Wire;

namespace Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

public sealed partial class DevCommandsPacket
{
    public static void Write(
        PacketWireWriter writer,
        string command,
        int integerArgument,
        float value,
        float trailingValue)
    {
        writer.WriteString(command);
        writer.WriteInt32(integerArgument);
        writer.WriteSingle(value);
        writer.WriteSingle(trailingValue);
    }

    public static (string Command, int IntegerArgument, float Value, float TrailingValue) Read(PacketWireReader reader)
    {
        string command = reader.ReadString();
        int integerArgument = reader.ReadInt32();
        float value = reader.ReadSingle();
        float trailing = reader.ReadSingle();
        return (Command: command, IntegerArgument: integerArgument, Value: value, TrailingValue: trailing);
    }
}

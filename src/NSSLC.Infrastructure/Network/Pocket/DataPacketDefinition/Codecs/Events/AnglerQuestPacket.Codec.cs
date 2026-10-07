using Terraria.NetWork.Prototype.PacketDesignCompiler.Wire;

namespace Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

public sealed partial class AnglerQuestPacket
{
    public static void Write(PacketWireWriter writer, byte quest, bool alreadyFinished)
    {
        writer.WriteByte(quest);
        writer.WriteBoolean(alreadyFinished);
    }

    public static (byte Quest, bool AlreadyFinished) Read(PacketWireReader reader) => (Quest: reader.ReadByte(), AlreadyFinished: reader.ReadBoolean());
}

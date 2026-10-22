using Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;
using Terraria.NetWork.Prototype.PacketDesignCompiler.Wire;

namespace MinimalImplementation.Modules.Creative;

public sealed record CreativeUnlockReportWireMessage(byte PlayerId, ushort ItemId, ushort Amount);

public sealed class CreativeUnlockReportWireCodec : IPacket82ModuleWireCodec
{
  public ushort ModuleId => (ushort)Packet82ModuleId.CreativeUnlocksPlayerReport;

  public void Write(
    PacketWireWriter writer,
    object? payload,
    int tagEffectNpcSlotCount,
    Func<short, bool> tagEffectUsesProcTimes)
  {
    ArgumentNullException.ThrowIfNull(writer);
    ArgumentNullException.ThrowIfNull(tagEffectUsesProcTimes);
    if (payload is not CreativeUnlockReportWireMessage report)
    {
      throw new PacketWireFormatException("Creative unlock report codec requires its typed payload.");
    }

    writer.WriteByte(report.PlayerId);
    writer.WriteUInt16(report.ItemId);
    writer.WriteUInt16(report.Amount);
  }

  public object Read(
    PacketWireReader reader,
    int tagEffectNpcSlotCount,
    Func<short, bool> tagEffectUsesProcTimes)
  {
    ArgumentNullException.ThrowIfNull(reader);
    ArgumentNullException.ThrowIfNull(tagEffectUsesProcTimes);
    return new CreativeUnlockReportWireMessage(reader.ReadByte(), reader.ReadUInt16(), reader.ReadUInt16());
  }
}

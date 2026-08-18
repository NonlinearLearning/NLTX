using Terraria.Dome.Protocol.V1456.Protocol;

namespace Terraria.Dome.Protocol.V1456.Packets;

public readonly record struct HelloPacket
{
  public HelloPacket()
    : this(TerrariaProtocolVersion.HelloIdentifier)
  {
  }

  public HelloPacket(string protocolIdentifier)
  {
    ProtocolIdentifier = protocolIdentifier;
  }

  public string ProtocolIdentifier { get; }
}

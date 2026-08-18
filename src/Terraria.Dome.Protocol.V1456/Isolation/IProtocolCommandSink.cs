namespace Terraria.Dome.Protocol.V1456.Isolation;

public interface IProtocolCommandSink
{
  ProtocolCommandResult Accept(NetworkInboundEnvelope envelope);
}

namespace Terraria.Communication;

/// <summary>Nonblocking seam from ECS projection to the host delivery pump.</summary>
public interface ICommunicationOutboundPort
{
  bool TrySubmit(OutboundPacketIntent intent);
}

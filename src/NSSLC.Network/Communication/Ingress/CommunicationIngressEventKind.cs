namespace Terraria.Network;

/// <summary>Kind of structural event carried on the bounded ingress queue.</summary>
public enum CommunicationIngressEventKind : byte
{
  Open = 0,
  Packet = 1,
  Close = 2
}

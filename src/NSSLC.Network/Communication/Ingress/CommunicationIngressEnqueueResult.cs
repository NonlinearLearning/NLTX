namespace Terraria.Network;

public enum CommunicationIngressEnqueueResult : byte
{
  Accepted = 0,
  Full = 1,
  Closed = 2,
  Cancelled = 3
}

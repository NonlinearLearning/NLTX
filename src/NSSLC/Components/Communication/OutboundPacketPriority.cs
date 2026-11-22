namespace Terraria.Communication;

/// <summary>Controls delivery order when independent outbound facts share one session.</summary>
public enum OutboundPacketPriority : byte
{
  Normal = 0,
  Realtime = 1
}

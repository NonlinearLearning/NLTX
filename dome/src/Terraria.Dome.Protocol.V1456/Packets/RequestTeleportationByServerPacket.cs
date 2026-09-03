namespace Terraria.Dome.Protocol.V1456.Packets;

public readonly record struct RequestTeleportationByServerPacket(byte Selector)
{
  public bool IsValid => Selector <= 4;
}

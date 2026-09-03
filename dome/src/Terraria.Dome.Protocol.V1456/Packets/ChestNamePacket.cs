using System;

namespace Terraria.Dome.Protocol.V1456.Packets;

public readonly record struct ChestNamePacket
{
  public ChestNamePacket(short chestId, short tileX, short tileY, string name)
  {
    ArgumentNullException.ThrowIfNull(name);
    ChestId = chestId;
    TileX = tileX;
    TileY = tileY;
    Name = name;
  }

  public short ChestId { get; }
  public short TileX { get; }
  public short TileY { get; }
  public string Name { get; }
}

using System;

namespace Terraria.Items;

public sealed class TileEntityBindingComponent
{
  public TileEntityBindingComponent(
    int tileEntityKind,
    TileCoordinates tilePosition,
    Guid persistentTileEntityId = default)
  {
    TileEntityKind = tileEntityKind;
    TilePosition = tilePosition;
    PersistentTileEntityId = persistentTileEntityId;
  }

  public int TileEntityKind;
  public TileCoordinates TilePosition;
  public Guid PersistentTileEntityId;

  public bool IsBound => TileEntityKind > 0 && PersistentTileEntityId != Guid.Empty;
}

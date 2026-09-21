namespace Terraria.NonAuthoritative.WorldStorage.TileEntities;

public sealed class TileEntityUpdateScheduleComponent
{
  public TileEntityUpdateScheduleComponent(bool requiresUpdates)
  {
    RequiresUpdates = requiresUpdates;
  }

  public bool RequiresUpdates { get; }
}

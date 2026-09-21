namespace Terraria.NonAuthoritative.WorldStorage.TileEntities;

public sealed class TileEntityRuntimeIdentityComponent
{
  public TileEntityRuntimeIdentityComponent(int runtimeId)
  {
    if (runtimeId < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(runtimeId));
    }

    RuntimeId = runtimeId;
  }

  public int RuntimeId { get; }
}

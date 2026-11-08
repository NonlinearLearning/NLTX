namespace Terraria.World;

/// <summary>Outcome of one applied <see cref="WorldTileEntityRequest"/>.</summary>
public enum WorldTileEntityResultKind : byte
{
  Applied = 0,
  NotFound = 1,
  NotOwner = 2,
  InvalidText = 3,
  Rejected = 4
}

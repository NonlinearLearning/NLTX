namespace Terraria.Unshared.World;

[Flags]
public enum WorldInteractionPermission : byte {
  None = 0,
  ReadTile = 1 << 0,
  BreakTile = 1 << 1,
  PlaceTile = 1 << 2
}

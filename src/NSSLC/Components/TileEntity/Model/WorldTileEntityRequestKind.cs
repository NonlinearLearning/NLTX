namespace Terraria.World;

/// <summary>Intentions a session may raise against a chest, a sign or a tile entity.</summary>
public enum WorldTileEntityRequestKind : byte
{
  OpenChest = 0,
  CloseChest = 1,
  EditSign = 2
}

namespace Terraria.Player.Interaction;

public readonly record struct PlayerTileTargetCoordinateInput(
  int MouseX,
  int MouseY,
  float ScreenPositionX,
  float ScreenPositionY,
  int ScreenHeight,
  float GravityDirection,
  int MaxTilesX,
  int MaxTilesY);

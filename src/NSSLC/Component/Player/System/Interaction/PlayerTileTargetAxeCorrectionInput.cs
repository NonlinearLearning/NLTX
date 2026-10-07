namespace Terraria.Player.Interaction;

public readonly record struct PlayerTileTargetAxeCorrectionInput(
  int AxePower,
  bool CenterTileActive,
  int CreateWall,
  int HammerPower,
  bool LeftTileActive,
  int LeftTileType,
  int LeftTileFrameY,
  bool RightTileActive,
  int RightTileType,
  int RightTileFrameY);

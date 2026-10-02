namespace Terraria.Player.Environment;

public readonly record struct PlayerSunScorchTileFact(
  bool Exists,
  int WallType,
  bool InvisibleWall,
  bool Solid,
  int Type,
  bool InvisibleBlock);

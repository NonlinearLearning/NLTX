namespace Terraria.Dome.Simulation.WorldModel;

public readonly record struct WorldEnvironmentChange(
  long DueTick,
  WorldEnvironmentChangeKind Kind,
  int X,
  int Y,
  byte Width,
  byte Height,
  byte ChangeType,
  WorldTile Tile);

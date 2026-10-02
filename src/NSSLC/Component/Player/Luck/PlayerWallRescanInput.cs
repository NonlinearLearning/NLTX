using System.Numerics;

namespace Terraria.Player.Luck;

public readonly record struct PlayerWallRescanInput(
  bool DualDungeonsSeed,
  bool Force,
  Vector2 PlayerCenter);

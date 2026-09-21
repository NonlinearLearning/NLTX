using System.Numerics;

namespace Terraria.Player.Presentation;

public readonly record struct PlayerOverheadMessageInput(
  PlayerCompositeArmSnapshot FrontArm,
  PlayerCompositeArmSnapshot BackArm,
  string ChatText,
  IReadOnlyList<string> Snippets,
  Vector2 MessageSize,
  int TimeLeft,
  PlayerAppearanceColor Color);

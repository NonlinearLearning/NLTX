namespace Terraria.Player.Presentation;

public readonly record struct PlayerCompositeArmsSnapshot(
  PlayerCompositeArmSnapshot FrontArm,
  PlayerCompositeArmSnapshot BackArm);

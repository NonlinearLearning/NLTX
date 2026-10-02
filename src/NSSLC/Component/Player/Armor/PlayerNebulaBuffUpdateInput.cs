namespace Terraria.Player.Armor;

public readonly record struct PlayerNebulaBuffUpdateInput(
  PlayerNebulaResourceKind Resource,
  int BaseBuffType,
  int BuffType,
  int BuffTime);

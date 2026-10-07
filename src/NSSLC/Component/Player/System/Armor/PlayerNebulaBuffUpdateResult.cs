namespace Terraria.Player.Armor;

public readonly record struct PlayerNebulaBuffUpdateResult(
  int BuffType,
  int BuffTime,
  int ResourceLevel,
  bool ChangedBuffSlot);

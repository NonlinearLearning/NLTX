namespace Terraria.Player.Progression;

public readonly record struct ConsumePlayerUpgradeCommand(
  PlayerConsumedProgressionUpgrade Upgrade,
  PlayerProgressionCommandToken Token);

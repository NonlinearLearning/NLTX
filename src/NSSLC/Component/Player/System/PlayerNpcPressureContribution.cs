namespace Terraria.Player;

public readonly record struct PlayerNpcPressureContribution(
  bool ShouldContribute,
  float SlotWeight);

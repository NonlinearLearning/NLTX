namespace Terraria.Player;

public readonly record struct PlayerInventoryEffectIntent(
  PlayerInventoryEffectIntentKind Kind,
  PlayerInventoryCommitTarget Target,
  ItemEntityRef Item,
  int Amount,
  bool IsCoin,
  bool LongText,
  bool MakeNewAndShiny);

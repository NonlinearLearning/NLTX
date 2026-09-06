namespace Terraria.Content;

public sealed record ItemEffectDefinition(
  int HealLife,
  int HealMana,
  int LifeRegeneration,
  int Mana,
  int ManaIncrease,
  int? BuffTypeId,
  int BuffDurationTicks,
  bool IsPotion,
  bool IsConsumable,
  bool NoWet);

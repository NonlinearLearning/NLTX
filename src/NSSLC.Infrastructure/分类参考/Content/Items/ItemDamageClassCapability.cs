namespace Terraria.Content.Items;

public sealed record ItemDamageClassCapability(
  bool IsMelee,
  bool IsMagic,
  bool IsRanged,
  bool IsSummon,
  bool IsSentry);

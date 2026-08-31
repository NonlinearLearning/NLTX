using Terraria.Dome.Simulation.Items.Definitions;

namespace Terraria.Dome.Simulation.Items;

public readonly record struct ItemDefinition(
  ushort ItemType,
  int StackLimit,
  int HealthRestore = 0,
  int UseCooldownTicks = 0,
  ItemIdentityDefinition? Identity = null,
  ItemUseDefinition? Use = null,
  ItemCombatDefinition? Combat = null,
  ItemPlacementDefinition? Placement = null,
  ItemRecoveryDefinition? Recovery = null,
  ItemEquipmentDefinition? Equipment = null,
  int Width = 0,
  int Height = 0,
  int Value = 0,
  int Rarity = 0,
  ItemPrefixDefinition? Prefixes = null,
  ItemExtractinatorDefinition? Extractinator = null,
  int Alpha = 0,
  float Scale = 1f,
  ItemToolDefinition? Tools = null,
  ItemGatheringDefinition? Gathering = null,
  ItemSummoningDefinition? Summoning = null,
  ItemAppearanceDefinition? Appearance = null,
  bool IsSentry = false,
  bool Dd2Summon = false)
{
  public bool IsDd2Summon => Dd2Summon;

  public int OriginalDamage => Combat?.Damage ?? 0;

  public int OriginalDefense => Equipment?.Defense ?? 0;

  public int OriginalRarity => Rarity;

  public bool IsNoMelee => Combat?.NoMelee ?? false;

  public bool IsConsumable => Use?.Consumable ?? Recovery?.Consumable ?? HealthRestore > 0;

  public bool IsACoin => Systems.ItemCurrencySystem.IsCoinType(ItemType);

  public bool IsShopItem => Identity?.IsShopItem ?? false;

  public bool IsBuyable => Identity?.Buy ?? false;

  public bool IsBuyOnce => Identity?.BuyOnce ?? false;

  public bool IsExpertOnly => Identity?.ExpertOnly ?? false;

  public bool IsExpert => Identity?.Expert ?? false;

  public bool IsUniqueStack => Identity?.UniqueStack ?? false;

  public bool CanStack => !IsUniqueStack && StackLimit > 1;

  public bool IsMaterial => Identity?.IsMaterial ?? false;

  public bool IsQuestItem => Identity?.IsQuestItem ?? false;

  public bool IsExtractinatorConsumable => Extractinator?.ExtractionMode >= 0;

  public const int CommonMaxStack = 9999;
  public const int LuckPotionDuration1 = 18000;
  public const int LuckPotionDuration2 = 36000;
  public const int LuckPotionDuration3 = 54000;
  public const int FlaskDurationTicks = 72000;
  public const int RestorationDelayTicks = 2700;
  public const int EggnogDelayTicks = 2400;
  public const int MushroomDelayTicks = 1800;
  public const int CoinGrabRange = 350;
  public const int ManaGrabRange = 300;
  public const int LifeGrabRange = 250;
  public const int TreasureGrabRange = 150;
  public const int PickupReplacementTimeTicks = 1200;
  public const int PotionDelayTicks = 3600;
  public const int FoodWidth = 22;
  public const int FoodHeight = 22;
  public const int WallPlacementUseTime = 7;
  public const int SlotsRemainingBeforeEmergencyStackingInMultiplayer = 40;
}

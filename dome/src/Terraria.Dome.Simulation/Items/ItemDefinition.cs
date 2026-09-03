using System;
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
  bool Dd2Summon = false,
  ItemPickupRangeKind PickupRangeKind = ItemPickupRangeKind.Default,
  ItemEconomyDefinition? Economy = null,
  bool NoWet = false)
{
  public ushort Type => ItemType;

  public int ItemWidth => Width;

  public int ItemHeight => Height;

  public int MaxStack => StackLimit;

  public int Damage => Combat?.Damage ?? 0;

  public float Knockback => Combat?.Knockback ?? 0;

  public int Defense => Equipment?.Defense ?? 0;

  public int HealthRestoreAmount => Recovery?.Health ?? HealthRestore;

  public int ManaRestoreAmount => Recovery?.Mana ?? 0;

  public int ManaCost => Use?.ManaCost ?? 0;

  public int BuffType => Recovery?.BuffType ?? 0;

  public int BuffDurationTicks => Recovery?.BuffDurationTicks ?? 0;

  public int PotionDelay => Recovery?.PotionDelayTicks ?? PotionDelayTicks;

  public int FlaskDuration => Recovery?.FlaskDurationTicks ?? FlaskDurationTicks;

  public int RecoveryFoodWidth => Recovery?.FoodWidth ?? FoodWidth;

  public int RecoveryFoodHeight => Recovery?.FoodHeight ?? FoodHeight;

  public byte LuckPotionLevel => Recovery?.LuckPotionLevel ?? 0;

  public int LuckPotionDurationTicks => LuckPotionLevel switch
  {
    1 => LuckPotionDuration1,
    2 => LuckPotionDuration2,
    3 => LuckPotionDuration3,
    _ => 0
  };

  public int UseStyle => Use?.UseStyle ?? 0;

  public int UseTime => Use?.UseTime ?? 0;

  public int UseAnimation => Use?.UseAnimation ?? 0;

  public int ReuseDelay => Use?.ReuseDelayTicks ?? UseCooldownTicks;

  public int EffectiveUseCooldownTicks
  {
    get
    {
      int placementCooldown = Placement?.UseTimeTicks ?? 0;
      int useCooldown = Use?.CooldownTicks ?? 0;
      int reuseDelay = Use?.ReuseDelayTicks ?? 0;
      return Math.Max(placementCooldown, Math.Max(UseCooldownTicks, Math.Max(useCooldown, reuseDelay)));
    }
  }

  public ushort Shoot => Use?.ShootType ?? Combat?.ProjectileType ?? 0;

  public float ShootSpeed => Use?.ShootSpeed ?? Combat?.ProjectileSpeed ?? 0;

  public ushort Ammo => Combat?.AmmoType ?? Use?.AmmoType ?? 0;

  public bool UsesAmmo => Use?.ConsumesAmmo ?? Combat?.ConsumesAmmo ?? false;

  public ushort UseAmmo => Use?.AmmoType ?? 0;

  public bool IsMelee => Combat?.DamageClass == ItemDamageClass.Melee;

  public bool IsMagic => Combat?.DamageClass == ItemDamageClass.Magic;

  public bool IsRanged => Combat?.DamageClass == ItemDamageClass.Ranged;

  public bool IsSummon => Combat?.DamageClass == ItemDamageClass.Summon;

  public int PickupReplacementTime => PickupReplacementTimeTicks;

  public bool Channel => Use?.Channel ?? false;

  public bool AutoReuse => Use?.AutoReuse ?? false;

  public bool UseTurn => Use?.UseTurn ?? false;

  public bool NoUseGraphic => Use?.NoUseGraphic ?? false;

  public bool Potion => Use?.Potion ?? false;

  public bool Consumable => IsConsumable;

  public int CreateTile => Placement?.TileType ?? -1;

  public int CreateWall => Placement?.WallType ?? -1;

  public int PlaceStyle => Placement?.PlaceStyle ?? 0;

  public int TileBoost => Placement?.TileBoost ?? 0;

  public int PlacementUseTime => Placement?.UseTimeTicks ?? 0;

  public bool CartTrack => Placement?.CartTrack ?? false;

  public int PickPower => Tools?.PickPower ?? 0;

  public int AxePower => Tools?.AxePower ?? 0;

  public int HammerPower => Tools?.HammerPower ?? 0;

  public int TileWandPower => Tools?.TileWandPower ?? 0;

  public int FishingPolePower => Gathering?.FishingPolePower ?? 0;

  public int BaitPower => Gathering?.BaitPower ?? 0;

  public int MakeNpc => Summoning?.NpcType ?? 0;

  public int MountType => Summoning?.MountType ?? -1;

  public int LifeRegen => Equipment?.LifeRegen ?? 0;

  public int ManaIncrease => Equipment?.ManaIncrease ?? 0;

  public bool Accessory => Equipment?.Accessory ?? false;

  public ItemEquipmentSlot EquipmentSlot => Equipment?.Slot ?? ItemEquipmentSlot.None;

  public int SentryCapacityBonus => Equipment?.SentryCapacityBonus ?? 0;

  public bool Vanity => Equipment?.Vanity ?? false;

  public bool Social => Equipment?.Social ?? false;

  public bool HasVanityEffects => Equipment?.HasVanityEffects ?? false;

  public int HeadSlot => Equipment?.HeadSlot ?? -1;

  public int HeadType => HeadSlot;

  public int BodySlot => Equipment?.BodySlot ?? -1;

  public int BodyType => BodySlot;

  public int LegSlot => Equipment?.LegSlot ?? -1;

  public int LegType => LegSlot;

  public sbyte HandOnSlot => Equipment?.HandOnSlot ?? -1;

  public sbyte HandOffSlot => Equipment?.HandOffSlot ?? -1;

  public sbyte BackSlot => Equipment?.BackSlot ?? -1;

  public sbyte FrontSlot => Equipment?.FrontSlot ?? -1;

  public sbyte ShoeSlot => Equipment?.ShoeSlot ?? -1;

  public sbyte WaistSlot => Equipment?.WaistSlot ?? -1;

  public sbyte WingSlot => Equipment?.WingSlot ?? -1;

  public sbyte ShieldSlot => Equipment?.ShieldSlot ?? -1;

  public sbyte NeckSlot => Equipment?.NeckSlot ?? -1;

  public sbyte FaceSlot => Equipment?.FaceSlot ?? -1;

  public sbyte BalloonSlot => Equipment?.BalloonSlot ?? -1;

  public sbyte BeardSlot => Equipment?.BeardSlot ?? -1;

  public sbyte VoiceSlot => Equipment?.VoiceSlot ?? 0;

  public bool IsDd2Summon => Dd2Summon;

  public int OriginalDamage => Damage;

  public int OriginalDefense => Defense;

  public int OriginalRarity => Rarity;

  public string NameKey => Identity?.NameKey ?? string.Empty;

  public bool IsShopCurrency => Identity?.IsShopCurrency ?? false;

  public int CriticalChance => Combat?.CriticalChance ?? 0;

  public int ArmorPenetration => Combat?.ArmorPenetration ?? 0;

  public int BonusTagDamage => Combat?.BonusTagDamage ?? 0;

  public bool Flame => Combat?.Flame ?? false;

  public bool Mech => Combat?.Mech ?? false;

  public bool NotAmmo => Combat?.NotAmmo ?? false;

  public bool Staff => Combat?.Staff ?? false;

  public bool Claw => Combat?.Claw ?? false;

  public bool ShootsEveryUse => Use?.ShootsEveryUse ?? false;

  public bool IsChlorophyteExtractinatorConsumable => Extractinator?.ChlorophyteOnly ?? false;

  public bool IsNoMelee => Combat?.NoMelee ?? false;

  public bool IsConsumable => Use?.Consumable ?? Recovery?.Consumable ?? HealthRestore > 0;

  public bool IsACoin => Systems.ItemCurrencySystem.IsCoinType(ItemType);

  public bool IsShopItem => Identity?.IsShopItem ?? false;

  public bool IsBuyable => Identity?.Buy ?? false;

  public bool IsBuyOnce => Identity?.BuyOnce ?? false;

  public int? ShopCustomPrice => Economy?.CustomPriceCopper;

  public int ShopSpecialCurrency => Economy?.SpecialCurrencyId ?? -1;

  public int SellPriceCopper => Systems.ItemPriceSystem.CalculateSellPriceFromItemValue(Value);

  public bool IsExpertOnly => Identity?.ExpertOnly ?? false;

  public bool IsExpert => Identity?.Expert ?? false;

  public bool IsUniqueStack => Identity?.UniqueStack ?? false;

  public bool CanStack => !IsUniqueStack && StackLimit > 1;

  public bool IsMaterial => Identity?.IsMaterial ?? false;

  public bool IsQuestItem => Systems.ItemQuestPolicy.IsQuestItem(this);

  public ItemPickupRangeKind PickupRange => PickupRangeKind;

  public bool IsExtractinatorConsumable => Extractinator?.ExtractionMode >= 0;

  public bool PreventsWetPlacement => NoWet;

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

  public float GetPickupRange(float defaultRange)
  {
    if (PickupRangeKind == ItemPickupRangeKind.Default && IsACoin)
    {
      return CoinGrabRange;
    }

    if (PickupRangeKind == ItemPickupRangeKind.Default && Recovery?.Mana > 0)
    {
      return ManaGrabRange;
    }

    if (PickupRangeKind == ItemPickupRangeKind.Default && Recovery?.Health > 0)
    {
      return LifeGrabRange;
    }

    return PickupRangeKind switch
    {
      ItemPickupRangeKind.Coin => CoinGrabRange,
      ItemPickupRangeKind.Mana => ManaGrabRange,
      ItemPickupRangeKind.Life => LifeGrabRange,
      ItemPickupRangeKind.Treasure => TreasureGrabRange,
      _ => defaultRange
    };
  }
}

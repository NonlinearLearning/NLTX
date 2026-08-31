using System;
using System.Collections.Generic;
using Terraria.Dome.Simulation.Items;
using Terraria.Dome.Simulation.Items.Compatibility;
using Terraria.Dome.Simulation.Items.Definitions;
using Terraria.Dome.Simulation.Items.Systems;

ItemDefinition validHealing = new(
  1,
  99,
  HealthRestore: 25,
  UseCooldownTicks: 10,
  Identity: new ItemIdentityDefinition("Item.HealingPotion", IsMaterial: false),
  Use: new ItemUseDefinition(
    UseTime: 10,
    UseAnimation: 10,
    Consumable: true,
    HealthRestore: 25,
    CooldownTicks: 10));

ItemDefinition projectile = new(
  2,
  999,
  Combat: new ItemCombatDefinition(
    Damage: 10,
    CriticalChance: 12,
    ArmorPenetration: 4,
    DamageClass: ItemDamageClass.Ranged));
ItemDefinition dimensional = new(
  3,
  20,
  Width: 24,
  Height: 18,
  Value: 1250,
  Rarity: 4,
  Alpha: 128,
  Scale: 0.75f);
ItemDefinitionRegistry registry = new([validHealing, projectile, dimensional]);
if (ItemDefinition.CommonMaxStack != 9999 || ItemDefinition.PotionDelayTicks != 3600 ||
    ItemDefinition.CoinGrabRange != 350 || ItemDefinition.ManaGrabRange != 300 ||
    ItemDefinition.LifeGrabRange != 250 || ItemDefinition.TreasureGrabRange != 150 ||
    ItemDefinition.RestorationDelayTicks != 2700 || ItemDefinition.EggnogDelayTicks != 2400 ||
    ItemDefinition.MushroomDelayTicks != 1800 ||
    ItemDefinition.PickupReplacementTimeTicks != 1200 ||
    ItemDefinition.LuckPotionDuration1 != 18000 ||
    ItemDefinition.LuckPotionDuration2 != 36000 || ItemDefinition.LuckPotionDuration3 != 54000 ||
    ItemDefinition.FlaskDurationTicks != 72000)
{
  throw new InvalidOperationException(
    "Shared legacy item definition constants were not preserved as immutable metadata.");
}

if (LegacyFlaskDefinitionRegistry.Definitions.Count != 8 ||
    !LegacyFlaskDefinitionRegistry.TryGet(1340, out LegacyFlaskDefinition venomFlask) ||
    venomFlask.BuffType != 71 ||
    venomFlask.BuffDurationTicks != ItemDefinition.FlaskDurationTicks ||
    !LegacyFlaskDefinitionRegistry.TryGetBuffType(1359, out ushort poisonBuffType) ||
    poisonBuffType != 79 ||
    LegacyFlaskDefinitionRegistry.TryGet(1341, out _))
{
  throw new InvalidOperationException(
    "Legacy Flask definitions did not preserve the source item-to-buff duration mapping.");
}

if (ItemDefinition.FoodWidth != 22 || ItemDefinition.FoodHeight != 22 ||
    ItemDefinition.WallPlacementUseTime != 7 ||
    ItemDefinition.SlotsRemainingBeforeEmergencyStackingInMultiplayer != 40)
{
  throw new InvalidOperationException(
    "Shared item placement, food and emergency-stacking constants were not preserved.");
}

if (registry.Count != 3 || registry.Get(1) != validHealing ||
    registry.Get(3) != dimensional)
{
  throw new InvalidOperationException("Valid immutable definitions were not retained.");
}

if (registry.OrderedDefinitions.Count != 3 ||
    registry.OrderedDefinitions[0] != validHealing ||
    registry.OrderedDefinitions[2] != dimensional)
{
  throw new InvalidOperationException("Item definition registry did not preserve registration order.");
}

try
{
  ((IDictionary<ushort, ItemDefinition>)registry.Definitions)[1] = validHealing;
  throw new InvalidOperationException("Item definition projection was mutable.");
}
catch (NotSupportedException)
{
}

if (registry.Get(3).Width != 24 || registry.Get(3).Height != 18 ||
    registry.Get(3).Value != 1250 || registry.Get(3).Rarity != 4 ||
    registry.Get(3).Alpha != 128 || registry.Get(3).Scale != 0.75f)
{
  throw new InvalidOperationException(
    "The immutable item definition discarded dimensions, value or rarity metadata.");
}

ItemDefinition derived = new(
  4,
  1,
  Rarity: 6,
  Combat: new ItemCombatDefinition(Damage: 31, BonusTagDamage: 4),
  Equipment: new ItemEquipmentDefinition(ItemEquipmentSlot.Head, Defense: 9));
if (derived.OriginalDamage != 31 || derived.Combat?.BonusTagDamage != 4 ||
    derived.OriginalDefense != 9 || derived.OriginalRarity != 6)
{
  throw new InvalidOperationException(
    "Item definition derived original metadata was not exposed consistently.");
}

if (!new ItemDefinition(9, 1, HealthRestore: 1).IsConsumable ||
    !new ItemDefinition(10, 1, Recovery: new ItemRecoveryDefinition(Consumable: true)).IsConsumable ||
    new ItemDefinition(11, 1, Use: new ItemUseDefinition(Consumable: false),
      HealthRestore: 1).IsConsumable)
{
  throw new InvalidOperationException(
    "Item consumable classification did not preserve explicit use and recovery ownership.");
}

ItemDefinition identityWithoutName = LegacyItemDefinitionAdapter.ToDefinition(
  new LegacyItemDefinitionRecord(
    8,
    1,
    ExpertOnly: true,
    Expert: true,
    IsMaterial: true,
    IsQuestItem: true,
    UniqueStack: true,
    IsShopItem: true));
if (identityWithoutName.Identity is not ItemIdentityDefinition identity ||
    !identity.ExpertOnly || !identity.IsMaterial || !identity.IsShopItem)
{
  throw new InvalidOperationException(
    "Legacy identity flags were discarded when the definition had no name key.");
}

if (!identityWithoutName.IsShopItem || !identityWithoutName.IsExpertOnly ||
    !identityWithoutName.IsExpert ||
    new ItemDefinition(12, 1).IsShopItem || new ItemDefinition(13, 1).IsExpertOnly ||
    !new ItemDefinition(14, 1, Identity: new ItemIdentityDefinition(Buy: true)).IsBuyable ||
    new ItemDefinition(15, 1).IsBuyable ||
    !new ItemDefinition(
      16,
      1,
      Identity: new ItemIdentityDefinition(Buy: true, BuyOnce: true)).IsBuyOnce ||
    new ItemDefinition(17, 1, Identity: new ItemIdentityDefinition(Buy: true)).IsBuyOnce)
{
  throw new InvalidOperationException("Item shop qualification did not expose stable Definition queries.");
}

if (!identityWithoutName.IsUniqueStack || !identityWithoutName.IsMaterial ||
    !identityWithoutName.IsQuestItem || new ItemDefinition(14, 1).IsUniqueStack ||
    new ItemDefinition(15, 1).IsMaterial || new ItemDefinition(16, 1).IsQuestItem)
{
  throw new InvalidOperationException("Item unique-stack qualification did not expose a stable query.");
}

if (!new ItemDefinition(17, 1, Extractinator: new ItemExtractinatorDefinition(4))
      .IsExtractinatorConsumable || new ItemDefinition(18, 1).IsExtractinatorConsumable)
{
  throw new InvalidOperationException("Extractinator qualification did not expose a stable query.");
}

ItemDefinition tools = new(
  5,
  1,
  Tools: new ItemToolDefinition(PickPower: 55, AxePower: 12, HammerPower: 80, TileWandPower: 4));
if (tools.Tools?.PickPower != 55 || tools.Tools?.AxePower != 12 ||
    tools.Tools?.HammerPower != 80 || tools.Tools?.TileWandPower != 4)
{
  throw new InvalidOperationException("Item tool metadata was not retained in the immutable definition.");
}

ItemCombatDefinition combat = registry.Get(2).Combat!.Value;
if (combat.CriticalChance != 12 || combat.ArmorPenetration != 4 ||
    combat.DamageClass != ItemDamageClass.Ranged)
{
  throw new InvalidOperationException(
    "The immutable combat definition discarded critical, armor penetration or damage-class metadata.");
}

if (ItemPriceSystem.CalculateBuyPrice(1, 2, 3, 4) != 1020304 ||
    ItemPriceSystem.CalculateSellPrice(1, 2, 3, 4) != 5101520 ||
    ItemPriceSystem.ShadowOrbPrice != 75000 ||
    ItemPriceSystem.DungeonPrice != 87500 ||
    ItemPriceSystem.QueenBeePrice != 100000 ||
    ItemPriceSystem.HellPrice != 125000 ||
    ItemPriceSystem.EclipsePrice != 375000 ||
    ItemPriceSystem.EclipsePostPlanteraPrice != 500000 ||
    ItemPriceSystem.EclipseMothronPrice != 625000)
{
  throw new InvalidOperationException(
    "Item currency conversion did not preserve the authoritative buy/sell price contract.");
}

AssertRejects(
  () => ItemPriceSystem.CalculateBuyPrice(copper: -1),
  "negative item currency values");
AssertRejects(
  () => ItemPriceSystem.CalculateBuyPrice(platinum: int.MaxValue),
  "overflowing buy price");
AssertRejects(
  () => ItemPriceSystem.CalculateSellPrice(platinum: 2000),
  "overflowing sell price");

AssertRejects(
  () => new ItemDefinitionRegistry([new ItemDefinition(1, 10), new ItemDefinition(1, 20)]),
  "duplicate item type");
AssertRejects(
  () => new ItemDefinitionRegistry([new ItemDefinition(0, 10)]),
  "zero item type");
AssertRejects(
  () => new ItemDefinitionRegistry([new ItemDefinition(1, 0)]),
  "invalid stack limit");
AssertRejects(
  () => new ItemDefinitionRegistry([
    new ItemDefinition(1, ItemDefinition.CommonMaxStack + 1)]),
  "stack limit above the common maximum");
AssertRejects(
  () => new ItemDefinitionRegistry([
    new ItemDefinition(1, 10, Use: new ItemUseDefinition(ConsumesAmmo: true))]),
  "ammo without an ammo type");
AssertRejects(
  () => new ItemDefinitionRegistry([
    new ItemDefinition(
      1,
      10,
      Use: new ItemUseDefinition(AmmoType: 2, ConsumesAmmo: true),
      Combat: new ItemCombatDefinition(AmmoType: 3, ConsumesAmmo: true)),
    new ItemDefinition(2, 99),
    new ItemDefinition(3, 99)]),
  "conflicting use and combat ammo contracts");
AssertRejects(
  () => new ItemDefinitionRegistry([
    new ItemDefinition(
      1,
      10,
      Use: new ItemUseDefinition(AmmoType: 99, ConsumesAmmo: true))]),
  "unknown ammunition item type");

if (new ItemDefinitionRegistry([
      new ItemDefinition(1, 10, Use: new ItemUseDefinition(ShootType: 614))]).Get(1)
      .Use?.ShootType != 614)
{
  throw new InvalidOperationException(
    "The item definition compiler treated a projectile type as an item definition reference.");
}
AssertRejects(
  () => new ItemDefinitionRegistry([
    new ItemDefinition(
      1,
      10,
      Equipment: new ItemEquipmentDefinition(Defense: 5))]),
  "equipment attributes without a slot");
AssertRejects(
  () => new ItemDefinitionRegistry([
    new ItemDefinition(
      1,
      10,
      Equipment: new ItemEquipmentDefinition(ItemEquipmentSlot.Head, LifeRegen: -1))]),
  "negative equipment life regeneration");
AssertRejects(
  () => new ItemDefinitionRegistry([
    new ItemDefinition(
      1,
      10,
      Equipment: new ItemEquipmentDefinition(
        ItemEquipmentSlot.Accessory,
        SentryCapacityBonus: -1))]),
  "negative sentry capacity equipment bonus");
AssertRejects(
  () => new ItemDefinitionRegistry([
    new ItemDefinition(
      1,
      10,
      Equipment: new ItemEquipmentDefinition(ItemEquipmentSlot.Head, HandOnSlot: -2))]),
  "invalid equipment visual slot");
AssertRejects(
  () => new ItemDefinitionRegistry([
    new ItemDefinition(1, 10, Identity: new ItemIdentityDefinition(BuyOnce: true))]),
  "buy-once item without buy permission");
AssertRejects(
  () => new ItemDefinitionRegistry([
    new ItemDefinition(
      1,
      10,
      Equipment: new ItemEquipmentDefinition((ItemEquipmentSlot)255))]),
  "unknown equipment slot");
AssertRejects(
  () => new ItemDefinitionRegistry([
    new ItemDefinition(1, 10, Use: new ItemUseDefinition(UseTime: 20, UseAnimation: 10))]),
  "use time after animation");
AssertRejects(
  () => new ItemDefinitionRegistry([
    new ItemDefinition(1, 10, Use: new ItemUseDefinition(UseStyle: -1))]),
  "negative item use style");
AssertRejects(
  () => new ItemDefinitionRegistry([
    new ItemDefinition(1, 10, Use: new ItemUseDefinition(ShootsEveryUse: true))]),
  "shoots-every-use item without a projectile");
AssertRejects(
  () => new ItemDefinitionRegistry([
    new ItemDefinition(1, 10, Use: new ItemUseDefinition(ShootSpeed: 1.0f))]),
  "projectile speed without a projectile");
AssertRejects(
  () => new ItemDefinitionRegistry([
    new ItemDefinition(1, 10, Combat: new ItemCombatDefinition(ProjectileSpeed: 1.0f))]),
  "combat projectile speed without a projectile");
AssertRejects(
  () => new ItemDefinitionRegistry([new ItemDefinition(1, 10, Value: -1)]),
  "negative item value");
AssertRejects(
  () => new ItemDefinitionRegistry([
    new ItemDefinition(1, 10, Placement: new ItemPlacementDefinition(PlaceStyle: 1))]),
  "placement metadata without a tile or wall type");
AssertRejects(
  () => new ItemDefinitionRegistry([
    new ItemDefinition(1, 10, Tools: new ItemToolDefinition(PickPower: -1))]),
  "negative tool power");
AssertRejects(
  () => new ItemDefinitionRegistry([
    new ItemDefinition(1, 10, Tools: new ItemToolDefinition(TileWandPower: -1))]),
  "negative tile wand power");
AssertRejects(
  () => new ItemDefinitionRegistry([
    new ItemDefinition(1, 10, Gathering: new ItemGatheringDefinition(BaitPower: -1))]),
  "negative bait power");
AssertRejects(
  () => new ItemDefinitionRegistry([new ItemDefinition(1, 10, Alpha: -1)]),
  "negative item alpha");
AssertRejects(
  () => new ItemDefinitionRegistry([new ItemDefinition(1, 10, Alpha: 256)]),
  "out-of-range item alpha");
AssertRejects(
  () => new ItemDefinitionRegistry([new ItemDefinition(1, 10, Scale: 0)]),
  "non-positive item scale");
AssertRejects(
  () => new ItemDefinitionRegistry([new ItemDefinition(1, 10, Scale: float.NaN)]),
  "non-finite item scale");
AssertRejects(
  () => new ItemDefinitionRegistry([
    new ItemDefinition(1, 10, Combat: new ItemCombatDefinition(CriticalChance: -1))]),
  "negative combat critical chance");
AssertRejects(
  () => new ItemDefinitionRegistry([
    new ItemDefinition(1, 10, Combat: new ItemCombatDefinition(BonusTagDamage: -1))]),
  "negative combat bonus tag damage");
AssertRejects(
  () => new ItemDefinitionRegistry([
    new ItemDefinition(1, 10, Combat: new ItemCombatDefinition(ArmorPenetration: -1))]),
  "negative combat armor penetration");
AssertRejects(
  () => new ItemDefinitionRegistry([
    new ItemDefinition(
      1,
      10,
      Combat: new ItemCombatDefinition(DamageClass: (ItemDamageClass)255))]),
  "unknown combat damage class");
AssertRejects(
  () => new ItemDefinitionRegistry([
    new ItemDefinition(
      1,
      10,
      Placement: new ItemPlacementDefinition(WallType: -2))]),
  "negative wall placement type");
AssertRejects(
  () => new ItemDefinitionRegistry([
    new ItemDefinition(
      1,
      10,
      Placement: new ItemPlacementDefinition(TileType: 2, TileBoost: -1))]),
  "negative tile placement boost");
AssertRejects(
  () => new ItemDefinitionRegistry([
    new ItemDefinition(
      1,
      10,
      Placement: new ItemPlacementDefinition(TileType: 2, WallType: 3))]),
  "ambiguous tile and wall placement types");
AssertRejects(
  () => new ItemDefinitionRegistry([
    new ItemDefinition(
      1,
      10,
      Recovery: new ItemRecoveryDefinition(BuffType: 5))]),
  "buff recovery without a duration");
AssertRejects(
  () => new ItemDefinitionRegistry([
    new ItemDefinition(
      1,
      10,
      Recovery: new ItemRecoveryDefinition(BuffDurationTicks: 30))]),
  "buff recovery without a buff type");

ExtractinatorRuleRegistry extractinatorRules = ExtractinatorRuleRegistry.CreateVersion4();
foreach ((ushort itemType, int mode) in new (ushort ItemType, int Mode)[]
{
  (424, 0), (1103, 0), (3347, 1), (2339, 2), (2338, 2), (2337, 2),
  (4354, 3), (4389, 3), (4377, 3), (4378, 3), (5127, 3), (5128, 3),
  (5395, 4), (1124, 5), (4090, 6), (173, 6)
})
{
  if (!extractinatorRules.TryGetMode(itemType, out int actualMode) || actualMode != mode)
  {
    throw new InvalidOperationException("The Version4 Extractinator input-mode mapping was not retained.");
  }
}

if (extractinatorRules.TryGetMode(1, out _))
{
  throw new InvalidOperationException("An unmapped item type was accepted as Extractinator input.");
}

foreach ((ushort source, ushort output) in new (ushort Source, ushort Output)[]
{
  (12, 699), (699, 12), (11, 700), (700, 11), (14, 701), (701, 14), (13, 702), (702, 13),
  (56, 880), (880, 56), (364, 1104), (1104, 364), (365, 1105), (1105, 365), (366, 1106),
  (1106, 366), (134, 137), (137, 139), (139, 134), (20, 703), (703, 20), (22, 704),
  (704, 22), (21, 705), (705, 21), (19, 706), (706, 19), (57, 1257), (1257, 57),
  (381, 1184), (1184, 381), (382, 1191), (1191, 382), (391, 1198), (1198, 391),
  (86, 1329), (1329, 86), (61, 3), (836, 3), (409, 3), (370, 169), (1246, 169),
  (408, 169), (833, 664), (835, 664), (834, 664), (3276, 3271), (3277, 3271),
  (3339, 3271), (3274, 3272), (3275, 3272), (3338, 3272)
})
{
  if (!extractinatorRules.TryGetChlorophyteTrade(source, out ushort actualOutput) ||
      actualOutput != output)
  {
    throw new InvalidOperationException("The Version4 Chlorophyte trade table was not retained.");
  }
}

_ = extractinatorRules.TryGetMode(424, out int siltMode);

ExtractinatorSystem extractinator = new();
ExtractinatorResult chlorophyteTrade = extractinator.Roll(
  extractinatorRules,
  extractionMode: -1,
  extractinatorTileType: ExtractinatorSystem.ChlorophyteExtractinatorTileType,
  sourceItemType: 12,
  isHardMode: false,
  new ExtractinatorRandom(123));
if (!chlorophyteTrade.IsAccepted || chlorophyteTrade.Output != new ItemStack(699, 1) ||
    !chlorophyteTrade.IsTrade)
{
  throw new InvalidOperationException("The Chlorophyte Extractinator ore trade was not retained.");
}

ExtractinatorResult firstRoll = extractinator.Roll(
  extractinatorRules,
  siltMode,
  ExtractinatorSystem.ExtractinatorTileType,
  sourceItemType: 424,
  isHardMode: false,
  new ExtractinatorRandom(456));
ExtractinatorResult secondRoll = extractinator.Roll(
  extractinatorRules,
  siltMode,
  ExtractinatorSystem.ExtractinatorTileType,
  sourceItemType: 424,
  isHardMode: false,
  new ExtractinatorRandom(456));
if (!firstRoll.IsAccepted || firstRoll != secondRoll || firstRoll.Output.IsEmpty)
{
  throw new InvalidOperationException("Extractinator output was not deterministic for an identical seed.");
}

AssertRejects(
  () => new ItemDefinitionRegistry([
    new ItemDefinition(1, 10, Extractinator: new ItemExtractinatorDefinition(7))]),
  "unsupported Extractinator mode");

if (MeleeItemScalePolicy.Apply(1.0f, isMelee: true, hasMeleeScaleGlove: true) != 1.1f ||
    MeleeItemScalePolicy.Apply(1.5f, isMelee: false, hasMeleeScaleGlove: true) != 1.5f ||
    MeleeItemScalePolicy.Apply(1.5f, isMelee: true, hasMeleeScaleGlove: false) != 1.5f)
{
  throw new InvalidOperationException("Melee item scale policy did not preserve source ordering.");
}

AssertRejects(
  () => MeleeItemScalePolicy.Apply(float.NaN, isMelee: true, hasMeleeScaleGlove: true),
  "non-finite melee scale");

Console.WriteLine("PASS: immutable item definition registry accepts valid data and rejects invalid references");

static void AssertRejects(Action action, string scenario)
{
  try
  {
    action();
  }
  catch (ArgumentException)
  {
    return;
  }

  throw new InvalidOperationException($"Definition registry accepted {scenario}.");
}

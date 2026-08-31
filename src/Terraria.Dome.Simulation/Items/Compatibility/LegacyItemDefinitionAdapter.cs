using System;
using Terraria.Dome.Simulation.Items.Definitions;
using Terraria.Dome.Simulation.Player.Definitions;

namespace Terraria.Dome.Simulation.Items.Compatibility;

public readonly record struct LegacyItemDefinitionRecord(
  int ItemType,
  int MaxStack,
  int HealthRestore = 0,
  int UseCooldownTicks = 0,
  string NameKey = "",
  bool IsMaterial = false,
  bool IsQuestItem = false,
  bool UniqueStack = false,
  bool ExpertOnly = false,
  bool IsShopCurrency = false,
  bool Buy = false,
  bool BuyOnce = false,
  short TileType = -1,
  int WallType = -1,
  int PlaceStyle = 0,
  int TileBoost = 0,
  ItemEquipmentDefinition? Equipment = null,
  int Alpha = 0,
  float Scale = 1f,
  int Width = 0,
  int Height = 0,
  int Value = 0,
  int Rarity = 0,
  int UseStyle = 0,
  int UseTime = 0,
  int UseAnimation = 0,
  bool Consumable = false,
  bool Channel = false,
  bool AutoReuse = false,
  bool UseTurn = false,
  bool NoUseGraphic = false,
  int ManaRestore = 0,
  int ManaCost = 0,
  int BuffType = 0,
  int BuffDurationTicks = 0,
  int AmmoType = 0,
  bool NotAmmo = false,
  bool ConsumesAmmo = false,
  bool Staff = false,
  bool Claw = false,
  int ShootType = 0,
  float ShootSpeed = 0,
  ItemEquipmentSlot EquipmentSlot = ItemEquipmentSlot.None,
  int Defense = 0,
  int LifeRegen = 0,
  bool Accessory = false,
  bool Vanity = false,
  bool Social = false,
  int PickPower = 0,
  int AxePower = 0,
  int HammerPower = 0,
  int TileWandPower = 0,
  int Damage = 0,
  float Knockback = 0,
  int CriticalChance = 0,
  int ArmorPenetration = 0,
  int BonusTagDamage = 0,
  ItemDamageClass DamageClass = ItemDamageClass.None,
  int ProjectileType = 0,
  float ProjectileSpeed = 0,
  int FishingPolePower = 0,
  int BaitPower = 0,
  int MakeNpc = 0,
  int MountType = -1,
  bool Potion = false,
  bool Expert = false,
  int ManaIncrease = 0,
  int HeadSlot = -1,
  int BodySlot = -1,
  int LegSlot = -1,
  bool HasVanityEffects = false,
  int HairDye = -1,
  byte Paint = 0,
  byte PaintCoating = 0,
  int ReuseDelayTicks = 0,
  bool ShootsEveryUse = false,
  bool Flame = false,
  bool Mech = false,
  bool Melee = false,
  bool Magic = false,
  bool Ranged = false,
  bool Summon = false,
  sbyte HandOnSlot = -1,
  sbyte HandOffSlot = -1,
  sbyte BackSlot = -1,
  sbyte FrontSlot = -1,
  sbyte ShoeSlot = -1,
  sbyte WaistSlot = -1,
  sbyte WingSlot = -1,
  sbyte ShieldSlot = -1,
  sbyte NeckSlot = -1,
  sbyte FaceSlot = -1,
  sbyte BalloonSlot = -1,
  sbyte BeardSlot = -1,
  sbyte VoiceSlot = 0,
  bool IsShopItem = false,
  bool ChlorophyteExtractinatorConsumable = false,
  bool Sentry = false,
  int SentryCapacityBonus = 0,
  bool NoMelee = false,
  bool Dd2Summon = false,
  bool CartTrack = false);

public static class LegacyItemDefinitionAdapter
{
  public static ItemDefinition ToDefinition(LegacyItemDefinitionRecord record)
  {
    ItemDamageClass damageClass = ResolveDamageClass(record);
    if (record.ItemType <= 0 || record.ItemType > ushort.MaxValue || record.MaxStack <= 0 ||
        record.HealthRestore < 0 || record.UseCooldownTicks < 0 || record.TileType < -1 ||
        record.Alpha < 0 || record.Alpha > 255 || !float.IsFinite(record.Scale) || record.Scale <= 0 ||
        record.Width < 0 || record.Height < 0 || record.Value < 0 || record.Rarity < 0 ||
        record.UseStyle < 0 || record.UseTime < 0 || record.UseAnimation < 0 ||
        record.ReuseDelayTicks < 0 ||
        record.ManaRestore < 0 || record.ManaCost < 0 || record.BuffType < 0 ||
        record.BuffType > ushort.MaxValue || record.BuffDurationTicks < 0 || record.Damage < 0 ||
        record.AmmoType < 0 || record.AmmoType > ushort.MaxValue ||
        (record.ConsumesAmmo && record.AmmoType == 0) ||
        record.ShootType < 0 || record.ShootType > ushort.MaxValue ||
        !float.IsFinite(record.ShootSpeed) || record.ShootSpeed < 0 ||
        (record.ShootSpeed > 0 && record.ShootType == 0) ||
        !Enum.IsDefined(record.EquipmentSlot) || record.Defense < 0 || record.LifeRegen < 0 ||
        record.PickPower < 0 || record.AxePower < 0 || record.HammerPower < 0 ||
        record.TileWandPower < 0 ||
        record.FishingPolePower < 0 || record.BaitPower < 0 ||
        record.MakeNpc < 0 || record.MakeNpc > ushort.MaxValue ||
        !MountTypeRegistry.IsValid(record.MountType) ||
        record.ManaIncrease < 0 || record.HeadSlot < -1 || record.BodySlot < -1 ||
        record.LegSlot < -1 || record.HairDye < -1 ||
        record.HandOnSlot < -1 || record.HandOffSlot < -1 || record.BackSlot < -1 ||
        record.FrontSlot < -1 || record.ShoeSlot < -1 || record.WaistSlot < -1 ||
        record.WingSlot < -1 || record.ShieldSlot < -1 || record.NeckSlot < -1 ||
        record.FaceSlot < -1 || record.BalloonSlot < -1 || record.BeardSlot < -1 ||
        record.VoiceSlot < -1 ||
        record.SentryCapacityBonus < 0 ||
        record.CriticalChance < 0 || record.ArmorPenetration < 0 || record.BonusTagDamage < 0 ||
        (record.ShootsEveryUse && record.ShootType == 0) ||
        (record.BuyOnce && !record.Buy) ||
        record.ProjectileType < 0 ||
        record.ProjectileType > ushort.MaxValue || !float.IsFinite(record.Knockback) ||
        record.Knockback < 0 || !float.IsFinite(record.ProjectileSpeed) ||
        record.ProjectileSpeed < 0 ||
        record.WallType < -1 || record.WallType > short.MaxValue || record.PlaceStyle < 0 ||
        record.TileBoost < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(record));
    }

    if (LegacyFlaskDefinitionRegistry.TryGet(
          (ushort)record.ItemType,
          out LegacyFlaskDefinition flaskDefinition))
    {
      if ((record.BuffType != 0 && record.BuffType != flaskDefinition.BuffType) ||
          (record.BuffDurationTicks != 0 &&
           record.BuffDurationTicks != flaskDefinition.BuffDurationTicks))
      {
        throw new ArgumentException(
          "Legacy Flask items must retain their source buff and duration.",
          nameof(record));
      }

      record = record with
      {
        BuffType = flaskDefinition.BuffType,
        BuffDurationTicks = flaskDefinition.BuffDurationTicks,
        Consumable = true
      };
    }

    ItemEquipmentDefinition? mappedSentryEquipment = null;
    bool hasSourceSentryEquipment = LegacySentryEquipmentRegistry.TryGet(
      (ushort)record.ItemType,
      out ItemEquipmentDefinition sourceSentryEquipment);
    if (hasSourceSentryEquipment)
    {
      if (record.EquipmentSlot != ItemEquipmentSlot.None &&
          record.EquipmentSlot != sourceSentryEquipment.Slot)
      {
        throw new ArgumentException(
          "Legacy sentry equipment must retain its source equipment slot.",
          nameof(record));
      }

      if (record.SentryCapacityBonus != 0 &&
          record.SentryCapacityBonus != sourceSentryEquipment.SentryCapacityBonus)
      {
        throw new ArgumentException(
          "Legacy sentry equipment must retain its source capacity bonus.",
          nameof(record));
      }

      if (record.Equipment is ItemEquipmentDefinition explicitEquipment)
      {
        if (explicitEquipment.Slot != ItemEquipmentSlot.None &&
            explicitEquipment.Slot != sourceSentryEquipment.Slot)
        {
          throw new ArgumentException(
            "Legacy sentry equipment must retain its source equipment slot.",
            nameof(record));
        }

        if (explicitEquipment.SentryCapacityBonus != 0 &&
            explicitEquipment.SentryCapacityBonus != sourceSentryEquipment.SentryCapacityBonus)
        {
          throw new ArgumentException(
            "Legacy sentry equipment must retain its source capacity bonus.",
            nameof(record));
        }

        mappedSentryEquipment = explicitEquipment with
        {
          Slot = sourceSentryEquipment.Slot,
          SentryCapacityBonus = sourceSentryEquipment.SentryCapacityBonus
        };
      }
    }

    ItemEquipmentSlot equipmentSlot = hasSourceSentryEquipment
      ? sourceSentryEquipment.Slot
      : record.EquipmentSlot;
    int sentryCapacityBonus = hasSourceSentryEquipment
      ? sourceSentryEquipment.SentryCapacityBonus
      : record.SentryCapacityBonus;

    ItemUseDefinition? use = record.UseCooldownTicks > 0 || record.UseStyle > 0 ||
      record.UseTime > 0 || record.UseAnimation > 0 || record.Consumable || record.Channel ||
      record.AutoReuse || record.UseTurn || record.NoUseGraphic || record.ShootsEveryUse ||
      record.HealthRestore > 0 ||
      record.Potion || record.ReuseDelayTicks > 0 ||
      record.ManaRestore > 0 ||
      record.ManaCost > 0 || record.ConsumesAmmo || record.ShootType > 0 || record.ShootSpeed > 0
      ? new ItemUseDefinition(
        UseStyle: record.UseStyle,
        ReuseDelayTicks: record.ReuseDelayTicks,
        UseTime: record.UseTime,
        UseAnimation: record.UseAnimation,
        CooldownTicks: record.UseCooldownTicks,
        HealthRestore: record.HealthRestore,
        ManaRestore: record.ManaRestore,
        ManaCost: record.ManaCost,
        AmmoType: (ushort)record.AmmoType,
        ConsumesAmmo: record.ConsumesAmmo,
        ShootType: (ushort)record.ShootType,
        ShootSpeed: record.ShootSpeed,
        Consumable: record.Consumable,
        Channel: record.Channel,
        AutoReuse: record.AutoReuse,
        UseTurn: record.UseTurn,
        ShootsEveryUse: record.ShootsEveryUse,
        NoUseGraphic: record.NoUseGraphic,
        Potion: record.Potion)
      : null;
    ItemRecoveryDefinition? recovery = record.BuffType > 0 || record.BuffDurationTicks > 0
      ? new ItemRecoveryDefinition(
        BuffType: (ushort)record.BuffType,
        BuffDurationTicks: record.BuffDurationTicks,
        Consumable: record.Consumable)
      : null;
    ItemEquipmentDefinition? equipment = mappedSentryEquipment ?? record.Equipment ??
       (equipmentSlot != ItemEquipmentSlot.None || record.Defense > 0 ||
       record.LifeRegen > 0 || record.ManaIncrease > 0 || sentryCapacityBonus > 0 ||
       record.HeadSlot >= 0 ||
       record.BodySlot >= 0 || record.LegSlot >= 0 || record.HandOnSlot >= 0 ||
       record.HandOffSlot >= 0 || record.BackSlot >= 0 || record.FrontSlot >= 0 ||
       record.ShoeSlot >= 0 || record.WaistSlot >= 0 || record.WingSlot >= 0 ||
       record.ShieldSlot >= 0 || record.NeckSlot >= 0 || record.FaceSlot >= 0 ||
       record.BalloonSlot >= 0 || record.BeardSlot >= 0 || record.VoiceSlot > 0 ||
       record.HasVanityEffects || record.Accessory ||
       record.Vanity || record.Social
        ? new ItemEquipmentDefinition(
          Slot: equipmentSlot,
          Defense: record.Defense,
          LifeRegen: record.LifeRegen,
          ManaIncrease: record.ManaIncrease,
          HeadSlot: record.HeadSlot,
          BodySlot: record.BodySlot,
          LegSlot: record.LegSlot,
          HandOnSlot: record.HandOnSlot,
          HandOffSlot: record.HandOffSlot,
          BackSlot: record.BackSlot,
          FrontSlot: record.FrontSlot,
          ShoeSlot: record.ShoeSlot,
          WaistSlot: record.WaistSlot,
          WingSlot: record.WingSlot,
          ShieldSlot: record.ShieldSlot,
          NeckSlot: record.NeckSlot,
          FaceSlot: record.FaceSlot,
          BalloonSlot: record.BalloonSlot,
          BeardSlot: record.BeardSlot,
          VoiceSlot: record.VoiceSlot,
          HasVanityEffects: record.HasVanityEffects,
          Accessory: record.Accessory,
          Vanity: record.Vanity,
          Social: record.Social,
          SentryCapacityBonus: sentryCapacityBonus)
        : null);
    ItemToolDefinition? tools = record.PickPower > 0 || record.AxePower > 0 || record.HammerPower > 0 ||
      record.TileWandPower > 0
      ? new ItemToolDefinition(
        record.PickPower,
        record.AxePower,
        record.HammerPower,
        record.TileWandPower)
      : null;
    ItemGatheringDefinition? gathering = record.FishingPolePower > 0 || record.BaitPower > 0
      ? new ItemGatheringDefinition(record.FishingPolePower, record.BaitPower)
      : null;
    ItemSummoningDefinition? summoning = record.MakeNpc > 0 || record.MountType >= 0
      ? new ItemSummoningDefinition(record.MakeNpc, record.MountType)
      : null;
    ItemAppearanceDefinition? appearance = record.HairDye >= 0 || record.Paint != 0 ||
      record.PaintCoating != 0
      ? new ItemAppearanceDefinition(record.HairDye, record.Paint, record.PaintCoating)
      : null;
    ItemExtractinatorDefinition? extractinator = record.ChlorophyteExtractinatorConsumable
      ? new ItemExtractinatorDefinition(4)
      : null;
    ItemCombatDefinition? combat = record.Damage > 0 || record.Knockback > 0 ||
      record.CriticalChance > 0 || record.ArmorPenetration > 0 || record.BonusTagDamage > 0 ||
        damageClass != ItemDamageClass.None || record.Flame || record.Mech || record.ProjectileType > 0 ||
      record.ProjectileSpeed > 0 || record.ConsumesAmmo || record.NotAmmo || record.Staff ||
      record.Claw || record.NoMelee
      ? new ItemCombatDefinition(
        Damage: record.Damage,
        Knockback: record.Knockback,
        CriticalChance: record.CriticalChance,
        ArmorPenetration: record.ArmorPenetration,
        BonusTagDamage: record.BonusTagDamage,
        DamageClass: damageClass,
        Flame: record.Flame,
        Mech: record.Mech,
        ProjectileType: (ushort)record.ProjectileType,
        ProjectileSpeed: record.ProjectileSpeed,
        AmmoType: (ushort)record.AmmoType,
        ConsumesAmmo: record.ConsumesAmmo,
        NotAmmo: record.NotAmmo,
        Staff: record.Staff,
        Claw: record.Claw,
        NoMelee: record.NoMelee)
      : null;
    ItemPlacementDefinition? placement = record.TileType >= 0 || record.WallType >= 0 ||
      record.PlaceStyle > 0 || record.TileBoost > 0 || record.CartTrack
      ? new ItemPlacementDefinition(
        TileType: record.TileType,
        WallType: (short)record.WallType,
        PlaceStyle: record.PlaceStyle,
        TileBoost: record.TileBoost,
        CartTrack: record.CartTrack)
      : null;
    bool hasIdentityMetadata = !string.IsNullOrEmpty(record.NameKey) || record.IsMaterial ||
      record.IsQuestItem || record.UniqueStack || record.ExpertOnly || record.Expert ||
      record.IsShopCurrency || record.IsShopItem || record.Buy || record.BuyOnce;
    ItemIdentityDefinition? identity = !hasIdentityMetadata
      ? null
      : new ItemIdentityDefinition(
        NameKey: record.NameKey,
        IsMaterial: record.IsMaterial,
        IsQuestItem: record.IsQuestItem,
        UniqueStack: record.UniqueStack,
        ExpertOnly: record.ExpertOnly,
        Expert: record.Expert,
        IsShopCurrency: record.IsShopCurrency,
        IsShopItem: record.IsShopItem,
        Buy: record.Buy,
        BuyOnce: record.BuyOnce);
    ItemDefinition definition = new(
      ItemType: (ushort)record.ItemType,
      StackLimit: record.MaxStack,
      HealthRestore: record.HealthRestore,
      UseCooldownTicks: record.UseCooldownTicks,
      Identity: identity,
      Use: use,
      Combat: combat,
      Placement: placement,
      Tools: tools,
      Recovery: recovery,
      Equipment: equipment,
      Alpha: record.Alpha,
      Scale: record.Scale,
      Width: record.Width,
      Height: record.Height,
      Value: record.Value,
      Rarity: record.Rarity,
      Gathering: gathering,
      Summoning: summoning,
      Appearance: appearance,
      Extractinator: extractinator,
      IsSentry: record.Sentry,
      Dd2Summon: record.Dd2Summon);
    ItemDefinitionCompiler.Validate(definition);
    return definition;
  }

  private static ItemDamageClass ResolveDamageClass(LegacyItemDefinitionRecord record)
  {
    ItemDamageClass flaggedClass = ItemDamageClass.None;
    int flaggedCount = 0;
    if (record.Melee)
    {
      flaggedClass = ItemDamageClass.Melee;
      flaggedCount++;
    }

    if (record.Magic)
    {
      flaggedClass = ItemDamageClass.Magic;
      flaggedCount++;
    }

    if (record.Ranged)
    {
      flaggedClass = ItemDamageClass.Ranged;
      flaggedCount++;
    }

    if (record.Summon)
    {
      flaggedClass = ItemDamageClass.Summon;
      flaggedCount++;
    }

    if (flaggedCount > 1 ||
        (record.DamageClass != ItemDamageClass.None &&
         flaggedCount == 1 && record.DamageClass != flaggedClass))
    {
      throw new ArgumentOutOfRangeException(nameof(record), "Damage class flags conflict.");
    }

    ItemDamageClass resolved = record.DamageClass != ItemDamageClass.None
      ? record.DamageClass
      : flaggedClass;
    if (!Enum.IsDefined(resolved))
    {
      throw new ArgumentOutOfRangeException(nameof(record));
    }

    return resolved;
  }
}

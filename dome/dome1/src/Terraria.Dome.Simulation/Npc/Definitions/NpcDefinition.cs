using System;

namespace Terraria.Dome.Simulation.Npc.Definitions;

public enum NpcBehaviorId
{
  OrdinaryChase = 1,
  TownHome = 2,
  Segment = 3,
  TrainingDummy = 4,
  FloatingEye = 5
}

public enum NpcFaction
{
  Hostile = 1,
  Town = 2,
  Neutral = 3
}

public enum NpcCategory
{
  Enemy = 1,
  Town = 2,
  Segment = 3
}

public readonly record struct NpcDefinition
{
  private const int MaxAiStyle = 127;
  private const int MaxNetId = 696;

  public NpcDefinition(
    int DefinitionId,
    int NetId,
    int MaximumHealth,
    int Defense,
    float ColliderWidth,
    float ColliderHeight,
    NpcBehaviorId BehaviorId,
    int LootTableId,
    NpcFaction Faction = NpcFaction.Hostile,
    NpcCategory Category = NpcCategory.Enemy,
    int AiStyle = 0,
    bool IsImmortal = false,
    bool AlwaysReplicate = false,
    bool IsLikeTownNpc = false,
    bool IsTownPet = false,
    bool SupportsNpcTargets = false,
    bool IsBoss = false,
    bool ShouldBeCountedAsBossForRainbowBoulders = false,
    float TakenDamageMultiplier = 1.0f,
    bool SuppressLootWhenSpawnedFromStatue = false,
    float NpcSlotCost = 1.0f,
    bool IsTrapImmune = false,
    bool IsLavaImmune = false,
    int Damage = 0,
    bool IsColdDamage = false,
    int FriendlyRegen = 0,
    float KnockBackResist = 1.0f)
  {
    if (DefinitionId <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(DefinitionId));
    }

    if (NetId <= 0 || NetId > MaxNetId)
    {
      throw new ArgumentOutOfRangeException(nameof(NetId));
    }

    if (MaximumHealth <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(MaximumHealth));
    }

    if (Defense < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(Defense));
    }

    if (LootTableId < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(LootTableId));
    }

    if (AiStyle < 0 || AiStyle > MaxAiStyle)
    {
      throw new ArgumentOutOfRangeException(nameof(AiStyle));
    }

    if (!Enum.IsDefined(BehaviorId) || !Enum.IsDefined(Faction) || !Enum.IsDefined(Category))
    {
      throw new ArgumentOutOfRangeException(nameof(BehaviorId));
    }

    if (BehaviorId == NpcBehaviorId.TownHome &&
        (Faction != NpcFaction.Town || Category != NpcCategory.Town))
    {
      throw new ArgumentException(
        "Town-home behavior requires town faction and category.",
        nameof(BehaviorId));
    }

    if (BehaviorId == NpcBehaviorId.Segment && Category != NpcCategory.Segment)
    {
      throw new ArgumentException(
        "Segment behavior requires the segment category.",
        nameof(Category));
    }

    if (Category == NpcCategory.Segment && BehaviorId != NpcBehaviorId.Segment)
    {
      throw new ArgumentException(
        "The segment category requires segment behavior.",
        nameof(Category));
    }

    if (!float.IsFinite(ColliderWidth) || !float.IsFinite(ColliderHeight) ||
        ColliderWidth <= 0.0f || ColliderHeight <= 0.0f)
    {
      throw new ArgumentOutOfRangeException(nameof(ColliderWidth));
    }

    if (!float.IsFinite(TakenDamageMultiplier) || TakenDamageMultiplier < 1.0f)
    {
      throw new ArgumentOutOfRangeException(nameof(TakenDamageMultiplier));
    }

    if (!float.IsFinite(NpcSlotCost) || NpcSlotCost < 0.0f)
    {
      throw new ArgumentOutOfRangeException(nameof(NpcSlotCost));
    }

    if (Damage < 0 || FriendlyRegen < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(Damage));
    }

    if (!float.IsFinite(KnockBackResist) || KnockBackResist < 0.0f)
    {
      throw new ArgumentOutOfRangeException(nameof(KnockBackResist));
    }

    this.DefinitionId = DefinitionId;
    this.NetId = NetId;
    this.MaximumHealth = MaximumHealth;
    this.Defense = Defense;
    this.ColliderWidth = ColliderWidth;
    this.ColliderHeight = ColliderHeight;
    this.BehaviorId = BehaviorId;
    this.LootTableId = LootTableId;
    this.Faction = Faction;
    this.Category = Category;
    this.AiStyle = AiStyle;
    this.IsImmortal = IsImmortal;
    this.AlwaysReplicate = AlwaysReplicate;
    this.IsLikeTownNpc = IsLikeTownNpc;
    this.IsTownPet = IsTownPet;
    this.SupportsNpcTargets = SupportsNpcTargets;
    this.IsBoss = IsBoss;
    this.ShouldBeCountedAsBossForRainbowBoulders = ShouldBeCountedAsBossForRainbowBoulders;
    this.TakenDamageMultiplier = TakenDamageMultiplier;
    this.SuppressLootWhenSpawnedFromStatue = SuppressLootWhenSpawnedFromStatue;
    this.NpcSlotCost = NpcSlotCost;
    this.IsTrapImmune = IsTrapImmune;
    this.IsLavaImmune = IsLavaImmune;
    this.Damage = Damage;
    this.IsColdDamage = IsColdDamage;
    this.FriendlyRegen = FriendlyRegen;
    this.KnockBackResist = KnockBackResist;
  }

  public int DefinitionId { get; }
  public int NetId { get; }
  public int MaximumHealth { get; }
  public int Defense { get; }
  public float ColliderWidth { get; }
  public float ColliderHeight { get; }
  public NpcBehaviorId BehaviorId { get; }
  public int LootTableId { get; }
  public NpcFaction Faction { get; }
  public NpcCategory Category { get; }
  public int AiStyle { get; }
  public bool IsImmortal { get; }
  public bool AlwaysReplicate { get; }
  public bool IsLikeTownNpc { get; }
  public bool IsTownPet { get; }
  public bool SupportsNpcTargets { get; }
  public bool IsBoss { get; }
  public bool ShouldBeCountedAsBossForRainbowBoulders { get; }
  public float TakenDamageMultiplier { get; }
  public bool SuppressLootWhenSpawnedFromStatue { get; }
  public float NpcSlotCost { get; }
  public bool IsTrapImmune { get; }
  public bool IsLavaImmune { get; }
  public int Damage { get; }
  public bool IsColdDamage { get; }
  public int FriendlyRegen { get; }
  public float KnockBackResist { get; }

  public bool TreatedAsABossForRainbowBoulders =>
    IsBoss || ShouldBeCountedAsBossForRainbowBoulders;

  public NpcDefinition WithSupportsNpcTargets(bool supportsNpcTargets)
  {
    return new NpcDefinition(
      DefinitionId,
      NetId,
      MaximumHealth,
      Defense,
      ColliderWidth,
      ColliderHeight,
      BehaviorId,
      LootTableId,
      Faction,
      Category,
      AiStyle,
      IsImmortal,
      AlwaysReplicate,
      IsLikeTownNpc,
      IsTownPet,
      supportsNpcTargets,
      IsBoss,
      ShouldBeCountedAsBossForRainbowBoulders,
      TakenDamageMultiplier,
      SuppressLootWhenSpawnedFromStatue,
      NpcSlotCost,
      IsTrapImmune,
      IsLavaImmune,
      Damage,
      IsColdDamage,
      FriendlyRegen,
      KnockBackResist);
  }
}

using System.Collections.Frozen;
using Terraria.Content;
using Terraria.Projectile;

namespace Terraria.NonAuthoritative.SimulationHost;

internal static class SimulationContentSupportManifest
{
  internal enum NpcNaturalDespawnPolicy
  {
    Persistent,
    OutsideLivingPlayerRangeAfterGracePeriod,
  }

  public const string Version = "simulation-core-v3";
  public const int ZombieNetId = 3;
  public const int BlueSlimeNetId = 1;
  public const int DemonEyeNetId = 2;
  public const int GuideNetId = 22;
  public const int GreenSlimeNetId = 16;
  public const int OldManNetId = 37;
  public const int EyeOfCthulhuNetId = 4;
  public const int ServantOfCthulhuNetId = 5;
  public const int TrainingDummyNetId = 488;
  public const int SupportedArrowLifetimeTicks = 1_200;

  private static readonly FrozenSet<int> SupportedItemTypes =
    new[] { 23, 39, 40 }.ToFrozenSet();
  private static readonly FrozenSet<int> SupportedNpcNetIds =
    new[]
    {
      BlueSlimeNetId,
      DemonEyeNetId,
      ZombieNetId,
      GreenSlimeNetId,
      GuideNetId,
      OldManNetId,
      EyeOfCthulhuNetId,
      ServantOfCthulhuNetId,
      TrainingDummyNetId,
    }.ToFrozenSet();
  private static readonly IReadOnlyDictionary<int, int> SupportedNpcAiStyles =
    new Dictionary<int, int>
    {
      [ZombieNetId] = 3,
      [BlueSlimeNetId] = 1,
      [DemonEyeNetId] = 2,
      [GreenSlimeNetId] = 1,
      [TrainingDummyNetId] = 0,
      [GuideNetId] = 0,
      [OldManNetId] = 0,
      [EyeOfCthulhuNetId] = 4,
      [ServantOfCthulhuNetId] = 5,
    };
  private static readonly IReadOnlyDictionary<int, NpcNaturalDespawnPolicy>
    SupportedNpcNaturalDespawnPolicies = new Dictionary<int, NpcNaturalDespawnPolicy>
    {
      [BlueSlimeNetId] = NpcNaturalDespawnPolicy.OutsideLivingPlayerRangeAfterGracePeriod,
      [DemonEyeNetId] = NpcNaturalDespawnPolicy.OutsideLivingPlayerRangeAfterGracePeriod,
      [ZombieNetId] = NpcNaturalDespawnPolicy.OutsideLivingPlayerRangeAfterGracePeriod,
      [GreenSlimeNetId] = NpcNaturalDespawnPolicy.OutsideLivingPlayerRangeAfterGracePeriod,
      [GuideNetId] = NpcNaturalDespawnPolicy.Persistent,
      [OldManNetId] = NpcNaturalDespawnPolicy.Persistent,
      [EyeOfCthulhuNetId] = NpcNaturalDespawnPolicy.Persistent,
      [ServantOfCthulhuNetId] =
        NpcNaturalDespawnPolicy.OutsideLivingPlayerRangeAfterGracePeriod,
      [TrainingDummyNetId] = NpcNaturalDespawnPolicy.Persistent,
    };
  private static readonly FrozenSet<int> SupportedProjectileTypes =
    new[] { 1 }.ToFrozenSet();

  public static bool TryGetNpcNaturalDespawnPolicy(
    int netId,
    out NpcNaturalDespawnPolicy policy)
  {
    return SupportedNpcNaturalDespawnPolicies.TryGetValue(netId, out policy);
  }

  public static void Validate(ContentCatalog catalog)
  {
    ArgumentNullException.ThrowIfNull(catalog);
    RequireExactSet(
      catalog.Snapshot.Items.DefinitionsByType.Keys,
      SupportedItemTypes,
      "item");
    RequireExactSet(
      catalog.Snapshot.Npcs.DefinitionsByNetId.Keys,
      SupportedNpcNetIds,
      "NPC");
    RequireExactSet(
      SupportedNpcNaturalDespawnPolicies.Keys,
      SupportedNpcNetIds,
      "NPC natural despawn policy");
    RequireExactSet(
      catalog.Snapshot.Projectiles.DefinitionsByType.Keys,
      SupportedProjectileTypes,
      "projectile");
    foreach ((int netId, int aiStyle) in SupportedNpcAiStyles)
    {
      if (!catalog.Npcs.TryGetByNetId(netId, out NpcDefinition definition) ||
          definition.Spawn.AiStyle != aiStyle)
      {
        throw new InvalidOperationException(
          $"NPC {netId} requires supported AI style {aiStyle} in the simulation catalog.");
      }
    }

    if (!catalog.Projectiles.TryGet(1, out ProjectileDefinition arrow) ||
        arrow.Geometry.Width != 10 ||
        arrow.Geometry.Height != 10 ||
        arrow.Geometry.Scale != 1.0f ||
        arrow.Geometry.OwnerHitCheckDistance != 1000.0f ||
        arrow.Behavior.DefaultTimeLeft != SupportedArrowLifetimeTicks ||
        arrow.Behavior.AiStyle != 1 ||
        arrow.Behavior.ExtraUpdates != 0 ||
        arrow.Behavior.NumUpdates != 0 ||
        arrow.Behavior.DecidesManualFallThrough ||
        arrow.Behavior.ShouldFallThrough ||
        arrow.Presentation.TrailCacheLength != 10 ||
        arrow.Presentation.FrameCount != 1 ||
        arrow.Presentation.Light != 0.0f ||
        arrow.Presentation.Hide ||
        arrow.Presentation.Alpha != 0 ||
        arrow.Presentation.DrawLayer != 0 ||
        (arrow.Presentation.GlowMaskId.HasValue &&
         arrow.Presentation.GlowMaskId.Value != -1) ||
        ProjectileTrailCacheSystem.GetTrailingMode(1) != -1 ||
        arrow.Identity.NeedsUuid != false ||
        arrow.Network.NetworkImportant ||
        !arrow.Geometry.TileCollide ||
        arrow.Geometry.IgnoreWater ||
        arrow.Geometry.CorrectSlopeCollision ||
        arrow.Penetration.StopsDealingDamageAfterPenetrateHits ||
        arrow.Penetration.UsesLocalNpcImmunity ||
        arrow.Penetration.UsesIdStaticNpcImmunity ||
        arrow.Penetration.AppliesImmunityTimeOnSingleHits ||
        arrow.Penetration.LocalNpcHitCooldown != -2 ||
        arrow.Penetration.IdStaticNpcHitCooldown != -1 ||
        arrow.Penetration.DefaultPenetrate != 1 ||
        arrow.Penetration.MaxPenetrate != 1 ||
        arrow.Combat.Damage != 0 ||
        arrow.Combat.KnockBack != 0.0f ||
        !arrow.Combat.Friendly ||
        arrow.Combat.Hostile ||
        arrow.Combat.Melee ||
        !arrow.Combat.Ranged ||
        arrow.Combat.Magic ||
        arrow.Combat.ColdDamage ||
        arrow.Combat.Trap ||
        arrow.Combat.NpcProjectile ||
        arrow.Combat.HostileDamageScaling != ProjectileHostileDamageScalingDefinition.Default ||
        !arrow.Capabilities.IsArrow ||
        arrow.Capabilities.IsBobber ||
        arrow.Capabilities.IsMinion ||
        arrow.Capabilities.IsSentry ||
        arrow.Capabilities.IsHook ||
        arrow.Capabilities.IsCounterweight ||
        arrow.Capabilities.MinionSlots != 0.0f ||
        arrow.Capabilities.IsOwnerHitCheck ||
        arrow.Capabilities.UsesOwnerMeleeHitCooldown ||
        arrow.Capabilities.UsesOwnerLight ||
        arrow.Capabilities.NoEnchantments ||
        arrow.Capabilities.NoEnchantmentVisuals)
    {
      throw new InvalidOperationException(
        "Projectile 1 must match the supported ordinary-arrow simulation profile.");
    }
  }

  private static void RequireExactSet(
    IEnumerable<int> actual,
    IReadOnlySet<int> supported,
    string contentKind)
  {
    foreach (int contentId in actual)
    {
      if (!supported.Contains(contentId))
      {
        throw new InvalidOperationException(
          $"The simulation content catalog includes unsupported {contentKind} id {contentId}.");
      }
    }

    foreach (int supportedId in supported)
    {
      if (!actual.Contains(supportedId))
      {
        throw new InvalidOperationException(
          $"The simulation content catalog is missing required {contentKind} id {supportedId}.");
      }
    }
  }
}

using System;
using System.Numerics;
using Terraria.Ai;
using Terraria.Attributes;
using Terraria.Combat;
using Terraria.Entity;
using Terraria.Liquid;
using Terraria.Movement;
using Terraria.Npc;
using Terraria.Physics;
using Terraria.Relationships;
using MovementVelocityComponent = Terraria.Movement.VelocityComponent;

namespace NSSLC.Server;

/// <summary>NPC 实体的不可变定义：向导与史莱姆各一份，差异集中在定义里而不是创建处的三元表达式。</summary>
/// <remarks>
/// 审计 §30（#6）选择 A 的收尾项：宿主补齐后，玩家与投射物已走共享定义框架，
/// NPC 若继续 inline 装配就还是两套路径。生成身份由工厂每次新建；
/// 出生坐标与网络标识属于实例级输入，由调用方在生成后写入。
/// </remarks>
public static class NpcEntityDefinition
{
  public static readonly EntityDefinitionId GuideId = new("npc.guide");

  public static readonly EntityDefinitionId SlimeId = new("npc.slime");

  public static void Register(EntityDefinitionRegistry definitions)
  {
    ArgumentNullException.ThrowIfNull(definitions);

    definitions.Register(BuildGuide());
    definitions.Register(BuildSlime());
  }

  /// <summary>两种 NPC 的出生纵向偏移：贴地时按各自碰撞体高度回退。</summary>
  public static float SpawnOffsetY(bool isSlime)
  {
    return isSlime ? 18f : 40f;
  }

  private static EntityDefinition BuildGuide()
  {
    return new EntityDefinitionBuilder(GuideId)
        .Add(() => new EntityIdentityComponent(EntityUuid.Create()))
        .Add(() => new EntityFactionComponent(Faction.Friendly))
        .Add(() => new LifeComponent(current: 250, maximum: 250))
        // 出生坐标由调用方写入，这里只放占位值。
        .Add(() => new LocationComponent(Vector2.Zero))
        .Add(() => MovementVelocityComponent.Stationary)
        .Add(() => new ColliderComponent(new Vector2(18f, 40f)))
        .Add(() => GroundingComponent.Airborne)
        .Add(() => new EntityCollisionCapabilityComponent(
            Enabled: true,
            Layer: 1,
            Mask: uint.MaxValue))
        .Add(() => new MovementCapabilityComponent(
            enabled: true,
            maximumSpeed: 1f,
            acceleration: 8f,
            canJump: true,
            jumpSpeed: 5f))
        .Add(() => new GravityCapabilityComponent(Enabled: true, CanToggleDirection: false))
        .Add(() => new GravityConfigurationComponent())
        .Add(() => new GravityDirectionStateComponent(GravityDirection.Normal))
        .Add(() => new GravityModifierCollectionComponent())
        .Add(() => new GravityStateComponent(0.4f, 10f, GravityDirection.Normal, false, false))
        .Add(() => new TileCollisionCapabilityComponent(Enabled: true))
        .Add(() => new LiquidInteractionCapabilityComponent(
            Enabled: true,
            AffectedLiquids: LiquidContactFlags.Water | LiquidContactFlags.Honey |
                LiquidContactFlags.Lava | LiquidContactFlags.Shimmer))
        .Add(() => MovementIntent.Stop(tick: 0))
        .Add(() => new NpcBehaviorStateComponent())
        .WithAi<GuideAiBehavior>()
        .Add(() => new AiBehaviorSelectionComponent("Guide", "guide.default"))
        .Build();
  }

  private static EntityDefinition BuildSlime()
  {
    return new EntityDefinitionBuilder(SlimeId)
        .Add(() => new EntityIdentityComponent(EntityUuid.Create()))
        .Add(() => new EntityFactionComponent(Faction.Hostile))
        .Add(() => new LifeComponent(current: 50, maximum: 50))
        .Add(() => new LocationComponent(Vector2.Zero))
        .Add(() => MovementVelocityComponent.Stationary)
        .Add(() => new ColliderComponent(new Vector2(24f, 18f)))
        .Add(() => GroundingComponent.Airborne)
        .Add(() => new EntityCollisionCapabilityComponent(
            Enabled: true,
            Layer: 1,
            Mask: uint.MaxValue))
        .Add(() => new MovementCapabilityComponent(
            enabled: true,
            maximumSpeed: 2.5f,
            acceleration: 12f,
            canJump: true,
            jumpSpeed: 6f))
        .Add(() => new GravityCapabilityComponent(Enabled: true, CanToggleDirection: false))
        .Add(() => new GravityConfigurationComponent())
        .Add(() => new GravityDirectionStateComponent(GravityDirection.Normal))
        .Add(() => new GravityModifierCollectionComponent())
        .Add(() => new GravityStateComponent(0.4f, 10f, GravityDirection.Normal, false, false))
        .Add(() => new TileCollisionCapabilityComponent(Enabled: true))
        .Add(() => new LiquidInteractionCapabilityComponent(
            Enabled: true,
            AffectedLiquids: LiquidContactFlags.Water | LiquidContactFlags.Honey |
                LiquidContactFlags.Lava | LiquidContactFlags.Shimmer))
        .Add(() => MovementIntent.Stop(tick: 0))
        .Add(() => new NpcBehaviorStateComponent())
        .WithAi<SlimeAiBehavior>()
        .Add(() => new AiBehaviorSelectionComponent("Slime", "slime.default"))
        .Build();
  }
}

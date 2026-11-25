using System;
using System.Collections.Generic;
using Terraria.Share.Entities;
using Terraria.Share.Liquid;
using Terraria.Share.Movement;
using Terraria.Share.Physics;

namespace Terraria.Unshared.Projectiles;

/// <summary>Turns one recipe into an entity definition without knowing any content family.</summary>
/// <remarks>
/// 折叠函数是本计划的核心：它只做「两遍遍历 + 前置能力组 + 完整性校验」，不含任何族名。
/// 新增一个内容族只需要写一个新 section，不需要动这里——连续三处（codec 字面量、装配函数、
/// 定义构造函数）排队改的时代到此结束。
/// <para>
/// 两遍的理由见 <see cref="IProjectileSection" />：追踪族的转向率会进入运动能力组件，
/// 单遍折叠会让结果取决于 section 顺序。
/// </para>
/// </remarks>
internal static class ProjectileComposition
{
  /// <summary>
  /// 共享空间协调系统的静默前置条件。缺任何一项，查询与回调都不会报错——弹体只是永远不动。
  /// </summary>
  private static readonly Type[] SpatialPrerequisiteTypes = [
      typeof(GroundingComponent),
      typeof(MovementCapabilityComponent),
      typeof(GravityCapabilityComponent),
      typeof(GravityConfigurationComponent),
      typeof(GravityStateComponent),
      typeof(TileCollisionCapabilityComponent),
      typeof(SubstepCapabilityComponent),
      typeof(MovementIntent),
  ];

  public static EntityDefinition Create(ProjectileRecipe recipe)
  {
    ArgumentNullException.ThrowIfNull(recipe);

    ProjectileSectionContext context = new(recipe.Id);
    ProjectileCompositionParameters parameters = new();
    foreach (IProjectileSection section in recipe.Sections)
    {
      section.Collect(context, parameters);
    }

    // 折叠结束后立即封口：Contribute 期间再改数值说明有 section 越过了「只声明不收尾」的边界。
    parameters.Seal();

    EntityBuilder builder = new(recipe.EntityId);
    AddSpatialPrerequisites(builder, parameters);
    foreach (IProjectileSection section in recipe.Sections)
    {
      section.Contribute(context, parameters, builder);
    }

    EntityDefinition definition = builder.Build();
    EnsureSpatialPrerequisites(recipe.Id, definition, parameters);
    return definition;
  }

  /// <summary>添加共享空间管线要求的前置能力组。</summary>
  /// <remarks>
  /// 由本函数统一添加，而不让任何内容族"顺带"完成：否则删掉某个 section 会静默拆掉运动能力，
  /// 而症状是"弹体不动"，离原因很远。
  /// </remarks>
  private static void AddSpatialPrerequisites(
      EntityBuilder builder,
      ProjectileCompositionParameters parameters)
  {
    builder
        .Add(() => GroundingComponent.Airborne)
        // acceleration 来自参数块而不是硬编码 0：追踪族的转向率是唯一的跨族读数。
        // maximumSpeed 取 0 表示不钳制（SpatialSimulationSystem 只在 >0 时生效）。
        .Add(() => new MovementCapabilityComponent(
            enabled: true,
            maximumSpeed: 0f,
            acceleration: parameters.MovementAcceleration,
            canJump: false,
            jumpSpeed: 0f,
            mode: parameters.MovementMode))
        .Add(() => new TileCollisionCapabilityComponent(
            Enabled: parameters.TileCollisionEnabled))
        .Add(() => new SubstepCapabilityComponent(parameters.SubstepCount))
        .Add(() => MovementIntent.Stop(tick: 0));
  }

  /// <summary>把所有静默前置条件变成带内容 id 与缺失类型名的异常。</summary>
  private static void EnsureSpatialPrerequisites(
      ProjectileId id,
      EntityDefinition definition,
      ProjectileCompositionParameters parameters)
  {
    List<string> missing = [];
    foreach (Type required in SpatialPrerequisiteTypes)
    {
      if (!definition.Provides(required))
      {
        missing.Add(required.Name);
      }
    }

    if (parameters.LiquidEnabled)
    {
      // 启用液体却缺 contact 组件时，液体策略是一个没人读的开关，等于内容声明被静默忽略。
      RequireMissing<LiquidInteractionCapabilityComponent>(definition, missing);
      RequireMissing<LiquidContactComponent>(definition, missing);
    }

    if (missing.Count != 0)
    {
      throw new InvalidOperationException(
          string.Concat(
              "Projectile composition '",
              id.Value,
              "' is missing required components: ",
              string.Join(", ", missing),
              "."));
    }
  }

  private static void RequireMissing<TComponent>(EntityDefinition definition, List<string> missing)
  {
    if (!definition.Provides(typeof(TComponent)))
    {
      missing.Add(typeof(TComponent).Name);
    }
  }
}

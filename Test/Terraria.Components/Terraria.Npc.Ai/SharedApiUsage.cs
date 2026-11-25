using System;
using System.Numerics;
using Arch.Core;
using ArchEntity = Arch.Core.Entity;
using ArchWorld = Arch.Core.World;
using GoapComparison = Terraria.Share.Ai.Goap.Core.Comparison;
using Terraria.Share.Ai.Examples;
using Terraria.Share.Ai.Goap.Runtime;
using Terraria.Share.Attributes;
using Terraria.Share.Combat;
using Terraria.Share.Entities;

namespace Terraria.Verification;

/// <summary>
/// 让"只有文档引用、没有代码引用"的四份 API 获得真实消费者。
/// </summary>
/// <remarks>
/// 这些类型此前只出现在 README 与 SOURCES.md 的示例里，任何一次零引用清理都会把它们判成死代码。
/// 本类逐项实际调用一次，使它们进入可编译、可运行的验证路径：
/// <list type="bullet">
/// <item><c>ExampleAiUsage</c>：构造共同的 AI 系统入口。</item>
/// <item><c>EcsServerCommandBuffer</c>：执行一次延迟结构变更并回放。</item>
/// <item><c>ReferenceCondition</c>：与 <c>WorldKey</c> 组合成 GOAP 条件。</item>
/// <item><c>AttributeContributionKindMapping</c>：验证十一类形态到来源分类的映射是全的。</item>
/// </list>
/// </remarks>
public static class SharedApiUsage {
  /// <summary>
  /// 逐项验证并返回一行摘要。任何一项不成立都抛出 <see cref="InvalidOperationException"/>。
  /// </summary>
  public static string Verify() {
    VerifyExampleAiUsage();
    VerifyCommandBuffer();
    VerifyReferenceCondition();
    int kinds = VerifyContributionKindMapping();
    return $"shared-api | ai=ok buffer=ok condition=ok contributionKinds={kinds}";
  }

  private static void VerifyExampleAiUsage() {
    var system = ExampleAiUsage.CreateSystem();
    Require(system is not null, "ExampleAiUsage.CreateSystem returned null.");
  }

  private static void VerifyCommandBuffer() {
    using ArchWorld world = ArchWorld.Create();
    ArchEntity existing = world.Create(new LifeComponent(10, 20));
    using EcsServerCommandBuffer buffer = new();
    // Create 只登记结构意图，实体标识在 Playback 之后才由世界分配，
    // 因此这里验证 Size 增长，并用真实实体验证 Set / Add 的回放效果。
    buffer.Create(typeof(LocationComponent));
    buffer.Set(existing, new LifeComponent(50, 100));
    buffer.Add(existing, new LocationComponent(Vector2.Zero));
    Require(buffer.Size > 0, "The command buffer recorded nothing.");
    buffer.Playback(world);
    Require(
        world.Get<LifeComponent>(existing).Maximum == 100,
        "The command buffer did not replay the LifeComponent replacement.");
    Require(
        world.Has<LocationComponent>(existing),
        "The command buffer did not replay the LocationComponent addition.");
  }

  private static void VerifyReferenceCondition() {
    WorldKey worldKey = new("player.health");
    WorldKey valueKey = new("threshold");
    ReferenceCondition condition = new(worldKey, GoapComparison.GreaterThan, valueKey);
    Require(condition.WorldKey == worldKey, "ReferenceCondition lost its world key.");
    Require(condition.ValueKey == valueKey, "ReferenceCondition lost its value key.");
    Require(
        condition.Comparison == GoapComparison.GreaterThan,
        "ReferenceCondition lost its comparison.");
  }

  private static int VerifyContributionKindMapping() {
    AttributeContributionKind[] kinds = Enum.GetValues<AttributeContributionKind>();
    foreach (AttributeContributionKind kind in kinds) {
      AttributeSourceCategory category = AttributeContributionKindMapping.ToSourceCategory(kind);
      Require(
          Enum.IsDefined(category),
          $"Contribution kind {kind} maps to an undefined source category.");
    }

    return kinds.Length;
  }

  private static void Require(bool condition, string message) {
    if (!condition) {
      throw new InvalidOperationException(message);
    }
  }
}

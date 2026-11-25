using Terraria.Share.Ai;

namespace Terraria.Unshared.Npc;

/// <summary>
/// Composition-root AI declarations for the NPC domain: which behaviors exist, what each one
/// reads, how each runtime is constructed, and which intent kinds each may emit.
/// </summary>
/// <remarks>
/// This is the only place where an NPC behavior's inputs and outputs are stated.
/// <see cref="NpcAiRuntime"/> registers from the sealed catalog and rejects a behavior whose reads
/// cannot be projected; <see cref="NpcAiIntentAdapter"/> rejects a behavior whose emitted kinds it
/// cannot dispatch.
/// </remarks>
public static class NpcAiDeclaration
{
  /// <summary>Builds the sealed NPC behavior catalog.</summary>
  public static NpcAiCatalog CreateCatalog()
  {
    NpcAiCatalog catalog = new();
    catalog.Add(Guide());
    catalog.Add(Slime());
    catalog.Seal();
    return catalog;
  }

  private static NpcAiBehaviorDeclaration Guide()
  {
    AiRequirements reads = AiRequirements.Empty
        .Require<NpcAiMovementFact>();
    AiBehaviorDeclaration<NpcAiIntent> behavior =
        AiBehaviorDeclaration<NpcAiIntent>.For<GuideAiBehavior>(
            static requirements => new GuideAiBehavior(requirements),
            reads);
    // GuideAiBehavior 提交行为状态与移动意图，StopCore 额外提交一次停止。
    return new NpcAiBehaviorDeclaration(
        behavior,
        new NpcAiIntentKind[]
        {
          NpcAiIntentKind.SetBehaviorState,
          NpcAiIntentKind.Move,
          NpcAiIntentKind.StopMovement
        },
        // AI 决策必须先于移动意图装入：适配器写意图队列，MovementIntentApply 读它。
        after: new[] { AiAnchor.InputApply },
        before: new[] { AiAnchor.MovementCommit });
  }

  private static NpcAiBehaviorDeclaration Slime()
  {
    AiRequirements reads = AiRequirements.Empty
        .Require<NpcAiMovementFact>()
        .Require<NpcAiTargetFact>()
        .Require<NpcAiLifeFact>()
        .Require<NpcAiWorldFact>();
    AiBehaviorDeclaration<NpcAiIntent> behavior =
        AiBehaviorDeclaration<NpcAiIntent>.For<SlimeAiBehavior>(
            static requirements => new SlimeAiBehavior(requirements),
            reads);
    // SlimeAiRules 只产出行为状态与移动意图，StopCore 额外提交一次停止。
    return new NpcAiBehaviorDeclaration(
        behavior,
        new NpcAiIntentKind[]
        {
          NpcAiIntentKind.SetBehaviorState,
          NpcAiIntentKind.Move,
          NpcAiIntentKind.StopMovement
        },
        after: new[] { AiAnchor.InputApply },
        before: new[] { AiAnchor.MovementCommit });
  }
}

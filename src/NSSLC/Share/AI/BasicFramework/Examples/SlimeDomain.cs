using Terraria.Share.Ai.Htn;
using Terraria.Share.Ai.Htn.Compounds;
using Terraria.Share.Ai.Htn.Conditions;
using Terraria.Share.Ai.Htn.Operators;
using Terraria.Share.Ai.Htn.PrimitiveTasks;
using HtnTaskStatus = Terraria.Share.Ai.Htn.TaskStatus;

namespace Terraria.Share.Ai.Examples;

/// <summary>Example decision only. Physics, jump cooldown and targeting remain domain state.</summary>
public static class SlimeDomain {
  public static Domain<SlimeContext> Create() {
    Domain<SlimeContext> domain = new("Slime");
    Selector chooseBehavior = new() { Name = "Choose behavior" };

    PrimitiveTask jump = new() { Name = "Jump towards target" };
    jump.AddCondition(new FuncCondition<SlimeContext>("Target visible", HasTarget));
    jump.AddCondition(new FuncCondition<SlimeContext>("On ground", IsGrounded));
    jump.AddCondition(new FuncCondition<SlimeContext>("Jump cooldown elapsed", IsJumpReady));
    jump.SetOperator(new FuncOperator<SlimeContext>(Jump));

    PrimitiveTask wait = new() { Name = "Wait" };
    wait.SetOperator(new FuncOperator<SlimeContext>(Wait));

    domain.Add(domain.Root, chooseBehavior);

    // Selector tries children in insertion order, so keep the fallback last.
    domain.Add(chooseBehavior, jump);
    domain.Add(chooseBehavior, wait);

    return domain;
  }

  private static bool HasTarget(SlimeContext context) {
    return context.IsSet(SlimeFact.HasTarget);
  }

  private static bool IsGrounded(SlimeContext context) {
    return context.IsSet(SlimeFact.IsGrounded);
  }

  private static bool IsJumpReady(SlimeContext context) {
    return context.IsSet(SlimeFact.JumpReady);
  }

  private static HtnTaskStatus Jump(SlimeContext context) {
    context.WantsJump = true;
    context.MoveDirection = context.TargetDirection;
    return HtnTaskStatus.Success;
  }

  private static HtnTaskStatus Wait(SlimeContext context) {
    return HtnTaskStatus.Success;
  }
}

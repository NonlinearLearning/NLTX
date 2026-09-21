namespace Terraria.Player;

public sealed class PlayerActivityQuery
{
  public PlayerActivityDecision Evaluate(in PlayerActivityInput input)
  {
    if (input.IsPaused || input.IsDead)
    {
      return new(
        CountAfk: false,
        ResetAfkCounter: true,
        ResetKitingCounter: true);
    }

    bool itemActivityAllowsAfk =
      !input.IsItemAnimationActive || input.HeldItemIsKite;
    bool canCountAfk = input.IsConsideredStandingStill &&
      itemActivityAllowsAfk &&
      !input.HasMovementInput &&
      !input.HasControlInput &&
      !input.HasAttackInput;
    bool resetKitingCounter = !canCountAfk ||
      input.HasMouseItem ||
      input.IsPetting ||
      input.IsSitting ||
      input.IsSleeping;

    return new(
      CountAfk: canCountAfk,
      ResetAfkCounter: !canCountAfk,
      ResetKitingCounter: resetKitingCounter);
  }
}

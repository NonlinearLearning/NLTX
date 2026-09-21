namespace Terraria.Combat.Resolution;

public sealed class NpcKillResolutionSystem
{
  public NpcKillResolutionResult Resolve(
    NpcStrikeCommitCommand command,
    NpcStrikeCommitResult commit)
  {
    bool confirmed = commit.DamageCommitted &&
      commit.TargetDeadAfterCommit &&
      !commit.Duplicate;
    return new NpcKillResolutionResult(command.Attempt, confirmed);
  }
}

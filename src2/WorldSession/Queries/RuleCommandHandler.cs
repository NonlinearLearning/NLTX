using Terraria.WorldSession.Session;

namespace Terraria.WorldSession.Queries;

public sealed class RuleCommandHandler
{
  private WorldSessionRuleSnapshot _snapshot;

  public RuleCommandHandler(WorldSessionRuleSnapshot initialSnapshot)
  {
    _snapshot = initialSnapshot;
  }

  public WorldSessionRuleSnapshot Snapshot => _snapshot;

  public bool TrySetGameMode(int gameMode)
  {
    if (gameMode is < 0 or > 3)
    {
      return false;
    }

    _snapshot = _snapshot with { GameModeValue = gameMode };
    return true;
  }
}

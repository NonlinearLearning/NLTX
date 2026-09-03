namespace Terraria.Dome.Simulation.Combat.Systems;

public static class PaladinShieldDefensePolicy
{
  public static bool CanDefend(
    bool isActive,
    bool isDead,
    bool hasPaladinShield,
    int team,
    int otherPlayerTeam,
    int life,
    int maximumLife)
  {
    if (maximumLife <= 0 || life < 0 || life > maximumLife)
    {
      return false;
    }

    return isActive && !isDead && hasPaladinShield && team > 0 &&
      team == otherPlayerTeam && life > maximumLife * 0.25f;
  }
}

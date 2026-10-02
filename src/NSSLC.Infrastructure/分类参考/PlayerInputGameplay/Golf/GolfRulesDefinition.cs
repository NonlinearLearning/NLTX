namespace NLTX.PlayerInputGameplay.Golf;

public sealed class GolfRulesDefinition
{
  public const int PointsNeededForLevel1 = 500;

  public const int PointsNeededForLevel2 = 1000;

  public const int PointsNeededForLevel3 = 2000;

  public const int BallReturnPenalty = 1;

  public const int ScoreTimeMax = 3600;

  public const int ScoreDelay = 90;

  public GolfRulesDefinition(float friction, float restitution)
  {
    if (float.IsNaN(friction) || float.IsInfinity(friction) || friction < 0f)
    {
      throw new ArgumentOutOfRangeException(nameof(friction));
    }

    if (float.IsNaN(restitution) || float.IsInfinity(restitution) || restitution < 0f)
    {
      throw new ArgumentOutOfRangeException(nameof(restitution));
    }

    Friction = friction;
    Restitution = restitution;
  }

  public float Friction { get; }

  public float Restitution { get; }
}

namespace Terraria.Dome.Simulation.Player.Components;

public struct PlayerGolfStateComponent
{
  public int ScoreAccumulated;

  public void AddScore(int score)
  {
    if (score < 0)
    {
      throw new System.ArgumentOutOfRangeException(nameof(score));
    }

    ScoreAccumulated = checked(ScoreAccumulated + score);
  }
}

namespace NLTX.PlayerInputGameplay.Runtime;

public sealed class FallingStarEventStateComponent
{
  public bool StarGame { get; private set; }

  public int StarsHit { get; private set; }

  public void Start(int starsHit = 0)
  {
    if (starsHit < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(starsHit));
    }

    StarGame = true;
    StarsHit = starsHit;
  }

  public void RecordHit()
  {
    StarsHit = checked(StarsHit + 1);
  }

  public void Clear()
  {
    StarGame = false;
    StarsHit = 0;
  }
}

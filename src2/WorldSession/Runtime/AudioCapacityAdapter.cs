namespace Terraria.WorldSession.Runtime;

public sealed class AudioCapacityAdapter
{
  public int MaxMusic { get; private set; }

  public void Configure(int maxMusic)
  {
    MaxMusic = maxMusic < 0
      ? throw new ArgumentOutOfRangeException(nameof(maxMusic))
      : maxMusic;
  }
}

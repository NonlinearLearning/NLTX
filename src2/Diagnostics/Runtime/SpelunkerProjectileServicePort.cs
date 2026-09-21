namespace Terraria.NonAuthoritative.Diagnostics;

public sealed class SpelunkerProjectileServicePort
{
  private readonly Action<int> _process;

  public SpelunkerProjectileServicePort(Action<int> process)
  {
    _process = process ?? throw new ArgumentNullException(nameof(process));
  }

  public void Process(int projectileIndex)
  {
    _process.Invoke(projectileIndex);
  }
}

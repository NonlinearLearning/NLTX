namespace Terraria.NonAuthoritative.Diagnostics;

public sealed class RuntimeTickContextComponent
{
  public int ProjectileUpdateLoopIndex { get; private set; } = -1;

  public void BeginProjectile(int loopIndex)
  {
    if (loopIndex < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(loopIndex));
    }

    ProjectileUpdateLoopIndex = loopIndex;
  }

  public void EndFrame()
  {
    ProjectileUpdateLoopIndex = -1;
  }
}

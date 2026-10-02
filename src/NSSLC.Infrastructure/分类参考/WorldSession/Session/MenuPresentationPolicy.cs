namespace Terraria.WorldSession.Session;

public sealed class MenuPresentationPolicy
{
  public bool LockBackgroundChange { get; private set; }

  public void SetBackgroundLock(bool locked)
  {
    LockBackgroundChange = locked;
  }
}

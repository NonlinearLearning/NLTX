namespace NLTX.PlayerInputGameplay.Presentation;

public sealed class MeteorFallPresentationProjection
{
  public bool CanShowMeteorFall { get; private set; }

  public void SetCanShow(bool canShow)
  {
    CanShowMeteorFall = canShow;
  }

  public bool Consume()
  {
    var canShow = CanShowMeteorFall;
    CanShowMeteorFall = false;
    return canShow;
  }
}

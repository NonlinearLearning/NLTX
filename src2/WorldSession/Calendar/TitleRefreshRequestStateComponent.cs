namespace Terraria.WorldSession.Calendar;

public sealed class TitleRefreshRequestStateComponent
{
  public TitleRefreshRequestStateComponent(bool pending = false)
  {
    Pending = pending;
  }

  public bool Pending { get; internal set; }
}

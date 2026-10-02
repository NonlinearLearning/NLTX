namespace NLTX.PlayerInputGameplay.Focus;

public interface IMouseVisibilityPort
{
  void SetVisible(bool visible);
}

public sealed class FocusPresentationAdapter
{
  private readonly IMouseVisibilityPort _mouseVisibility;

  public FocusPresentationAdapter(IMouseVisibilityPort mouseVisibility)
  {
    _mouseVisibility = mouseVisibility ?? throw new ArgumentNullException(nameof(mouseVisibility));
  }

  public void Apply(bool gameplayActive)
  {
    _mouseVisibility.SetVisible(!gameplayActive);
  }
}

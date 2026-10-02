namespace Terraria.Player.Presentation;

public sealed class PlayerFootballPresentationStateComponent
{
  public bool HasFootball { get; private set; }

  public bool IsDrawingFootball { get; private set; }

  internal void ApplyInput(in PlayerFootballPresentationInput input)
  {
    if (input.ResetState)
    {
      Reset();
      return;
    }

    HasFootball = input.HasFootball;
    IsDrawingFootball = input.HasFootball &&
      input.CanDrawFootball &&
      !input.IsFootballItemAnimating;
  }

  internal void Reset()
  {
    HasFootball = false;
    IsDrawingFootball = false;
  }

  public PlayerFootballPresentationSnapshot ToSnapshot()
  {
    return new PlayerFootballPresentationSnapshot(HasFootball, IsDrawingFootball);
  }
}

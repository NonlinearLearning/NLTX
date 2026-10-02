namespace NLTX.PlayerInputGameplay.Presentation;

public enum ReturnFromRejectionMenuAction
{
  None,
  ExitToMenu,
  Retry
}

public sealed class PlayerRejectionPresentationComponent
{
  public ReturnFromRejectionMenuAction ExitAction { get; private set; }

  public string TextToShow { get; private set; } = string.Empty;

  public bool IsVisible => ExitAction != ReturnFromRejectionMenuAction.None || TextToShow.Length > 0;

  public void Show(ReturnFromRejectionMenuAction exitAction, string textToShow)
  {
    if (string.IsNullOrWhiteSpace(textToShow))
    {
      throw new ArgumentException("Rejection text is required.", nameof(textToShow));
    }

    ExitAction = exitAction;
    TextToShow = textToShow;
  }

  public void Clear()
  {
    ExitAction = ReturnFromRejectionMenuAction.None;
    TextToShow = string.Empty;
  }
}

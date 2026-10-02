namespace NLTX.PlayerInputGameplay.Presentation;

public interface IInputTextCaptureTarget
{
  void ClearInputText();
}

public sealed class InputTextCaptureAdapter
{
  public IInputTextCaptureTarget? CurrentTarget { get; private set; }

  public void Capture(IInputTextCaptureTarget target)
  {
    ArgumentNullException.ThrowIfNull(target);
    CurrentTarget = target;
  }

  public void Clear()
  {
    CurrentTarget = null;
  }
}

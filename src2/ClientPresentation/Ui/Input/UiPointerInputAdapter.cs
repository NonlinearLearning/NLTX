using Terraria.ClientPresentation.Ui.Layout;

namespace Terraria.ClientPresentation.Ui.Input;

public sealed class UiPointerInputAdapter
{
  public bool TryRead(
    IMonotonicClock clock,
    UiVector2 pointerPosition,
    bool isVisible,
    out Sample sample)
  {
    ArgumentNullException.ThrowIfNull(clock);
    long timestampMilliseconds = clock.NowMilliseconds;
    if (timestampMilliseconds < 0)
    {
      sample = default;
      return false;
    }

    sample = new Sample(pointerPosition, isVisible, timestampMilliseconds);
    return true;
  }

  public readonly record struct Sample(
    UiVector2 PointerPosition,
    bool IsVisible,
    long TimestampMilliseconds);

  public interface IMonotonicClock
  {
    long NowMilliseconds { get; }
  }
}

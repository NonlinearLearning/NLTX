using Terraria.WorldGeneration.Support;

namespace Terraria.WorldGeneration.Adapters;

public sealed class GenerationFeedbackPort
{
  private readonly bool _displayText;
  private bool _isSpreadActive;
  private int _eventCount;
  private GenerationFeedbackReason _lastReason;
  private int _lastX;
  private int _lastY;
  private int _lastIteration;

  private GenerationFeedbackPort(bool displayText)
  {
    _displayText = displayText;
  }

  public static GenerationFeedbackPort WithText => new(displayText: true);

  public static GenerationFeedbackPort WithoutText => new(displayText: false);

  public bool StopOnFail => true;

  public bool DisplayText => _displayText;

  public GenerationFeedbackSnapshot CreateSnapshot()
  {
    return new GenerationFeedbackSnapshot(
      StopOnFail,
      DisplayText,
      _isSpreadActive,
      _eventCount,
      _lastReason,
      _lastX,
      _lastY,
      _lastIteration);
  }

  internal void BeginSpread()
  {
    _isSpreadActive = true;
    _eventCount = 0;
    _lastReason = GenerationFeedbackReason.None;
    _lastX = 0;
    _lastY = 0;
    _lastIteration = 0;
  }

  internal void Record(
    GenerationFeedbackReason reason,
    int x,
    int y,
    int iteration)
  {
    if (!_isSpreadActive)
    {
      throw new InvalidOperationException("Feedback events require an active spread.");
    }

    checked
    {
      _eventCount++;
    }

    _lastReason = reason;
    _lastX = x;
    _lastY = y;
    _lastIteration = iteration;
  }

  internal void EndSpread()
  {
    _isSpreadActive = false;
  }
}

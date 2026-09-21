using System.Collections.Concurrent;

namespace NLTX.PlayerInputGameplay.Runtime;

public sealed class MainThreadActionQueue
{
  private readonly ConcurrentQueue<Action> _actions = new();

  public int Count => _actions.Count;

  public void Enqueue(Action action)
  {
    ArgumentNullException.ThrowIfNull(action);
    _actions.Enqueue(action);
  }

  public int Drain(int maximumActions = int.MaxValue)
  {
    if (maximumActions < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(maximumActions));
    }

    var executed = 0;
    while (executed < maximumActions && _actions.TryDequeue(out var action))
    {
      action();
      executed++;
    }

    return executed;
  }
}

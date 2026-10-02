namespace Terraria.WorldSession.Definitions;

public sealed class DifficultyCurve
{
  private readonly DifficultyCurveKey[] _keys;

  public DifficultyCurve(IEnumerable<DifficultyCurveKey> keys)
  {
    ArgumentNullException.ThrowIfNull(keys);
    _keys = keys.ToArray();
    if (_keys.Length == 0)
    {
      throw new ArgumentException("A difficulty curve requires at least one key.", nameof(keys));
    }

    for (int i = 1; i < _keys.Length; i++)
    {
      if (_keys[i].Input < _keys[i - 1].Input)
      {
        throw new ArgumentException("Difficulty curve inputs must be ordered.", nameof(keys));
      }
    }
  }

  public IReadOnlyList<DifficultyCurveKey> Keys => _keys;

  public float Sample(float value)
  {
    DifficultyCurveKey first = _keys[0];
    DifficultyCurveKey previous = first;
    DifficultyCurveKey next = first;

    for (int i = 0; i < _keys.Length; i++)
    {
      next = _keys[i];
      if (value <= next.Input)
      {
        break;
      }

      previous = next;
    }

    float inputDelta = next.Input - previous.Input;
    if (inputDelta == 0f)
    {
      return previous.Output;
    }

    return (value - previous.Input) * (next.Output - previous.Output) / inputDelta +
      previous.Output;
  }
}

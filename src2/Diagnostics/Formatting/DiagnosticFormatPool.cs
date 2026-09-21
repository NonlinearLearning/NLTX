using System.Globalization;

namespace Terraria.NonAuthoritative.Diagnostics;

public sealed class DiagnosticFormatPool
{
  private readonly string _format;
  private readonly double _minValue;
  private readonly double _rounding;
  private readonly string[] _strings;
  private readonly string _nullString;

  public DiagnosticFormatPool(
    string format,
    double maxValue,
    double minValue = 0,
    double rounding = 1,
    string? nullString = null)
  {
    if (string.IsNullOrEmpty(format))
    {
      throw new ArgumentException("A format is required.", nameof(format));
    }

    if (maxValue < minValue || rounding <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(maxValue));
    }

    _format = format;
    _minValue = minValue;
    _rounding = rounding;
    int cacheLength = checked((int)((maxValue - minValue) / rounding) + 1);
    _strings = new string[cacheLength];
    _nullString = nullString ??
      string.Format(CultureInfo.InvariantCulture, _format, (object?)null);
  }

  public string NullString => _nullString;

  public string Format(double value)
  {
    int index = (int)Math.Round((value - _minValue) / _rounding);
    if (index < 0 || index >= _strings.Length)
    {
      return string.Format(
        CultureInfo.InvariantCulture,
        _format,
        value);
    }

    string? cached = _strings[index];
    if (cached is not null)
    {
      return cached;
    }

    string formatted = string.Format(
      CultureInfo.InvariantCulture,
      _format,
      value);
    _strings[index] = formatted;
    return formatted;
  }
}

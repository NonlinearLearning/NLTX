using System.Collections;
using System.Globalization;
using System.Text.Json;

namespace Terraria.NonAuthoritative.Persistence;

public sealed class ConfigurationAdapter
{
  private readonly IReadOnlyDictionary<string, object?> _values;

  public ConfigurationAdapter(IReadOnlyDictionary<string, object?> values)
  {
    ArgumentNullException.ThrowIfNull(values);
    _values = new Dictionary<string, object?>(values, StringComparer.Ordinal);
  }

  public bool TryGet<T>(string entry, out T value)
  {
    ArgumentNullException.ThrowIfNull(entry);
    if (_values.TryGetValue(entry, out object? raw) &&
      PreferencesValueConverter.TryConvert(raw, out value))
    {
      return true;
    }

    value = default!;
    return false;
  }

  public T Get<T>(string entry, T defaultValue)
  {
    return TryGet(entry, out T value) ? value : defaultValue;
  }
}

internal static class PreferencesValueConverter
{
  public static bool TryConvert<T>(object? value, out T converted)
  {
    try
    {
      if (value is T exact)
      {
        converted = exact;
        return true;
      }

      Type targetType = Nullable.GetUnderlyingType(typeof(T)) ?? typeof(T);
      if (value is null)
      {
        converted = default!;
        return !targetType.IsValueType || Nullable.GetUnderlyingType(typeof(T)) is not null;
      }

      if (targetType.IsEnum)
      {
        converted = (T)Enum.Parse(targetType, Convert.ToString(value, CultureInfo.InvariantCulture)!, ignoreCase: true);
        return true;
      }

      if (value is JsonElement element)
      {
        converted = element.Deserialize<T>()!;
        return true;
      }

      if (targetType == typeof(string))
      {
        converted = (T)(object)Convert.ToString(value, CultureInfo.InvariantCulture)!;
        return true;
      }

      if (value is IDictionary || value is IEnumerable && value is not string)
      {
        string json = JsonSerializer.Serialize(value);
        converted = JsonSerializer.Deserialize<T>(json)!;
        return true;
      }

      converted = (T)Convert.ChangeType(value, targetType, CultureInfo.InvariantCulture);
      return true;
    }
    catch
    {
      converted = default!;
      return false;
    }
  }
}

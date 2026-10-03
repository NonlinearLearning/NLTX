using System.Collections.Generic;
using System.Reflection;

namespace NSSLC.WorldGeneration.Runtime;

public sealed class IdDictionary {
  private readonly Dictionary<long, string> _names = new();
  public static IdDictionary Create<T, TValue>() {
    var dictionary = new IdDictionary();
    foreach (FieldInfo field in typeof(T).GetFields(BindingFlags.Public | BindingFlags.Static)) {
      if (field.IsLiteral && field.FieldType == typeof(TValue)) {
        dictionary._names[System.Convert.ToInt64(field.GetRawConstantValue())] = field.Name;
      }
    }
    return dictionary;
  }
  public string GetName(int id) => _names.GetValueOrDefault(id, id.ToString());
}

using System;
using System.Collections.Generic;

namespace Terraria.Share.Ai;

/// <summary>
/// The composition-root table that resolves each <see cref="AiAnchor"/> to the registration key
/// implementing it.
/// </summary>
/// <remarks>
/// Declarations name anchors; only the host knows keys. This type is the single place where the
/// two meet, so a declaration never embeds a registration key and the host never embeds an order.
///
/// Binding is total by construction: <see cref="Create"/> rejects a table that leaves an anchor
/// unbound, which is what keeps the anchor set from growing into a list of names nobody filled in.
/// The table is intentionally not a lookup that can miss at use time.
/// </remarks>
public sealed class AiAnchorBindings
{
  private readonly Dictionary<AiAnchor, string> _keys;

  private AiAnchorBindings(Dictionary<AiAnchor, string> keys)
  {
    _keys = keys;
    List<AiAnchor> ordered = [];
    foreach (AiAnchor anchor in Enum.GetValues<AiAnchor>())
    {
      ordered.Add(anchor);
    }

    Anchors = ordered.AsReadOnly();
  }

  /// <summary>Every anchor in declaration order; always the complete enum member set.</summary>
  public IReadOnlyList<AiAnchor> Anchors { get; }

  /// <summary>Creates the table and requires it to bind every anchor exactly once.</summary>
  public static AiAnchorBindings Create(IReadOnlyDictionary<AiAnchor, string> keys)
  {
    ArgumentNullException.ThrowIfNull(keys);
    AiAnchor[] declared = Enum.GetValues<AiAnchor>();
    Dictionary<AiAnchor, string> bound = [];
    foreach (KeyValuePair<AiAnchor, string> pair in keys)
    {
      if (!Enum.IsDefined(pair.Key))
      {
        throw new ArgumentOutOfRangeException(
            nameof(keys), pair.Key, "An AI anchor binding names an undefined anchor.");
      }

      bound.Add(pair.Key, RequireKey(pair.Value, pair.Key));
    }

    List<AiAnchor> missing = [];
    foreach (AiAnchor anchor in declared)
    {
      if (!bound.ContainsKey(anchor))
      {
        missing.Add(anchor);
      }
    }

    if (missing.Count != 0)
    {
      throw new ArgumentException(
          "Every AI anchor must be bound exactly once; unbound: " + string.Join(", ", missing),
          nameof(keys));
    }

    HashSet<string> seenKeys = new(StringComparer.Ordinal);
    foreach (KeyValuePair<AiAnchor, string> pair in bound)
    {
      if (!seenKeys.Add(pair.Value))
      {
        throw new ArgumentException(
            $"AI anchors {pair.Key} and another anchor both bind to '{pair.Value}'.", nameof(keys));
      }
    }

    return new AiAnchorBindings(bound);
  }

  /// <summary>Resolves one anchor to its registration key.</summary>
  public string KeyOf(AiAnchor anchor)
  {
    if (!Enum.IsDefined(anchor))
    {
      throw new ArgumentOutOfRangeException(nameof(anchor));
    }

    return _keys[anchor];
  }

  /// <summary>The registration keys this table binds, in anchor declaration order.</summary>
  public IReadOnlyList<string> Keys()
  {
    List<string> keys = new(_keys.Count);
    foreach (AiAnchor anchor in Anchors)
    {
      keys.Add(_keys[anchor]);
    }

    return keys.AsReadOnly();
  }

  /// <summary>
  /// Requires every bound key to exist in the schedule the host actually built. Called by the
  /// host after registration, so a stale binding fails at startup rather than silently dropping
  /// an ordering constraint.
  /// </summary>
  public void EnsureResolvable(IReadOnlyList<string> registrationKeys)
  {
    ArgumentNullException.ThrowIfNull(registrationKeys);
    List<string> unknown = [];
    foreach (AiAnchor anchor in Anchors)
    {
      string key = _keys[anchor];
      bool found = false;
      for (int i = 0; i < registrationKeys.Count; i++)
      {
        if (string.Equals(registrationKeys[i], key, StringComparison.Ordinal))
        {
          found = true;
          break;
        }
      }

      if (!found)
      {
        unknown.Add($"{anchor}={key}");
      }
    }

    if (unknown.Count != 0)
    {
      throw new InvalidOperationException(
          "AI anchor bindings name registrations that are not in the schedule: " +
          string.Join(", ", unknown));
    }
  }

  private static string RequireKey(string? key, AiAnchor anchor)
  {
    if (string.IsNullOrWhiteSpace(key) || key != key!.Trim())
    {
      throw new ArgumentException(
          $"The registration key bound to AI anchor {anchor} must be non-whitespace text " +
          "without surrounding whitespace.",
          nameof(key));
    }

    return key;
  }
}

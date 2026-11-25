// Adapted from CrashKonijn/GOAP WorldDataBase; see LICENSE.txt and SOURCES.md.
using System;
using System.Collections.Generic;
using Terraria.Share.Ai.Goap.Core;

namespace Terraria.Share.Ai.Goap.Runtime;

/// <summary>Per-agent observations. Sensor and graph keys use the same canonical name.</summary>
public sealed class WorldData : IWorldData {
  private readonly Dictionary<string, int> _states = new(StringComparer.Ordinal);

  public void SetState<TKey>(int value) where TKey : IWorldKey, new() {
    SetState(new TKey(), value);
  }

  public void SetState(IWorldKey key, int value) {
    ArgumentNullException.ThrowIfNull(key);
    _states[key.Name] = value;
  }

  public void Clear() {
    _states.Clear();
  }

  public bool IsTrue(ICondition condition) {
    ArgumentNullException.ThrowIfNull(condition);
    if (!_states.TryGetValue(condition.WorldKey.Name, out int state)) {
      return false;
    }

    int amount;
    if (condition is IValueCondition valueCondition) {
      amount = valueCondition.Amount;
    } else if (condition is IReferenceCondition referenceCondition) {
      if (!_states.TryGetValue(referenceCondition.ValueKey.Name, out amount)) {
        return false;
      }
    } else {
      throw new ArgumentException("Unsupported condition type.", nameof(condition));
    }

    return condition.Comparison switch {
      Comparison.GreaterThan => state > amount,
      Comparison.GreaterThanOrEqual => state >= amount,
      Comparison.SmallerThan => state < amount,
      Comparison.SmallerThanOrEqual => state <= amount,
      _ => throw new ArgumentOutOfRangeException(nameof(condition))
    };
  }
}

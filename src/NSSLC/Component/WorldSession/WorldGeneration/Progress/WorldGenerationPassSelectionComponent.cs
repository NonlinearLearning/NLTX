using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Terraria.WorldGeneration.Components;

/// <summary>
/// Stores the selected enabled state for each registered generation pass.
/// </summary>
public sealed class WorldGenerationPassSelectionComponent
{
  public WorldGenerationPassSelectionComponent(
    IReadOnlyDictionary<string, bool> passEnabledById)
  {
    ArgumentNullException.ThrowIfNull(passEnabledById);

    Dictionary<string, bool> copy = new(StringComparer.Ordinal);
    foreach (KeyValuePair<string, bool> selection in passEnabledById)
    {
      ArgumentException.ThrowIfNullOrWhiteSpace(selection.Key);
      if (!copy.TryAdd(selection.Key, selection.Value))
      {
        throw new ArgumentException(
          "Pass identifiers must be unique.",
          nameof(passEnabledById));
      }
    }

    PassEnabledById = new ReadOnlyDictionary<string, bool>(copy);
  }

  public IReadOnlyDictionary<string, bool> PassEnabledById { get; }
}

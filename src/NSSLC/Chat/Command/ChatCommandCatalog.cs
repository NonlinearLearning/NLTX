using System;
using System.Collections.Generic;

namespace Terraria.Unshared.Chat;

/// <summary>Immutable-after-sealing directory of chat commands.</summary>
public sealed class ChatCommandCatalog
{
  private readonly Dictionary<string, ChatCommand> _commands =
      new(StringComparer.OrdinalIgnoreCase);
  private bool _sealed;

  public int Count => _commands.Count;

  public bool IsSealed => _sealed;

  public void Register(ChatCommand definition)
  {
    ArgumentNullException.ThrowIfNull(definition);
    if (_sealed)
    {
      throw new InvalidOperationException("The chat command catalog is sealed.");
    }

    if (!definition.IsValid)
    {
      throw new ArgumentException(
          $"Chat command '{definition.Name}' is invalid.", nameof(definition));
    }

    if (!_commands.TryAdd(definition.Name, definition))
    {
      throw new InvalidOperationException(
          $"Chat command '{definition.Name}' is already registered.");
    }
  }

  public void Seal()
  {
    _sealed = true;
  }

  public bool TryGet(string name, out ChatCommand definition)
    => _commands.TryGetValue(name, out definition!);

  /// <summary>Registered command names in ordinal order, for help output and diagnostics.</summary>
  public string[] Names()
  {
    string[] names = [.. _commands.Keys];
    Array.Sort(names, StringComparer.OrdinalIgnoreCase);
    return names;
  }
}

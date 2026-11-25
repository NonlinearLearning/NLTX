using System;

namespace Terraria.Unshared.Chat;

/// <summary>Pure helpers for the server's chat commands.</summary>
/// <remarks>
/// Version4 resolves commands through localized <c>ChatCommandAttribute</c> names; the server
/// normalizes the name to lower case and formats its own replies. <see cref="Roll"/> is
/// deterministic from the tick and the speaker so a replayed tick stream produces the same result,
/// matching the server's authoritative-randomness rule elsewhere.
/// </remarks>
public static class ChatCommandRules
{
  /// <summary>Command used when a line has no prefix.</summary>
  public const string DefaultCommandName = "say";

  public static string FormatHelp(ChatCommandCatalog catalog)
  {
    ArgumentNullException.ThrowIfNull(catalog);
    return $"Commands: {string.Join(", ", Array.ConvertAll(catalog.Names(), static name => "/" + name))}";
  }

  public static string FormatPlayerCount(int count) => $"Players online: {count}";

  /// <summary>Deterministic 1–100 roll from the tick and a speaker salt.</summary>
  public static int Roll(long tick, int salt)
  {
    ulong state = unchecked((ulong)tick * 0x9E3779B97F4A7C15UL) ^ (ulong)(uint)salt;
    state ^= state >> 30;
    state *= 0xBF58476D1CE4E5B9UL;
    state ^= state >> 27;
    state *= 0x94D049BB133111EBUL;
    state ^= state >> 31;
    return 1 + (int)(state % 100);
  }
}

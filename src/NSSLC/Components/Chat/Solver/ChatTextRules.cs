using System;
using System.Text;

namespace Terraria.Chat;

/// <summary>Text rules for one chat line, extracted from the reference chat pipeline.</summary>
/// <remarks>
/// Version4 stores the typed line in <c>ChatMessage.Text</c> and sends it as one protocol string.
/// The server normalizes it first: chat renders on a single line, so line breaks and tabs collapse
/// into single spaces and other control characters are dropped. The length bound is a server policy
/// (the reference bounds the client editor, not the server), chosen to keep one line inside one
/// text-module packet.
/// </remarks>
public static class ChatTextRules
{
  /// <summary>Server policy: maximum characters of one chat line.</summary>
  public const int MaximumLength = 512;

  /// <summary>Server policy: maximum characters of one command name.</summary>
  public const int MaximumCommandNameLength = 32;

  /// <summary>The reference's command prefix (<c>/</c>).</summary>
  public const char CommandPrefix = '/';

  /// <summary>Normalizes a typed line; rejects empty and overlong input.</summary>
  public static bool TrySanitize(string? text, out string sanitized)
  {
    sanitized = string.Empty;
    if (string.IsNullOrEmpty(text))
    {
      return false;
    }

    StringBuilder builder = new(text.Length);
    bool pendingSpace = false;
    for (int index = 0; index < text.Length; index++)
    {
      char value = text[index];
      if (char.IsWhiteSpace(value))
      {
        pendingSpace = builder.Length > 0;
        continue;
      }

      if (char.IsControl(value))
      {
        continue;
      }

      if (pendingSpace)
      {
        builder.Append(' ');
        pendingSpace = false;
      }

      builder.Append(value);
    }

    if (builder.Length == 0 || builder.Length > MaximumLength)
    {
      return false;
    }

    sanitized = builder.ToString();
    return true;
  }

  /// <summary>Splits a sanitized line into a command name and its arguments.</summary>
  public static bool TryParseCommand(string text, out string name, out string arguments)
  {
    name = string.Empty;
    arguments = string.Empty;
    if (text.Length < 2 || text[0] != CommandPrefix)
    {
      return false;
    }

    int end = text.IndexOf(' ', 1);
    string rawName = end < 0 ? text[1..] : text[1..end];
    if (rawName.Length == 0 || rawName.Length > MaximumCommandNameLength)
    {
      return false;
    }

    name = rawName.ToLowerInvariant();
    arguments = end < 0 ? string.Empty : text[(end + 1)..].Trim();
    return true;
  }
}

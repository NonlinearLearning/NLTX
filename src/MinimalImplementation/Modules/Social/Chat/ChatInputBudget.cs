using System.Text;

namespace MinimalImplementation.Server.Modules.Social.Chat;

/// <summary>
/// Bounds the decoded Version4 ChatMessage before command dispatch. The outer frame is already
/// bounded by P01; these smaller limits prevent a valid frame from becoming an unbounded command
/// or text allocation at the domain boundary.
/// </summary>
internal static class ChatInputBudget
{
  internal const int MaximumCommandIdUtf8Bytes = 64;
  internal const int MaximumTextUtf8Bytes = 4_096;

  private static readonly UTF8Encoding StrictUtf8 = new(false, true);

  internal static bool TryValidate(ChatMessage message, out string? error)
  {
    ArgumentNullException.ThrowIfNull(message);
    if (string.IsNullOrWhiteSpace(message.CommandId))
    {
      error = "A chat message requires a non-empty command id.";
      return false;
    }

    if (!TryGetUtf8ByteCount(message.CommandId, out int commandIdBytes)
        || commandIdBytes > MaximumCommandIdUtf8Bytes)
    {
      error = $"The chat command id exceeds the {MaximumCommandIdUtf8Bytes}-byte UTF-8 budget.";
      return false;
    }

    if (message.Text is null
        || !TryGetUtf8ByteCount(message.Text, out int textBytes)
        || textBytes > MaximumTextUtf8Bytes)
    {
      error = $"The chat text exceeds the {MaximumTextUtf8Bytes}-byte UTF-8 budget.";
      return false;
    }

    error = null;
    return true;
  }

  private static bool TryGetUtf8ByteCount(string value, out int byteCount)
  {
    try
    {
      byteCount = StrictUtf8.GetByteCount(value);
      return true;
    }
    catch (EncoderFallbackException)
    {
      byteCount = 0;
      return false;
    }
  }
}

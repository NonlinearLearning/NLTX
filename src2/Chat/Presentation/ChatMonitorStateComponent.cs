using System.Collections.ObjectModel;

namespace Terraria.Chat.Presentation;

public sealed class ChatMonitorStateComponent
{
  public const int DefaultMaxMessages = 500;

  private readonly ReadOnlyCollection<string> _messageTexts;

  public ChatMonitorStateComponent(
    IReadOnlyList<string>? messageTexts = null,
    int maxMessages = DefaultMaxMessages,
    int lastChatWidthLimit = -1)
  {
    if (maxMessages <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(maxMessages));
    }

    if (lastChatWidthLimit < -1)
    {
      throw new ArgumentOutOfRangeException(nameof(lastChatWidthLimit));
    }

    if (messageTexts is not null && messageTexts.Count > maxMessages)
    {
      throw new ArgumentException(
        "Message text snapshots cannot exceed the monitor capacity.",
        nameof(messageTexts));
    }

    var copiedMessageTexts = messageTexts is null
      ? Array.Empty<string>()
      : messageTexts.ToArray();

    for (int index = 0; index < copiedMessageTexts.Length; index++)
    {
      if (copiedMessageTexts[index] is null)
      {
        throw new ArgumentException(
          "Message text snapshots cannot contain null values.",
          nameof(messageTexts));
      }
    }

    MaxMessages = maxMessages;
    LastChatWidthLimit = lastChatWidthLimit;
    _messageTexts = Array.AsReadOnly(copiedMessageTexts);
  }

  public int MaxMessages { get; }

  public IReadOnlyList<string> MessageTexts => _messageTexts;

  public int MessageCount => _messageTexts.Count;

  public int LastChatWidthLimit { get; }
}

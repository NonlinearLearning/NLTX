using System.Collections.Generic;

namespace Terraria.Chat;

/// <summary>Tick-scoped queue of chat lines ready for delivery.</summary>
public sealed class ChatBroadcastQueueComponent
{
  private readonly List<ChatBroadcast> _pending = [];

  public int Count => _pending.Count;

  public void Enqueue(in ChatBroadcast broadcast) => _pending.Add(broadcast);

  public IReadOnlyList<ChatBroadcast> Drain()
  {
    if (_pending.Count == 0)
    {
      return [];
    }

    ChatBroadcast[] broadcasts = _pending.ToArray();
    _pending.Clear();
    return broadcasts;
  }

  public void Clear() => _pending.Clear();
}

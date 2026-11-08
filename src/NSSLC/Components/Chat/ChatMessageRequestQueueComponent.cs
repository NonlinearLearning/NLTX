using System.Collections.Generic;

namespace Terraria.Chat;

/// <summary>Tick-scoped queue of chat lines typed by entities that can speak.</summary>
public sealed class ChatMessageRequestQueueComponent
{
  private readonly List<ChatMessageRequest> _pending = [];

  public int Count => _pending.Count;

  public void Enqueue(in ChatMessageRequest request) => _pending.Add(request);

  public IReadOnlyList<ChatMessageRequest> Drain()
  {
    if (_pending.Count == 0)
    {
      return [];
    }

    ChatMessageRequest[] requests = _pending.ToArray();
    _pending.Clear();
    return requests;
  }

  public void Clear() => _pending.Clear();
}

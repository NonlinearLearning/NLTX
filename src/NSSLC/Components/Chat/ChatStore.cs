using System;
using System.Collections.Generic;

namespace Terraria.Chat;

/// <summary>Short server-side history of accepted chat lines.</summary>
/// <remarks>
/// Version4 keeps no server chat history; it only forwards lines. The server keeps a bounded window
/// so a joining client or a moderator can read recent activity. The window is a server policy and
/// deliberately small: chat text is client-authored and unbounded history is a memory hazard.
/// </remarks>
public sealed class ChatStore
{
  /// <summary>Maximum number of retained lines (server policy).</summary>
  public const int Capacity = 200;

  private readonly object _gate = new();
  private readonly Queue<ChatMessage> _history = new();
  private long _revision;

  public int Count
  {
    get
    {
      lock (_gate)
      {
        return _history.Count;
      }
    }
  }

  public long Revision
  {
    get
    {
      lock (_gate)
      {
        return _revision;
      }
    }
  }

  /// <summary>Appends one line, dropping the oldest when the window is full.</summary>
  public void Record(in ChatMessage message)
  {
    if (!message.IsValid)
    {
      throw new ArgumentException("A chat history entry requires text and a tick.", nameof(message));
    }

    lock (_gate)
    {
      _history.Enqueue(message);
      while (_history.Count > Capacity)
      {
        _history.Dequeue();
      }

      _revision++;
    }
  }

  /// <summary>Snapshot of the retained lines, oldest first.</summary>
  public ChatMessage[] Snapshot()
  {
    lock (_gate)
    {
      return [.. _history];
    }
  }

  public void Clear()
  {
    lock (_gate)
    {
      _history.Clear();
      _revision++;
    }
  }
}

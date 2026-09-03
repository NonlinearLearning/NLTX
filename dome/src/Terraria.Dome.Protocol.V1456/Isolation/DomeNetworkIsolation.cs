using System;
using System.Collections.Generic;
using System.Threading;
using Terraria.Dome.Protocol.V1456.Protocol;

namespace Terraria.Dome.Protocol.V1456.Isolation;

public sealed class DomeNetworkIsolation
{
  private readonly object _syncRoot = new();
  private readonly int _maxInboundCount;
  private readonly int _maxOutboundCount;
  private List<NetworkInboundEnvelope> _inbound = new();
  private List<NetworkOutboundEnvelope> _outbound = new();
  private readonly List<TerrariaMessageId> _unsupportedMessageIds = new();
  private long _nextSequence;
  private bool _isClosed;
  private int _rejectedInboundCount;
  private int _rejectedOutboundCount;

  public DomeNetworkIsolation(int maxInboundCount, int maxOutboundCount)
  {
    if (maxInboundCount < 1)
    {
      throw new ArgumentOutOfRangeException(nameof(maxInboundCount));
    }

    if (maxOutboundCount < 1)
    {
      throw new ArgumentOutOfRangeException(nameof(maxOutboundCount));
    }

    _maxInboundCount = maxInboundCount;
    _maxOutboundCount = maxOutboundCount;
  }

  public int RejectedInboundCount => Volatile.Read(ref _rejectedInboundCount);

  public int RejectedOutboundCount => Volatile.Read(ref _rejectedOutboundCount);

  public IReadOnlyList<TerrariaMessageId> UnsupportedMessageIds
  {
    get
    {
      lock (_syncRoot)
      {
        return _unsupportedMessageIds.ToArray();
      }
    }
  }

  public bool IsClosed
  {
    get
    {
      lock (_syncRoot)
      {
        return _isClosed;
      }
    }
  }

  public bool EnqueueInbound(NetworkInboundEnvelope envelope)
  {
    ArgumentNullException.ThrowIfNull(envelope);
    lock (_syncRoot)
    {
      if (_isClosed || _inbound.Count >= _maxInboundCount)
      {
        _rejectedInboundCount++;
        return false;
      }

      envelope.Sequence = ++_nextSequence;
      _inbound.Add(envelope);
      return true;
    }
  }

  public int Update(IProtocolCommandSink commandSink)
  {
    ArgumentNullException.ThrowIfNull(commandSink);
    List<NetworkInboundEnvelope> pending;
    lock (_syncRoot)
    {
      pending = _inbound;
      _inbound = new List<NetworkInboundEnvelope>();
    }

    int processedCount = 0;
    for (int index = 0; index < pending.Count; index++)
    {
      NetworkInboundEnvelope envelope = pending[index];
      ProtocolCommandResult result = commandSink.Accept(envelope);
      if (result != ProtocolCommandResult.Accepted)
      {
        Interlocked.Increment(ref _rejectedInboundCount);
        if (result == ProtocolCommandResult.Unsupported)
        {
          lock (_syncRoot)
          {
            if (_unsupportedMessageIds.Count < _maxInboundCount)
            {
              _unsupportedMessageIds.Add(envelope.MessageId);
            }
          }
        }
      }

      processedCount++;
    }

    return processedCount;
  }

  public bool EnqueueOutbound(NetworkOutboundEnvelope envelope)
  {
    ArgumentNullException.ThrowIfNull(envelope);
    lock (_syncRoot)
    {
      if (_isClosed || _outbound.Count >= _maxOutboundCount)
      {
        _rejectedOutboundCount++;
        return false;
      }

      envelope.Sequence = ++_nextSequence;
      _outbound.Add(envelope);
      return true;
    }
  }

  public IReadOnlyList<NetworkOutboundEnvelope> ReadOutbound()
  {
    lock (_syncRoot)
    {
      return _outbound.ToArray();
    }
  }

  public void ClearOutbound()
  {
    lock (_syncRoot)
    {
      _outbound.Clear();
    }
  }

  public void Close()
  {
    lock (_syncRoot)
    {
      _isClosed = true;
      _inbound.Clear();
      _outbound.Clear();
    }
  }
}

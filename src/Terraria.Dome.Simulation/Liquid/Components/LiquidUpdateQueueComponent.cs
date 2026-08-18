using System;
using System.Collections.Generic;

namespace Terraria.Dome.Simulation.Liquid.Components;

public readonly record struct LiquidUpdateNode(int X, int Y, long Sequence);

public sealed class LiquidUpdateQueueComponent
{
  private readonly HashSet<(int X, int Y)> _coordinates = new();
  private readonly List<LiquidUpdateNode> _nodes = new();
  private readonly Dictionary<(int X, int Y), int> _retryCounts = new();
  private readonly Dictionary<(int X, int Y), LiquidSourceComponent> _sources = new();

  public LiquidUpdateQueueComponent(int maximumLength)
  {
    if (maximumLength <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(maximumLength));
    }

    MaximumLength = maximumLength;
  }

  public int Count => _nodes.Count;
  public int MaximumLength { get; }
  public IReadOnlyList<LiquidUpdateNode> PendingNodes
  {
    get
    {
      List<LiquidUpdateNode> pending = [.. _nodes];
      pending.Sort(LiquidUpdateNodeComparer.Instance);
      return pending;
    }
  }

  public int GetRetryCount(int x, int y)
  {
    return _retryCounts.TryGetValue((x, y), out int count) ? count : 0;
  }

  public bool TryEnqueue(int x, int y, long sequence)
  {
    if (sequence < 0 || _nodes.Count >= MaximumLength || !_coordinates.Add((x, y)))
    {
      return false;
    }

    _nodes.Add(new LiquidUpdateNode(x, y, sequence));
    return true;
  }

  public bool TryEnqueue(LiquidSourceComponent source)
  {
    if (!TryEnqueue(source.X, source.Y, source.Sequence))
    {
      return false;
    }

    _sources[(source.X, source.Y)] = source;
    return true;
  }

  public bool TryRequeue(LiquidUpdateNode node, int maximumRetries)
  {
    if (maximumRetries <= 0 || GetRetryCount(node.X, node.Y) >= maximumRetries)
    {
      return false;
    }

    _retryCounts[(node.X, node.Y)] = GetRetryCount(node.X, node.Y) + 1;
    return TryEnqueue(node.X, node.Y, node.Sequence);
  }

  public void ResetRetryCount(int x, int y)
  {
    _retryCounts.Remove((x, y));
  }

  public bool TryGetSource(LiquidUpdateNode node, out LiquidSourceComponent source)
  {
    return _sources.TryGetValue((node.X, node.Y), out source);
  }

  public IReadOnlyList<LiquidUpdateNode> Drain(int maximumCount)
  {
    if (maximumCount <= 0)
    {
      return [];
    }

    _nodes.Sort(LiquidUpdateNodeComparer.Instance);
    int count = Math.Min(maximumCount, _nodes.Count);
    List<LiquidUpdateNode> drained = _nodes.GetRange(0, count);
    _nodes.RemoveRange(0, count);
    for (int index = 0; index < drained.Count; index++)
    {
      LiquidUpdateNode node = drained[index];
      _coordinates.Remove((node.X, node.Y));
    }

    return drained;
  }

  public void Clear()
  {
    _nodes.Clear();
    _coordinates.Clear();
    _sources.Clear();
    _retryCounts.Clear();
  }

  public void RemoveSource(LiquidUpdateNode node)
  {
    _sources.Remove((node.X, node.Y));
  }

  public void Cancel(int x, int y)
  {
    _coordinates.Remove((x, y));
    _sources.Remove((x, y));
    _retryCounts.Remove((x, y));
    _ = _nodes.RemoveAll(node => node.X == x && node.Y == y);
  }

  private sealed class LiquidUpdateNodeComparer : IComparer<LiquidUpdateNode>
  {
    public static readonly LiquidUpdateNodeComparer Instance = new();

    public int Compare(LiquidUpdateNode first, LiquidUpdateNode second)
    {
      int sequence = first.Sequence.CompareTo(second.Sequence);
      if (sequence != 0)
      {
        return sequence;
      }

      int x = first.X.CompareTo(second.X);
      return x != 0 ? x : first.Y.CompareTo(second.Y);
    }
  }
}

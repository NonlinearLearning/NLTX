using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Terraria.Relationships;

namespace Terraria.Combat;

/// <summary>Explicit per-tick buffer shared by attack adapters and the Combat commit system.</summary>
public sealed class CombatDamageRequestBuffer
{
  private readonly List<DamageRequest> _pending = [];
  private readonly List<CombatDamageResult> _results = [];
  private long _resultsTick = -1;

  public int PendingCount => _pending.Count;

  public void Enqueue(in DamageRequest request)
  {
    _pending.Add(request);
  }

  public IReadOnlyList<DamageRequest> DrainPending()
  {
    DamageRequest[] requests = _pending.ToArray();
    _pending.Clear();
    return Array.AsReadOnly(requests);
  }

  public void Publish(in DamageRequest request, in DamageProcessResult result)
  {
    if (!request.Source.IsValid)
    {
      throw new ArgumentException("A result source handle is required.", nameof(request));
    }

    _results.Add(new CombatDamageResult(request, result));
  }

  public void BeginResultTick(long tick)
  {
    if (tick < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(tick));
    }

    if (tick == _resultsTick)
    {
      return;
    }

    _results.Clear();
    _resultsTick = tick;
  }

  public IReadOnlyList<CombatDamageResult> ConsumeResults(EntityRuntimeHandle source)
  {
    if (!source.IsValid)
    {
      return Array.Empty<CombatDamageResult>();
    }

    List<CombatDamageResult> matches = [];
    for (int index = _results.Count - 1; index >= 0; index--)
    {
      if (!_results[index].Request.Source.Matches(source))
      {
        continue;
      }

      matches.Add(_results[index]);
      _results.RemoveAt(index);
    }

    matches.Reverse();
    return new ReadOnlyCollection<CombatDamageResult>(matches);
  }

  public IReadOnlyList<CombatDamageResult> ConsumeResults(
      EntityRuntimeHandle source,
      long requestId)
  {
    if (!source.IsValid || requestId < 0)
    {
      return Array.Empty<CombatDamageResult>();
    }

    for (int index = 0; index < _results.Count; index++)
    {
      CombatDamageResult candidate = _results[index];
      if (candidate.Request.Source.Matches(source) &&
          candidate.Request.RequestId == requestId)
      {
        _results.RemoveAt(index);
        return new[] { candidate };
      }
    }

    return Array.Empty<CombatDamageResult>();
  }

  public IReadOnlyList<CombatDamageResult> ConsumeResults(
      EntityRuntimeHandle source,
      IReadOnlyCollection<long> requestIds)
  {
    ArgumentNullException.ThrowIfNull(requestIds);
    if (!source.IsValid || requestIds.Count == 0)
    {
      return Array.Empty<CombatDamageResult>();
    }

    List<CombatDamageResult> matches = [];
    for (int index = _results.Count - 1; index >= 0; index--)
    {
      CombatDamageResult candidate = _results[index];
      if (candidate.Request.Source.Matches(source) &&
          requestIds.Contains(candidate.Request.RequestId))
      {
        matches.Add(candidate);
        _results.RemoveAt(index);
      }
    }

    matches.Reverse();
    return new ReadOnlyCollection<CombatDamageResult>(matches);
  }

  public void Clear()
  {
    _pending.Clear();
    _results.Clear();
    _resultsTick = -1;
  }
}

/// <summary>Associates a processed result with the source that submitted it.</summary>
public readonly record struct CombatDamageResult(
    DamageRequest Request,
    DamageProcessResult Result)
{
  public EntityRuntimeHandle Source => Request.Source;

  public long RequestId => Request.RequestId;
}

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Terraria.StatusEffects;

/// <summary>
/// 保存主体当前持有的共享 Buff 实例；规则由 BuffSystem 维护。
/// </summary>
public sealed class BuffListComponent
{
  private readonly List<ActiveBuff> _entries;
  private readonly ReadOnlyCollection<ActiveBuff> _readOnlyEntries;

  public BuffListComponent() : this(Array.Empty<ActiveBuff>())
  {
  }

  public BuffListComponent(IEnumerable<BuffEffectId> effects) : this(
      effects.Select((effect, index) => new ActiveBuff(
          effect,
          new Terraria.Combat.BuffId(effect.Value),
          index + 1,
          -1,
          1)))
  {
  }

  public BuffListComponent(IEnumerable<ActiveBuff> entries)
  {
    ArgumentNullException.ThrowIfNull(entries);
    _entries = new List<ActiveBuff>(entries);
    _readOnlyEntries = _entries.AsReadOnly();
  }

  public IReadOnlyList<ActiveBuff> Entries => _readOnlyEntries;

  public IReadOnlyList<BuffEffectId> Effects
  {
    get
    {
      BuffEffectId[] effects = new BuffEffectId[_entries.Count];
      for (int index = 0; index < _entries.Count; index++)
      {
        effects[index] = _entries[index].Effect;
      }

      return Array.AsReadOnly(effects);
    }
  }

  internal bool TryGet(BuffEffectId effect, long sourceId, out ActiveBuff activeBuff)
  {
    foreach (ActiveBuff candidate in _entries)
    {
      if (candidate.Effect == effect && candidate.SourceId == sourceId)
      {
        activeBuff = candidate;
        return true;
      }
    }

    activeBuff = default;
    return false;
  }

  internal void AddOrReplace(ActiveBuff activeBuff)
  {
    for (int index = 0; index < _entries.Count; index++)
    {
      ActiveBuff candidate = _entries[index];
      if (candidate.Effect == activeBuff.Effect && candidate.SourceId == activeBuff.SourceId)
      {
        _entries[index] = activeBuff;
        return;
      }
    }

    _entries.Add(activeBuff);
  }

  internal IReadOnlyList<ActiveBuff> AdvanceOneTick()
  {
    List<ActiveBuff> expired = [];
    for (int index = _entries.Count - 1; index >= 0; index--)
    {
      ActiveBuff activeBuff = _entries[index];
      if (activeBuff.IsPermanent)
      {
        continue;
      }

      int remainingTicks = activeBuff.RemainingTicks - 1;
      if (remainingTicks <= 0)
      {
        expired.Add(activeBuff);
        _entries.RemoveAt(index);
      }
      else
      {
        _entries[index] = activeBuff.WithDuration(remainingTicks);
      }
    }

    return expired.AsReadOnly();
  }
}


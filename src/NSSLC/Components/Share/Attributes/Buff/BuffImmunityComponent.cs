using System;
using System.Collections.Frozen;
using System.Collections.Generic;

namespace Terraria.Attributes;

/// <summary>
/// 保存按状态效果定义编号索引的 Buff 与 Debuff 免疫表。
/// </summary>
public readonly struct BuffImmunityComponent {
  private static readonly IReadOnlySet<BuffId> _emptyBuffIds =
      Array.Empty<BuffId>().ToFrozenSet();
  private readonly FrozenSet<BuffId>? _buffIds;

  public IReadOnlySet<BuffId> BuffIds => _buffIds ?? _emptyBuffIds;

  public BuffImmunityComponent(IEnumerable<BuffId> buffIds) {
    ArgumentNullException.ThrowIfNull(buffIds);
    _buffIds = buffIds.ToFrozenSet();
  }

  public bool Contains(BuffId buffId) {
    return BuffIds.Contains(buffId);
  }
}


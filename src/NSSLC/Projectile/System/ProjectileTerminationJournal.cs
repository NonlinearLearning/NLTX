using System;
using System.Collections.Generic;
using Terraria.Share.Relationships;

namespace Terraria.Unshared.Projectiles;

/// <summary>Remembers which projectile handles already reached the terminated state.</summary>
/// <remarks>
/// 归属是投射物的生命周期，不是共享的句柄注册表：实体存活时"已终止"由
/// <see cref="ProjectileDispositionStateComponent" /> 承载，实体销毁后组件随之消失，
/// 只剩这份历史能把「已终止」与「陈旧句柄」区分开——《ProjectileLifecycleSystem》据此
/// 分别返回 <see cref="ProjectileLifecycleStatus.AlreadyTerminated" /> 与
/// <see cref="ProjectileLifecycleStatus.StaleReference" />。
/// <para>
/// 共享 <see cref="Terraria.Share.Entities.EntityRuntimeRegistry" /> 只保存代际，不保存终止历史；
/// 本类型是投射物域自己的扩展，因此不挂在注册表上，注册表也不再背这份状态。
/// </para>
/// <para>
/// 边界与会话一致：只接受同一世界作用域的句柄；<see cref="Clear" /> 用于整世界回收。
/// </para>
/// </remarks>
public sealed class ProjectileTerminationJournal
{
  private readonly HashSet<EntityRuntimeHandle> _terminated = [];
  private readonly long _worldScope;

  /// <summary>Creates a journal bound to one world scope.</summary>
  /// <param name="worldScope">World scope the recorded handles must belong to.</param>
  public ProjectileTerminationJournal(long worldScope)
  {
    _worldScope = worldScope;
  }

  /// <summary>World scope the recorded handles must belong to.</summary>
  public long WorldScope => _worldScope;

  /// <summary>Number of terminated handles still remembered.</summary>
  public int Count => _terminated.Count;

  /// <summary>Whether <paramref name="handle" /> was already recorded as terminated.</summary>
  public bool Contains(EntityRuntimeHandle handle)
  {
    return _terminated.Contains(handle);
  }

  /// <summary>Records one terminated handle.</summary>
  /// <exception cref="ArgumentException">
  /// <paramref name="handle" /> is invalid or belongs to another world scope.
  /// </exception>
  public void Record(EntityRuntimeHandle handle)
  {
    if (!handle.IsValid || handle.WorldScope != _worldScope)
    {
      throw new ArgumentException("The runtime handle belongs to another world.", nameof(handle));
    }

    _terminated.Add(handle);
  }

  /// <summary>Forgets every recorded handle; used when the whole world is released.</summary>
  public void Clear()
  {
    _terminated.Clear();
  }
}

using System;
using System.Collections.Generic;
using ArchWorld = Arch.Core.World;
using Terraria.Relationships;
using Terraria.Session;

namespace Terraria.Network;

/// <summary>Exposes the session roster to gameplay domains through the read-only port.</summary>
/// <remarks>
/// 把 <see cref="SessionPlayerRoster"/> 的静态查询适配成 <see cref="ISessionPlayerSource"/>，
/// 让 Terraria.Components 的系统在不引用网络程序集的前提下读到"谁在线"。适配器本身不写状态。
/// </remarks>
public sealed class SessionPlayerSource : ISessionPlayerSource
{
  private readonly ArchWorld _world;
  private readonly IEntityRuntimeResolver _resolver;

  public SessionPlayerSource(ArchWorld world, IEntityRuntimeResolver resolver)
  {
    _world = world ?? throw new ArgumentNullException(nameof(world));
    _resolver = resolver ?? throw new ArgumentNullException(nameof(resolver));
  }

  public void Collect(List<SessionPlayer> destination)
  {
    SessionPlayerRoster.Collect(_world, _resolver, destination);
  }

  public SessionPlayer[] Snapshot()
  {
    return SessionPlayerRoster.Snapshot(_world, _resolver);
  }
}

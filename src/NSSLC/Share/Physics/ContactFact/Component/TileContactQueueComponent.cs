using System;
using System.Collections.Generic;

namespace Terraria.Share.Physics;

/// <summary>本 tick 捕获的瓦片接触事实队列。</summary>
/// <remarks>
/// 与 <see cref="PhysicsContactQueueComponent" /> 同形：生产者 <see cref="BeginCapture" /> 后
/// <see cref="Add" />，消费者 <see cref="Drain" /> 取走并清空。挂在专职的世界级实体上，
/// 而不是由宿主注入一个可变缓冲，读写双方才能看到同一份数据。
/// </remarks>
public sealed class TileContactQueueComponent
{
  private readonly List<TileContactFact> _contacts = [];
  private long _tick = -1;
  private SpatialWorldRevision _revision;

  public int Count => _contacts.Count;

  public long Tick => _tick;

  public SpatialWorldRevision Revision => _revision;

  public void BeginCapture(long tick, SpatialWorldRevision revision)
  {
    if (tick < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(tick));
    }

    if (!revision.IsValid)
    {
      throw new ArgumentException("A valid spatial revision is required.", nameof(revision));
    }

    _contacts.Clear();
    _tick = tick;
    _revision = revision;
  }

  public void Add(in TileContactFact contact)
  {
    if (_tick < 0 || contact.Tick != _tick || contact.Revision != _revision)
    {
      throw new InvalidOperationException(
          "A tile contact fact must belong to the active tile capture frame.");
    }

    _contacts.Add(contact);
  }

  public IReadOnlyList<TileContactFact> Drain()
  {
    TileContactFact[] contacts = _contacts.ToArray();
    _contacts.Clear();
    return Array.AsReadOnly(contacts);
  }

  public void Clear()
  {
    _contacts.Clear();
    _tick = -1;
    _revision = default;
  }
}

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Terraria.Share.Relationships;

namespace Terraria.Share.Physics;

/// <summary>
/// Tick-scoped queue of geometric contacts captured during one simulation tick.
/// </summary>
/// <remarks>
/// The queue lives on a dedicated world-scoped entity instead of being handed to systems by the
/// host, so capture and consumption read the same component rather than sharing an injected
/// object. Producers call <see cref="BeginCapture"/> then <see cref="Add"/>; consumers call
/// <see cref="Drain"/>, which clears the queue.
/// </remarks>
public sealed class PhysicsContactQueueComponent
{
  private readonly List<PhysicsContactFact> _contacts = [];
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

  public void Add(in PhysicsContactFact contact)
  {
    if (_tick < 0 || contact.Tick != _tick || contact.Revision != _revision)
    {
      throw new InvalidOperationException(
          "A contact fact must belong to the active Physics capture frame.");
    }

    for (int index = 0; index < _contacts.Count; index++)
    {
      PhysicsContactFact existing = _contacts[index];
      if (existing.Substep == contact.Substep &&
          ((existing.First.Matches(contact.First) && existing.Second.Matches(contact.Second)) ||
           (existing.First.Matches(contact.Second) && existing.Second.Matches(contact.First))))
      {
        return;
      }
    }

    _contacts.Add(contact);
  }

  public IReadOnlyList<PhysicsContactFact> Drain()
  {
    PhysicsContactFact[] contacts = _contacts.ToArray();
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

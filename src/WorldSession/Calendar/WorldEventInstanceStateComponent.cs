using System;

namespace Terraria.WorldSession.Calendar;

public sealed class WorldEventInstanceStateComponent
{
  public WorldEventInstanceStateComponent(
    EventInstanceId eventInstanceId,
    WorldEventKind eventKind,
    WorldEventLifecycleState lifecycleState)
  {
    EventInstanceId = eventInstanceId;
    EventKind = eventKind;
    LifecycleState = lifecycleState;
    StartedAtTick = null;
    EndedAtTick = null;
    OwnerEntityId = null;
    PersistentEntityId = null;
    NetworkId = null;
    Validate();
  }

  public EventInstanceId EventInstanceId;
  public WorldEventKind EventKind;
  public WorldEventLifecycleState LifecycleState;
  public long? StartedAtTick;
  public long? EndedAtTick;
  public EntityId? OwnerEntityId;
  public PersistentEntityId? PersistentEntityId;
  public NetworkId? NetworkId;

  public void Validate()
  {
    if (StartedAtTick.HasValue && StartedAtTick.Value < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(StartedAtTick));
    }

    if (EndedAtTick.HasValue && EndedAtTick.Value < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(EndedAtTick));
    }

    if (StartedAtTick.HasValue &&
        EndedAtTick.HasValue &&
        EndedAtTick.Value < StartedAtTick.Value)
    {
      throw new ArgumentException(
        "An event cannot end before it starts.",
        nameof(EndedAtTick));
    }
  }
}

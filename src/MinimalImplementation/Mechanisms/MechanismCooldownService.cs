using Terraria.WorldStorage;

namespace MinimalImplementation.Server.Mechanisms;

public sealed class MechanismCooldownService
{
  public enum ScheduleResult
  {
    Scheduled,
    AlreadyScheduled,
    CapacityReached,
    WrongWorld
  }

  public enum DeviceKind
  {
    Cannon,
    BunnyCannon,
    SnowballCannon
  }

  public readonly record struct Snapshot(TileCoordinate Anchor, int RemainingTicks)
  {
    public bool IsDue => RemainingTicks <= 0;
  }

  /// <summary>A private candidate bound to the owner's revision and commit window.</summary>
  public sealed class Preparation
  {
    private readonly MechanismCooldownService _owner;
    private readonly MechanismCooldownService _candidate;
    private readonly long _baseRevision;
    private readonly long _nextRevision;
    private bool _committed;

    internal Preparation(MechanismCooldownService owner)
    {
      _owner = owner;
      _baseRevision = owner._revision;
      _nextRevision = checked(_baseRevision + 1);
      _candidate = new MechanismCooldownService(owner.WorldScope)
      {
        _remainingTicksByAnchor = new(owner._remainingTicksByAnchor),
        _orderedAnchors = new(owner._orderedAnchors),
        _deviceCooldowns = (int[])owner._deviceCooldowns.Clone()
      };
    }

    public long WorldScope => _owner.WorldScope;

    public ScheduleResult Schedule(TileCoordinate anchor, int cooldownTicks)
    {
      EnsureUncommitted();
      return _candidate.Schedule(WorldScope, anchor, cooldownTicks);
    }

    public bool Validate()
    {
      return !_committed && _owner._revision == _baseRevision;
    }

    public void Commit()
    {
      if (!Validate())
      {
        throw new InvalidOperationException("The prepared mechanism cooldowns are stale.");
      }
      _owner._remainingTicksByAnchor = _candidate._remainingTicksByAnchor;
      _owner._orderedAnchors = _candidate._orderedAnchors;
      _owner._deviceCooldowns = _candidate._deviceCooldowns;
      _owner._revision = _nextRevision;
      _committed = true;
    }

    internal bool TrySetRemainingTicks(TileCoordinate anchor, int remainingTicks)
    {
      EnsureUncommitted();
      return _candidate.TrySetRemainingTicks(WorldScope, anchor, remainingTicks);
    }

    internal bool Remove(TileCoordinate anchor)
    {
      EnsureUncommitted();
      return _candidate.Remove(WorldScope, anchor);
    }

    internal void AdvanceOneTick(Action<Snapshot> advanceMechanism)
    {
      EnsureUncommitted();
      _candidate.AdvanceOneTick(WorldScope, advanceMechanism);
    }

    private void EnsureUncommitted()
    {
      if (_committed)
      {
        throw new InvalidOperationException("The cooldown preparation was already committed.");
      }
    }
  }

  private const int DeviceKindCount = 3;

  public const int MaximumScheduledMechanisms = 999;

  private readonly long _worldScope;
  private Dictionary<TileCoordinate, int> _remainingTicksByAnchor = new();
  private List<TileCoordinate> _orderedAnchors = new();
  private int[] _deviceCooldowns = new int[DeviceKindCount];
  private long _revision;

  public MechanismCooldownService(long worldScope)
  {
    if (worldScope <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(worldScope));
    }

    _worldScope = worldScope;
  }

  public long WorldScope => _worldScope;

  public int Count => _orderedAnchors.Count;

  public Preparation Prepare()
  {
    return new Preparation(this);
  }

  public ScheduleResult Schedule(
    long worldScope,
    TileCoordinate anchor,
    int cooldownTicks)
  {
    if (worldScope != _worldScope)
    {
      return ScheduleResult.WrongWorld;
    }

    if (_remainingTicksByAnchor.ContainsKey(anchor))
    {
      return ScheduleResult.AlreadyScheduled;
    }

    if (_orderedAnchors.Count >= MaximumScheduledMechanisms)
    {
      return ScheduleResult.CapacityReached;
    }

    _revision = checked(_revision + 1);
    _remainingTicksByAnchor.Add(anchor, cooldownTicks);
    _orderedAnchors.Add(anchor);
    return ScheduleResult.Scheduled;
  }

  public bool TryGetRemainingTicks(
    long worldScope,
    TileCoordinate anchor,
    out int remainingTicks)
  {
    if (worldScope != _worldScope)
    {
      remainingTicks = 0;
      return false;
    }

    return _remainingTicksByAnchor.TryGetValue(anchor, out remainingTicks);
  }

  public bool TrySetRemainingTicks(
    long worldScope,
    TileCoordinate anchor,
    int remainingTicks)
  {
    if (worldScope != _worldScope || !_remainingTicksByAnchor.ContainsKey(anchor))
    {
      return false;
    }

    _revision = checked(_revision + 1);
    _remainingTicksByAnchor[anchor] = remainingTicks;
    return true;
  }

  public bool TryGetDeviceCooldown(
    long worldScope,
    DeviceKind deviceKind,
    out int remainingTicks)
  {
    int index = GetDeviceKindIndex(deviceKind);
    if (worldScope != _worldScope)
    {
      remainingTicks = 0;
      return false;
    }

    remainingTicks = _deviceCooldowns[index];
    return true;
  }

  public bool TrySetDeviceCooldown(
    long worldScope,
    DeviceKind deviceKind,
    int remainingTicks)
  {
    int index = GetDeviceKindIndex(deviceKind);
    if (worldScope != _worldScope || remainingTicks < 0)
    {
      return false;
    }

    _revision = checked(_revision + 1);
    _deviceCooldowns[index] = remainingTicks;
    return true;
  }

  public bool Remove(long worldScope, TileCoordinate anchor)
  {
    if (worldScope != _worldScope || !_remainingTicksByAnchor.ContainsKey(anchor))
    {
      return false;
    }

    _revision = checked(_revision + 1);
    _remainingTicksByAnchor.Remove(anchor);
    _orderedAnchors.Remove(anchor);
    return true;
  }

  public IReadOnlyList<Snapshot> AdvanceOneTick(long worldScope)
  {
    return AdvanceOneTick(worldScope, advanceMechanism: null);
  }

  public bool Clear(long worldScope)
  {
    if (worldScope != _worldScope)
    {
      return false;
    }

    _revision = checked(_revision + 1);
    _remainingTicksByAnchor.Clear();
    _orderedAnchors.Clear();
    Array.Clear(_deviceCooldowns);
    return true;
  }

  private IReadOnlyList<Snapshot> AdvanceOneTick(
    long worldScope,
    Action<Snapshot>? advanceMechanism)
  {
    if (worldScope != _worldScope)
    {
      throw new ArgumentException("The cooldown service belongs to a different world.",
        nameof(worldScope));
    }

    // Version4 decrements each entry immediately before its effects, in reverse registration
    // order. The private candidate callback may reset or remove only the current entry.
    _revision = checked(_revision + 1);
    var snapshots = new Snapshot[_orderedAnchors.Count];
    AdvanceDeviceCooldowns();
    int snapshotIndex = 0;
    for (int anchorIndex = _orderedAnchors.Count - 1; anchorIndex >= 0; anchorIndex--)
    {
      TileCoordinate anchor = _orderedAnchors[anchorIndex];
      int remainingTicks = _remainingTicksByAnchor[anchor];
      if (remainingTicks > int.MinValue)
      {
        remainingTicks--;
      }

      _remainingTicksByAnchor[anchor] = remainingTicks;
      snapshots[snapshotIndex] = new Snapshot(anchor, remainingTicks);
      advanceMechanism?.Invoke(snapshots[snapshotIndex]);
      snapshotIndex++;
    }

    return Array.AsReadOnly(snapshots);
  }

  private static int GetDeviceKindIndex(DeviceKind deviceKind)
  {
    int index = (int)deviceKind;
    if (index < 0 || index >= DeviceKindCount)
    {
      throw new ArgumentOutOfRangeException(nameof(deviceKind));
    }

    return index;
  }

  private void AdvanceDeviceCooldowns()
  {
    for (int index = 0; index < _deviceCooldowns.Length; index++)
    {
      if (_deviceCooldowns[index] > 0)
      {
        _deviceCooldowns[index]--;
      }
    }
  }
}

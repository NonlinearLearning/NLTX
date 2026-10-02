namespace Terraria.LeashedEntity;

/// <summary>
/// Stores lifecycle state for one leashed entity.
/// status: implemented
/// componentOwner: LeashedEntitySimulation
/// crossSubsystemOwner: none for local state
/// </summary>
public struct LeashedEntityLifecycleComponent
{
  /// <summary>
  /// The only stored lifecycle authority for active/inactive state.
  /// </summary>
  public LeashedEntityLifecycle State;

  /// <summary>
  /// Whether the instance has entered its spawned runtime state.
  /// </summary>
  public bool Spawned;

  /// <summary>
  /// Candidate idempotency/revision field. Version4 does not provide this field;
  /// registration ownership remains unresolved.
  /// </summary>
  public ulong TransitionSequence;

  /// <summary>
  /// Derived view only; do not persist as a second active authority.
  /// </summary>
  public bool IsActive => State == LeashedEntityLifecycle.Active;

  /// <summary>
  /// Derived terminal-state view only.
  /// </summary>
  public bool IsRemoved => State == LeashedEntityLifecycle.Removed;

  public LeashedEntityLifecycleComponent()
    : this(LeashedEntityLifecycle.Inactive, false, 0)
  {
  }

  public LeashedEntityLifecycleComponent(
    LeashedEntityLifecycle state,
    bool spawned,
    ulong transitionSequence)
  {
    State = state;
    Spawned = spawned;
    TransitionSequence = transitionSequence;
  }
}

using MinimalImplementation.Contracts;
using Terraria.WorldStorage;

namespace MinimalImplementation.Server.Mechanisms;

public sealed record TileEntityRuntimeSnapshot(
  MechanismEntityHandle Handle,
  int PersistenceId,
  int NetworkId,
  byte Type,
  TileCoordinate Anchor,
  MechanismSectionCoordinate Section,
  long Revision,
  MechanismEntityLifecycle Lifecycle,
  TileEntitySnapshot Persistence);

public sealed record MechanismWorldSnapshot(
  CommitVersion CommitVersion,
  long WorldScope,
  IReadOnlyList<TileEntityRuntimeSnapshot> TileEntities);

public sealed record MechanismSectionSnapshot(
  CommitVersion CommitVersion,
  long WorldScope,
  MechanismSectionCoordinate Section,
  IReadOnlyList<TileEntityRuntimeSnapshot> TileEntities);

public sealed class MechanismSnapshotCapture
{
  public MechanismWorldSnapshot Capture(
    TileEntityRegistry registry,
    CommitVersion commitVersion,
    long worldScope)
  {
    ArgumentNullException.ThrowIfNull(registry);
    if (!commitVersion.IsValid)
    {
      throw new ArgumentException("A mechanism snapshot requires a committed version.",
        nameof(commitVersion));
    }
    if (worldScope <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(worldScope));
    }
    if (registry.WorldScope != worldScope)
    {
      throw new ArgumentException("The registry belongs to a different world scope.",
        nameof(worldScope));
    }
    var entities = registry.ActiveEntities
      .OrderBy(state => state.PersistenceId)
      .Select(state =>
      {
        if (state.Handle.WorldScope != worldScope ||
            state.Lifecycle != MechanismEntityLifecycle.Active)
        {
          throw new InvalidOperationException(
            "The registry contains an inactive TileEntity or one from another world scope.");
        }
        return new TileEntityRuntimeSnapshot(
          state.Handle,
          state.PersistenceId,
          state.NetworkId,
          state.Definition.Type,
          state.Anchor,
          state.Section,
          state.Revision,
          state.Lifecycle,
          state.CapturePersistence());
      })
      .ToArray();
    return new MechanismWorldSnapshot(commitVersion, worldScope,
      Array.AsReadOnly(entities));
  }

  public MechanismSectionSnapshot CaptureSection(
    TileEntityRegistry registry,
    MechanismSectionCoordinate section,
    CommitVersion commitVersion)
  {
    ArgumentNullException.ThrowIfNull(registry);
    if (!commitVersion.IsValid)
    {
      throw new ArgumentException("A mechanism snapshot requires a committed version.",
        nameof(commitVersion));
    }
    TileEntityRuntimeState[] states = registry.GetSection(section).ToArray();
    var entities = new TileEntityRuntimeSnapshot[states.Length];
    for (int index = 0; index < states.Length; index++)
    {
      TileEntityRuntimeState state = states[index];
      if (state.Handle.WorldScope != registry.WorldScope ||
          state.Lifecycle != MechanismEntityLifecycle.Active)
      {
        throw new InvalidOperationException(
          "The registry contains an inactive TileEntity or one from another world scope.");
      }
      entities[index] = new TileEntityRuntimeSnapshot(
        state.Handle,
        state.PersistenceId,
        state.NetworkId,
        state.Definition.Type,
        state.Anchor,
        state.Section,
        state.Revision,
        state.Lifecycle,
        state.CapturePersistence());
    }
    return new MechanismSectionSnapshot(commitVersion, registry.WorldScope, section,
      Array.AsReadOnly(entities));
  }
}

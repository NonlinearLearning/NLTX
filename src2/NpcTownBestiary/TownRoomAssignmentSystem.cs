namespace Terraria.NpcTownBestiary;

public sealed class TownRoomAssignmentSystem
{
  public TownRoomMutationResult Assign(
    TownRoomRegistryComponent registry,
    AssignTownRoomCommand command)
  {
    ArgumentNullException.ThrowIfNull(registry);
    bool applied = registry.TryAssign(command.NpcType, command.Room, command.ExpectedRevision);
    return new TownRoomMutationResult(applied, !applied, registry.Revision);
  }

  public TownRoomMutationResult Evict(
    TownRoomRegistryComponent registry,
    EvictTownResidentCommand command)
  {
    ArgumentNullException.ThrowIfNull(registry);
    bool applied = registry.TryEvict(command.NpcType, command.ExpectedRevision);
    return new TownRoomMutationResult(applied, !applied, registry.Revision);
  }
}

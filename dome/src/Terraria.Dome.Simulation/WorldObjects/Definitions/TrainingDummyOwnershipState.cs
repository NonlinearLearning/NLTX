namespace Terraria.Dome.Simulation.WorldObjects.Definitions;

public readonly record struct TrainingDummyOwnershipState(
  int EntityId,
  int TileX,
  int TileY,
  NpcHandle Npc,
  long Revision)
{
  public static bool TryLink(
    TrainingDummyTileEntityState entity,
    NpcHandle npcHandle,
    TrainingDummyNpcLinkSnapshot npc,
    out TrainingDummyOwnershipState ownership)
  {
    if (entity.NpcId != -1 ||
        !npcHandle.IsValid ||
        !TrainingDummyNpcLinkValidityQuery.IsValid(entity with { NpcId = 0 }, npc))
    {
      ownership = default;
      return false;
    }

    ownership = new TrainingDummyOwnershipState(
      entity.EntityId,
      entity.TileX,
      entity.TileY,
      npcHandle,
      Revision: 1);
    return true;
  }

  public static bool TryClear(
    TrainingDummyOwnershipState current,
    TrainingDummyTileEntityState entity,
    TrainingDummyNpcLinkSnapshot npc,
    out TrainingDummyOwnershipState cleared)
  {
    if (!current.Npc.IsValid ||
        current.EntityId != entity.EntityId ||
        current.TileX != entity.TileX ||
        current.TileY != entity.TileY ||
        current.Npc.Value > short.MaxValue ||
        !TrainingDummyDeactivationDecisionQuery.ShouldDeactivate(
          entity with { NpcId = checked((short)current.Npc.Value) },
          npc))
    {
      cleared = default;
      return false;
    }

    cleared = current with
    {
      Npc = default,
      Revision = checked(current.Revision + 1)
    };
    return true;
  }

  public static bool TryRestore(
    TrainingDummyTileEntityState entity,
    NpcHandle npcHandle,
    TrainingDummyNpcLinkSnapshot npc,
    out TrainingDummyOwnershipState ownership)
  {
    if (entity.NpcId < 0 ||
        !npcHandle.IsValid ||
        !TrainingDummyNpcLinkValidityQuery.IsValid(entity, npc))
    {
      ownership = default;
      return false;
    }

    ownership = new TrainingDummyOwnershipState(
      entity.EntityId,
      entity.TileX,
      entity.TileY,
      npcHandle,
      Revision: 1);
    return true;
  }
}

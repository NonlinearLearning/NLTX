using Terraria.Relationships;

namespace Terraria.WorldInteraction.TileEntities;

public sealed class TrainingDummyComponent
{
  public EntityReference Npc { get; internal set; } = EntityReference.None;
  public int ActivationRetryCooldownTicks { get; internal set; }
  public bool IsActive => !Npc.IsEmpty;
}

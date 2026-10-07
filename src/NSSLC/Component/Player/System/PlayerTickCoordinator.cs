namespace Terraria.Player;

// status: implemented-isolated-core
// source-members: Player.Update local order; P09-10272, P09-4048, P09-4300, P09-9547
// crossSubsystemOwner: the outer scheduler, server/client phase, Item definitions, and renderer/network sinks remain integration-review
public sealed class PlayerTickCoordinator
{
  private readonly PlayerEquipmentEffectSystem _effectSystem;
  private readonly PlayerEquipmentProjectionSystem _projectionSystem;
  private readonly PlayerDefenseInteractionSystem _defenseSystem;

  public PlayerTickCoordinator(
    PlayerEquipmentEffectSystem? effectSystem = null,
    PlayerEquipmentProjectionSystem? projectionSystem = null,
    PlayerDefenseInteractionSystem? defenseSystem = null)
  {
    _effectSystem = effectSystem ?? new PlayerEquipmentEffectSystem();
    _projectionSystem = projectionSystem ?? new PlayerEquipmentProjectionSystem();
    _defenseSystem = defenseSystem ?? new PlayerDefenseInteractionSystem();
  }

  public PlayerTickCoordinatorResult Tick(
    PlayerEquipmentEffectStateComponent effectState,
    PlayerEquipmentRelationComponent equipment,
    PlayerAppearanceSelectionComponent appearance,
    IPlayerEquipmentVisualItemQuery itemQuery,
    PlayerEquipmentColorProjectionComponent colors,
    PlayerVisibleEquipmentSelectionComponent visible,
    PlayerStatusEffectSystem statusEffects,
    PlayerDefenseStateComponent defenseState,
    in PlayerEquipmentProjectionInput projectionInput,
    in PlayerEquipmentEffectRebuildInput effectInput,
    in PlayerStatusEffectTickInput buffTickInput)
  {
    return TickCore(
      effectState,
      equipment,
      appearance,
      itemQuery,
      colors,
      visible,
      statusEffects,
      defenseState,
      in projectionInput,
      in effectInput,
      in buffTickInput,
      resourceSystem: null,
      resourceRebuildInput: null,
      resourceTickInput: null);
  }

  public PlayerTickCoordinatorResult Tick(
    PlayerEquipmentEffectStateComponent effectState,
    PlayerEquipmentRelationComponent equipment,
    PlayerAppearanceSelectionComponent appearance,
    IPlayerEquipmentVisualItemQuery itemQuery,
    PlayerEquipmentColorProjectionComponent colors,
    PlayerVisibleEquipmentSelectionComponent visible,
    PlayerStatusEffectSystem statusEffects,
    PlayerDefenseStateComponent defenseState,
    in PlayerEquipmentProjectionInput projectionInput,
    in PlayerEquipmentEffectRebuildInput effectInput,
    in PlayerStatusEffectTickInput buffTickInput,
    PlayerBuffResourceSystem resourceSystem,
    in PlayerBuffResourceRebuildInput resourceRebuildInput,
    in PlayerBuffResourceTickInput resourceTickInput)
  {
    ArgumentNullException.ThrowIfNull(resourceSystem);

    return TickCore(
      effectState,
      equipment,
      appearance,
      itemQuery,
      colors,
      visible,
      statusEffects,
      defenseState,
      in projectionInput,
      in effectInput,
      in buffTickInput,
      resourceSystem,
      resourceRebuildInput,
      resourceTickInput);
  }

  private PlayerTickCoordinatorResult TickCore(
    PlayerEquipmentEffectStateComponent effectState,
    PlayerEquipmentRelationComponent equipment,
    PlayerAppearanceSelectionComponent appearance,
    IPlayerEquipmentVisualItemQuery itemQuery,
    PlayerEquipmentColorProjectionComponent colors,
    PlayerVisibleEquipmentSelectionComponent visible,
    PlayerStatusEffectSystem statusEffects,
    PlayerDefenseStateComponent defenseState,
    in PlayerEquipmentProjectionInput projectionInput,
    in PlayerEquipmentEffectRebuildInput effectInput,
    in PlayerStatusEffectTickInput buffTickInput,
    PlayerBuffResourceSystem? resourceSystem,
    PlayerBuffResourceRebuildInput? resourceRebuildInput,
    PlayerBuffResourceTickInput? resourceTickInput)
  {
    ArgumentNullException.ThrowIfNull(effectState);
    ArgumentNullException.ThrowIfNull(equipment);
    ArgumentNullException.ThrowIfNull(appearance);
    ArgumentNullException.ThrowIfNull(itemQuery);
    ArgumentNullException.ThrowIfNull(colors);
    ArgumentNullException.ThrowIfNull(visible);
    ArgumentNullException.ThrowIfNull(statusEffects);
    ArgumentNullException.ThrowIfNull(defenseState);

    // This order mirrors the confirmed local Player.Update sequence. Each
    // System owns only its own component; the coordinator owns ordering.
    _effectSystem.ResetForTick(effectState);
    PlayerBuffResourceSnapshot? resources = resourceSystem?.ResetForTick();
    PlayerEquipmentProjectionResult projection = _projectionSystem.Project(
      equipment,
      appearance,
      itemQuery,
      colors,
      visible,
      projectionInput);
    statusEffects.ResetForTick();
    PlayerStatusEffectTickResult buffs = statusEffects.Tick(in buffTickInput);
    PlayerBuffResourceRebuildResult? resourceRebuild = null;
    if (resourceSystem is not null && resourceRebuildInput.HasValue)
    {
      PlayerBuffResourceRebuildResult rebuild = resourceSystem.Rebuild(
        resourceRebuildInput.Value);
      resourceRebuild = rebuild;
      resources = rebuild.Snapshot;
      if (rebuild.Applied && resourceTickInput.HasValue)
      {
        resources = resourceSystem.AdvanceTick(resourceTickInput.Value);
      }
    }

    PlayerEquipmentEffectSnapshot effects = _effectSystem.Rebuild(
      effectState,
      effectInput);
    PlayerDefenseInteractionResult defense = _defenseSystem.AdvanceTick(defenseState);

    return new PlayerTickCoordinatorResult(projection, effects, buffs, defense)
    {
      Resources = resources,
      ResourceRebuild = resourceRebuild,
    };
  }
}

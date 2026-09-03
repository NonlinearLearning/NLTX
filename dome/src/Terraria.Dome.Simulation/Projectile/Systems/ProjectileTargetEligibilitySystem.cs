using Terraria.Dome.Simulation.Components;
using Terraria.Dome.Simulation.Npc.Components;
using Terraria.Dome.Simulation.Npc.Definitions;
using Terraria.Dome.Simulation.Npc.Systems;
using Terraria.Dome.Simulation.Projectile.Definitions;

namespace Terraria.Dome.Simulation.Projectile.Systems;

public sealed class ProjectileTargetEligibilitySystem
{
  private readonly NpcCombatClassificationSystem _npcCombatClassification = new();

  public bool CanDamageNpc(ProjectileDefinitionComponent definition)
  {
    return CanDamageNpc(definition, definition.Friendly);
  }

  public bool CanDamageNpc(
    ProjectileDefinitionComponent definition,
    bool runtimeFriendly)
  {
    return definition.Friendly && runtimeFriendly;
  }

  public bool CanTargetNpc(
    ProjectileDefinitionComponent projectile,
    NpcDefinitionComponent npcDefinition,
    NpcAuthorityComponent npcAuthority,
    NpcBehaviorStateComponent npcBehavior,
    NpcLifecycleComponent npcLifecycle,
    HealthComponent npcHealth,
    bool ignoreDoesNotTakeDamage = false,
    bool allowImmortalTargetDummy = false)
  {
    return CanTargetNpc(
      projectile,
      npcDefinition,
      npcAuthority,
      npcBehavior,
      npcLifecycle,
      npcHealth,
      ignoreDoesNotTakeDamage,
      allowImmortalTargetDummy,
      projectile.Friendly);
  }

  public bool CanTargetNpc(
    ProjectileDefinitionComponent projectile,
    NpcDefinitionComponent npcDefinition,
    NpcAuthorityComponent npcAuthority,
    NpcBehaviorStateComponent npcBehavior,
    NpcLifecycleComponent npcLifecycle,
    HealthComponent npcHealth,
    bool ignoreDoesNotTakeDamage,
    bool allowImmortalTargetDummy,
    bool runtimeFriendly)
  {
    return CanDamageNpc(projectile, runtimeFriendly) &&
      (!projectile.IsTrap || !npcAuthority.IsTrapImmune) &&
      _npcCombatClassification.CanBeChasedBy(
        npcLifecycle.IsActive,
        npcBehavior.IsChaseable,
        npcHealth.Maximum,
        npcBehavior.DoesNotTakeDamage,
        npcDefinition.Faction == NpcFaction.Town,
        npcAuthority.IsImmortal,
        ignoreDoesNotTakeDamage,
        allowImmortalTargetDummy);
  }

  public bool CanDamagePlayer(ProjectileDefinitionComponent definition)
  {
    return definition.Hostile && definition.PlayerDamagePolicy != PlayerDamagePolicy.None;
  }

  public bool CanDamagePlayerTarget(
    ProjectileDefinitionComponent definition,
    PlayerHandle owner,
    PlayerHandle target,
    bool isPvpEnabled)
  {
    return CanDamagePlayer(definition) && target.IsValid && target != owner &&
      (definition.PlayerDamagePolicy == PlayerDamagePolicy.HostileNonPvp || isPvpEnabled);
  }
}

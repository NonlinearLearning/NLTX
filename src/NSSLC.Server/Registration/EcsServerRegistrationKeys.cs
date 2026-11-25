namespace Terraria.Server;

/// <summary>Stable composition-root identities for server ECS registrations.</summary>
internal static class EcsServerRegistrationKeys
{
  internal const string CommunicationIngressPreparation = "communication.ingress.preparation";
  internal const string CommunicationSessionTick = "communication.session.tick";
  internal const string CommunicationSessionCommit = "communication.session.commit";
  internal const string CommunicationOutboundProjection = "communication.outbound.projection";

  internal const string CombatProjectileSpawn = "combat.projectile.spawn";
  internal const string CombatProjectileEmissionDrain = "combat.projectile.emission-drain";
  internal const string CombatProjectileTargetAcquisition = "combat.projectile.target-acquisition";
  internal const string CombatProjectileAiAdapter = "combat.projectile.ai-adapter";
  internal const string CombatPhysicsContactCapture = "combat.physics.contact-capture";
  internal const string CombatProjectileLifetime = "combat.projectile.lifetime";
  internal const string CombatProjectileDamageRequest = "combat.projectile.damage-request";
  internal const string CombatProjectileTileResolution = "combat.projectile.tile-resolution";
  internal const string CombatImmunityWindow = "combat.immunity-window";
  internal const string CombatAttackerImmunity = "combat.attacker-immunity";
  internal const string CombatAttackRequest = "combat.attack-request";
  internal const string CombatDamageRequest = "combat.damage-request";
  internal const string CombatProjectileDamageResult = "combat.projectile.damage-result";
  internal const string CombatNpcResult = "combat.npc.result";
  internal const string CombatPlayerResult = "combat.player.result";
  internal const string CombatPlayerDeathRespawn = "combat.player.death-respawn";
  internal const string CombatLifeRestore = "combat.life-restore";
  internal const string CombatProjectileTermination = "combat.projectile.termination";

  internal const string PlayerInputApply = "player.input.apply";
  internal const string LifeAttributeSynchronization = "combat.life-attribute-synchronization";
  internal const string NetworkNpcProjection = "network.npc.projection";
  internal const string ChatNetworkProjection = "chat.network.projection";
  internal const string WorldItemRequestCommit = "world.item-request.commit";
  internal const string NetworkWorldProjection = "network.world.projection";
  internal const string WorldTick = "world.tick";
  internal const string WorldItemTick = "world.item.tick";
  internal const string InventoryPickupRequest = "inventory.pickup.request";
  internal const string EntitySpawn = "entity.spawn";
  internal const string NpcNetworkIdentity = "npc.network.identity";
  internal const string WorldTileEntityRequest = "world.tile-entity.request";
  internal const string PlayerItemUse = "player.item-use";
  internal const string ProjectileItemUseSpawn = "combat.projectile.item-use-spawn";
  internal const string ChatDomain = "chat.domain";
  internal const string NpcAiIntent = "npc.ai.intent";
  internal const string GravityTick = "movement.gravity.tick";
  internal const string AttributeTick = "attributes.tick";
  internal const string BuffTick = "buffs.tick";
  internal const string BuffApply = "buffs.apply";
  internal const string StatusEffectBehavior = "buffs.behavior";
  internal const string StatusEffectPeriodic = "buffs.periodic";
  internal const string CooldownTick = "cooldowns.tick";
  internal const string MovementIntentApply = "movement.intent.apply";
  internal const string SpatialSimulation = "movement.spatial.simulation";
  internal const string WorldInteractionCommit = "world.interaction.commit";
}

using System.Collections.ObjectModel;

namespace Terraria.Content;

/// <summary>Validates content references before publishing an immutable runtime catalog.</summary>
public static class ContentCatalogBuildSystem
{
  public static ContentCatalog Build(ContentCatalogSnapshot source)
  {
    ArgumentNullException.ThrowIfNull(source);
    IReadOnlyList<string> failures = Validate(source);
    if (failures.Count > 0)
    {
      throw new InvalidOperationException(
        "Content catalog validation failed: " + string.Join("; ", failures));
    }

    return new ContentCatalog(source.CreateValidated());
  }

  public static IReadOnlyList<string> Validate(ContentCatalogSnapshot source)
  {
    ArgumentNullException.ThrowIfNull(source);
    List<string> failures = new();
    if (source.CatalogRevision <= 0)
    {
      failures.Add("catalog revision must be positive");
    }

    if (string.IsNullOrWhiteSpace(source.SourceKey))
    {
      failures.Add("source key must not be empty");
    }

    ValidateItems(source, failures);
    ValidateNpcs(source, failures);
    ValidateProjectiles(source, failures);
    return new ReadOnlyCollection<string>(failures);
  }

  private static void ValidateItems(ContentCatalogSnapshot source, List<string> failures)
  {
    foreach (ItemDefinition item in source.Items.DefinitionsByType.Values)
    {
      int typeId = item.Identity.TypeId;
      if (typeId < 0 || string.IsNullOrWhiteSpace(item.Identity.PersistentId))
      {
        failures.Add($"item {typeId} has no valid identity");
      }

      if (item.Stack.MaxStack <= 0 || item.Stack.DefaultStack < 0 ||
          item.Stack.DefaultStack > item.Stack.MaxStack)
      {
        failures.Add($"item {typeId} has invalid stack limits");
      }

      if (item.Use.UseAnimationTicks < 0 || item.Use.UseTimeTicks < 0 ||
          item.Use.ReuseDelayTicks < 0 || item.Combat.Damage < 0 ||
          !float.IsFinite(item.Combat.KnockBack) || !float.IsFinite(item.Combat.ShootSpeed))
      {
        failures.Add($"item {typeId} has invalid use or combat values");
      }

      if (!source.Identities.TryGetItemPersistentId(typeId, out string itemPersistentId) ||
          !StringComparer.Ordinal.Equals(itemPersistentId, item.Identity.PersistentId))
      {
        failures.Add($"item {typeId} is missing its matching persistent identity");
      }

      if (item.Combat.ShootTypeId is int projectileType &&
          !source.Projectiles.TryGet(projectileType, out _))
      {
        failures.Add($"item {typeId} references missing projectile {projectileType}");
      }

      if (item.Combat.AmmoTypeId is int ammoType &&
          !source.Items.TryGet(ammoType, out _))
      {
        failures.Add($"item {typeId} references missing ammo item {ammoType}");
      }

      if (item.Combat.UseAmmoTypeId is int useAmmoType &&
          !source.Items.TryGet(useAmmoType, out _))
      {
        failures.Add($"item {typeId} references missing use-ammo item {useAmmoType}");
      }

      if (item.Effects.BuffTypeId is int buffType &&
          !source.Buffs.TryGet(buffType, out _))
      {
        failures.Add($"item {typeId} references missing buff {buffType}");
      }

      if (item.Placement.CreateTileTypeId is int tileType &&
          !source.Tiles.TryGet(tileType, out _))
      {
        failures.Add($"item {typeId} references missing tile {tileType}");
      }

      if (item.Placement.CreateWallTypeId is int wallType &&
          !source.Walls.TryGet(wallType, out _))
      {
        failures.Add($"item {typeId} references missing wall {wallType}");
      }
    }

    if (source.Identities.ItemPersistentIdByType.Count != source.Items.Count)
    {
      failures.Add("item identity count does not match the item definition count");
    }
  }

  private static void ValidateNpcs(ContentCatalogSnapshot source, List<string> failures)
  {
    foreach (NpcDefinition npc in source.Npcs.DefinitionsByNetId.Values)
    {
      if (npc.NetId < 0 || npc.TypeId < 0 || string.IsNullOrWhiteSpace(npc.PersistentId))
      {
        failures.Add($"NPC {npc.NetId} has no valid identity");
      }

      if (npc.Core.DefaultLifeMax <= 0 || npc.Movement.Width <= 0 ||
          npc.Movement.Height <= 0 || !float.IsFinite(npc.Movement.Scale) ||
          npc.Movement.Scale <= 0)
      {
        failures.Add($"NPC {npc.NetId} has invalid core or movement values");
      }

      if (!source.Identities.TryGetNpcPersistentId(npc.NetId, out string persistentId) ||
          !StringComparer.Ordinal.Equals(persistentId, npc.PersistentId))
      {
        failures.Add($"NPC {npc.NetId} is missing its matching persistent identity");
      }

      if (npc.Spawn.CatchItemTypeId is int catchItemType &&
          !source.Items.TryGet(catchItemType, out _))
      {
        failures.Add($"NPC {npc.NetId} references missing catch item {catchItemType}");
      }
    }

    if (source.Identities.NpcPersistentIdByNetId.Count != source.Npcs.Count)
    {
      failures.Add("NPC identity count does not match the NPC definition count");
    }
  }

  private static void ValidateProjectiles(
    ContentCatalogSnapshot source,
    List<string> failures)
  {
    foreach (ProjectileDefinition projectile in source.Projectiles.DefinitionsByType.Values)
    {
      int typeId = projectile.TypeId;
      if (typeId < 0 || projectile.Geometry.Width <= 0 || projectile.Geometry.Height <= 0 ||
          !float.IsFinite(projectile.Geometry.Scale) || projectile.Geometry.Scale <= 0 ||
          projectile.Behavior.DefaultTimeLeft <= 0 || projectile.Behavior.ExtraUpdates < 0 ||
          projectile.Combat.Damage < 0 || !float.IsFinite(projectile.Combat.KnockBack))
      {
        failures.Add($"projectile {typeId} has invalid geometry, lifetime, or combat values");
      }

      if (projectile.PersistentId is string persistentId &&
          (!source.Identities.TryGetProjectilePersistentId(typeId, out string catalogId) ||
           !StringComparer.Ordinal.Equals(catalogId, persistentId)))
      {
        failures.Add($"projectile {typeId} is missing its matching persistent identity");
      }
    }

    if (source.Identities.ProjectilePersistentIdByType.Count !=
        source.Projectiles.DefinitionsByType.Values.Count(
          static projectile => projectile.PersistentId is not null))
    {
      failures.Add("projectile identity count does not match persistent projectile definitions");
    }
  }
}

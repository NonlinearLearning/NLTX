using System;
using System.Collections.Generic;
using System.Linq;

using NSSLC.WorldGeneration;
using NSSLC.WorldGeneration.ID;
using Terraria.Content;

namespace NSSLC.Tools.NetworkServer;

/// <summary>Adds the source-backed content required by the Social NPC effects.</summary>
internal static class SocialNpcEffectProductionContentCatalog
{
  private const int DryadTypeId = 20;
  private const int StardewProjectileType = 995;

  public static ContentCatalog Extend(ContentCatalogSnapshot hostContent)
  {
    ArgumentNullException.ThrowIfNull(hostContent);
    EnsureLegacyTileFactsAreInitialized();

    NpcDefinition dryad = CreateDryadDefinition();
    ProjectileDefinition stardewProjectile = CreateStardewProjectileDefinition();
    NpcDefinition[] npcs = hostContent.Npcs.DefinitionsByNetId.Values
      .Where(definition => definition.TypeId != DryadTypeId &&
        definition.NetId != DryadTypeId)
      .Append(dryad)
      .ToArray();
    ProjectileDefinition[] projectiles = hostContent.Projectiles.DefinitionsByType.Values
      .Where(definition => definition.TypeId != StardewProjectileType)
      .Append(stardewProjectile)
      .ToArray();
    ContentIdentityCatalog identities = ExtendIdentities(hostContent, dryad, stardewProjectile);
    TileDefinitionCatalog tiles = CaptureTileFacts(hostContent.Tiles);

    var extended = new ContentCatalogSnapshot(
      catalogRevision: checked(hostContent.CatalogRevision + 1),
      sourceKey: hostContent.SourceKey + "+social-npc-effects-v4",
      items: hostContent.Items,
      npcs: new NpcDefinitionCatalog(npcs),
      projectiles: new ProjectileDefinitionCatalog(projectiles),
      identities: identities,
      buffs: hostContent.Buffs,
      tiles: tiles,
      walls: hostContent.Walls,
      recipes: hostContent.Recipes,
      recipeGroups: hostContent.RecipeGroups,
      dropRules: hostContent.DropRules,
      fishingDropRules: hostContent.FishingDropRules);
    return ContentCatalogBuildSystem.Build(extended);
  }

  private static NpcDefinition CreateDryadDefinition()
  {
    return new NpcDefinition(
      new NpcIdentityDefinition(DryadTypeId, DryadTypeId, "npc.dryad"),
      new NpcCoreDefinition(DefaultLifeMax: 250, DefaultDamage: 10, DefaultDefense: 15),
      new NpcMovementDefinition(Width: 18, Height: 40),
      new NpcSpawnDefinition(AiStyle: 7, CanBeCaught: false),
      new NpcTownDefinition(IsTownNpc: true),
      new NpcCapabilitiesDefinition(Friendly: true, Hostile: false));
  }

  private static ProjectileDefinition CreateStardewProjectileDefinition()
  {
    return new ProjectileDefinition(
      new ProjectileIdentityDefinition(
        StardewProjectileType,
        "projectile.jumino-stardrop-animation",
        NeedsUuid: false),
      new ProjectileGeometryDefinition(
        Width: 240,
        Height: 104,
        Scale: 1.0f,
        TileCollide: false,
        CorrectSlopeCollision: false)
      {
        IgnoreWater = true
      },
      new ProjectileBehaviorDefinition(AiStyle: 192, ExtraUpdates: 0, DefaultTimeLeft: 18000),
      new ProjectileCombatDefinition(Damage: 0, KnockBack: 0.0f, Friendly: true, Hostile: false),
      new ProjectilePenetrationDefinition(
        DefaultPenetrate: -1,
        MaxPenetrate: -1,
        StopsDealingDamageAfterPenetrateHits: false),
      new ProjectileCapabilitiesDefinition(false, false, false, false),
      new ProjectilePresentationDefinition(FrameCount: 1, Light: 0.0f, Hide: false))
    {
      Network = new ProjectileNetworkDefinition(NetworkImportant: true)
    };
  }

  private static ContentIdentityCatalog ExtendIdentities(
    ContentCatalogSnapshot hostContent,
    NpcDefinition dryad,
    ProjectileDefinition stardewProjectile)
  {
    Dictionary<int, string> npcIdentities =
      hostContent.Identities.NpcPersistentIdByNetId
        .ToDictionary(pair => pair.Key, pair => pair.Value);
    foreach (NpcDefinition replaced in hostContent.Npcs.DefinitionsByNetId.Values.Where(
      definition => definition.TypeId == DryadTypeId || definition.NetId == DryadTypeId))
    {
      npcIdentities.Remove(replaced.NetId);
    }

    npcIdentities[dryad.NetId] = dryad.PersistentId;
    Dictionary<int, string> projectileIdentities =
      hostContent.Identities.ProjectilePersistentIdByType
        .ToDictionary(pair => pair.Key, pair => pair.Value);
    projectileIdentities[stardewProjectile.TypeId] = stardewProjectile.PersistentId!;

    return new ContentIdentityCatalog(
      hostContent.Identities.ItemPersistentIdByType,
      npcIdentities,
      projectileIdentities,
      hostContent.Identities.NpcBestiaryCreditIdByNetId);
  }

  private static TileDefinitionCatalog CaptureTileFacts(TileDefinitionCatalog existingTiles)
  {
    Dictionary<int, TileDefinition> tiles = existingTiles.DefinitionsByType
      .ToDictionary(pair => pair.Key, pair => pair.Value);
    for (int typeId = 0; typeId < TileID.Count; typeId++)
    {
      tiles.TryGetValue(typeId, out TileDefinition? existing);
      tiles[typeId] = new TileDefinition(
        typeId,
        existing?.PersistentId,
        Main.tileSolid[typeId],
        Main.tileSolidTop[typeId],
        existing?.Lighted ?? false,
        Main.tileFrameImportant[typeId],
        existing?.Container ?? false,
        existing?.Sign ?? false);
    }

    return new TileDefinitionCatalog(tiles.Values);
  }

  private static void EnsureLegacyTileFactsAreInitialized()
  {
    if (Main.tileSolid.Length != TileID.Count ||
        Main.tileSolidTop.Length != TileID.Count ||
        Main.tileFrameImportant.Length != TileID.Count)
    {
      throw new InvalidOperationException(
        "The headless Terraria tile content must be initialized before composing Social content.");
    }
  }
}

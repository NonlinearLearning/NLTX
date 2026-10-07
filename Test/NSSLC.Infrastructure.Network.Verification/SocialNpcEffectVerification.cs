using System;
using System.Collections.Generic;
using System.Numerics;

using EntityEcs;
using Terraria.Content;
using Terraria.Network;
using Terraria.NonAuthoritative.Persistence;
using Terraria.Relationships;
using Terraria.WorldStorage;

namespace NSSLC.NetworkVerification;

internal static class SocialNpcEffectVerification {
  private const int TileMapWidth = 100;
  private const int TileMapHeight = 100;
  private const int DryadTileX = 19;
  private const int DryadScanStartY = 10;
  private const int StardewProjectileWidth = 240;
  private const int StardewProjectileHeight = 104;

  public static async Task RunAsync() {
    await VerifyNpcReactionUsesTopGeometryAsync();
    await VerifyPlayerReactionUsesTopGeometryAsync();
    await VerifyDryadProjectileScanAsync();
    await VerifyStaleWorldRuntimeAsync();
  }

  private static async Task VerifyPlayerReactionUsesTopGeometryAsync() {
    EntityRuntimeId worldRuntimeId = default;
    await using var worldOwner = CreateWorldOwner(
        solidTile: (31, 6),
        runtimeId => worldRuntimeId = runtimeId);
    await worldOwner.Ready.WaitAsync(TimeSpan.FromSeconds(5));

    var effects = new SocialNpcEffectOwner(worldOwner, CreateContentCatalog());
    var player = new SocialPlayerNpcInteractionSnapshot(
        PlayerSlot: 7,
        Position: new Vector2(495, 100),
        Width: 20,
        Height: 42,
        Active: false,
        Dead: false,
        ShouldNotDraw: false,
        Stealth: 1.0f,
        WorldRuntimeId: worldRuntimeId);
    SocialNpcEffectResult result = await effects.ApplyPlayerEmoteAsync(
        worldRuntimeId,
        player);

    Verify.That(result.Status == SocialNpcEffectStatus.Applied &&
        result.UpdatedNpcs.Count == 1 &&
        result.UpdatedNpcs[0].Ai[0] == 19.0f &&
        result.UpdatedNpcs[0].Ai[1] == 220.0f &&
        result.UpdatedNpcs[0].Ai[2] == player.PlayerSlot &&
        result.UpdatedNpcs[0].Direction == 1 &&
        result.UpdatedNpcs[0].NetworkUpdatePending,
        "Packet 120 must trace through Player.Top at X + width / 2; a solid tile in that endpoint tile must trigger the reaction.");
  }

  private static async Task VerifyNpcReactionUsesTopGeometryAsync() {
    EntityRuntimeId worldRuntimeId = default;
    await using var worldOwner = CreateWorldOwner(
        solidTile: (12, 6),
        runtimeId => worldRuntimeId = runtimeId);
    await worldOwner.Ready.WaitAsync(TimeSpan.FromSeconds(5));

    var effects = new SocialNpcEffectOwner(worldOwner, CreateContentCatalog());
    var player = new SocialPlayerNpcInteractionSnapshot(
        PlayerSlot: 7,
        Position: new Vector2(500, 100),
        Width: 20,
        Height: 42,
        Active: false,
        Dead: false,
        ShouldNotDraw: false,
        Stealth: 1.0f,
        WorldRuntimeId: worldRuntimeId);
    SocialNpcEffectResult result = await effects.ApplyPlayerEmoteAsync(
        worldRuntimeId,
        player);

    Verify.That(result.Status == SocialNpcEffectStatus.NoEffect &&
        result.UpdatedNpcs.Count == 0,
        "Packet 120 line of sight must start at NPC.Top and Player.Top; a solid tile at the NPC's left edge must not block the center-to-center ray.");
  }

  private static async Task VerifyDryadProjectileScanAsync() {
    await VerifyDryadProjectilePositionAsync(
        solidOffset: 0,
        new Vector2(309, 88),
        "A solid first scan tile must keep the Dryad projectile at the scan origin before the -52 pixel offset.");
    await VerifyDryadProjectilePositionAsync(
        solidOffset: 4,
        new Vector2(312, 172),
        "A solid middle scan tile must place the Dryad projectile at that tile's center X and top Y before the -52 pixel offset.");
    await VerifyDryadProjectilePositionAsync(
        solidOffset: null,
        new Vector2(312, 332),
        "An exhausted 15-tile scan must use the last checked row, scanStartY + 14, before the -52 pixel offset.");
  }

  private static async Task VerifyDryadProjectilePositionAsync(
      int? solidOffset,
      Vector2 expectedSpawnCenter,
      string assertionMessage) {
    EntityRuntimeId worldRuntimeId = default;
    (int X, int Y)? solidTile = solidOffset is int offset
        ? (DryadTileX, DryadScanStartY + offset)
        : null;
    await using var worldOwner = CreateWorldOwner(
        solidTile,
        runtimeId => worldRuntimeId = runtimeId);
    await worldOwner.Ready.WaitAsync(TimeSpan.FromSeconds(5));

    var effects = new SocialNpcEffectOwner(worldOwner, CreateContentCatalog());
    SocialNpcEffectResult result = await effects.RequestDryadAnimationAsync(worldRuntimeId);
    Verify.That(result.Status == SocialNpcEffectStatus.Applied && result.UpdatedNpcs.Count == 1,
        "An active Dryad must produce one committed NPC effect before projectile assertions.");
    SocialNpcEffectSnapshot dryad = result.UpdatedNpcs.Single();
    SocialProjectileSpawnSnapshot projectile = result.SpawnedProjectile ??
        throw new InvalidOperationException("The Dryad effect did not return its committed projectile.");

    Verify.That(dryad.TypeId == 20 && dryad.Ai[0] == 24.0f && dryad.Ai[1] == 480.0f &&
        dryad.Ai[2] == 0.0f && dryad.LocalAi[2] == 480.0f && dryad.LocalAi[3] == 0.0f &&
        dryad.Direction == 1 && dryad.SpriteDirection == 1 && dryad.NetworkUpdatePending,
        "Packet 144 must commit the Dryad AI, local AI, direction and NPC sync intent before returning its snapshot.");
    Verify.That(projectile.ProjectileType == 995 &&
        projectile.RuntimeHandle.RuntimeId == worldRuntimeId &&
        projectile.EntityReference.RuntimeId == worldRuntimeId &&
        projectile.SlotGeneration == 1 && projectile.OwnerSlot == byte.MaxValue &&
        projectile.ProjectileUuid is null && projectile.NetworkImportant,
        "Packet 144 must return the real, network-important type-995 lifecycle instance without a UUID.");
    Verify.That(projectile.Position.X == expectedSpawnCenter.X - StardewProjectileWidth * 0.5f,
        $"{assertionMessage} Expected committed top-left X " +
        $"{expectedSpawnCenter.X - StardewProjectileWidth * 0.5f}, got {projectile.Position.X}.");
    Verify.That(projectile.Position.Y == expectedSpawnCenter.Y - StardewProjectileHeight * 0.5f,
        $"{assertionMessage} Expected committed top-left Y " +
        $"{expectedSpawnCenter.Y - StardewProjectileHeight * 0.5f}, got {projectile.Position.Y}.");
    Verify.That(projectile.Velocity == Vector2.Zero &&
        projectile.Damage == 0 && projectile.OriginalDamage == 0 && projectile.Knockback == 0.0f &&
        projectile.TimeLeft == 18000,
        "Packet 144 must preserve the source's zero motion/damage/knockback and type-995 lifetime.");
  }

  private static async Task VerifyStaleWorldRuntimeAsync() {
    EntityRuntimeId activeWorldRuntimeId = default;
    await using var worldOwner = CreateWorldOwner(
        solidTile: null,
        runtimeId => activeWorldRuntimeId = runtimeId);
    await worldOwner.Ready.WaitAsync(TimeSpan.FromSeconds(5));
    EntityRuntimeId staleRuntimeId = new(Guid.NewGuid());

    var effects = new SocialNpcEffectOwner(worldOwner, CreateContentCatalog());
    var player = new SocialPlayerNpcInteractionSnapshot(
        PlayerSlot: 3,
        Position: Vector2.Zero,
        Width: 20,
        Height: 42,
        Active: true,
        Dead: false,
        ShouldNotDraw: false,
        Stealth: 1.0f,
        WorldRuntimeId: staleRuntimeId);
    SocialNpcEffectResult result = await effects.ApplyPlayerEmoteAsync(
        staleRuntimeId,
        player);

    Verify.That(activeWorldRuntimeId != staleRuntimeId &&
        result.Status == SocialNpcEffectStatus.StaleWorldRuntime &&
        result.UpdatedNpcs.Count == 0 && result.SpawnedProjectile is null,
        "A Social NPC effect queued for a replaced world runtime must be rejected without effects.");
  }

  internal static NetworkWorldOwner CreateWorldOwner(
      (int X, int Y)? solidTile,
      Action<EntityRuntimeId> captureRuntimeId) {
    ArgumentNullException.ThrowIfNull(captureRuntimeId);
    return new NetworkWorldOwner(() => {
      var session = new LoadedWorldSession();
      captureRuntimeId(session.WorldRuntimeId);
      session.World.Descriptor.SizeX = TileMapWidth;
      session.World.Descriptor.SizeY = TileMapHeight;

      var tiles = new TileCellState[TileMapWidth * TileMapHeight];
      if (solidTile is (int x, int y)) {
        tiles[x * TileMapHeight + y] = new TileCellState {
          TileHeader = 0x20,
          Type = 1
        };
      }

      TileMapRestoreSystem.Apply(
          session.Storage.TileMap,
          new TileMapSnapshot(TileMapWidth, TileMapHeight, tiles));
      var dryad = new WorldNpcState(
          netId: 20,
          legacyTypeName: null,
          isTownNpc: true,
          name: "Dryad",
          x: 200,
          y: 100,
          homeless: false,
          home: new TileCoordinate(0, 0),
          variation: null,
          homelessDespawn: false);
      if (!session.Storage.Npcs.TryAllocateAt(
          new Terraria.WorldStorage.NpcSlot(0),
          dryad,
          out _)) {
        session.Dispose();
        throw new InvalidOperationException("The verification world could not allocate its Dryad slot.");
      }

      return session;
    });
  }

  internal static ContentCatalog CreateContentCatalog() {
    var dryad = new NpcDefinition(
        typeId: 20,
        netId: 20,
        persistentId: "npc.dryad",
        defaultLifeMax: 250,
        defaultDamage: 10,
        defaultDefense: 15,
        width: 18,
        height: 40,
        aiStyle: 7,
        isTownNpc: true,
        friendly: true,
        hostile: false);
    var stardewProjectile = new ProjectileDefinition(
        new ProjectileIdentityDefinition(995, null, NeedsUuid: false),
        new ProjectileGeometryDefinition(
            Width: StardewProjectileWidth,
            Height: StardewProjectileHeight,
            Scale: 1.0f,
            TileCollide: false,
            CorrectSlopeCollision: false) { IgnoreWater = true },
        new ProjectileBehaviorDefinition(AiStyle: 192, ExtraUpdates: 0, DefaultTimeLeft: 18000),
        new ProjectileCombatDefinition(Damage: 0, KnockBack: 0.0f, Friendly: true, Hostile: false),
        new ProjectilePenetrationDefinition(
            DefaultPenetrate: -1,
            MaxPenetrate: -1,
            StopsDealingDamageAfterPenetrateHits: false),
        new ProjectileCapabilitiesDefinition(false, false, false, false),
        new ProjectilePresentationDefinition(FrameCount: 1, Light: 0.0f, Hide: false)) {
      Network = new ProjectileNetworkDefinition(NetworkImportant: true)
    };
    var source = new ContentCatalogSnapshot(
        catalogRevision: 1,
        sourceKey: "social-npc-effect-verification",
        items: new ItemDefinitionCatalog(Array.Empty<ItemDefinition>()),
        npcs: new NpcDefinitionCatalog(new[] { dryad }),
        projectiles: new ProjectileDefinitionCatalog(new[] { stardewProjectile }),
        identities: new ContentIdentityCatalog(
            new Dictionary<int, string>(),
            new Dictionary<int, string> { [dryad.NetId] = dryad.PersistentId }),
        tiles: new TileDefinitionCatalog(new[] {
          new TileDefinition(1, null, Solid: true, SolidTop: false,
              Lighted: false, FrameImportant: false, Container: false, Sign: false)
        }));
    return ContentCatalogBuildSystem.Build(source);
  }
}

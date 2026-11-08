using System;
using System.Collections.Generic;
using System.Numerics;
using Arch.Core;
using Arch.System;
using ArchEntity = Arch.Core.Entity;
using ArchWorld = Arch.Core.World;
using Terraria.Combat;
using Terraria.Communication;
using Terraria.Entity;
using Terraria.Movement;
using Terraria.Physics;
using Terraria.Relationships;
using Terraria.World;
using MovementVelocityComponent = Terraria.Movement.VelocityComponent;

namespace Terraria.Npc;

/// <summary>
/// Spawns hostile creatures around players, ported from Version4 <c>NPC.Spawner.SpawnNPC</c>.
/// </summary>
/// <remarks>
/// Per tick the reference walks every active player, asks <c>GetSpawnRate</c> for that player's
/// budget, rolls <c>Main.rand.Next(spawnRate) == 0</c>, finds a ground tile off screen, picks a
/// creature and stops after the first success. This system keeps that sequence and replaces the
/// hard-coded creature table with <see cref="NpcSpawnTable"/>, so content grows by registering
/// candidates rather than by editing spawn logic.
///
/// <para>
/// The system owns no state between ticks: the random cursor is derived from the world seed and
/// the tick, and every count is read back from the entity store. The reference's per-player
/// <c>nearbyActiveNPCs</c> counter is recomputed here from positions inside the spawn area.
/// </para>
/// </remarks>
public sealed class NpcSpawnSystem : BaseSystem<ArchWorld, EcsServerTickContext>
{
  /// <summary>Version4 <c>Main.maxNPCs</c>.</summary>
  public const int MaximumNpcs = 200;

  private readonly IWorldTileGrid _tiles;
  private readonly IEntityRuntimeResolver _resolver;
  private readonly EntityDefinitionSystem<ArchEntity> _spawner;
  private readonly NpcAiRuntime _ai;
  private readonly NpcSpawnTable _table;
  private readonly long _worldSeed;
  private readonly List<SessionPlayer> _players = [];
  private readonly List<ArchEntity> _npcs = [];
  private readonly QueryDescription _npcQuery = new QueryDescription().WithAll<
      NpcBehaviorStateComponent,
      LocationComponent,
      ColliderComponent>();
  private NpcSpawnFactQueueComponent? _facts;

  public NpcSpawnSystem(
      ArchWorld ecsWorld,
      IWorldTileGrid tiles,
      IEntityRuntimeResolver resolver,
      EntityDefinitionSystem<ArchEntity> spawner,
      NpcAiRuntime ai,
      NpcSpawnTable table,
      NpcSpawnEnvironment environment,
      long worldSeed)
      : base(ecsWorld ?? throw new ArgumentNullException(nameof(ecsWorld)))
  {
    _tiles = tiles ?? throw new ArgumentNullException(nameof(tiles));
    _resolver = resolver ?? throw new ArgumentNullException(nameof(resolver));
    _spawner = spawner ?? throw new ArgumentNullException(nameof(spawner));
    _ai = ai ?? throw new ArgumentNullException(nameof(ai));
    _table = table ?? throw new ArgumentNullException(nameof(table));
    if (!environment.IsValid)
    {
      throw new ArgumentException("The spawn environment layers must be ordered.", nameof(environment));
    }

    Environment = environment;
    _worldSeed = worldSeed;
  }

  /// <summary>World flags used by the rate rules; replaced when the world clock advances.</summary>
  public NpcSpawnEnvironment Environment { get; set; }

  // 在查询之外解析队列实体：Require 内部本身要跑一次查询。
  private NpcSpawnFactQueueComponent Facts => _facts ??= NpcSpawnFactQueue.Require(World);

  public override void Update(in EcsServerTickContext context)
  {
    SessionPlayerRoster.Collect(World, _resolver, _players);
    if (_players.Count == 0)
    {
      return;
    }

    NpcSpawnFactQueueComponent facts = Facts;
    CollectNpcs();
    if (_npcs.Count >= MaximumNpcs)
    {
      return;
    }

    float tileSize = _tiles.TileSize;
    NpcSpawnRandom random = NpcSpawnRandom.ForTick(_worldSeed, context.Tick);
    for (int index = 0; index < _players.Count; index++)
    {
      SessionPlayer player = _players[index];
      if (!player.IsAlive)
      {
        continue;
      }

      NpcSpawnArea area = NpcSpawnAreaRules.GetSpawnArea(
          player.Position,
          tileSize,
          _tiles.Width,
          _tiles.Height);
      NpcSpawnBudget budget = NpcSpawnBudgetRules.For(Environment, player.Position, tileSize);
      if (CountHostileNpcsInside(area.Spawn) >= budget.MaximumSpawns)
      {
        continue;
      }

      if (random.Next(budget.SpawnRate) != 0)
      {
        continue;
      }

      if (!NpcSpawnAreaRules.TryFindSpawnTile(area, _tiles, ref random, out WorldTileCoordinate ground))
      {
        continue;
      }

      if (NpcSpawnAreaRules.IsVisibleToAnyPlayer(
          ground.X,
          ground.Y,
          tileSize,
          _players,
          area.SafeHalfExtentX,
          area.SafeHalfExtentY))
      {
        continue;
      }

      NpcSpawnCandidate candidate = _table.Select(random.Next(_table.TotalWeight));
      Spawn(ground, candidate, tileSize, facts);
      // Version4 每个 tick 最多成功一次刷怪。
      return;
    }
  }

  private void Spawn(
      WorldTileCoordinate groundTile,
      NpcSpawnCandidate candidate,
      float tileSize,
      NpcSpawnFactQueueComponent facts)
  {
    ArchEntity npc = _spawner.Spawn(candidate.DefinitionId);
    Vector2 size = World.Get<ColliderComponent>(npc).Size;
    // 贴地：底边对齐地面瓦片顶边，水平在建制宽度内居中。
    World.Set(
        npc,
        new LocationComponent(new Vector2(
            groundTile.X * tileSize + (tileSize - size.X) / 2f,
            groundTile.Y * tileSize - size.Y)));
    World.Set(npc, MovementVelocityComponent.Stationary);
    _ai.Attach(npc, candidate.Ai);
    facts.Enqueue(new NpcSpawnedFact(npc, candidate.NetworkType));
  }

  private void CollectNpcs()
  {
    _npcs.Clear();
    World.Query(in _npcQuery, (ArchEntity entity) => _npcs.Add(entity));
  }

  private int CountHostileNpcsInside(WorldTileArea area)
  {
    int count = 0;
    for (int index = 0; index < _npcs.Count; index++)
    {
      ArchEntity entity = _npcs[index];
      if (!World.Has<EntityFactionComponent>(entity) ||
          World.Get<EntityFactionComponent>(entity).Value != Faction.Hostile)
      {
        continue;
      }

      if (!World.Has<LocationComponent>(entity) || !World.Has<ColliderComponent>(entity))
      {
        continue;
      }

      Vector2 position = World.Get<LocationComponent>(entity).Coordinates;
      Vector2 size = World.Get<ColliderComponent>(entity).Size;
      float tileSize = _tiles.TileSize;
      int centerX = (int)((position.X + size.X / 2f) / tileSize);
      int centerY = (int)((position.Y + size.Y / 2f) / tileSize);
      if (area.Contains(centerX, centerY))
      {
        count++;
      }
    }

    return count;
  }
}

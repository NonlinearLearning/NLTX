using System.Numerics;
using MinimalImplementation.Combat.Npcs;
using MinimalImplementation.Combat.Projectiles;
using MinimalImplementation.Contracts;
using MinimalImplementation.Server.Mechanisms;
using MinimalImplementation.Server.World;

namespace MinimalImplementation.Server.Host;

/// <summary>Domain query ports over one detached P03 world and NPC capture.</summary>
public sealed class WorldSpatialQueries : INpcMotionQuery, IProjectileCollisionQuery,
    ILeashedEntityEnvironment {
  private readonly WorldSpatialSnapshot _world;
  private readonly IReadOnlyList<NpcView> _npcs;

  public WorldSpatialQueries(WorldSpatialSnapshot world, IReadOnlyList<NpcView> npcs) {
    ArgumentNullException.ThrowIfNull(world);
    ArgumentNullException.ThrowIfNull(npcs);
    _world = world;
    _npcs = Array.AsReadOnly(npcs.ToArray());
    WorldScope = checked((long)world.WorldEpoch);
  }

  public long WorldScope { get; }
  public ulong WorldEpoch => _world.WorldEpoch;
  public CommitVersion Version => _world.Version;
  public int Width => _world.Header.Geometry.Width;
  public int Height => _world.Header.Geometry.Height;

  public bool IsWet(NpcView npc) {
    return _world.GetLiquidContact(npc.Position, npc.Width, npc.Height).Wet;
  }

  public NpcMotionResult Move(NpcView npc, Vector2 desiredVelocity) {
    WorldMotionResult result = _world.Move(
        npc.Position, desiredVelocity, npc.Width, npc.Height);
    return new NpcMotionResult(result.Position, result.Velocity, result.CollideX, result.CollideY);
  }

  public bool HasSolidTileCollision(Vector2 position, int width, int height) {
    return _world.HasSolidTileCollision(position, width, height);
  }

  public IReadOnlyList<ProjectileCollisionTarget> QueryNpcHits(
      ProjectileView projectile, Vector2 position) {
    var hits = new List<ProjectileCollisionTarget>();
    foreach (NpcView npc in _npcs) {
      if (npc.Life > 0 && position.X + projectile.Width > npc.Position.X &&
          position.X < npc.Position.X + npc.Width &&
          position.Y + projectile.Height > npc.Position.Y &&
          position.Y < npc.Position.Y + npc.Height) {
        hits.Add(new ProjectileCollisionTarget(npc.Entity));
      }
    }
    return hits.AsReadOnly();
  }

  public bool IsSolidTile2(int x, int y) {
    return _world.IsSolidTile2(x, y);
  }

  public bool HasLiquid(int x, int y, int? liquidType = null) {
    return _world.HasLiquid(x, y, liquidType);
  }

  public bool ContainsPylonTile(WorldSectionBounds interactionRectangle) {
    return _world.ContainsActiveTile(interactionRectangle, 597);
  }
}

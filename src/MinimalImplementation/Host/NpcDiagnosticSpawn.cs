using System.Numerics;
using MinimalImplementation.Combat.Npcs;
using MinimalImplementation.Contracts;
using MinimalImplementation.Server.World;

namespace MinimalImplementation.Server.Host;

/// <summary>Explicit runtime-only actors for the requested isolated AI/damage experiment.</summary>
public static class NpcDiagnosticSpawn {
  public static void Create(ServerComposition composition) {
    if (composition.NpcManager.ActiveCount != 0) {
      throw new InvalidOperationException("Diagnostic spawning requires an empty runtime NPC owner.");
    }
    WorldHeaderCapture header = composition.WorldOwner.CaptureWorldHeader();
    TileCoordinate spawn = header.Header.Spawn;
    Spawn(composition, 1, FindPosition(composition, spawn.X + 12, spawn.Y, 24, 18), 25,
        null, null);
    Spawn(composition, 22, FindPosition(composition, spawn.X - 8, spawn.Y, 18, 40), 250,
        new NpcHomeState(true, spawn.X - 8, spawn.Y), null);
  }

  private static Vector2 FindPosition(
      ServerComposition composition, int x, int y, int width, int height) {
    WorldGeometry geometry = composition.WorldOwner.CaptureWorldHeader().Header.Geometry;
    int left = Math.Clamp(x - 4, 0, geometry.Width - 16);
    int top = Math.Clamp(y - 25, 0, geometry.Height - 80);
    WorldSpatialSnapshot spatial = composition.WorldOwner.CaptureSpatialSnapshot(
        new WorldSectionBounds(left, top, 16, 80));
    for (int tileY = top + 7; tileY < top + 74; tileY++) {
      Vector2 position = new(x * 16, tileY * 16 - height);
      if (!spatial.HasSolidTileCollision(position, width, height) &&
          spatial.Move(position, new Vector2(0, 2), width, height).CollideY) {
        return position;
      }
    }
    throw new InvalidOperationException("No usable diagnostic NPC ground near world spawn.");
  }

  private static void Spawn(
      ServerComposition composition, short type, Vector2 position, int life,
      NpcHomeState? home, int? maximumLifeOverride) {
    var state = new NpcStateUpdate(position, Vector2.Zero, 0, true, true, true,
        [0, 0, 0, 0], life, life, false, true, false, null, null, null);
    var prepared = composition.NpcManager.PrepareSpawn(
        new NpcSpawnRequest(type, state, home, maximumLifeOverride));
    if (!prepared.IsPrepared || prepared.Plan is null) {
      throw new InvalidOperationException(prepared.Result.Detail);
    }
    var committed = composition.NpcManager.Commit(prepared.Plan);
    if (committed.Result.Status != OwnerCommitStatus.Applied || committed.Npc is null) {
      throw new InvalidOperationException(committed.Result.Detail);
    }
    byte generation = composition.NpcNetwork.GetGeneration(committed.Npc);
    ServerLog.Info("server.npc.diagnostic_spawn",
        $"slot={committed.Npc.Slot} generation={generation} type={type} " +
        $"life={life} maximumLife={life} position={position}");
  }
}

using System.Numerics;
using MinimalImplementation.Combat.Npcs;
using MinimalImplementation.Server.Mechanisms;
using MinimalImplementation.Server.World;
using static MinimalImplementation.Server.Protocol.Version4ModulePayloads;

namespace MinimalImplementation.Server.Host;

/// <summary>Captures each critter's bounded environment within its owner commit window.</summary>
public sealed class WorldLeashedEntityMotionPort : ILeashedEntityMotionPort {
  private readonly WorldOwner _world;

  public WorldLeashedEntityMotionPort(WorldOwner world) {
    _world = world ?? throw new ArgumentNullException(nameof(world));
  }

  public bool TryAdvance(
      LeashedEntityRuntimeSnapshot entity, long tick, out LeashedEntityMotionStep? step) {
    ArgumentNullException.ThrowIfNull(entity);
    if (entity.State is not CritterLeashedEntityState critter) {
      throw new NotSupportedException("Kite motion requires its own migrated environment rules.");
    }
    WorldHeaderCapture current = _world.CaptureWorldHeader();
    if (checked((long)current.WorldEpoch) != entity.Handle.WorldScope) {
      throw new InvalidOperationException("The critter belongs to a different world epoch.");
    }
    Vector2 position = LeashedCritterMotionService.CapturePosition(entity);
    int width = checked((int)critter.SizeX!.Value);
    int height = checked((int)critter.SizeY!.Value);
    WorldSectionBounds body = WorldSpatialSnapshot.GetQueryBounds(
        current.Header.Geometry, position, width, height);
    // Jumper target searches reach twelve tiles; include its ground and ceiling probes.
    int left = Math.Max(0, Math.Min(body.OriginX, entity.Anchor.X - 16));
    int top = Math.Max(0, Math.Min(body.OriginY, entity.Anchor.Y - 16));
    int right = Math.Min(current.Header.Geometry.Width,
        Math.Max(body.OriginX + body.Width, entity.Anchor.X + 17));
    int bottom = Math.Min(current.Header.Geometry.Height,
        Math.Max(body.OriginY + body.Height, entity.Anchor.Y + 17));
    WorldSpatialSnapshot captured = _world.CaptureSpatialSnapshot(
        new WorldSectionBounds(left, top, right - left, bottom - top));
    var queries = new WorldSpatialQueries(captured, Array.Empty<NpcView>());
    var motion = new LeashedCritterMotionService(() => queries);
    bool advanced = motion.TryAdvance(entity, tick, out step);
    if (!_world.IsCurrentWorldRevision(captured.WorldEpoch, captured.Version)) {
      throw new InvalidOperationException("The world changed during critter motion preparation.");
    }
    return advanced;
  }
}

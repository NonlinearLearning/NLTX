using System.Numerics;

namespace Terraria.Npc;

public readonly record struct NpcGuideSourceProfileInput(
  int TypeId,
  int NetId,
  int AiStyle,
  Vector2 Position,
  Vector2 Velocity,
  NpcGuideSourceProfileState State,
  bool DayTime,
  bool Raining,
  bool Eclipse,
  bool SlimeRain,
  bool IsStorming,
  float WorldSurface,
  bool ServerAuthority,
  bool TownNpc,
  bool Homeless,
  bool InGoodRestingSpot,
  bool CurrentAreaOccupiedByPlayer,
  bool HomeAreaOccupiedByPlayer,
  bool HasHome,
  int HomeTileX,
  int HomeTileY,
  int Width,
  int Height,
  INpcHomeReturnCollisionQuery? CollisionQuery,
  // Kept for source-slice input compatibility. The source always calls the
  // sitting helper after a successful teleport; the owner decides its result.
  bool SittingCandidateAvailable = false);

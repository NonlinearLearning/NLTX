using System.Numerics;

namespace Terraria.Npc;

public readonly record struct NpcGuideSourceProfileResult(
  Vector2 Position,
  Vector2 Velocity,
  NpcGuideSourceProfileState State,
  bool ReturnPressureActive,
  bool HomeReturnEligible,
  bool HomeTeleportRequested,
  bool HomeTeleportSucceeded,
  bool HomeTeleportFailed,
  bool NetworkUpdateRequested,
  bool ForceSittingRequested,
  NpcGuideForceSittingRequest ForceSittingRequest,
  bool HomelessUpdateRequested,
  bool QuickFindHomeRequested,
  int HomeTeleportCandidateOffset,
  Vector2 HomeTeleportPosition,
  NpcTaskFailureReason FailureReason,
  NpcGuideSourceBranch Branches);

public readonly record struct NpcGuideSourceEffectApplicationResult(
  bool HomeTeleportApplied,
  bool NetworkSyncRequested,
  bool ForceSittingCommitted,
  bool HomelessMarked,
  bool QuickFindHomeRequested);

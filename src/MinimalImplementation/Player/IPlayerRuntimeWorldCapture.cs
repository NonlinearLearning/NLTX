using MinimalImplementation.Contracts;
using MinimalImplementation.Server.Identity;
using MinimalImplementation.Server.Session;
using Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

namespace MinimalImplementation.Server.Player;

/// <summary>Captured world-owned inputs used while Player computes its own equipment effects.</summary>
public interface IPlayerRuntimeWorldCapture
{
  SessionEpoch Epoch { get; }

  EntityHandle PlayerHandle { get; }

  CommitVersion PlayerVersion { get; }

  CommitVersion Revision { get; }

  int DayRate { get; }

  bool LanternsUp { get; }

  bool JourneyWorld { get; }

  bool ExpertMode { get; }

  bool MasterMode { get; }

  bool FarPlacementPowerUnlocked { get; }

  bool FarPlacementPowerEnabled { get; }

  bool? IsWet { get; }

  bool? CanFitPortableStool { get; }

  /// <summary>Null means the collision owner has not captured TryChangingSizeFromBottomCenter.</summary>
  bool? CanFitDismountedPlayer { get; }

  /// <summary>Result top-left from the collision owner's bottom-center size-change attempt.</summary>
  PacketVector2 DismountedPlayerPosition { get; }

  OwnerCommitResult Validate();
}

/// <summary>Reads current world and collision facts for one already resolved Player binding.</summary>
public interface IPlayerRuntimeWorldCaptureSource
{
  IPlayerRuntimeWorldCapture? Capture(
    SessionBinding binding,
    Player player,
    PlayerMovementInput? pendingMovement = null,
    PlayerRuntimeSpatialState? pendingSpatial = null);
}

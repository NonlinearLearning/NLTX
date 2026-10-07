using System.Numerics;

namespace Terraria.Npc;

/// <summary>
/// Pure first slice of Terraria's type=22 / netID=22 / aiStyle=7
/// AI_007_TownEntities behavior.
///
/// The profile covers the weather return pressure, the server-side home-return
/// gate, the source home candidate query, and the observable effect order. Path
/// prediction, danger avoidance, conversation, and the full sitting helper stay
/// outside this slice.
/// </summary>
public static class NpcGuideSourceProfile
{
  public static bool CanHandle(int typeId, int netId, int aiStyle)
  {
    return typeId == 22 && netId == 22 && aiStyle == 7;
  }

  public static NpcGuideSourceProfileResult Evaluate(
    in NpcGuideSourceProfileInput input)
  {
    if (!CanHandle(input.TypeId, input.NetId, input.AiStyle))
    {
      throw new InvalidOperationException(
        "Guide source profile requires type=22, netID=22, and aiStyle=7.");
    }

    bool returnPressureActive = input.Raining ||
      !input.DayTime ||
      input.Eclipse ||
      input.SlimeRain ||
      (input.IsStorming && input.Position.Y / 16f < input.WorldSurface);

    NpcGuideSourceBranch branches = returnPressureActive
      ? NpcGuideSourceBranch.WeatherReturnPressure
      : NpcGuideSourceBranch.None;
    NpcGuideSourceProfileState state = input.State;

    bool serverReturnAuthority = input.ServerAuthority;
    if (serverReturnAuthority)
    {
      branches |= NpcGuideSourceBranch.ServerReturnAuthority;
    }

    bool homeReturnEligible = returnPressureActive &&
      serverReturnAuthority &&
      input.TownNpc &&
      !input.Homeless &&
      !input.InGoodRestingSpot &&
      input.HasHome;
    if (!homeReturnEligible)
    {
      return CreateResult(
        input,
        state,
        returnPressureActive,
        homeReturnEligible: false,
        branches);
    }

    branches |= NpcGuideSourceBranch.HomeReturnEligibility;
    if (input.CurrentAreaOccupiedByPlayer || input.HomeAreaOccupiedByPlayer)
    {
      branches |= NpcGuideSourceBranch.PlayerOccupancyBlocked;
      return CreateResult(
        input,
        state,
        returnPressureActive,
        homeReturnEligible: true,
        branches);
    }

    branches |= NpcGuideSourceBranch.HomeDestinationProbe;
    if (input.CollisionQuery is null)
    {
      throw new ArgumentNullException(
        nameof(input.CollisionQuery),
        "An eligible Guide home return requires a collision query owned by the caller.");
    }
    if (!NpcHomeReturnDestinationQuery.TryFindDestination(
          input.HomeTileX,
          input.HomeTileY,
          input.Width,
          input.Height,
          input.CollisionQuery,
          out NpcHomeReturnDestination destination))
    {
      branches |= NpcGuideSourceBranch.HomeReturnNoPath |
        NpcGuideSourceBranch.HomeReassignment;
      return new NpcGuideSourceProfileResult(
        input.Position,
        input.Velocity,
        state,
        returnPressureActive,
        HomeReturnEligible: true,
        HomeTeleportRequested: true,
        HomeTeleportSucceeded: false,
        HomeTeleportFailed: true,
        NetworkUpdateRequested: false,
        ForceSittingRequested: false,
        ForceSittingRequest: new NpcGuideForceSittingRequest(
          input.HomeTileX,
          input.HomeTileY),
        HomelessUpdateRequested: true,
        QuickFindHomeRequested: true,
        HomeTeleportCandidateOffset: 0,
        HomeTeleportPosition: Vector2.Zero,
        FailureReason: NpcTaskFailureReason.NoPath,
        Branches: branches);
    }

    branches |= NpcGuideSourceBranch.HomeTeleport |
      NpcGuideSourceBranch.NetworkSynchronization;
    branches |= NpcGuideSourceBranch.ForceSitting;

    return new NpcGuideSourceProfileResult(
      destination.Position,
      Vector2.Zero,
      state,
      returnPressureActive,
      HomeReturnEligible: true,
      HomeTeleportRequested: true,
      HomeTeleportSucceeded: true,
      HomeTeleportFailed: false,
      NetworkUpdateRequested: true,
      ForceSittingRequested: true,
      ForceSittingRequest: new NpcGuideForceSittingRequest(
        input.HomeTileX,
        input.HomeTileY),
      HomelessUpdateRequested: false,
      QuickFindHomeRequested: false,
      HomeTeleportCandidateOffset: destination.CandidateOffset,
      HomeTeleportPosition: destination.Position,
      FailureReason: NpcTaskFailureReason.None,
      Branches: branches);
  }

  public static void ApplyEffects(
    in NpcGuideSourceProfileResult result,
    INpcGuideSourceEffectPort effectPort)
  {
    _ = ApplyEffectsAndObserve(in result, effectPort);
  }

  public static NpcGuideSourceEffectApplicationResult ApplyEffectsAndObserve(
    in NpcGuideSourceProfileResult result,
    INpcGuideSourceEffectPort effectPort)
  {
    ArgumentNullException.ThrowIfNull(effectPort);

    bool homeTeleportApplied = false;
    bool networkSyncRequested = false;
    bool forceSittingCommitted = false;
    bool homelessMarked = false;
    bool quickFindHomeRequested = false;

    if (result.HomeTeleportSucceeded)
    {
      effectPort.ApplyHomeTeleport(result.HomeTeleportPosition, result.Velocity);
      homeTeleportApplied = true;
      if (result.NetworkUpdateRequested)
      {
        effectPort.RequestNetworkSync();
        networkSyncRequested = true;
      }

      if (result.ForceSittingRequested)
      {
        NpcGuideForceSittingRequest sittingRequest = result.ForceSittingRequest;
        forceSittingCommitted = effectPort.TryForceSitting(
          in sittingRequest);
      }
    }

    if (result.HomeTeleportFailed && result.HomelessUpdateRequested)
    {
      effectPort.MarkHomeless();
      homelessMarked = true;
      if (result.QuickFindHomeRequested)
      {
        effectPort.RequestQuickFindHome();
        quickFindHomeRequested = true;
      }
    }

    return new NpcGuideSourceEffectApplicationResult(
      homeTeleportApplied,
      networkSyncRequested,
      forceSittingCommitted,
      homelessMarked,
      quickFindHomeRequested);
  }

  private static NpcGuideSourceProfileResult CreateResult(
    in NpcGuideSourceProfileInput input,
    NpcGuideSourceProfileState state,
    bool returnPressureActive,
    bool homeReturnEligible,
    NpcGuideSourceBranch branches)
  {
    return new NpcGuideSourceProfileResult(
      input.Position,
      input.Velocity,
      state,
      returnPressureActive,
      homeReturnEligible,
      HomeTeleportRequested: false,
      HomeTeleportSucceeded: false,
      HomeTeleportFailed: false,
      NetworkUpdateRequested: false,
      ForceSittingRequested: false,
      ForceSittingRequest: new NpcGuideForceSittingRequest(
        input.HomeTileX,
        input.HomeTileY),
      HomelessUpdateRequested: false,
      QuickFindHomeRequested: false,
      HomeTeleportCandidateOffset: 0,
      HomeTeleportPosition: Vector2.Zero,
      FailureReason: NpcTaskFailureReason.None,
      Branches: branches);
  }
}

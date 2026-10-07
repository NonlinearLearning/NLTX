namespace Terraria.Npc;

public readonly record struct NpcGuidePartnerStateResetRequest(
  int EntityId,
  NpcGuideSourceProfileState State,
  int Direction);

public readonly record struct NpcGuideDangerProfileInput(
  int TypeId,
  int NetId,
  int AiStyle,
  NpcGuideSourceProfileState State,
  int Direction,
  bool ServerAuthority,
  bool InfectedSeed,
  bool PlayerTalking,
  int ConversationPartnerEntityId,
  bool ConversationPartnerActive,
  float PrettySafeDistance,
  bool WalkAvoidFalling,
  NpcGuideDangerScanResult Scan);

[Flags]
public enum NpcGuideDangerProfileBranch
{
  None = 0,
  DangerDirectionSelected = 1 << 0,
  PrettySafeSuppressed = 1 << 1,
  AiEightReturnedToWalk = 1 << 2,
  WalkStateEntered = 1 << 3,
  WalkDirectionCorrected = 1 << 4,
  PartnerResetRequested = 1 << 5,
  NetworkSynchronization = 1 << 6,
}

public readonly record struct NpcGuideDangerProfileResult(
  NpcGuideSourceProfileState State,
  int Direction,
  int PreferredDirection,
  bool NetworkUpdateRequested,
  NpcGuidePartnerStateResetRequest? PartnerReset,
  NpcGuideDangerProfileBranch Branches);

public interface INpcGuideDangerRandomPort
{
  int Next(int maxExclusive);
}

public interface INpcGuideDangerEffectPort
{
  void RequestPartnerStateReset(in NpcGuidePartnerStateResetRequest request);

  void RequestNetworkSync();
}

/// <summary>
/// Pure state decision for the source danger response after the explicit
/// snapshot scan. Partner NPC mutation is returned as a capability request.
/// </summary>
public static class NpcGuideDangerProfile
{
  public static NpcGuideDangerProfileResult Evaluate(
    in NpcGuideDangerProfileInput input,
    int random300Roll = 0,
    int random120OwnRoll = 0,
    int random120PartnerRoll = 0)
  {
    if (!NpcGuideSourceProfile.CanHandle(input.TypeId, input.NetId, input.AiStyle))
    {
      throw new InvalidOperationException(
        "Guide danger profile requires type=22, netID=22, and aiStyle=7.");
    }

    NpcGuideDangerScanResult scan = input.Scan;
    int preferredDirection = SelectDirection(in scan);
    NpcGuideSourceProfileState state = input.State;
    int direction = input.Direction;
    bool networkUpdateRequested = false;
    NpcGuidePartnerStateResetRequest? partnerReset = null;
    NpcGuideDangerProfileBranch branches = preferredDirection == 0
      ? NpcGuideDangerProfileBranch.None
      : NpcGuideDangerProfileBranch.DangerDirectionSelected;

    if (!input.ServerAuthority ||
        input.PlayerTalking ||
        !input.Scan.DangerWithinBaseRange)
    {
      return new NpcGuideDangerProfileResult(
        state,
        direction,
        preferredDirection,
        networkUpdateRequested,
        partnerReset,
        branches);
    }

    float nearestDangerDistance = NearestDangerDistance(in scan);
    if (input.State.Ai0 == 8f)
    {
      if (direction == -preferredDirection)
      {
        state = state with
        {
          Ai0 = 1f,
          Ai1 = 300f + random300Roll,
          Ai2 = 0f,
          LocalAi3 = 0f,
        };
        networkUpdateRequested = true;
        branches |= NpcGuideDangerProfileBranch.AiEightReturnedToWalk |
          NpcGuideDangerProfileBranch.NetworkSynchronization;
      }

      return new NpcGuideDangerProfileResult(
        state,
        direction,
        preferredDirection,
        networkUpdateRequested,
        partnerReset,
        branches);
    }

    if (state.Ai0 is 10f or 12f or 13f or 14f or 15f)
    {
      return new NpcGuideDangerProfileResult(
        state,
        direction,
        preferredDirection,
        networkUpdateRequested,
        partnerReset,
        branches);
    }

    if (input.PrettySafeDistance != -1 &&
        input.PrettySafeDistance < nearestDangerDistance)
    {
      return new NpcGuideDangerProfileResult(
        state,
        direction,
        preferredDirection,
        networkUpdateRequested,
        partnerReset,
        branches | NpcGuideDangerProfileBranch.PrettySafeSuppressed);
    }

    if (state.Ai0 == 1f)
    {
      if (!input.InfectedSeed && direction != -preferredDirection)
      {
        direction = -preferredDirection;
        networkUpdateRequested = true;
        branches |= NpcGuideDangerProfileBranch.WalkDirectionCorrected |
          NpcGuideDangerProfileBranch.NetworkSynchronization;
      }

      return new NpcGuideDangerProfileResult(
        state,
        direction,
        preferredDirection,
        networkUpdateRequested,
        partnerReset,
        branches);
    }

    if (input.WalkAvoidFalling)
    {
      return new NpcGuideDangerProfileResult(
        state,
        direction,
        preferredDirection,
        networkUpdateRequested,
        partnerReset,
        branches);
    }

    int partnerEntityId = input.ConversationPartnerActive
      ? input.ConversationPartnerEntityId
      : -1;
    if (state.Ai0 is 3f or 4f or 16f or 17f &&
        partnerEntityId != -1)
    {
      NpcGuideSourceProfileState partnerState = new(
        Ai0: 1f,
        Ai1: 120f + random120PartnerRoll,
        Ai2: 0f,
        LocalAi3: 0f);
      partnerReset = new NpcGuidePartnerStateResetRequest(
        partnerEntityId,
        partnerState,
        -preferredDirection);
      branches |= NpcGuideDangerProfileBranch.PartnerResetRequested;
    }

    state = state with
    {
      Ai0 = 1f,
      Ai1 = 120f + random120OwnRoll,
      Ai2 = 0f,
      LocalAi3 = 0f,
    };
    direction = input.InfectedSeed ? preferredDirection : -preferredDirection;
    networkUpdateRequested = true;
    branches |= NpcGuideDangerProfileBranch.WalkStateEntered |
      NpcGuideDangerProfileBranch.NetworkSynchronization;
    return new NpcGuideDangerProfileResult(
      state,
      direction,
      preferredDirection,
      networkUpdateRequested,
      partnerReset,
      branches);
  }

  public static NpcGuideDangerProfileResult EvaluateWithRandom(
    in NpcGuideDangerProfileInput input,
    INpcGuideDangerRandomPort randomPort)
  {
    ArgumentNullException.ThrowIfNull(randomPort);
    int random300Roll = 0;
    int random120OwnRoll = 0;
    int random120PartnerRoll = 0;
    if (input.ServerAuthority &&
        !input.PlayerTalking &&
        input.Scan.DangerWithinBaseRange)
    {
      NpcGuideDangerScanResult scan = input.Scan;
      int preferredDirection = SelectDirection(in scan);
      if (input.State.Ai0 == 8f &&
          input.Direction == -preferredDirection)
      {
        random300Roll = randomPort.Next(300);
      }
      else if (input.State.Ai0 is not 10f and not 12f and not 13f and not 14f and not 15f &&
               input.State.Ai0 != 1f &&
               !input.WalkAvoidFalling &&
               (input.PrettySafeDistance == -1 ||
                input.PrettySafeDistance >= NearestDangerDistance(in scan)))
      {
        int partnerEntityId = input.ConversationPartnerActive
          ? input.ConversationPartnerEntityId
          : -1;
        if (input.State.Ai0 is 3f or 4f or 16f or 17f &&
            partnerEntityId != -1)
        {
          random120PartnerRoll = randomPort.Next(120);
        }

        random120OwnRoll = randomPort.Next(120);
      }
    }

    return Evaluate(in input, random300Roll, random120OwnRoll, random120PartnerRoll);
  }

  public static void ApplyEffects(
    in NpcGuideDangerProfileResult result,
    INpcGuideDangerEffectPort effectPort)
  {
    ArgumentNullException.ThrowIfNull(effectPort);
    if (result.PartnerReset is NpcGuidePartnerStateResetRequest partnerReset)
    {
      effectPort.RequestPartnerStateReset(in partnerReset);
    }

    if (result.NetworkUpdateRequested)
    {
      effectPort.RequestNetworkSync();
    }
  }

  private static int SelectDirection(in NpcGuideDangerScanResult scan)
  {
    if (scan.LeftNearestOffset == -1f)
    {
      return scan.RightNearestOffset == -1f ? 0 : 1;
    }

    if (scan.RightNearestOffset == -1f)
    {
      return -1;
    }

    return scan.RightNearestOffset < -scan.LeftNearestOffset ? 1 : -1;
  }

  private static float NearestDangerDistance(in NpcGuideDangerScanResult scan)
  {
    bool hasLeft = scan.LeftNearestOffset != -1f;
    bool hasRight = scan.RightNearestOffset != -1f;
    if (!hasLeft)
    {
      return hasRight ? scan.RightNearestOffset : 0f;
    }

    if (!hasRight)
    {
      return -scan.LeftNearestOffset;
    }

    return MathF.Min(-scan.LeftNearestOffset, scan.RightNearestOffset);
  }

}

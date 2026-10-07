using System.Numerics;

namespace Terraria.Npc;

public readonly record struct NpcGuideConversationTickInput(
  int TypeId,
  int NetId,
  int AiStyle,
  NpcGuideSourceProfileState State,
  Vector2 Velocity,
  int Direction,
  bool ConversationTargetValid,
  float NpcCenterX,
  float TargetCenterX);

[Flags]
public enum NpcGuideConversationTickBranch
{
  None = 0,
  LocalAi3Clamped = 1 << 0,
  HorizontalDamping = 1 << 1,
  ConversationTargetInvalid = 1 << 2,
  FacingTarget = 1 << 3,
  ExpiredToIdle = 1 << 4,
  NetworkSynchronization = 1 << 5,
}

public readonly record struct NpcGuideConversationTickResult(
  NpcGuideSourceProfileState State,
  Vector2 Velocity,
  int Direction,
  bool NetworkUpdateRequested,
  NpcGuideConversationTickBranch Branches);

public interface INpcGuideConversationTickRandomPort
{
  int Next(int maxExclusive);
}

/// <summary>
/// Source-shaped player-facing conversation tick for Guide conversation
/// states 6, 7, 18, and 19. Target lookup and line-of-sight checks are inputs.
/// </summary>
public static class NpcGuideConversationTickProfile
{
  public static NpcGuideConversationTickResult EvaluateWithRandom(
    in NpcGuideConversationTickInput input,
    INpcGuideConversationTickRandomPort randomPort)
  {
    ArgumentNullException.ThrowIfNull(randomPort);
    if (!NpcGuideSourceProfile.CanHandle(input.TypeId, input.NetId, input.AiStyle))
    {
      throw new InvalidOperationException(
        "Guide conversation tick requires type=22, netID=22, and aiStyle=7.");
    }

    if (input.State.Ai0 is not (6f or 7f or 18f or 19f))
    {
      return new NpcGuideConversationTickResult(
        input.State,
        input.Velocity,
        input.Direction,
        false,
        NpcGuideConversationTickBranch.None);
    }

    NpcGuideSourceProfileState state = input.State;
    NpcGuideConversationTickBranch branches =
      NpcGuideConversationTickBranch.HorizontalDamping;
    if (state.Ai0 == 18f && (state.LocalAi3 < 1f || state.LocalAi3 > 2f))
    {
      state = state with { LocalAi3 = 2f };
      branches |= NpcGuideConversationTickBranch.LocalAi3Clamped;
    }

    Vector2 velocity = input.Velocity with
    {
      X = input.Velocity.X * 0.8f,
    };
    state = state with { Ai1 = state.Ai1 - 1f };
    bool networkUpdateRequested = false;
    int direction = input.Direction;

    if (!input.ConversationTargetValid)
    {
      state = state with { Ai1 = 0f };
      branches |= NpcGuideConversationTickBranch.ConversationTargetInvalid;
    }

    if (state.Ai1 > 0f)
    {
      int targetDirection = input.NpcCenterX < input.TargetCenterX ? 1 : -1;
      if (targetDirection != direction)
      {
        direction = targetDirection;
        networkUpdateRequested = true;
        branches |= NpcGuideConversationTickBranch.FacingTarget |
          NpcGuideConversationTickBranch.NetworkSynchronization;
      }
    }
    else
    {
      state = state with
      {
        Ai0 = 0f,
        Ai1 = 60f + randomPort.Next(60),
        Ai2 = 0f,
        LocalAi3 = 30f + randomPort.Next(60),
      };
      networkUpdateRequested = true;
      branches |= NpcGuideConversationTickBranch.ExpiredToIdle |
        NpcGuideConversationTickBranch.NetworkSynchronization;
    }

    return new NpcGuideConversationTickResult(
      state,
      velocity,
      direction,
      networkUpdateRequested,
      branches);
  }
}

using System.Numerics;

namespace Terraria.Npc;

public readonly record struct NpcGuideAiEightInput(
  int TypeId,
  int NetId,
  int AiStyle,
  NpcGuideSourceProfileState State,
  Vector2 Velocity,
  bool DangerWithinBaseRange);

[Flags]
public enum NpcGuideAiEightBranch
{
  None = 0,
  HorizontalDamping = 1 << 0,
  DangerTimerRefreshed = 1 << 1,
  ExpiredToIdle = 1 << 2,
  NetworkSynchronization = 1 << 3,
}

public readonly record struct NpcGuideAiEightResult(
  NpcGuideSourceProfileState State,
  Vector2 Velocity,
  bool NetworkUpdateRequested,
  NpcGuideAiEightBranch Branches);

public interface INpcGuideAiEightRandomPort
{
  int Next(int maxExclusive);
}

/// <summary>
/// Source-shaped continuation for Guide ai0=8 after the danger response.
/// Danger scanning and state ownership remain outside this profile.
/// </summary>
public static class NpcGuideAiEightProfile
{
  public static NpcGuideAiEightResult EvaluateWithRandom(
    in NpcGuideAiEightInput input,
    INpcGuideAiEightRandomPort randomPort)
  {
    ArgumentNullException.ThrowIfNull(randomPort);
    if (!NpcGuideSourceProfile.CanHandle(input.TypeId, input.NetId, input.AiStyle))
    {
      throw new InvalidOperationException(
        "Guide ai0=8 profile requires type=22, netID=22, and aiStyle=7.");
    }

    if (input.State.Ai0 != 8f)
    {
      return new NpcGuideAiEightResult(
        input.State,
        input.Velocity,
        false,
        NpcGuideAiEightBranch.None);
    }

    NpcGuideSourceProfileState state = input.State with
    {
      Ai1 = input.State.Ai1 - 1f,
    };
    Vector2 velocity = input.Velocity with
    {
      X = input.Velocity.X * 0.8f,
    };
    bool networkUpdateRequested = false;
    NpcGuideAiEightBranch branches = NpcGuideAiEightBranch.HorizontalDamping;

    if (state.Ai1 < 60f && input.DangerWithinBaseRange)
    {
      state = state with { Ai1 = 180f };
      networkUpdateRequested = true;
      branches |= NpcGuideAiEightBranch.DangerTimerRefreshed |
        NpcGuideAiEightBranch.NetworkSynchronization;
    }

    if (state.Ai1 <= 0f)
    {
      state = state with
      {
        Ai0 = 0f,
        Ai1 = 60f + randomPort.Next(60),
        Ai2 = 0f,
        LocalAi3 = 30f + randomPort.Next(60),
      };
      networkUpdateRequested = true;
      branches |= NpcGuideAiEightBranch.ExpiredToIdle |
        NpcGuideAiEightBranch.NetworkSynchronization;
    }

    return new NpcGuideAiEightResult(
      state,
      velocity,
      networkUpdateRequested,
      branches);
  }
}

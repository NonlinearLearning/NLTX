using System.Numerics;

namespace Terraria.Npc;

public readonly record struct NpcGuideSittingRequest(
  int EntityId,
  int TileX,
  int TileY);

public readonly record struct NpcGuideSittingInput(
  int TypeId,
  int NetId,
  int AiStyle,
  int EntityId,
  NpcGuideSourceProfileState State,
  Vector2 Velocity,
  int SittingTileX,
  int SittingTileY,
  bool SittingTileIsChairOrBench);

[Flags]
public enum NpcGuideSittingBranch
{
  None = 0,
  HorizontalDamping = 1 << 0,
  SittingRegistered = 1 << 1,
  InvalidSeatExpired = 1 << 2,
  ExpiredToIdle = 1 << 3,
  NetworkSynchronization = 1 << 4,
}

public readonly record struct NpcGuideSittingResult(
  NpcGuideSourceProfileState State,
  Vector2 Velocity,
  NpcGuideSittingRequest? SittingRequest,
  bool NetworkUpdateRequested,
  NpcGuideSittingBranch Branches);

public interface INpcGuideSittingRandomPort
{
  int Next(int maxExclusive);
}

public interface INpcGuideSittingEffectPort
{
  void RegisterSitting(in NpcGuideSittingRequest request);

  void RequestNetworkSync();
}

/// <summary>
/// Source-shaped Guide ai0=5 sitting tick. Seat registration and network
/// effects remain explicit owner capabilities.
/// </summary>
public static class NpcGuideSittingProfile
{
  public static NpcGuideSittingResult EvaluateWithRandom(
    in NpcGuideSittingInput input,
    INpcGuideSittingRandomPort randomPort)
  {
    ArgumentNullException.ThrowIfNull(randomPort);
    if (!NpcGuideSourceProfile.CanHandle(input.TypeId, input.NetId, input.AiStyle))
    {
      throw new InvalidOperationException(
        "Guide sitting profile requires type=22, netID=22, and aiStyle=7.");
    }

    if (input.State.Ai0 != 5f)
    {
      return new NpcGuideSittingResult(
        input.State,
        input.Velocity,
        null,
        false,
        NpcGuideSittingBranch.None);
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
    NpcGuideSittingRequest? sittingRequest = null;
    NpcGuideSittingBranch branches = NpcGuideSittingBranch.HorizontalDamping;

    if (!input.SittingTileIsChairOrBench)
    {
      state = state with { Ai1 = 0f };
      branches |= NpcGuideSittingBranch.InvalidSeatExpired;
    }
    else
    {
      sittingRequest = new NpcGuideSittingRequest(
        input.EntityId,
        input.SittingTileX,
        input.SittingTileY);
      branches |= NpcGuideSittingBranch.SittingRegistered;
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
      branches |= NpcGuideSittingBranch.ExpiredToIdle |
        NpcGuideSittingBranch.NetworkSynchronization;
    }

    return new NpcGuideSittingResult(
      state,
      velocity,
      sittingRequest,
      networkUpdateRequested,
      branches);
  }

  public static void ApplyEffects(
    in NpcGuideSittingResult result,
    INpcGuideSittingEffectPort effectPort)
  {
    ArgumentNullException.ThrowIfNull(effectPort);
    if (result.SittingRequest is NpcGuideSittingRequest sittingRequest)
    {
      effectPort.RegisterSitting(in sittingRequest);
    }

    if (result.NetworkUpdateRequested)
    {
      effectPort.RequestNetworkSync();
    }
  }
}

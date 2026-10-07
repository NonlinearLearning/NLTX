namespace Terraria.Npc;

public readonly record struct NpcGuideIdleTalkNpcSnapshot(
  int EntityId,
  bool Active,
  bool CanBeTalkedTo,
  bool TownPet,
  bool Busy,
  bool Wet,
  float Distance,
  float CenterX,
  bool CanHit);

public readonly record struct NpcGuideIdleInput(
  int TypeId,
  int NetId,
  int AiStyle,
  NpcGuideSourceProfileState State,
  int Direction,
  float VelocityY,
  bool ServerAuthority,
  bool PlayerTalking,
  bool DangerWithinBaseRange,
  bool Wet,
  bool CanTalk,
  bool TownPet,
  int SelfEntityId,
  float CenterX,
  IReadOnlyList<NpcGuideIdleTalkNpcSnapshot> NearbyNpcs);

public readonly record struct NpcGuideIdlePartnerConversationRequest(
  int EntityId,
  NpcGuideSourceProfileState State,
  int Direction);

[Flags]
public enum NpcGuideIdleProfileBranch
{
  None = 0,
  IdleEligible = 1 << 0,
  NpcConversation = 1 << 1,
  NpcConversationRandomMiss = 1 << 2,
  PartnerConversation = 1 << 3,
  GenericIdle = 1 << 4,
  NetworkSynchronization = 1 << 5,
}

public readonly record struct NpcGuideIdleProfileResult(
  NpcGuideSourceProfileState State,
  int Direction,
  NpcGuideIdlePartnerConversationRequest? PartnerConversation,
  bool NetworkUpdateRequested,
  NpcGuideIdleProfileBranch Branches);

public interface INpcGuideIdleRandomPort
{
  int Next(int maxExclusive);
}

public interface INpcGuideIdleEffectPort
{
  void RequestPartnerConversation(
    in NpcGuideIdlePartnerConversationRequest request);

  void RequestNetworkSync();
}

/// <summary>
/// Source-shaped random idle and NPC-to-NPC conversation entry for Guide.
/// Partner state writes are returned as an explicit owner request.
/// </summary>
public static class NpcGuideIdleProfile
{
  public static NpcGuideIdleProfileResult EvaluateWithRandom(
    in NpcGuideIdleInput input,
    INpcGuideIdleRandomPort randomPort)
  {
    ArgumentNullException.ThrowIfNull(randomPort);
    if (!NpcGuideSourceProfile.CanHandle(input.TypeId, input.NetId, input.AiStyle))
    {
      throw new InvalidOperationException(
        "Guide idle profile requires type=22, netID=22, and aiStyle=7.");
    }

    NpcGuideSourceProfileState state = input.State;
    int direction = input.Direction;
    bool networkUpdateRequested = false;
    NpcGuideIdlePartnerConversationRequest? partnerConversation = null;
    NpcGuideIdleProfileBranch branches = NpcGuideIdleProfileBranch.None;
    bool idleEligible = state.Ai0 < 2f &&
      !input.DangerWithinBaseRange &&
      !input.Wet &&
      input.ServerAuthority &&
      !input.PlayerTalking;
    if (!idleEligible)
    {
      return new NpcGuideIdleProfileResult(
        state,
        direction,
        partnerConversation,
        networkUpdateRequested,
        branches);
    }

    branches |= NpcGuideIdleProfileBranch.IdleEligible;
    int conversationRoll = -1;
    if (input.CanTalk &&
        state.Ai0 == 0f &&
        input.VelocityY == 0f)
    {
      conversationRoll = randomPort.Next(300);
      if (conversationRoll == 0)
      {
        int duration = SelectConversationDuration(randomPort);
        NpcGuideIdleTalkNpcSnapshot? candidate = FindConversationCandidate(in input);
        if (candidate is NpcGuideIdleTalkNpcSnapshot npc)
        {
          int facingDirection = input.CenterX < npc.CenterX ? 1 : -1;
          state = state with
          {
            Ai0 = 3f,
            Ai1 = duration,
            Ai2 = npc.EntityId,
          };
          direction = facingDirection;
          partnerConversation = new NpcGuideIdlePartnerConversationRequest(
            npc.EntityId,
            new NpcGuideSourceProfileState(
              Ai0: 4f,
              Ai1: duration,
              Ai2: input.SelfEntityId,
              LocalAi3: 0f),
            -facingDirection);
          networkUpdateRequested = true;
          branches |= NpcGuideIdleProfileBranch.NpcConversation |
            NpcGuideIdleProfileBranch.PartnerConversation |
            NpcGuideIdleProfileBranch.NetworkSynchronization;
          return new NpcGuideIdleProfileResult(
            state,
            direction,
            partnerConversation,
            networkUpdateRequested,
            branches);
        }

        return new NpcGuideIdleProfileResult(
          state,
          direction,
          partnerConversation,
          networkUpdateRequested,
          branches | NpcGuideIdleProfileBranch.NpcConversationRandomMiss);
      }
    }
    if (conversationRoll != 0 &&
        input.CanTalk &&
        state.Ai0 == 0f &&
        input.VelocityY == 0f)
    {
      int secondConversationRoll = randomPort.Next(1800);
      if (secondConversationRoll == 0)
      {
        int duration = SelectConversationDuration(randomPort);
        NpcGuideIdleTalkNpcSnapshot? candidate = FindConversationCandidate(in input);
        if (candidate is NpcGuideIdleTalkNpcSnapshot npc && !npc.TownPet)
        {
          int facingDirection = input.CenterX < npc.CenterX ? 1 : -1;
          int localAi2 = randomPort.Next(4);
          int localAi3 = randomPort.Next(3 - localAi2);
          state = state with
          {
            Ai0 = 16f,
            Ai1 = duration,
            Ai2 = npc.EntityId,
            LocalAi2 = localAi2,
            LocalAi3 = localAi3,
          };
          direction = facingDirection;
          NpcGuideSourceProfileState partnerState =
            new NpcGuideSourceProfileState(
              Ai0: 17f,
              Ai1: duration,
              Ai2: input.SelfEntityId,
              LocalAi3: 0f)
            {
              LocalAi2 = 0f,
            };
          partnerConversation = new NpcGuideIdlePartnerConversationRequest(
            npc.EntityId,
            partnerState,
            -facingDirection);
          networkUpdateRequested = true;
          branches |= NpcGuideIdleProfileBranch.NpcConversation |
            NpcGuideIdleProfileBranch.PartnerConversation |
            NpcGuideIdleProfileBranch.NetworkSynchronization;
          return new NpcGuideIdleProfileResult(
            state,
            direction,
            partnerConversation,
            networkUpdateRequested,
            branches);
        }

        return new NpcGuideIdleProfileResult(
          state,
          direction,
          partnerConversation,
          networkUpdateRequested,
          branches | NpcGuideIdleProfileBranch.NpcConversationRandomMiss);
      }
    }

    if (!input.TownPet &&
        state.Ai0 == 0f &&
        input.VelocityY == 0f &&
        randomPort.Next(1800) == 0)
    {
      state = state with
      {
        Ai0 = 2f,
        Ai1 = 45f,
      };
      networkUpdateRequested = true;
      branches |= NpcGuideIdleProfileBranch.GenericIdle |
        NpcGuideIdleProfileBranch.NetworkSynchronization;
    }

    return new NpcGuideIdleProfileResult(
      state,
      direction,
      partnerConversation,
      networkUpdateRequested,
      branches);
  }

  public static void ApplyEffects(
    in NpcGuideIdleProfileResult result,
    INpcGuideIdleEffectPort effectPort)
  {
    ArgumentNullException.ThrowIfNull(effectPort);
    if (result.PartnerConversation is NpcGuideIdlePartnerConversationRequest request)
    {
      effectPort.RequestPartnerConversation(in request);
    }

    if (result.NetworkUpdateRequested)
    {
      effectPort.RequestNetworkSync();
    }
  }

  private static NpcGuideIdleTalkNpcSnapshot? FindConversationCandidate(
    in NpcGuideIdleInput input)
  {
    foreach (NpcGuideIdleTalkNpcSnapshot npc in input.NearbyNpcs)
    {
      if (npc.EntityId == input.SelfEntityId ||
          !npc.Active ||
          !npc.CanBeTalkedTo ||
          npc.TownPet ||
          npc.Busy ||
          npc.Wet ||
          npc.Distance >= 100f ||
          npc.Distance <= 20f ||
          !npc.CanHit)
      {
        continue;
      }

      return npc;
    }

    return null;
  }

  private static int SelectConversationDuration(INpcGuideIdleRandomPort randomPort)
  {
    int branch = randomPort.Next(2);
    int multiplier = branch != 0
      ? randomPort.Next(2) + 1
      : randomPort.Next(3) + 1;
    return 420 * multiplier;
  }
}

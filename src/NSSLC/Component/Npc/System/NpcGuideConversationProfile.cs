namespace Terraria.Npc;

[Flags]
public enum NpcGuideConversationBranch
{
  None = 0,
  PlayerTalking = 1 << 0,
  ConversationStateReset = 1 << 1,
  SpecialStatePreserved = 1 << 2,
  NetworkSynchronization = 1 << 3,
}

public readonly record struct NpcGuideConversationInput(
  int TypeId,
  int NetId,
  int AiStyle,
  NpcGuideSourceProfileState State,
  int Direction,
  bool PlayerIsTalking,
  float PlayerCenterX,
  float NpcCenterX);

public readonly record struct NpcGuideConversationResult(
  NpcGuideSourceProfileState State,
  int Direction,
  bool NetworkUpdateRequested,
  NpcGuideConversationBranch Branches);

public interface INpcGuideConversationEffectPort
{
  void RequestNetworkSync();
}

/// <summary>
/// The source conversation gate in AI_007_TownEntities: a talking player
/// resets ordinary movement state, while states 10/12/14/15 remain owned by
/// their specialized conversation branches.
/// </summary>
public static class NpcGuideConversationProfile
{
  public static NpcGuideConversationResult Evaluate(
    in NpcGuideConversationInput input)
  {
    if (!NpcGuideSourceProfile.CanHandle(input.TypeId, input.NetId, input.AiStyle))
    {
      throw new InvalidOperationException(
        "Guide conversation profile requires type=22, netID=22, and aiStyle=7.");
    }

    if (!input.PlayerIsTalking)
    {
      return new NpcGuideConversationResult(
        input.State,
        input.Direction,
        NetworkUpdateRequested: false,
        NpcGuideConversationBranch.None);
    }

    NpcGuideConversationBranch branches =
      NpcGuideConversationBranch.PlayerTalking;
    if (input.State.Ai0 is 10f or 12f or 14f or 15f)
    {
      return new NpcGuideConversationResult(
        input.State,
        input.Direction,
        NetworkUpdateRequested: false,
        branches | NpcGuideConversationBranch.SpecialStatePreserved);
    }

    int direction = input.PlayerCenterX < input.NpcCenterX ? -1 : 1;
    bool networkUpdateRequested = input.State.Ai0 != 0f;
    if (networkUpdateRequested)
    {
      branches |= NpcGuideConversationBranch.NetworkSynchronization;
    }

    return new NpcGuideConversationResult(
      input.State with
      {
        Ai0 = 0f,
        Ai1 = 300f,
        LocalAi3 = 100f,
      },
      direction,
      networkUpdateRequested,
      branches | NpcGuideConversationBranch.ConversationStateReset);
  }

  public static void ApplyEffects(
    in NpcGuideConversationResult result,
    INpcGuideConversationEffectPort effectPort)
  {
    ArgumentNullException.ThrowIfNull(effectPort);
    if (result.NetworkUpdateRequested)
    {
      effectPort.RequestNetworkSync();
    }
  }
}

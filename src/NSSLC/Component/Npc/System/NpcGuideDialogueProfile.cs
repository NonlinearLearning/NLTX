namespace Terraria.Npc;

public enum NpcGuideDialogueSelection
{
  SpecialEventText,
  BloodMoon170,
  BloodMoon171,
  BloodMoon172,
  LanternNightBeforeMoonlord,
  LanternNightAfterMoonlord,
  Eclipse,
  SlimeRain,
  Night173,
  HardmodeChatter1,
  HardmodeChatter2,
  Generic174,
  Generic175,
  Generic176,
}

public readonly record struct NpcGuideDialogueInput(
  int TypeId,
  int NetId,
  int AiStyle,
  bool HasSpecialEventText,
  bool BloodMoon,
  bool LanternsUp,
  bool DownedMoonlord,
  bool Eclipse,
  bool SlimeRain,
  bool DayTime,
  bool HardMode,
  bool StinkyDanger);

public readonly record struct NpcGuideDialogueResult(
  NpcGuideDialogueSelection Selection,
  int RandomRoll,
  bool RandomConsumed);

public interface INpcGuideDialogueRandomPort
{
  int Next(int maxExclusive);
}

/// <summary>
/// Source branch and random-consumption profile for Guide chat selection.
/// Text lookup remains an outer localization effect.
/// </summary>
public static class NpcGuideDialogueProfile
{
  public static NpcGuideDialogueResult EvaluateWithRandom(
    in NpcGuideDialogueInput input,
    INpcGuideDialogueRandomPort randomPort)
  {
    ArgumentNullException.ThrowIfNull(randomPort);
    if (!NpcGuideSourceProfile.CanHandle(input.TypeId, input.NetId, input.AiStyle))
    {
      throw new InvalidOperationException(
        "Guide dialogue profile requires type=22, netID=22, and aiStyle=7.");
    }

    if (input.HasSpecialEventText)
    {
      return new NpcGuideDialogueResult(
        NpcGuideDialogueSelection.SpecialEventText,
        0,
        RandomConsumed: false);
    }

    if (input.BloodMoon)
    {
      int roll = randomPort.Next(3);
      return new NpcGuideDialogueResult(
        roll switch
        {
          0 => NpcGuideDialogueSelection.BloodMoon170,
          1 => NpcGuideDialogueSelection.BloodMoon171,
          _ => NpcGuideDialogueSelection.BloodMoon172,
        },
        roll,
        RandomConsumed: true);
    }

    if (input.LanternsUp)
    {
      return new NpcGuideDialogueResult(
        input.DownedMoonlord
          ? NpcGuideDialogueSelection.LanternNightAfterMoonlord
          : NpcGuideDialogueSelection.LanternNightBeforeMoonlord,
        0,
        RandomConsumed: false);
    }

    if (input.Eclipse)
    {
      return new NpcGuideDialogueResult(
        NpcGuideDialogueSelection.Eclipse,
        0,
        RandomConsumed: false);
    }

    if (input.SlimeRain)
    {
      return new NpcGuideDialogueResult(
        NpcGuideDialogueSelection.SlimeRain,
        0,
        RandomConsumed: false);
    }

    if (!input.DayTime)
    {
      return new NpcGuideDialogueResult(
        NpcGuideDialogueSelection.Night173,
        0,
        RandomConsumed: false);
    }

    if (input.HardMode && input.StinkyDanger)
    {
      int chatterRoll = randomPort.Next(8);
      if (chatterRoll == 0)
      {
        return new NpcGuideDialogueResult(
          NpcGuideDialogueSelection.HardmodeChatter1,
          chatterRoll,
          RandomConsumed: true);
      }
    }

    if (input.HardMode)
    {
      int chatterRoll = randomPort.Next(8);
      if (chatterRoll == 0)
      {
        return new NpcGuideDialogueResult(
          NpcGuideDialogueSelection.HardmodeChatter2,
          chatterRoll,
          RandomConsumed: true);
      }
    }

    int genericRoll = randomPort.Next(3);
    return new NpcGuideDialogueResult(
      genericRoll switch
      {
        0 => NpcGuideDialogueSelection.Generic174,
        1 => NpcGuideDialogueSelection.Generic175,
        _ => NpcGuideDialogueSelection.Generic176,
      },
      genericRoll,
      RandomConsumed: true);
  }
}

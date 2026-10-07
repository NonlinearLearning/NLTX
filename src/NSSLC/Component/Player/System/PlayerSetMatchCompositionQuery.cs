namespace Terraria.Player;

public static class PlayerSetMatchCompositionQuery
{
  public static PlayerSetMatchCompositionResult Evaluate(
    in PlayerSetMatchCompositionInput input)
  {
    int legs = input.Legs;
    bool wearsRobe = false;
    bool somethingSpecial = false;

    PlayerSetMatchResult bodyMatch = PlayerSetMatchQuery.Evaluate(
      new PlayerSetMatchInput(
        Head: input.Head,
        Body: input.Body,
        Legs: legs,
        ArmorSlotRequested: 1,
        Male: input.Male,
        MountActive: input.MountActive,
        MountType: input.MountType,
        SomethingSpecial: wearsRobe));
    if (bodyMatch.Matched)
    {
      legs = bodyMatch.MatchedSlot;
      wearsRobe = bodyMatch.SomethingSpecial;
    }

    PlayerSetMatchResult legsMatch = PlayerSetMatchQuery.Evaluate(
      new PlayerSetMatchInput(
        Head: input.Head,
        Body: input.Body,
        Legs: legs,
        ArmorSlotRequested: 2,
        Male: input.Male,
        MountActive: input.MountActive,
        MountType: input.MountType,
        SomethingSpecial: somethingSpecial));
    if (legsMatch.Matched)
    {
      legs = legsMatch.MatchedSlot;
      somethingSpecial = legsMatch.SomethingSpecial;
    }

    PlayerSetMatchResult headMatch = PlayerSetMatchQuery.Evaluate(
      new PlayerSetMatchInput(
        Head: input.Head,
        Body: input.Body,
        Legs: legs,
        ArmorSlotRequested: 0,
        Male: input.Male,
        MountActive: input.MountActive,
        MountType: input.MountType,
        SomethingSpecial: somethingSpecial));
    int head = headMatch.Matched
      ? headMatch.MatchedSlot
      : input.Head;

    return new PlayerSetMatchCompositionResult(
      Head: head,
      Body: input.Body,
      Legs: legs,
      WearsRobe: wearsRobe,
      SomethingSpecial: somethingSpecial);
  }
}

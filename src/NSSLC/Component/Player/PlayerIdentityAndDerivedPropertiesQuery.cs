namespace Terraria.Player;

public static class PlayerIdentityAndDerivedPropertiesQuery
{
  private const float MiscCounterNormalization = 300.0f;

  public static PlayerIdentityAndDerivedPropertiesSnapshot Evaluate(
    in PlayerIdentityAndDerivedPropertiesInput input)
  {
    return new PlayerIdentityAndDerivedPropertiesSnapshot(
      input.MiscCounter / MiscCounterNormalization,
      input.IsMale);
  }
}

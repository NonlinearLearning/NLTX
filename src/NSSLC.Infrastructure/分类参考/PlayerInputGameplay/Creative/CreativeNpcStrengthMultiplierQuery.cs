namespace NLTX.PlayerInputGameplay.Creative;

public sealed class CreativeNpcStrengthMultiplierQuery
{
  public float Get(PerPlayerCreativePowerStateComponent state, float value)
  {
    ArgumentNullException.ThrowIfNull(state);
    return state.StrengthMultiplierToGiveNpcs(value);
  }
}

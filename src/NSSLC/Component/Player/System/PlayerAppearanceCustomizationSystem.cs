namespace Terraria.Player;

public sealed class PlayerAppearanceCustomizationSystem
{
  public PlayerAppearanceCustomizationSnapshot Update(
    PlayerAppearanceCustomizationComponent component,
    in PlayerAppearanceCustomizationInput input)
  {
    ArgumentNullException.ThrowIfNull(component);
    component.Apply(input);
    return component.ToSnapshot();
  }
}

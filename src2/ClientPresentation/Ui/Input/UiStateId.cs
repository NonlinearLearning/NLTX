namespace Terraria.ClientPresentation.Ui.Input;

public readonly record struct UiStateId(int Value)
{
  public static UiStateId Invalid => new(0);

  public bool IsValid => Value > 0;
}

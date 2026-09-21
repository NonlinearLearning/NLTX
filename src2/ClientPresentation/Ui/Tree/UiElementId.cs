namespace Terraria.ClientPresentation.Ui.Tree;

public readonly record struct UiElementId(int Value)
{
  public static UiElementId Invalid => new(0);

  public bool IsValid => Value > 0;
}

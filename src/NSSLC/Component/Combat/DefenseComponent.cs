namespace Terraria.Combat;

public struct DefenseComponent
{
  public DefenseComponent(int value)
  {
    Value = value;
  }

  public int Value;

  public bool HasValue => Value != 0;
}

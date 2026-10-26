namespace EntityEcs.Components;

/// <summary>
/// 保存实体液体类别、接触标记和接触计时。
/// </summary>
public struct LiquidComponent
{
  public List<LiquidKind> LiquidList;

}

public enum LiquidKind : byte
{
  Nano,
  Water,
  Shimmer,
  Honey,
  Lava,
}

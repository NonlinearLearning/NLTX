namespace Terraria.Unshared.Projectiles;

/// <summary>何时把一次物块反弹计入穿透预算。</summary>
/// <remarks>
/// 参考端两种顺序都存在，必须由内容声明而不是由代码分支决定：
/// <list type="bullet">
/// <item><see cref="BeforeBounce" />：先判预算，够了才反弹（脉冲矢 <c>:19020-19026</c>）。</item>
/// <item><see cref="AfterBounce" />：先反弹，扣完再判断存活（宝石法杖 <c>:18108-18168</c>）。</item>
/// </list>
/// </remarks>
public enum ProjectileBounceConsumeOrder : byte
{
  BeforeBounce = 0,
  AfterBounce = 1
}

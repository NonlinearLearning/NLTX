namespace Terraria.Unshared.Session;

/// <summary>Stable identity of one connection session, as seen by the gameplay domains.</summary>
/// <remarks>
/// 这是领域侧的会话身份，不是传输标识：WorldItem、TileEntity、Inventory 都把它当作
/// "哪个会话做的这件事" 的权威键。它必须留在 <c>Terraria.Components</c>，否则这些领域
/// 会反向依赖网络程序集。
/// </remarks>
public readonly record struct SessionEpoch(ulong Value)
{
  public bool IsValid => Value != 0;

  public static SessionEpoch Create(ulong value)
  {
    if (value == 0)
    {
      throw new ArgumentOutOfRangeException(nameof(value), "A session epoch must be non-zero.");
    }

    return new SessionEpoch(value);
  }
}

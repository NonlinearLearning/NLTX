namespace Terraria.WorldInteraction.Wiring;

/// <summary>
/// 保存各种发射器的独立冷却表。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.Wiring。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Wiring.cs。</para>
/// <para>
/// 主要源成员：cannonCoolDown（第 69 行）； bunnyCannonCoolDown（第 71 行）； snowballCannonCoolDown（第 73 行）。
/// </para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>
/// 拆分依据文件：2026-09-11-version4-P01-liquid-wiring-spatial-death-teleport-component-design.md。
/// </para>
/// <para>依据位置：第 246 行。</para>
/// </remarks>
public sealed class WiringDeviceCooldownComponent
{
  private int _cannonCooldownTicks;
  private int _bunnyCannonCooldownTicks;
  private int _snowballCannonCooldownTicks;

  public int CannonCooldownTicks
  {
    get => _cannonCooldownTicks;
    internal set => _cannonCooldownTicks = ValidateNonNegative(value);
  }

  public int BunnyCannonCooldownTicks
  {
    get => _bunnyCannonCooldownTicks;
    internal set => _bunnyCannonCooldownTicks = ValidateNonNegative(value);
  }

  public int SnowballCannonCooldownTicks
  {
    get => _snowballCannonCooldownTicks;
    internal set => _snowballCannonCooldownTicks = ValidateNonNegative(value);
  }

  internal void Reset()
  {
    CannonCooldownTicks = 0;
    BunnyCannonCooldownTicks = 0;
    SnowballCannonCooldownTicks = 0;
  }

  internal void AdvanceOneTick()
  {
    if (CannonCooldownTicks > 0)
    {
      CannonCooldownTicks--;
    }

    if (BunnyCannonCooldownTicks > 0)
    {
      BunnyCannonCooldownTicks--;
    }

    if (SnowballCannonCooldownTicks > 0)
    {
      SnowballCannonCooldownTicks--;
    }
  }

  private static int ValidateNonNegative(int value)
  {
    if (value < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(value));
    }

    return value;
  }
}

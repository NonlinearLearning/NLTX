namespace EntityEcs.Components;

/// <summary>
/// 保存实体液体类别、接触标记和接触计时。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.Entity。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Entity.cs。</para>
/// <para>
/// 主要源成员：wet（第 26 行）； shimmerWet（第 28 行）； honeyWet（第 30 行）； wetCount（第 32 行）； lavaWet（第 34 行）。
/// </para>
/// <para>拆分依据目录：docs/component-decomposition/baseline/。</para>
/// <para>拆分依据文件：组件设计报告.md。</para>
/// <para>依据位置：第 29 行。</para>
/// <para>
/// 用途：空间求解后记录水、熔岩、蜂蜜和微光接触事实，供呼吸、伤害、移动与环境效果读取。
/// </para>
/// <para>
/// 当前实现：同时保留 InLiquid 类别和各类接触标记，派生属性合并这些信息。WetTickCount 与 LiquidTimer 是同一字段的两个访问入口；一致性和
/// 计时推进由外部流程维护。
/// </para>
/// <para>
/// 原始分级记录（2026-10-08）：C158，L2（通用可选玩法）。分级理由：只有保留液体交互时需要这些接触状态，简单固体世界可关闭。
/// </para>
/// </remarks>
public struct LiquidComponent
{
  /// <summary>
  /// 求解器选出的主要接触液体，可为空。
  /// </summary>
  public LiquidKind? DominantLiquidKind;
  /// <summary>
  /// 当前记录的液体类别；Nano 是此枚举的零值。
  /// </summary>
  public LiquidKind InLiquid;
  /// <summary>
  /// 是否接触蜂蜜。
  /// </summary>
  public bool IsHoneyWet;
  /// <summary>
  /// 是否接触熔岩。
  /// </summary>
  public bool IsLavaWet;
  /// <summary>
  /// 是否接触微光。
  /// </summary>
  public bool IsShimmerWet;
  /// <summary>
  /// 是否具有原版 wet 接触标记。
  /// </summary>
  public bool IsWet;
  /// <summary>
  /// 液体接触计时的字节值，具体增长规则由外部流程决定。
  /// </summary>
  public byte LiquidTimer;
  /// <summary>
  /// 最近一次写入接触结果的游戏 tick，可为空。
  /// </summary>
  public long? ResolvedAtTick;

  /// <summary>
  /// 液体类别不为 Nano 或任一接触标记为 true 时返回 true。
  /// </summary>
  public bool AnyWet => InLiquid != LiquidKind.Nano || IsWet || IsLavaWet ||
    IsHoneyWet || IsShimmerWet;
  /// <summary>
  /// AnyWet 的同义读取入口。
  /// </summary>
  public bool HasAnyLiquidContact => AnyWet;
  /// <summary>
  /// 液体类别或接触标记表明接触熔岩、蜂蜜或微光时返回 true。
  /// </summary>
  public bool HasNonWaterContact => InLiquid is LiquidKind.Lava or LiquidKind.Honey or
    LiquidKind.Shimmer || IsLavaWet || IsHoneyWet || IsShimmerWet;
  /// <summary>
  /// HasNonWaterContact 的同义读取入口。
  /// </summary>
  public bool HasNonWaterLiquidContact => HasNonWaterContact;
  /// <summary>
  /// LiquidTimer 的读写别名，两者共享同一计时值。
  /// </summary>
  public byte WetTickCount
  {
    get => LiquidTimer;
    set => LiquidTimer = value;
  }

}

public enum LiquidKind : byte
{
  Nano,
  Water,
  Shimmer,
  Honey,
  Lava,
}

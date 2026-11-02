namespace Terraria.Attributes;

/// <summary>
/// 独立管线形态的分类。这些形态不在 Buff 列表中存放，但都以
/// <see cref="AttributeModifier" /> 向实体贡献数值，因此统一由
/// <see cref="AttributeContributionSystem" /> 写入属性集合。
/// </summary>
/// <remarks>
/// 分类依据是"状态由谁持有"：装备与饰品持有在物品槽，前缀与附魔持有在物品实例，
/// 召唤物与坐骑持有在独立实体或子系统，幸运值与潜行由本域直接聚合，
/// 旗帜由世界级计数驱动，冷却与布尔标记由各自的运行态持有。
/// </remarks>
public enum AttributeContributionKind : byte
{
  /// <summary>护甲套装效果。</summary>
  SetBonus = 0,
  /// <summary>饰品加成。</summary>
  Accessory = 1,
  /// <summary>武器前缀。</summary>
  Prefix = 2,
  /// <summary>武器附魔。</summary>
  Imbue = 3,
  /// <summary>召唤物、宠物与光照宠。</summary>
  Summon = 4,
  /// <summary>坐骑。</summary>
  Mount = 5,
  /// <summary>幸运值。</summary>
  Luck = 6,
  /// <summary>怪物旗帜减伤。</summary>
  Banner = 7,
  /// <summary>潜行与隐身。</summary>
  Concealment = 8,
  /// <summary>布尔状态标记的镜像。</summary>
  StatusFlag = 9,
  /// <summary>冷却与恢复窗口。</summary>
  Cooldown = 10
}

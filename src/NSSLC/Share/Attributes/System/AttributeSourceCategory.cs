namespace Terraria.Share.Attributes;

/// <summary>
/// 属性修饰器的来源分类。它描述来源，不表示实体继承关系。
/// </summary>
public enum AttributeSourceCategory : byte
{
  Buff,
  Equipment,
  Accessory,
  Weapon,
  Boss,
  Enemy,
  Npc,
  FriendlyCreature,

  // 独立管线来源：这些来源不经 Buff 列表，但同样以 AttributeModifier 表达贡献。
  /// <summary>护甲套装效果。</summary>
  SetBonus,
  /// <summary>物品前缀（修饰词）。</summary>
  Prefix,
  /// <summary>武器附魔（药剂瓶）。</summary>
  Imbue,
  /// <summary>召唤物、宠物或光照宠。</summary>
  Pet,
  /// <summary>坐骑。</summary>
  Mount,
  /// <summary>世界环境来源，例如怪物旗帜。</summary>
  Environment,
  /// <summary>布尔状态标记的镜像来源。</summary>
  StatusFlag,
  /// <summary>冷却与恢复窗口。</summary>
  Cooldown
}

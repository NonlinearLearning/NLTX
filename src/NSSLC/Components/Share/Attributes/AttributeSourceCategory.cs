namespace Terraria.Attributes;

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
  FriendlyCreature
}

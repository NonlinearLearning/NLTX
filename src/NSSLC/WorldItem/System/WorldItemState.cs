using System.Numerics;
using Terraria.Session;
using Terraria.Item;

namespace Terraria.World;

/// <summary>One authoritative world item occupying a Version4 item slot.</summary>
/// <remarks>
/// 物品身份、数量与前缀统一存放在共享的 <see cref="ItemStack" /> 里，
/// 权威世界状态因此不会再与物品域的其他部分产生两套定义。
/// 已清除的槽位用默认（空）的 <see cref="ItemStack" /> 表达，对应 <see cref="ItemStack.IsEmpty" />。
/// </remarks>
public sealed record WorldItemState(
    short Slot,
    ItemStack Item,
    Vector2 Position,
    Vector2 Velocity,
    SessionEpoch? ReservedFor,
    long Revision);

/// <summary>Kind of externally visible change recorded for a world item.</summary>
public enum WorldItemChangeKind : byte
{
  Spawn,
  Update,
  Retire,
  Ownership
}

/// <summary>One committed world item change waiting for network projection.</summary>
public readonly record struct WorldItemMutation(
    WorldItemChangeKind Kind,
    WorldItemState Item,
    SessionEpoch? Origin = null);

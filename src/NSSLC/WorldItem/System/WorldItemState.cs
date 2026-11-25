using System.Numerics;
using Terraria.Unshared.Session;
using Terraria.Share.Items;

namespace Terraria.Unshared.World;

/// <summary>One authoritative world item occupying a Version4 item slot.</summary>
/// <remarks>
/// 物品身份、数量与前缀统一存放在共享的 <see cref="ItemStack" /> 里，
/// 权威世界状态因此不会再与物品域的其他部分产生两套定义。
/// 已清除的槽位用默认（空）的 <see cref="ItemStack" /> 表达，对应 <see cref="ItemStack.IsEmpty" />。
/// <para>
/// 生命周期计数与 Version4 <c>WorldItem</c> 对齐，但只建模有读取方的那几个：
/// <c>noGrabDelay</c>（拾取延迟，<c>Player.GetItem(WorldItem, …)</c> 在 &gt; 0 时直接拒绝）、
/// <c>ownTime</c>/<c>keepTime</c>（归属与保留倒计时）、<c>timeSinceItemSpawned</c>（存活计数）、
/// <c>shimmered</c>（微光产物标记，参与合并判定）。
/// 它们全部是 <c>init</c> 属性而不是位置参数，这样既有构造点与 <c>with</c> 表达式都不必改写。
/// </para>
/// </remarks>
public sealed record WorldItemState(
    short Slot,
    ItemStack Item,
    Vector2 Position,
    Vector2 Velocity,
    SessionEpoch? ReservedFor,
    long Revision)
{
  /// <summary>Version4 <c>WorldItem.noGrabDelay</c>：&gt; 0 时该物品不可被拾取，每 tick 递减。</summary>
  public int NoGrabDelay { get; init; }

  /// <summary>Version4 <c>WorldItem.ownTime</c>：&gt; 0 时每 tick 递减，归零后清除归属忽略。</summary>
  public int OwnTime { get; init; }

  /// <summary>Version4 <c>WorldItem.keepTime</c>：&gt; 0 时每 tick 递减，期间不重新寻找归属。</summary>
  public int KeepTime { get; init; }

  /// <summary>Version4 <c>WorldItem.timeSinceItemSpawned</c>：每 tick 递增的存活计数。</summary>
  public int TimeSinceItemSpawned { get; init; }

  /// <summary>Version4 <c>WorldItem.shimmered</c>：微光产物标记，不同标记的堆不合并。</summary>
  public bool Shimmered { get; init; }
}

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

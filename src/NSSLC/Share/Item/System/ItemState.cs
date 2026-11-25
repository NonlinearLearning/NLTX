using System.Numerics;
using Terraria.Unshared.Session;
using Terraria.Share.Items;

namespace Terraria.Share.Items;

/// <summary>How one item record is currently resident in the world.</summary>
/// <remarks>
/// 物品只有两种驻留形态，没有第三种：
/// <list type="bullet">
/// <item><see cref="Embodied"/>：物品在世界里，占一个 0..399 的世界槽，有位置与速度，逐 tick 推进。</item>
/// <item><see cref="Dormant"/>：物品驻留在某个容器的槽位里，没有世界字段，不参与 tick。</item>
/// </list>
/// 原版用两个类型表达这件事（<c>Terraria.Item</c> 不继承 <c>Entity</c>，<c>Terraria.WorldItem</c>
/// 组合它并持有 <c>position</c>/<c>velocity</c>），本域用一条记录 + 状态判别表达同一件事。
/// </remarks>
public enum ItemPresence : byte
{
  /// <summary>Record occupies a world slot and is advanced by the item tick.</summary>
  Embodied,

  /// <summary>Record resides in a container slot and is only touched on interaction.</summary>
  Dormant
}

/// <summary>One authoritative item record: payload plus the state it currently has.</summary>
/// <remarks>
/// <para>
/// 物品身份、数量与前缀统一存放在共享的 <see cref="ItemStack"/> 里，权威状态因此不会再与
/// 物品域的其他部分产生两套定义。已清除的记录用默认（空）的 <see cref="ItemStack"/> 表达。
/// </para>
/// <para>
/// <see cref="Embodied"/> 专用字段只在 <see cref="Presence"/> 为 <see cref="ItemPresence.Embodied"/>
/// 时有意义；<see cref="Dormant"/> 记录不分配也不读取它们。这条边界靠
/// <see cref="ItemRules.RequiresEmbodied"/> 在写入侧把关——<c>record</c> 的 <c>with</c> 表达式
/// 无法在类型层禁止携带字段，因此校验必须发生在提交入口。
/// </para>
/// <para>
/// 生命周期计数与原版 <c>WorldItem</c> 对齐，只建模有读取方的那几个：<c>grabDelayTime</c>
/// （原版在 <c>Player.GetItem</c> 之前由 <c>GrabItems</c> 判定）、<c>keepTime</c>/<c>ownTime</c>
/// （归属与保留倒计时）、<c>timeSinceItemSpawned</c>（存活计数）、<c>Shimmered</c>（微光产物标记）。
/// </para>
/// </remarks>
public sealed record ItemState(
    short Slot,
    ItemStack Item,
    Vector2 Position,
    Vector2 Velocity,
    SessionEpoch? ReservedFor,
    long Revision)
{
  /// <summary>Resident form of this record; defaults to <see cref="ItemPresence.Embodied"/>.</summary>
  public ItemPresence Presence { get; init; } = ItemPresence.Embodied;

  /// <summary>
  /// 容器地址：<see cref="Presence"/> 为 <see cref="ItemPresence.Dormant"/> 时指向宿主与槽号。
  /// </summary>
  /// <remarks>
  /// 世界槽号与世界地址互斥：Embodied 记录的地址就是 <see cref="Slot"/>，Dormant 记录的地址
  /// 是这里的一对 (宿主, 槽号)，此时 <see cref="Slot"/> 不分配。地址恒有且只有一个，
  /// 不存在既不在世界也不在容器的游离记录。
  /// </remarks>
  public ItemContainerAddress? Container { get; init; }

  /// <summary>原版 <c>WorldItem.grabDelayTime</c>：&gt; 0 时该物品不可被拾取，每 tick 递减。</summary>
  public int NoGrabDelay { get; init; }

  /// <summary>原版 <c>WorldItem.timeToKeepReservation</c>：&gt; 0 时每 tick 递减，期间不重新寻找归属。</summary>
  public int KeepTime { get; init; }

  /// <summary>原版 <c>WorldItem.timeSinceItemSpawned</c>：每 tick 递增的存活计数。</summary>
  public int TimeSinceItemSpawned { get; init; }

  /// <summary>原版 <c>WorldItem.shimmered</c>：微光产物标记，不同标记的堆不合并。</summary>
  public bool Shimmered { get; init; }

  /// <summary>True when this record currently occupies a world slot.</summary>
  public bool IsEmbodied => Presence == ItemPresence.Embodied;
}

/// <summary>Address of a record that resides inside a container slot.</summary>
/// <remarks>
/// 宿主用不透明整数表达，容器域自己解释它指向哪个库存实体；物品域只负责保证"同一槽号不会被
/// 两条记录同时占用"。世界槽与容器槽是两段互不相交的地址空间。
/// </remarks>
public readonly record struct ItemContainerAddress(int Container, int Slot);

/// <summary>Kind of externally visible change recorded for an item.</summary>
public enum ItemChangeKind : byte
{
  /// <summary>An embodied record appeared in a world slot.</summary>
  Spawn,

  /// <summary>An embodied record changed its payload, position or velocity.</summary>
  Update,

  /// <summary>An embodied record left the world.</summary>
  Retire,

  /// <summary>An embodied record changed its reservation.</summary>
  Ownership,

  /// <summary>A record changed resident form (world slot ↔ container slot).</summary>
  Transfer
}

/// <summary>One committed item change waiting for network projection.</summary>
public readonly record struct ItemMutation(
    ItemChangeKind Kind,
    ItemState Item,
    SessionEpoch? Origin = null);

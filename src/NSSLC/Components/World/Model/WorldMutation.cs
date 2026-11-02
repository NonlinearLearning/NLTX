namespace Terraria.World;

/// <summary>
/// One committed tile change that still needs to be projected to connected clients.
/// </summary>
/// <remarks>
/// 记录的是"已提交、待投影"的变更，因此额外带上掉落信息（<see cref="DropItemType"/>、
/// <see cref="DropStack"/>）——客户端需要据此显示物品弹出。
///
/// 刻意不与 <c>Terraria.Physics.LiquidTileChange</c> 合并：后者是液面步进传给调用方的
/// 内部日志记录，只有坐标与前后状态三个字段，含义是"这一步改写了哪些格子"，与
/// "要投影给客户端的变更"不是同一概念。两者前三个字段相同只是巧合；合并会让液面逻辑
/// 被迫产出它根本不设置的掉落字段。
/// </remarks>
public readonly record struct WorldMutation(
    WorldTileCoordinate Coordinate,
    WorldTileState Previous,
    WorldTileState Current,
    int DropItemType = 0,
    int DropStack = 1);

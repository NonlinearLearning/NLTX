using ArchWorld = Arch.Core.World;
using Terraria.Share.Entities;

namespace Terraria.Unshared.World;

/// <summary>Locates the single entity that carries the world-item request queue.</summary>
/// <remarks>
/// 公开是因为网络投影系统（<c>NSSLC.Network</c> 中的 <c>WorldNetworkProjectionSystem</c>）
/// 需要读这个队列来生成掉落物报文。依赖方向仍是网络 → 领域，未引入反向引用。
/// </remarks>
public static class WorldItemRequestQueue
{
  public static WorldItemRequestQueueComponent Require(ArchWorld world)
    => SingletonComponent<WorldItemRequestQueueComponent>.Require(
        world,
        "A WorldItemRequestQueueComponent entity must exist before world-item requests are "
        + "recorded.");
}

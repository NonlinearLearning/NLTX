using Arch.Core;
using Arch.System;
using ArchWorld = Arch.Core.World;
using Terraria.Share.Entities;

namespace Terraria.Share.Combat;

/// <summary>Clears expired immunity slots once per simulation tick.</summary>
/// <remarks>
/// 只做过期归零，不承担「是否被挡」的判断——那个判断在读取点用 tick 比较完成，
/// 所以本系统晚跑或漏跑都不会让窗口错误地继续生效。它存在是为了让存储状态保持规范，
/// 使未来的投影与存档不会携带过期戳。
/// </remarks>
public sealed class ImmunityWindowExpirySystem : BaseSystem<ArchWorld, EcsServerTickContext>
{
  private readonly QueryDescription _query = new QueryDescription()
      .WithAll<ImmunityWindowComponent>();

  public ImmunityWindowExpirySystem(ArchWorld world) : base(world)
  {
  }

  public override void Update(in EcsServerTickContext context)
  {
    long tick = context.Tick;
    World.Query(
        in _query,
        (ref ImmunityWindowComponent window) => window.Expire(tick));
  }
}

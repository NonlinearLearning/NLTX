using System;
using System.Numerics;
using Terraria.Share.Attributes;
using Terraria.Unshared.Chat;
using Terraria.Share.Combat;
using Terraria.Share.Entities;
using Terraria.Share.Inventory;
using Terraria.Share.Items;
using Terraria.Share.Movement;
using Terraria.Share.Physics;
using Terraria.Unshared.Player;
using Terraria.Share.Relationships;
using Terraria.Unshared.World;
using MovementVelocityComponent = Terraria.Share.Movement.VelocityComponent;

namespace NSSLC.Server;

/// <summary>玩家实体的不可变定义与创建期超参数。</summary>
/// <remarks>
/// 审计 §30（#6）选择 A：先补宿主，再把玩家创建改成数据驱动。
/// 定义不持有运行期实体或可变状态，每次生成都由工厂产出独立的组件值；
/// 出生坐标属于"实例级输入"，因此在定义里只放占位值，由调用方创建后覆盖。
/// </remarks>
public static class PlayerEntityDefinition
{
  public static readonly EntityId Id = new("player");

  /// <summary>把玩家定义登记进注册表；超参数由组合根传入，定义本身不写死数值。</summary>
  public static void Register(
      EntityRegistry definitions,
      int inventorySlotCount,
      int maxStack,
      int respawnDelayTicks)
  {
    ArgumentNullException.ThrowIfNull(definitions);

    definitions.Register(new EntityBuilder(Id)
        // 身份每次生成都新建；宿主据此分配并登记运行时句柄。
        .Add(() => new EntityIdentityComponent(EntityUuid.Create()))
        .Add(() => new EntityFactionComponent(Faction.Friendly))
        .Add(() => new LifeComponent(current: 100, maximum: 100))
        .Add(() => new ShieldPoolComponent())
        .Add(() => new CooldownComponent())
        .Add(() => new BreathComponent(maximum: 200))
        .Add(() => new MountStateComponent())
        // 没有这两项时 PlayerDeathRespawnSystem 与 PlayerCombatResultSystem 在真实服务器中
        // 匹配 0 个实体：前者永不处理死亡，后者永不排空 CombatDamageResultQueue，
        // 对局越久玩家伤害结果堆积越多（与 AttributeTickSystem 那次的成因同类）。
        .Add(() => new PlayerDeathStateComponent())
        .Add(() => new PlayerRespawnStateComponent(respawnDelayTicks))
        // 属性基值取自参考客户端默认值（statDefense 0、moveSpeed 1），与 null 路径等价。
        .Add(CreateAttributes)
        // 没有这两项时 AttributeTickSystem 与 BuffTickSystem 在真实服务器中匹配 0 个实体。
        .Add(() => new BuffListComponent())
        // 没有这份能力时 BuffAdmissionSystem.Check 返回 MissingReceptionCapability。
        .Add(() => new BuffReceptionCapabilityComponent(BuffReceptionMode.AllowAll))
        // 出生坐标由连接时的落点决定，这里只放占位值，创建后立即覆盖。
        .Add(() => new LocationComponent(Vector2.Zero))
        // 受击后无敌窗口的载体，按 6 个伤害通道分槽。没有它时 ImmunityWindowExpirySystem
        // 匹配 0 个实体，免疫裁决也永远不会命中玩家。
        .Add(() => new ImmunityWindowComponent())
        .Add(() => new InventoryComponent(CreateInventory(inventorySlotCount, maxStack)))
        // 声明这份能力，玩家就能在聊天里说话（Chat 域只认这个组件，不查会话或实体类别）。
        .Add(() => ChatSenderComponent.Player())
        .Add(() => MovementVelocityComponent.Stationary)
        .Add(() => new ColliderComponent(new Vector2(16f, 32f)))
        .Add(() => new EntityCollisionCapabilityComponent(
            Enabled: true,
            Layer: 1,
            Mask: uint.MaxValue))
        .Add(() => MovementIntent.Stop(tick: 0))
        .Add(() => new WorldInteractionCapabilityComponent(
            WorldInteractionPermission.ReadTile |
            WorldInteractionPermission.BreakTile |
            WorldInteractionPermission.PlaceTile))
        .Add(() => WorldInteractionRequestComponent.ReadTile(
            requestId: 0,
            tick: 0,
            coordinate: new WorldTileCoordinate(0, 0)))
        .Add(() => new WorldInteractionResultComponent())
        .Build());
  }

  private static AttributeSetComponent CreateAttributes()
  {
    AttributeSetComponent attributes = new();
    PlayerAttributeBaseline.Apply(attributes, maximumLife: 100);
    return attributes;
  }

  private static InventorySlotDefinition[] CreateInventory(int slotCount, int maxStack)
  {
    InventorySlotDefinition[] slots = new InventorySlotDefinition[slotCount];
    for (int index = 0; index < slotCount; index++)
    {
      slots[index] = new InventorySlotDefinition(
          index,
          InventorySlotKind.General,
          ItemCategoryMask.Any,
          maxStack);
    }

    return slots;
  }
}

# CR-2026-10-23：广义 Inventory 能力

## 原始需求

> 考虑设计广义Inventory,游戏中存在许多狭义Inventory,例如猪猪存钱罐的slot,玩家饰品的槽位,npc使用武器的slot,和有些武器需要弹药也需要slot,没有Inventory就无法放入任何的item和拾取item,还有存在最为广泛,可以和玩家互动的箱子也存在slot,编写文档并实现,代码实现在D:\\TRbackup\\NLTX\\src\\NSSLC\\Components\\Share中,采用优秀参考项目的代码风格不使用D:\\TRbackup\\最新的客户端代码风格和命名风格,我需要一种组件实体存在此组件传入参数即可获得能力Inventory,与其他物体进行交互

提出时间：2026-10-23（Asia/Shanghai）  
渠道：对话

## 决策

- 级别：重大
- 排期：当前任务分批实施
- 方略：spec
- 批次：共享状态模型 → Arch 交互系统 → 文档 → 源码交付记录
- 用户追加原文：**不做任何测试**。后续不新增测试，也不运行构建、测试、冒烟、服务器或验证脚本。

## 范围

本批建立通用库存能力、物品栈状态、槽位类别约束和 Arch 交互系统。玩家、箱子、NPC、武器和猪猪存钱罐通过不同的槽位定义组合能力。

暂不接入网络协议、存档编码、客户端 UI、原版物品数据库、跨世界交易和复杂容器权限。

## 影响

- 新增共享领域文件：`src/NSSLC/Components/Share/Inventory/`、`src/NSSLC/Components/Share/Item/`。
- 组件是状态；`InventorySystem` 是跨实体写入和交互规则的唯一提交边界。
- Arch `Entity` 只用于系统的同步运行时入口；共享槽位和关系组件使用 `EntityUuid`，
  不将 Arch 句柄当作身份根。持久化、网络和 UI 通过后续适配器处理。
- 槽位保存值堆或独立物品实体的身份，二者互斥。带 Inventory 的物品必须保留原实体，
  由系统维护库存槽位与 `ContainedInInventoryComponent` 双向关系。
- 在宿主独占、同步且禁止重入修改的提交阶段，业务拒绝不产生部分写入；实体必须属于
  当前 Arch `World` 且存活。基础设施/运行时异常不吞掉，也不声称任意异常都可事务回滚。
- 宿主销毁实体或移除库存前须清空库存、断开所属关系；本批提供受控移除入口，
  不自动接管 Arch World 的任意销毁调用。

## 源码交付记录

- `InventoryComponent.cs` 保留状态、只读视图和内部槽位提交；容量规划移入 `InventorySystem`。
- `InventorySystem.Entities.cs` 是同一系统的 partial 文件，按实体归属与生命周期职责分组，
  不新增运行时系统、注册入口或调度节点。
- `ContainedInInventoryComponent` 的构造参数从 Arch `inventoryEntity` 调整为
  `EntityUuid inventoryId`，属性从 `InventoryEntity` 调整为 `InventoryId`。
  `InventorySlotDefinition` 参数改用 camelCase（例如 `maxStackSize`），调用方命名参数需同步。
- 公开入口增加可选 `targetSlotIndex`、`TryStoreEntity`、`TryTakeEntity`、`TryGetStack` 和
  `TryRemoveInventoryComponent`。带库存的物品使用 `TryInsert` 时保留完整实体。
- `Inventory/README.md` 记录模型、六类库存示例、失败语义、同步提交、世界物理排除及销毁协议；
  `Item/README.md` 和 `Share/README.md` 同步相邻领域的协作说明。
- 本轮仅源码与文档交付，不代表服务器接入或行为验收。遵照用户要求未新增或执行测试，
  未运行构建、冒烟、服务器或验证脚本；最终代码的编译及运行行为未验证。

## 回滚

回退本 CR 的 Inventory 文件与相关 README 章节。Item 模型被本批复用，回退时保留已有或被其他
任务使用的 Item 文件，仅撤回本批修改。不批量删除目录，不撤回其他任务的本地改动。


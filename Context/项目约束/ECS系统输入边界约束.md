# ECS 系统输入边界约束

状态：生效
适用：`src/NSSLC/**` 下的全部 ECS 系统，以及任何向这些系统传入输入的宿主代码
（如 `src/NSSLC.Server`）。

本约束规定"系统的输入从哪里来"。它不谈依赖注入语法、不谈目录命名，只谈三条边界。

本文与 [非函数式编码副作用隔离规范](非函数式编码副作用隔离规范.md) 的关系：该规范 §7 把显式
请求、command、事件视为**可选手段**；在本项目范围内，本文把"跨域输入"这一项收紧为必须，
其余部分不冲突时仍以该规范为准。

## 术语

- **域**：`Terraria.Share.<Name>` 或 `Terraria.Unshared.<Name>` 命名空间覆盖的一套组件与系统。
  例如 `Terraria.Share.Combat`、`Terraria.Share.Attributes.Buff`、`Terraria.Unshared.World`。
- **请求组件**：挂在实体上、承载"某方希望该域做某事"的组件，名称为
  `*RequestComponent` 或 `*IntentComponent`。
- **可变队列**：可在 tick 之间累积、并被一个以上系统读写的容器对象，例如 `*Buffer`、
  `List<T>`、`Queue<T>`、`Dictionary<TKey,TValue>`。
- **只读端口**：只暴露查询、不暴露修改的接口，例如 `IEntityRuntimeHandleResolver`、
  `ISpatialWorldQuery`、`IWorldInteractionWorld`；以及 `Seal()` 之后冻结的目录对象。当前已冻结的
  目录为 `ChatCommandCatalog`、`ProjectileBehaviorCatalog`、`ProjectileCatalog`、
  `CombatAttackCatalog`、`ItemRegistry`、`ItemUseCatalog` 和 `PacketGateway`。**没有 `Seal()` 的
  注册表（当前是 `BuffRegistry`、`ProjectileRuntimeRegistry`）不算不可变目录对象**，它们仍可被
  注册写入。

## 规则 1：跨域输入走请求组件

一个域要求另一个域改变状态时，必须把意图写成**目标域的请求组件**并挂到实体上，由目标域的
系统在本 tick 读取。跨域不得直接改状态。

禁止：

- 跨域直接调用对方域的状态修改方法，例如 `AttributeSetComponent.AddModifier(...)`、
  `BuffSystem.TryApply(...)`、`InventorySystem` 的转移方法；
- 跨域把对方的服务对象或可变存储作为构造依赖注入，例如把 `WorldItemStore` 注入到
  `Terraria.Communication` 的系统；
- 在同一 tick 内绕过目标域的系统直接写对方拥有的组件。

允许：

- 只读端口与不可变目录对象；
- 返回不可变结果的计算类；
- 域内自读自写（见规则 2）。

## 规则 2：域内 tick 推进直接读写组件

同一域内的 tick 推进系统可以查询自己的组件并就地修改，**不需要**请求层。

判定依据是"状态由谁拥有"，不是"是否跨进程/跨线程"。给纯推进系统套一层请求，只会增加
无谓的类型和一次间接跳转。

## 规则 3：禁止宿主注入可变队列

- 系统构造参数不得包含可变队列，也不得包含承载可变状态的域服务对象。
- 宿主不得把同一个可变对象交给两个以上系统共享。
- 需要跨 tick 累积时，把队列放成组件（随实体走、可查询、可测试），或由请求组件逐条表达。

系统构造参数只允许：`ArchWorld`、只读端口、不可变目录对象，以及同域内不承载可变状态的服务。

### 适用范围：共享或跨域

本规则针对**被共享或跨域**的可变容器与服务。系统独占、仅由所属域推进的状态归规则 2 管辖，
不受本规则约束。

例：`WorldTickSystem` 作为 World 域的推进者持有 `WorldRuntime` 是**合规**的；但同一个
`WorldRuntime` 再交给 `WorldNetworkProjectionSystem`（Communication 域）就同时触发规则 1 与规则 3。
判断顺序应当是先问"这个状态归谁所有"，再问"它有没有流出所属域"。

### 输出型端口不在此列

**输出型端口**可以注入，即使其实现内部持有队列、后台任务或最后投递状态。判定输出型端口看三条，
全部满足才算：

1. 端口只暴露**单向写入**方法（提交、发送、上报），没有把领域数据带回来供系统决策的读取方法；
2. 端口状态**不参与游戏判定**，只承担投递、缓冲、重试等机械职责；
3. 端口不别名到任何域的权威状态。

符合的例子：`ICommunicationOutboundPort`（唯一成员 `bool TrySubmit(OutboundPacketIntent)`）、
`IServerLogSink`。不符合的例子：`IWorldInteractionWorld`（`TryBreakTile`/`TryPlaceTile` 直接改
世界权威状态）、`WorldItemStore`（改世界物品）、`WorldRuntime`（改 tile 与液体）。

这条例外的理由：规则 3 要禁止的是把**输入**藏进注入对象，让系统读到看不见的来源；输出端口恰恰相反，
它是显式的效果执行边界。把它们一并禁止会与
[非函数式编码副作用隔离规范](非函数式编码副作用隔离规范.md) §8 冲突——该节要求外部能力通过显式依赖
或受控边界提供。

## 存量与例外

约束生效前已存在的注入队列属于**迁移债**，不影响其正确性，但新增代码不得扩大存量。

存量清单与迁移路径见
[2026-11-01 ECS 系统输入边界审计](../../docs/reviews/audits/2026-11-01-ecs-system-input-audit.md)。

**存量状态（2026-11-24 复核）**：2026-11-01 审计列出的迁移项已基本收敛；抽查 16 处系统的容器型
构造参数，得到的都是 `Seal()` 后的目录对象或同域 store。上段的迁移债描述保留作为历史记录，
不再指向待办。

对存量代码做超出当前任务的改造不在本约束要求内；本约束要求的是：新增系统按规则写，
触碰到的存量系统在改动范围内顺带收敛。

## 判定清单

- [ ] 系统构造参数只有 `ArchWorld`、只读端口、不可变目录对象
- [ ] 跨域输入是 `*RequestComponent` 或 `*IntentComponent`
- [ ] 同一个可变对象没有被两个以上系统共享
- [ ] 域内 tick 推进直接读写组件，没有多余请求层
- [ ] 没有在同一 tick 内绕过目标域系统直接写对方组件

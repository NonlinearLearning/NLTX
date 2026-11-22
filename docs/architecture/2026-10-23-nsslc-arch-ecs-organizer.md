# NSSLC Arch ECS 服务器组织者

状态：实施中（重大变更，spec 分批）  
范围：`src/NSSLC`，不包含 `MinimalImplementation`  
日期：2026-10-23

## 目标

为 NSSLC 提供明确的 Arch ECS 服务器组织入口，使服务器宿主能够：

1. 提供唯一的 Arch `World` 和服务器 Tick；
2. 按固定阶段注册 NSSLC 领域系统；
3. 在每个逻辑 Tick 内按阶段和注册顺序运行系统；
4. 区分组件值更新与实体结构变化提交；
5. 在世界关闭时按所有权释放系统和 `World`。

`MinimalImplementation` 是参考用 dome，不是本组织者的实现目标，也不接入本入口。

## 分层职责

```text
服务器宿主
  ├─ 拥有时钟、线程、停止流程和世界切换
  └─ 调用 EcsServer.Update(tick, deltaTime)
       ↓
NSSLC EcsServer
  ├─ 保存 Arch World 引用
  ├─ 按 EcsServerPhase 持有官方 Group<T>
  ├─ 执行 Initialize / BeforeUpdate / Update / AfterUpdate / Dispose
  └─ 维护 Tick 连续性、重入和故障状态
       ↓
Arch.System.Group<EcsServerTickContext>
       ↓
NSSLC 领域系统
       ↓
Arch World 中的组件状态
```

Arch Core 负责实体、组件存储和查询。它不会从组件自动发现系统，也不会决定服务器时钟、
系统顺序、请求操作的提交时点或领域写入者。组织者因此是必要的，但组织者不承担领域规则。

## 阶段合同

| 阶段 | 输入 | 主要责任 | 不负责 |
| --- | --- | --- | --- |
| `Preparation` | 宿主输入、上一轮已提交状态 | 捕获快照、整理输入、生成意图 | 直接伪造最终位置、伤害或网络确认 |
| `Simulation` | 本轮输入和领域组件 | 执行持续规则，更新所属组件值 | 任意跨领域结构变更 |
| `Commit` | 已校验的结果和结构变化意图 | 统一提交空间结果、实体结构变化和领域事务 | 读取未校验的客户端数据作为权威状态 |
| `Projection` | 本轮已提交状态 | 生成只读快照、复制投影和外部效果请求 | 回写权威组件 |

阶段是顺序和可见性边界，不创建线程、定时器或额外服务器时钟。阶段内系统按注册顺序执行。
空阶段仍保留在组织者中，所以增加或移除一个阶段系统不会改变其他阶段的相对顺序。

## 公共实现

实现文件位于：

- `src/NSSLC/Entity/System/EcsServer.cs`
- `src/NSSLC/Entity/System/EcsServerPhase.cs`
- `src/NSSLC/Entity/System/EcsServerSystemRegistration.cs`
- `src/NSSLC/Entity/System/EcsServerTickContext.cs`
- `src/NSSLC/Entity/Command/EcsServerCommandBuffer.cs`

阶段化装配：

```csharp
World world = World.Create();
EcsServer server = EcsServer.Create(
    world,
    [
      new(EcsServerPhase.Simulation, attributeTickSystem),
      new(EcsServerPhase.Simulation, gravityTickSystem),
      new(EcsServerPhase.Simulation, spatialSimulationCoordinatorSystem)
    ]);

server.Initialize();
server.Update(0, 1f / 60f);
server.Update(1, 1f / 60f);
server.Dispose();
```

组织者拒绝以下状态：未初始化更新、负数或跳跃 Tick、非有限或非正 `deltaTime`、重入、重复
注册同一系统实例、已释放后继续调用，以及更新异常后继续推进。更新异常会将组织者置为
`IsFaulted`，宿主应释放它并按自己的世界生命周期创建新的实例。

旧的 `new EcsServer(world, systems)` 构造方式仍可使用；它把传入系统全部放入 `Simulation`
阶段，适合只有一组持续模拟系统的宿主。

## 结构变化边界

组件字段的普通写入由拥有该状态的领域系统负责。实体创建、销毁、添加组件和移除组件等
结构变化使用 `EcsServerCommandBuffer` 记录，并在宿主确认查询阶段结束后播放：

```text
查询 / 计算 → 记录结构变化 → 结束查询 → Commit 阶段 Playback
```

Arch `CommandBuffer` 不是业务事务，也不负责身份注册、权限、网络请求、库存原子性、持久化
或跨实体关系。涉及这些不变量时，领域 owner 必须在播放前完成校验和登记；临时创建实体
不能在回放前当作正式网络或持久化身份发布。

## 接入规则

- 只有已明确组件读写集、生命周期和提交责任的系统才能注册到组织者。
- 持续模拟系统注册到 `Simulation`；请求驱动的库存、拾取和放置操作进入显式请求提交边界，
  不因为拥有组件就每 Tick 全量扫描。
- `Movement`、`Physics`、`AI`、`Combat`、`Item`、`Inventory`、`Liquid` 和 `World` 的完整
  Arch 系统必须按各自合同分批接入；组织入口存在不表示这些系统已经实现。
- 网络、文件、WorldFile、日志和平台资源属于宿主、应用或基础设施边界，不放进 NSSLC 共享组织者。
- 同一个 Arch `World` 只能由一个权威服务器循环推进，禁止旧循环和 `EcsServer` 双写。

## 当前批次状态

已实现：

- 官方 `Arch.System` 依赖；
- Tick 上下文；
- 阶段化 `EcsServer` 组织入口；
- 属性和重力 Tick 适配器；
- Arch 结构变化缓冲边界。
- 世界交互请求/结果组件；
- 使用 Arch `World.Query` 的 `WorldInteractionSystem` Commit 适配器；
- 地图仍由专用分块世界端口持有，未复制到 Arch Entity。

尚未宣称完成：

- 完整 AI 感知与意图执行；
- 完整 Movement / Physics 批量接入；
- Combat、Item、Inventory、Liquid 的统一 Tick 系统，以及 World 的完整地图、掉落、液体和
  网络行为；
- 宿主组合入口和真实服务器运行接线。

本批只做源码和文档实现及静态检查，不执行构建、测试、验证脚本或服务器启动。

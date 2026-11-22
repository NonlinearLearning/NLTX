# 重力属性与效果系统设计

文档 ID：DESIGN-2026-10-23-GRAVITY-ATTRIBUTE-SYSTEM  
状态：已实现第一阶段；尚未接入完整玩家移动运行时  
日期：2026-10-23（Asia/Shanghai）

## 1. 目标与范围

本设计把原版玩家重力相关状态拆成四个可协作的部分：

1. 重力幅度和最大下落速度作为可聚合的数值属性；
2. 重力切换资格作为能力；
3. 玩家选择的普通重力方向作为离散状态；
4. 重力方向覆盖作为带优先级和时限的状态效果。

实现位于 `src/NSSLC/Physics/Movement/`（原 `Share/Movement/`，
2026-11-01 并入 Physics 大类），不改写 `D:/TRbackup/最新的客户端`，
也不声称已经替换原版 `Player.Update` 的移动逻辑。当前项目没有已接入的玩家移动运行时，
因此本次交付实现了可独立调用的组件、纯求值器和状态系统，供后续移动系统消费。

## 2. 原版语义依据

当前客户端的 `Player` 源码显示：

- `gravity` 在 `Player.cs:2499` 定义为重力幅度；`Player.cs:24557` 每帧重置，
  之后由介质、状态和世界深度重新求值。
- 水、蜂蜜、微光和旋涡减益在 `Player.cs:24574-24627` 改变或覆盖重力。
- 深度缩放在 `Player.cs:24860-24882` 继续乘到重力上。
- `gravDir` 在 `Player.cs:1939` 是 `1f/-1f` 的离散方向，并被碰撞、跳跃、绘制和坐骑逻辑广泛读取。
- 重力药水和重力球分别在 `Player.cs:10112-10115`、`15180-15183` 授予方向切换能力，
  而不是直接把 `gravDir` 当作普通数值叠加。
- `forcedGravity` 在 `Player.cs:26490-26496` 以剩余帧数强制反向，属于临时覆盖状态。

因此，本实现不把所有字段压成一个 `float`，而是明确区分加值、倍率、覆盖、能力和离散状态。

## 3. 组件与所有权

| 类型 | 权威内容 | 写入者 |
| --- | --- | --- |
| `GravityConfigurationComponent` | 基础重力幅度、基础最大下落速度、默认方向 | 实体定义/创建流程 |
| `GravityDirectionStateComponent` | 玩家当前选择的非覆盖方向 | 重力输入/控制系统 |
| `GravityModifierCollectionComponent` | 来源可追踪的活动修饰器及其顺序 | 效果生命周期系统 |
| `GravityStateComponent` | 当前帧求值后的重力幅度、最大下落速度、有效方向和能力标记 | `GravityModifierSystem` |

`GravityStateComponent` 是派生投影，不应由网络、存档或其他系统并列写入。移动系统读取该
投影并负责把结果应用到速度、碰撞、跳跃和表现。

`GravityModifierCollectionComponent` 使用引用类型，因为它拥有需要原地维护的活动列表和
来源序号；其公开集合只读，增删只能通过系统边界完成。其他重力组件使用值语义，便于随
实体快照传递和在求值时保持输入显式。

## 4. 修饰器规则

修饰器包含来源 ID、修饰类型、数值、优先级、方向和可选剩余帧数。活动修饰器的求值规则为：

```text
magnitude =
  magnitudeOverride
  ?? max(0, (baseMagnitude + sum(magnitudeAdditive)) *
              product(magnitudeMultiplier))

maxFallSpeed =
  maxFallSpeedOverride
  ?? max(0, (baseMaxFallSpeed + sum(maxFallSpeedAdditive)) *
              product(maxFallSpeedMultiplier))
```

覆盖项按 `Priority` 优先、再按集合分配的 `Sequence` 决定相同优先级的最后写入者。
没有覆盖项时，方向使用 `GravityDirectionStateComponent.CurrentDirection`。
有方向覆盖时，最高优先级覆盖生效；能力修饰器的正值授予 `CanToggleDirection`。

倍率使用因子表示：`0.85` 表示剩余 85%，而不是增加 85%。所有最终数值在求值时限制为
不小于零。生命周期系统移除到期修饰器后重新求值，不通过“反向减回”恢复属性。

## 5. 系统 API 与副作用

`GravityModifierCalculator.Resolve` 是纯计算：只读取显式配置、方向状态和修饰器快照，
返回新的 `GravityStateComponent`，不读取时间、不发送事件、不修改集合。

`GravityModifierSystem` 负责三个明确操作：

- `Recalculate`：根据当前集合返回新的状态投影；实体宿主负责把返回值写入
  `GravityStateComponent`；
- `Advance`：推进修饰器时限并移除过期来源；调用方在需要时随后调用 `Recalculate`；
- `TryToggleDirection`：在没有方向覆盖且具备切换能力时修改方向状态。

这些方法不执行网络、存档、日志、音频或 UI。外部效果应由上层移动/表现系统消费状态变化后执行。

推荐的每帧调用顺序是：

```csharp
GravityModifierSystem.Advance(modifiers);
GravityStateComponent gravity = GravityModifierSystem.Recalculate(
    configuration,
    directionState,
    modifiers);
```

效果系统在本帧开始前添加或移除来源；`Advance` 只处理有限时长，随后由宿主把新的
`gravity` 投影写回实体。输入系统使用上一阶段或本阶段的 `gravity` 投影调用
`TryToggleDirection`，成功后下一次 `Recalculate` 才会反映新方向。这样可以明确到期、输入和
移动消费之间的时序，不依赖集合枚举顺序。

## 6. 当前实现与未接入部分

已实现：

- 基础值、加值、倍率、覆盖和能力的可追踪修饰器；
- 方向覆盖的优先级、来源顺序和剩余帧数；
- 到期移除、重新聚合和受控方向切换；
- 纯计算与副作用提交边界。
- 默认结构体值也能得到原版默认重力 `0.4`、最大下落速度 `10` 和普通方向；非法方向、
  非有限数值、负倍率/覆盖值会在构造时拒绝。

尚未实现：

- 将 `Player.cs` 的水、蜂蜜、微光、旋涡、深度和坐骑逻辑接入这些组件；
- 将组件挂载到现有 Arch 或其他 ECS 运行时实体；
- 用 `GravityStateComponent` 替换碰撞、跳跃、绘制和网络协议的现有读取；
- 网络/存档字段和旧 Buff 槽位的迁移。

这些项目需要真实玩家移动入口和行为测试，不能由本次组件库级实现单独宣称完成。

## 7. 验证计划

后续接入时至少验证：

1. 默认重力 `0.4` 和最大下落速度 `10`；
2. 加值、倍率和覆盖的组合顺序；
3. 同优先级覆盖的顺序稳定性；
4. 到期移除不会留下反向补偿；
5. 方向覆盖存在时输入不能切换，覆盖结束后可以恢复切换；
6. 多来源能力任一存在即可切换，全部移除后禁止切换；
7. 重力幅度为零、最大下落速度为零和负值输入的边界；
8. 将求值结果接入水、坐骑、碰撞和跳跃后与原版轨迹的差异。

本次没有新增测试项目或测试代码。已使用 `.agent-workplace/gravity-compile/` 中的临时
验证项目，仅编译 `Movement/*.cs`；输出位于 `Build/bin/verification/gravity/`，结果为
0 个错误、1 个 MSBuild 路径警告。生产项目整体构建仍受到工作区已有的
`LifeComponent.cs` 和 `VelocityComponent.cs` 语法错误阻塞；这些文件不属于本次变更，
也没有在本次变更中回退或修复。

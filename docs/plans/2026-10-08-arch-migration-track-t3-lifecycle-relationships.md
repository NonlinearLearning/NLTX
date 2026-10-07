# Arch 迁移长线 T3：实体创建、销毁、生命周期与关系

文档 ID：DOC-2026-10-08-ARCH-TRACK-T3  
状态：active；这是执行合同，不是已完成报告  
对应批次：A3  
依赖：T2 的 World、身份 registry 与生产签名合同  
主计划：[自定义 ECS 转向 Arch 执行计划](2026-10-07-custom-ecs-to-arch-execution-plan.md)

## 1. 目标

按真实 Spawn/Release/Load caller 验收 Player、NPC、Projectile、Item 和已支持 TileEntity，
证明 Arch 只承载组件存储与遍历，领域 owner 负责 UUID、生命周期、关系、槽位、容量、发布
和失败清理。重点不是手工 `world.Create` 能否读写，而是半成品、重建、复用、重复释放、
关系断开和同 tick 可见性都能解释并复现。

## 2. 设计不变量

- 构建中、终止中、已发布、已销毁是不同阶段；被查询命中不等于可参与普通模拟。
- 普通死亡/复活保留同一 EntityUuid；真实销毁重建分配新 UUID。
- 每个可变组件、数组、集合按实例独立构造；Arch 不负责深复制或防别名。
- 槽位、容量、协议 identity 和 Arch Entity 是不同投影，映射随生命周期注册/清理。
- 失败路径必须清理已经分配的组件、UUID、槽位、关系、订阅、请求和缓冲；不能只返回 false。
- 领域关系引用跨世界/会话时必须显式带 token 或 UUID 解析，不能以相同整数猜测。

## 3. 前置读取与范围

先读取根 AGENTS、progress、开发协作、构建、ECS 文件组织、ECS Entity、基础设施边界、
副作用隔离、C# 风格、主计划、研究文档和 T2 交接；读取 pua skill，并用 `/goal` 设置本线
目标。涉及组件新建/重命名时必须按组件命名约束先定归属；不得把组件塞进泛化 Shared/Data 目录。

主要检索范围：当前实际 `RuntimeNpc*`、`RuntimePlayer*`、Projectile store、Item registry/
world item store、TileEntity host、EntityIdentityRegistry、关系和槽位 owner，以及它们的
真实 Spawn/Release/Load caller。不要按文件名假设功能已存在，先用 `rg` 找调用闭包。

## 4. 分批执行

### T3-B0：实体类别支持矩阵

1. 为五类实体记录创建入口、初始组件组合、身份分配、初始化、发布边界、失败路径、销毁路径、
   关系/槽位/订阅清理和查询可见性。
2. 按 A0/现有代码标出真正支持集；Leashed 或其他未接线能力不得被扩大为已支持。
3. 找出同一领域状态的重复权威写入，给 T2/T4 精确指出冲突文件和建议 owner。

### T3-B1：NPC 与 Player

- NPC：验证容量 200、自然退场、槽位复用、父子生命路由、关系链、创建失败、TrainingDummy
  绑定和同 tick 新建 Servant 的可见性规则。
- Player：验证控制/背包 owner、普通死亡复活保实例、真实重建换 UUID、断线/会话重建清理。
- 创建流程按“容量预留 → 组件独立实例化 → UUID/关系登记 → 初始化 → 发布”表达；中间任何
 失败都回滚已经完成的步骤。

### T3-B2：Projectile、Item、TileEntity

- Projectile：组合、协议 identity/owner、碰撞/命中/免疫、过期目标和结束清理；只覆盖 A0
  manifest 声明的内容。
- Item：world-drop/inventory 转移、数量守恒、预留释放、容量失败、重复拾取和重建实例。
- TileEntity：仅对已支持 sensor/dummy 等路径做创建、anchor 失效、保存/重载映射和删除；
  未接线的 Leashed 路径明确保持未支持。

### T3-B3：关系、重复释放与查询可见性

1. 让关系 owner 在实体删除/World 卸载/候选失败时清理所有反向索引、订阅和投影。
2. 证明重复 release/destroy 的结果是幂等、明确拒绝或具名失败，不允许静默误删新实例。
3. 证明构建中/终止中实体不会进入不应参与的普通 Query；同 tick spawn 规则由领域调度显式
   决定，不依赖 Arch chunk 顺序。
4. 结构变化前释放所有 ref，提交后重新取得组件；禁止保留跨结构变化的可写 ref。

### T3-B4：约 10% 核心测试

选取约 10% 的高风险路径：一个 NPC 创建失败清理与槽位复用；一个 Player 复活/重建身份对照；
一个 Projectile 或 Item 真实 create-release；一个 TileEntity 已支持路径或明确拒绝；一个
关系断开/重复释放场景。不要运行全类全容量矩阵，未执行项列清。

## 5. 完成条件

- 至少一条真实 caller per supported category 使用 T2 的 Arch World/identity contract。
- 创建失败、发布失败、重复释放、关系清理和槽位复用都有当前源码证据。
- 普通复活与真实重建的 UUID/Entity 语义正确；没有第二份权威组件状态。
- 同 tick/next tick 的关键可见性不依赖 chunk/Entity.Id/创建顺序。
- 构建、10% 核心测试、支持集和未覆盖范围交接完整。

## 6. 失败切换与交接

若发现 T2 签名不足，不要在 T3 复制一个平行身份协议；记录最小接口缺口和最小隔离复现，
把不依赖该缺口的类别继续推进。若失败连续两次，按 pua 要求换方向（例如从真实 caller
回放切到最小 lifecycle fixture），并保存两次完整错误。交接包括每类实体的状态机/流程图、
changed files/commit、生命周期和关系合同、测试证据、已知半成品清理风险和 T4/T5 输入。


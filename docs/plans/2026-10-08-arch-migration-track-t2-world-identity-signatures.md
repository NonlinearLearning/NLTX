# Arch 迁移长线 T2：World、身份与生产签名协调切换

文档 ID：DOC-2026-10-08-ARCH-TRACK-T2  
状态：active；这是执行合同，不是已完成报告  
对应批次：A2  
依赖：T1 的 API/包探针；若 T1 未完成，只能做扫描和隔离式准备  
主计划：[自定义 ECS 转向 Arch 执行计划](2026-10-07-custom-ecs-to-arch-execution-plan.md)

## 1. 目标

把“每个加载会话一个 Arch World、领域 UUID 与 Arch Entity 分离、World token 先于实体
访问、生产 caller 使用同一套签名”落到真实代码闭包。T2 不是只加一个 PackageReference，
也不是保留一个旧 `EntityRuntime` 外壳再转发到 Arch；要让后续 T3/T4 能在明确合同上工作。

## 2. 必须遵守的设计合同

- `LoadedWorldSession` 是 Arch World 的唯一生命周期 owner；候选加载创建独立 World，失败
  时先撤销领域映射/关系/缓冲，再释放 World。
- `EntityUuid` 是领域实例身份；Arch `Entity` 只作为当前 World 内运行时定位，不能进入协议
  或持久化身份字段。
- 外部入口检查顺序固定为：会话 token → `entity.WorldId == world.Id` → `world.IsAlive` →
  领域生命周期/能力 → 组件访问。不能只检查 Id。
- World.Id 可回收；不能把旧 World.Id/Entity.Version 当作永不碰撞的全局 epoch。
- 一个实体上的一个组件只有一个权威 Arch 状态；不在旧 store、wrapper、数组中继续写第二份。
- 生产迁移允许破坏旧 API；短期过渡类型必须在交接中列出删除点和退出条件。

## 3. 前置读取与允许范围

先读取根入口、progress、开发协作约束、构建约束、ECS 文件组织、ECS Entity、基础设施边界、
副作用隔离、C# 风格、T1 交接（若有）、主计划与研究文档，并读取 pua skill。启动后用
`/goal` 设置本线目标。

主要范围：`LoadedWorldSession`、`EntityIdentityRegistry`、`WorldStorageRoot`、关系引用的
运行时解析字段、相关项目引用和所有生产构造/创建/访问 caller。实际路径以源码检索为准，
不要按主计划中的示例路径盲改。

不得：

- 删除旧框架全量实现（属于 T5/A8）。
- 在 T2 内重写 NPC/Item/Network 领域规则（交给 T3/T4）。
- 让 Arch Entity 替代 UUID、槽位、协议 identity、存档 key。
- 使用并发、PURE_ECS、事件扩展或 Arch.Persistence 作为迁移前置。

## 4. 分批执行

### T2-B0：签名与 ownership 盘点

1. 搜索所有 `LoadedWorldSession`、身份 registry、world storage、runtime handle、EntityRuntime
   构造和 `World` 生命周期入口。
2. 为每个 caller 标出输入/输出类型、是否跨 tick/线程、是否持有 ref、是否可能在世界切换
   后调用、失败清理责任和需要的 token。
3. 画出最小依赖 DAG，标记必须同批修改的构造链；不要通过兼容 overload 隐藏跨项目签名变化。
4. 阅读 T1 的实际 API 矩阵；如果 T1 未完成，做一份局部最小编译探针，记录为前置补证而非
   猜测。

### T2-B1：World/token/registry 核心切换

1. 让会话创建、发布、卸载、候选失败和 rollback 都经过一个明确的 Arch World owner。
2. 为会话生成独立 token；token 不复用 Arch World.Id，旧 token 请求必须在接触 World 前拒绝。
3. 将身份 registry 的运行时目标改成完整 Arch Entity + 会话 token；保持 UUID 签发、撤销、
   重建换 UUID 和普通复活保 UUID 的领域语义。
4. 明确关系、槽位、Projectile identity 等投影如何指向同一 Entity，且清理由对应 owner 完成。
5. 对 World dispose/clear、实体销毁、候选发布失败建立幂等清理顺序，不假定 CommandBuffer
   是事务。

### T2-B2：生产签名整体编译闭包

1. 一次性修改必要的字段、构造参数、返回值、slot 映射与真实消费者；不要留下隐式 old/new
   双写。
2. 将高频普通组件访问接到 Arch 原生 API 或明确领域接口，避免建立新的泛型转发层。
3. 添加 owner-thread、token、WorldId、alive、生命周期和能力检查；检查失败要有具名结果。
4. 结束所有 Arch ref 的借用后再调用结构变化或跨领域副作用；必要时改值快照/命令意图。
5. 只构建受影响项目和最小加载/一 tick/退出路径，不跑全量测试。

### T2-B3：约 10% 核心测试

选择：一个正常会话创建-一 tick-退出；一个候选 World 失败清理；一个旧 token/跨 World Entity
拒绝；一个 UUID 普通复活与真实重建身份差异；一个生产 caller 编译/冒烟路径。按实际测试
数量约取 10%，其余列为 not-run。重点观察世界/registry/关系/缓冲是否泄漏或误命中。

## 5. 完成条件

- 受影响生产项目编译，且一条真实装配路径使用 Arch World。
- 一个会话只有一个权威 World；没有同实体新旧组件双写。
- World token、WorldId、Version、UUID 的语义和校验顺序在代码/报告中可见。
- 候选失败、卸载和重复清理有证据；旧 API 过渡项有删除清单。
- 10% 核心测试、构建证据和未覆盖 caller 完整交接。

## 6. 失败处理与交接

若生产签名修改导致调用闭包无法构建，先读取完整错误和上下文、检查项目引用和目标框架，
并切换到“先生成调用闭包图/最小隔离项目”的不同方案；不得以兼容桥无限拖延。若 T1 API
结论冲突，暂停依赖部分，保留局部代码和精确错误。

交接必须包括 changed files、commit、调用闭包清单、World/token/UUID 不变量、构建命令、
10% 测试及 T3/T4 需要遵循的字段/接口合同。


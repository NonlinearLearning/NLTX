# 约束与代码漂移对比（准备更新 Context 下的约束）

文档 ID：AUDIT-2026-11-24-CONSTRAINT-DRIFT  
逻辑域：change-control  
产物类型：audit  
状态：complete  
日期：2026-11-24  
canonical 路径：`docs/reviews/audits/2026-11-24-constraint-drift-audit.md`  
审计对象：[Context/项目约束](../../../Context/项目约束/)、[Context/架构约束](../../../Context/架构约束/)

## 1. 范围与方法

读了 10 份约束正文（项目约束 6 份、架构约束 4 份，合计约 90 KB），把其中**可判定**的条目逐条
对到代码：目录存在性、类型存在性、命名空间、构造参数、`Seal()`、副作用调用、缩进与字段命名。
判定使用四种结论：

| 结论 | 含义 |
| --- | --- |
| **已失效** | 文档描述的对象（目录、项目、类型、命名空间）在代码中已不存在或是错的 |
| **代码已深化** | 代码比文档更严格，或已越过文档给出的过渡态 |
| **一致** | 抽样核对通过 |
| **需确认** | 有事实冲突，但需要维护者判断是"文档该改"还是"代码该改" |

样本：`src/**/*.cs` 1662 个文件、2043 个唯一类型名；`src/NSSLC/**` 38680 行用于格式统计。
仓库有并行会话持续新增文件，本文件的计数是 **2026-11-24 复核时点**的快照；判定结论不依赖具体数字。

> **落实状态（2026-11-24）**：§2 的 A1–A8 与 §3 的 B1–B5 已改入对应正文，
> 记录见 [CR-2026-11-24 约束文档与代码漂移同步](../../cr/CR-2026-11-24-constraints-drift-sync.md)。
> §5 的 D1–D6 按"需人工确认"未动。以下三节保留判定时的原始证据。

## 2. 已失效（文档写的东西已经不在）

| # | 出处 | 文档怎么说 | 代码现状 | 建议 |
| --- | --- | --- | --- | --- |
| A1 | [项目文件组织设计约束](../../../Context/架构约束/项目文件组织设计约束.md) §3、§3.1 | 目录树含 `dome/ # 独立工作区`；正文要求 `dome/src/`、`dome/Test/`、`dome/dome1/docs/` 的内部边界 | 仓库根无 `dome/`（根目录只有 AGENTS.md、Build、Context、docs、src、Test、README.md、global.json、Directory.Build.*、NuGet.config） | 删去 `dome/` 相关整节，或标注"独立工作区已移除" |
| A2 | 同上 §3.1；[ECS 领域与基础设施架构边界](../../../Context/架构约束/ECS领域与基础设施架构边界.md) §6 | `src/NSSLC.Infrastructure/分类参考/` 是只读参考目录，禁止在其中实现、生产项目不得编译引用 | 该目录**不存在** | 删除该条，或在重建参考目录后再恢复 |
| A3 | [ECS 领域与基础设施架构边界](../../../Context/架构约束/ECS领域与基础设施架构边界.md) §6 | 保存/恢复用例由 `src/NSSLC.Application/WorldStorage/Persistence…` 承担 | `src/` 下无 `NSSLC.Application`（实际为 NSSLC、NSSLC.Infrastructure、NSSLC.Network、NSSLC.Server、World） | 改指真实落位，或改为"应用层尚未拆出项目" |
| A4 | [ECS 系统输入边界约束](../../../Context/项目约束/ECS系统输入边界约束.md) 术语 | 域 = `Terraria.<Name>` 命名空间，例如 `Terraria.Combat`、`Terraria.Attributes`、`Terraria.Movement` | 命名空间根只有 `Terraria.Share`（383 个文件）与 `Terraria.Unshared`（180 个）；以 `Terraria.Combat`、`Terraria.Attributes`、`Terraria.Movement` 为根的文件为 **0** | 改为 `Terraria.Share.<域>` / `Terraria.Unshared.<域>`，并说明两者区别 |
| A5 | 同上 术语 | 把 `BuffDefinitionRegistry`、`ItemDefinitionRegistry`、`ProjectileRuntimeRegistry` 列为"没有 Seal 的注册表"，把 `CombatAttackDefinitionCatalog`、`ProjectileDefinitionCatalog` 列为已冻结目录 | 5 个名字中 4 个不存在：现名 `BuffRegistry`、`ItemRegistry`、`CombatAttackCatalog`、`ProjectileCatalog`；仅 `ProjectileRuntimeRegistry` 仍在 | 按现名重写该段 |
| A6 | [物品载荷与双宿主设计约束](../../../docs/architecture/物品载荷与双宿主设计约束.md) §7 | 载荷层为 `Share/Item/ItemStack`、`ItemDefinition`、`ItemDefinitionRegistry`、`ItemId` | `ItemDefinition`、`ItemDefinitionRegistry` 不存在；实际为 `ItemStack`、`Item`、`ItemRegistry`、`ItemId` | 换成现名 |
| A7 | [C# 风格约束](../../../Context/项目约束/Google-CSharp-Style-Guide-约束.md) §19 | 命名空间"通常不要超过 2 层深度" | 实际深度：3 层 428 个、4 层 34 个、5 层 101 个（如 `Terraria.Share.Attributes.Buff.System`） | 把上限放宽到 4–5 层，或按 `Terraria.Share.<域>.<职责>` / `Terraria.Unshared.<域>.<职责>` 给出明确形状 |
| A8 | [构建与验证约束](../../../Context/项目约束/构建与验证约束.md) | 目标 `net10.0`，"除非项目有书面记录的兼容例外" | `PacketDesignCompiler.Generator.csproj` 为 `netstandard2.0`，仓库内无例外记录 | 补一条源生成器必须 `netstandard2.0` 的例外 |

## 3. 代码已深化（文档落后于实现）

| # | 出处 | 文档口径 | 代码现状 | 建议 |
| --- | --- | --- | --- | --- |
| B1 | [ECS 系统输入边界约束](../../../Context/项目约束/ECS系统输入边界约束.md) 术语 | "没有 `Seal()` 的注册表不算不可变目录对象" | `Seal()` 已覆盖 6 个目录对象 + 1 个网关：`ItemRegistry`、`ItemUseCatalog`、`ChatCommandCatalog`、`ProjectileCatalog`、`ProjectileBehaviorCatalog`、`CombatAttackCatalog`、`PacketGateway`。原先"未冻结"的 `ItemRegistry` 现在也冻结了；仍无 `Seal()` 的只剩 `BuffRegistry` 与 `ProjectileRuntimeRegistry` | 改成按能力描述（"提供 `Seal()` 且构造后不再变更的目录"），不再逐个点名 |
| B2 | [组件命名设计约束](../../../Context/架构约束/组件命名设计约束.md) §7 | 大量篇幅给 `Base` 基类、`Shared` 基类、组件继承族 | 77 个组件中 32 个是 `readonly record struct`、35 个是 `sealed class`，**没有继承族**（继承自 `Base*`/其它 `Component` 的组件文件为 0 个）；组件身份靠结构组合，不靠基类 | 把 §7 降级为"历史模式/不推荐"，并在 §4.1 明确本仓库组件默认 `readonly record struct` |
| B3 | [C# 风格约束](../../../Context/项目约束/Google-CSharp-Style-Guide-约束.md) §9 | "默认几乎总是优先 `class`"，`struct` 需理由 | 组件层 `record struct` 占 42%（32/77），是领域默认写法 | 补一条"ECS 组件是一等 `record struct` 例外"，避免每次都要写理由 |
| B4 | [ECS 系统输入边界约束](../../../Context/项目约束/ECS系统输入边界约束.md) §存量与例外 | 指向 2026-11-01 审计的迁移债清单 | 抽查 16 处带容器型构造参数的系统，绝大多数已是 `Seal()` 后的目录对象或同域 store；跨域注入基本清零 | 重跑清单，把已还清的项从"迁移债"里删掉 |
| B5 | [非函数式编码副作用隔离规范](../../../Context/项目约束/非函数式编码副作用隔离规范.md) §8 | 建议通过端口隔离 I/O、时钟、随机、日志 | ECS 领域层（`src/NSSLC/**`）中 `DateTime.Now/UtcNow`、`new Random()`、`Console.*`、`File.*`/`Directory.*` 出现次数均为 **0** | 由"建议"升级为"已落实，可作为回归门禁" |

## 4. 一致（抽样核对通过）

| 条目 | 证据 |
| --- | --- |
| 目标框架 `net10.0` | 28 个 csproj 中 27 个 `net10.0`（唯一例外见 A8） |
| `global.json` 选定 SDK | 存在，`version: 10.0.400`，`rollForward: latestPatch` |
| 生成物落 `Build/` | `Directory.Build.props` → `Build/bin`、`Build/obj`、`Build/packages`；`Directory.Build.targets` → `Build/generated` |
| 缩进 2 空格、禁 tab | 2-mod-4 缩进行 16388 vs 4 的倍数 11851，2 空格占主导（例外见 D6） |
| 私有字段 `_camelCase` | 87 个符合 / 13 个不符 |
| `using` 以 `System` 优先 | 254/319 个含 using 的文件 |
| 组件以 `Component` 结尾 | 77 个组件文件中 76 个合规（例外见 D4） |
| `*System.cs` 不下沉到职责目录 | 全区 0 例外（`Component/`、`Command/`、`Query/`、`System/` 内均无） |
| progress 上限 70 行 / 6 KB | 复核时为 51 行 / 5105 字节；本轮已按生命周期整理为 49 行 / 4829 字节、8 条「最近轮次」 |

## 5. 需人工确认（文档与代码冲突，但方向要你定）

| # | 冲突 | 事实 |
| --- | --- | --- |
| D1 | 领域层出现 `System.IO` | `src/NSSLC/Chat/System/ChatNetTextFrame.cs` 用 `MemoryStream`+`BinaryWriter`+`Encoding.UTF8` 做网络帧编解码。要么把帧编解码移到 Infrastructure/Network，要么在[架构边界](../../../Context/架构约束/ECS领域与基础设施架构边界.md)里承认"协议编解码允许留在领域内" |
| D2 | 源码树里留有生成物与数据 | `src/NSSLC/Build/{bin,obj}`（被 .gitignore 覆盖，未提交）与 `src/World/科研.wld`、`科研.wld.bak2` |
| D3 | progress「最近轮次」曾 10 条 > 上限 8 条 | **已在本轮修正**：三条同日目录整理条目合并为一条，最旧的已关闭轮次移出，现为 8 条 / 49 行 / 4829 字节 |
| D4 | 两处组件命名违规 | `EntityFactionComponent.cs` 声明 `Faction`（缺 `Component` 后缀）；`ProjectileAttackReferenceComponent.cs` 声明 `ProjectileAttackDefinitionReferenceComponent`（文件名≠类型名） |
| D5 | 验证项目数 | `Test/` 现有 15 个 csproj；progress 基线写"14 个验证项目" |
| D6 | 22 行含 tab 字符 | 与"禁止 tab"冲突，属局部格式债 |

## 6. 建议的更新顺序

1. **先改硬失效**（A1–A6）：这些是读者按图索骥会直接踩空的地方，改动也最机械。
2. **再补代码已深化的口径**（B1、B2、B4）：把"过渡态描述"换成"当前状态描述"，避免下一轮再漂移。
3. **然后定 D 类**：D1、D2 需要你选"改代码"还是"改约束"；D3–D6 是数据维护，随时可做。
4. **A7、A8、B3** 属于放宽/例外登记，建议合并成一次小改动。

## 7. 复现方式

本审计的判定可复算，关键检查项：

```text
目录/项目存在性      fs.existsSync(repo/dome), fs.existsSync(src/NSSLC.Application), fs.existsSync(.../分类参考)
类型存在性           遍历 src/**/*.cs，正则捕获 public|internal + (class|record struct|record|struct|interface|enum) + 名称
Seal() 覆盖          文件内是否出现 void Seal(
命名空间分布         每文件首个 ^namespace 语句，按前两段聚合
副作用               在 src/NSSLC/** 内匹配 DateTime.(Now|UtcNow)、new Random(、Console.、File./Directory.
System 落位          文件名以 System.cs 结尾且路径含 Component|Command|Query|System 目录
缩进/行宽            每行前导空格宽度直方图；长度 > 100 计数
```

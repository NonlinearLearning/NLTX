# Arch 迁移 T3-B3：官方关系扩展兼容性与生命周期清理探针合同

文档 ID：DOC-2026-10-08-ARCH-TRACK-T3-B3  
状态：active；只做官方扩展兼容性与关系清理证据，不代表生产关系已迁移  
主线：T3 生命周期与关系；对应 A3，消费 T2-B2 的生产身份合同  
前置：T1 partial API handoff；T2-B2 生产签名尚未完成时，生产关系接线必须 blocked  
关联审计：[Arch 原生 API 覆盖与自定义 ECS 退出审计](../reviews/audits/2026-10-08-arch-native-api-coverage-audit.md)

## 1. 目标与边界

本批次只回答一个问题：Arch 官方关系能力在当前 Arch 2.1.0、net10.0、仓库 SDK 和 Release/Debug
构建配置下是否可取得、可编译、可运行并提供可接受的 source/target 清理行为。不得因为关系包
存在于 NuGet 或固定源码声明版本 2.1.0，就把它写成兼容通过。

允许：隔离验证项目、包/源码资产核验、关系行为 probe、只读关系 owner/清理矩阵。禁止：修改
生产 `RuntimeNpcStore`、`EntityRelationState`、`NpcParentRelationComponent`，创建新的通用
关系 graph/dictionary/runtime，或在 T2 World/token 合同未完成前把关系扩展接入生产。

## 2. 必须核对的来源与版本

1. `Arch.Relationships` NuGet index 和 1.0.1 nupkg/nuspec：记录实际依赖，尤其其旧 Arch
   `1.2.6.5-alpha` 声明；不降级 Arch 核心，不安装不存在的 2.1.0 包。
2. Arch.Extended 固定源码 commit `18d1e4c1faec6c83335434969933601b346d19fc`：记录关系
   csproj、源码 hash、Arch 依赖、`WorldRelationshipExtensions` 与清理实现。
3. `Arch-Events 2.1.0` nupkg/nuspec/Arch.dll：确认它是核心变体还是可共存程序集，记录 repository
   commit、资产 hash 和依赖图；不得把普通 Arch 与 Arch-Events 同时作为两个 World 后端。
4. Debug/Release 的 `EVENTS` 定义：确认关系清理方法在目标编译配置是否存在。NLTX 项目定义
   符号不能替代扩展程序集自身编译符号。

## 3. 探针行为

建立 `Test/Terraria.Arch.RelationshipVerification/` 或等价隔离项目，只引用最终实际可解析的
   官方资产。探针至少覆盖以下约 10% 高风险代表项（不能跑完整矩阵）：

- source 创建 relationship，读取 target；
- 同类型多关系或重复关系的实际行为；
- replace/remove 后 source 与反向观察结果；
- source destroy 后关系是否清理；
- target destroy 后关系是否清理；
- World Dispose 后关系/订阅是否可重复清理；
- Entity ID/Version 或 World 复用后旧关系是否误命中新实体。

每一项记录 return/error、权威状态变化、事件/回调、顺序、World 作用域、重复清理结果和未定义
行为。若官方关系 API 在当前组合不可编译，保留第一条完整错误和包/源码 hash，交付
`partial / blocked-by-prerequisite`；不得以自制关系结构替代探针目标。

## 4. 迁移交接

交接给 T2：包兼容性、程序集闭包和 World owner 要求。交接给 T3 生产线：关系新增/替换/删除
的精确 API、source/target 清理顺序、重复清理语义和未支持情形。交接给 T4：关系数据查询是否
可安全在系统中读取，是否存在结构变化/事件重入边界。

证据目录：`Build/diagnostics/ArchMigration/T3-B3/`。记录 restore/build/run、warning/error、
source/package/DLL/PDB hash 和 10% 选择/跳过项。生成 `handoff.md` 并提交 scoped commit；
没有真实生产 caller 接线，不得标记 A3 完成。


# LVCS 契约归入基础设施程序集

文档 ID：CR-2026-11-25-LVCS-CONTRACT-RELOCATION  
逻辑域：architecture、infrastructure  
产物类型：decision、execution  
状态：completed（源码、消费方与文档同步完成；三个受影响项目与验证项目 0 警告 0 错误）  
日期：2026-11-25  
canonical 路径：`docs/cr/CR-2026-11-25-lvcs-contract-relocation.md`  
上游依据：[ECS 领域层与基础设施六边形架构边界](../../Context/架构约束/ECS领域与基础设施架构边界.md) §3、§5；  
[LVCS 统一加载 API 设计](../architecture/2026-11-23-lvcs-unified-loading-api-design.md) §2、§2.1

## 1. 变更

LVCS（Localized Versioned Content Schema，即内容族统一加载契约）的 13 个契约文件从领域共享程序集
`src/NSSLC/Share/Lvcs/`（namespace `Terraria.Share.Lvcs`）迁入基础设施程序集
`src/NSSLC.Infrastructure/Lvcs/`（namespace `NSSLC.Infrastructure.Lvcs`），与原有的 4 个适配器文件同处一目录。

| 项 | 迁移前 | 迁移后 |
| --- | --- | --- |
| 契约位置 | `src/NSSLC/Share/Lvcs/` | `src/NSSLC.Infrastructure/Lvcs/` |
| 契约 namespace | `Terraria.Share.Lvcs` | `NSSLC.Infrastructure.Lvcs` |
| 契约所在程序集 | `Terraria.Components`（领域） | `NSSLC.Infrastructure.Lvcs`（基础设施） |
| 基础设施对领域的引用 | `NSSLC.Infrastructure.Lvcs` → `Terraria.Components` | 该 `ProjectReference` 删除 |
| 领域程序集对 LVCS 的引用 | 自己的 `Share/Lvcs/**` 编译项 | `Compile Include="Share/Lvcs/**/*.cs"` 删除，零引用 |

迁移的 13 个契约文件：`ILvcsCodec`、`ILvcsCodecDescriptor`、`ILvcsCodecRegistry`、`ILvcsLoader`、
`ILvcsSource`、`ILvcsUnit`、`LvcsContentException`、`LvcsDiagnostic`、`LvcsDiagnosticSeverity`、
`LvcsDiagnosticSink`、`LvcsFamilyId`、`LvcsLoadResult`、`LvcsUnit`。文件内容除 namespace 一行外未改。

## 2. 理由

三条都是可验证的事实，不是取舍偏好：

| 现象 | 证据 |
| --- | --- |
| 领域程序集导出自己从不使用的契约 | `git grep -n Lvcs 136c55d -- 'src/NSSLC/*.cs' 'src/NSSLC/**/*.cs'` 除 `Share/Lvcs/` 下的契约文件本身外零命中（该项目文件只剩一条 `<Compile Include="Share/Lvcs/**/*.cs" />`） |
| 基础设施程序集为看自己的契约反向依赖领域程序集 | 迁移前 `NSSLC.Infrastructure.Lvcs.csproj` 含 `ProjectReference ..\..\NSSLC\Terraria.Components.csproj` |
| 契约携带字节载荷，与边界 §3「ECS 领域层…不负责字节流、序列化器配置」冲突 | `ILvcsUnit` 产出 `byte[]`；`ILvcsSource.ReadUnits()` 直接返回字节单元 |

迁移后依赖方向单向：`NSSLC.Server`（内容族 codec）→ `NSSLC.Infrastructure.Lvcs`（契约 + 适配器 ← 领域侧只提供加载产物
`ItemUseDefinitionCatalog`、`ProjectileCatalog` 等不可变目录，不反向依赖加载栈。

## 3. 引用更新

| 范围 | 文件数 | 处理 |
| --- | --- | --- |
| `src/NSSLC.Server/`（`Program.cs`、`Content/ItemUse/ItemUseCodec.cs`、`Content/Projectile/` 8 个读取器与 codec） | 10 | `using Terraria.Share.Lvcs;` → `using NSSLC.Infrastructure.Lvcs;`，按既有 System → 第三方/项目 → `Terraria.*` 顺序落位 |
| `Test/NSSLC.Server/Terraria.Server.Lvcs/Program.cs` | 1 | 删除与新命名空间重复的旧 using |
| `docs/architecture/2026-11-23-lvcs-unified-loading-api-design.md` | 1 | §1.3 namespace、§2 分层块、§2 分层依据表与新增 §2.1 契约归位、§10.1 族表、§11 断言数与前缀表、§12 硬约束、§15 完成定义 |
| `Context/项目约束/错误处理约束.md` | 1 | 合格样板表的路径列改指新目录 |
| `docs/research/2026-11-25-error-handling-mechanism-research.md` | 1 | 调查表 namespace 列改为 `NSSLC.Infrastructure.Lvcs` |
| `src/NSSLC/Share/` | — | 空目录 `Share/Lvcs/` 删除（剩 8 个共享能力域） |

## 4. 验证

| 命令 | 退出码 | 结果 | 输出路径 |
| --- | --- | --- | --- |
| `dotnet build src/NSSLC.Infrastructure/Lvcs/NSSLC.Infrastructure.Lvcs.csproj` | 0 | 0 警告 0 错误 | `Build/bin/NSSLC.Infrastructure.Lvcs/Debug/net10.0/` |
| `dotnet build src/NSSLC/Terraria.Components.csproj` | 0 | 0 警告 0 错误 | `Build/bin/...` |
| `dotnet build src/NSSLC.Server/NSSLC.Server.csproj -v m` | 0 | 0 错误 19 警告；逐条核对后全部落在依赖项目 `NSSLC.WorldGeneration` 与 `NSSLC.Network` 的既有代码（CS0219 / CS0649 / CS0105 等），本轮改动的 `NSSLC.Server/Content/**`、`NSSLC.Infrastructure/Lvcs/**`、`NSSLC/Projectile/**` 各 0 条 | `Build/bin/NSSLC.Server/Debug/net10.0/` |
| `dotnet build Test/NSSLC.Server/Terraria.Server.Lvcs/Terraria.Server.Lvcs.csproj` | 0 | 0 警告 0 错误 | `Build/bin/...` |
| `dotnet run --project Test/NSSLC.Server/Terraria.Server.Lvcs/Terraria.Server.Lvcs.csproj --no-build` | 0 | `RESULT: PASS`，73 项断言全绿 | — |
| `git grep "using Terraria.Share.Lvcs"` | 1（无命中） | 旧命名空间零残留 | — |
| `git grep "Share/Lvcs" -- '*.cs' '*.csproj'` | 1（无命中） | 代码与项目文件零残留 | — |

## 5. 未改动

- 加载产物仍是领域共享类型（`ItemUseDefinitionCatalog`、`ProjectileCatalog` 等不可变目录），未随契约外迁。
- 具体内容族 codec 仍在 `src/NSSLC.Server/Content/`，未并入基础设施程序集；内容资产路径不变。
- 契约文件的行为、签名、诊断文案未改，只有 namespace 一行变化。
- 构建策略、`Build/` 输出布局、`Directory.Build.*` 未改。

## 6. 回滚

```powershell
cd D:\TRbackup\NLTX
git mv src\NSSLC.Infrastructure\Lvcs\ILvcsCodec.cs                src\NSSLC\Share\Lvcs\
# ……其余 12 个契约文件同理，逐个 git mv 回 Share\Lvcs\
git checkout -- src/NSSLC.Infrastructure/Lvcs/NSSLC.Infrastructure.Lvcs.csproj src/NSSLC/Terraria.Components.csproj
git checkout -- src/NSSLC.Server Test/NSSLC.Server/Terraria.Server.Lvcs
git checkout -- docs Context
```

回滚须同时把 13 个文件的 namespace 改回 `Terraria.Share.Lvcs`，并恢复
`NSSLC.Infrastructure.Lvcs.csproj` 对 `Terraria.Components.csproj` 的引用；否则契约类型会对消费方不可见。

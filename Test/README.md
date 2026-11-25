# Test 验证项目组织

## 定位

`Test/` 下每个项目都是一个**独立控制台验证程序**（`OutputType=Exe`，`net10.0`）：自带
`Program.cs`，逐条打印 `PASS` / `FAIL`，失败数非零时退出码为 1。没有测试框架、没有 runner
发现机制，**目录不参与执行**，执行入口始终是 `dotnet run --project`。

## 目录规则

1. **一级目录 = 被验证的生产边界**，目录名与生产 csproj 同名：

   | 目录 | 被验证的生产项目 |
   | --- | --- |
   | `Test/Terraria.Components/` | `src/NSSLC/Terraria.Components.csproj` |
   | `Test/NSSLC.Network/` | `src/NSSLC.Network/NSSLC.Network.csproj` |

   其他边界（`NSSLC.Server/`、`NSSLC.Infrastructure.*/`）等第一个验证项目出现再建，
   不预建空目录。

2. **项目名 = `Terraria.<生产领域目录>[.<验证目标>]`**，不使用统一后缀——`Test/` 本身已经
   表达这是验证程序。名字必须脱离目录也能读懂，并且**全局唯一**：构建产物按
   `Build/bin/<项目名>/` 落盘（见 [Directory.Build.props](../Directory.Build.props)），
   重名会合并产物目录。

3. **项目名跟生产目录，不跟命名空间。** `src/NSSLC/World/ComponentSystem/TileEntity/` 和
   `src/NSSLC/WorldItem/` 仍是 `Terraria.World`，`src/NSSLC/Share/ItemUse/` 仍是
   `Terraria.Player`。这是目录与命名空间解耦的既定事实（见
   [项目文件组织设计约束](../Context/架构约束/项目文件组织设计约束.md) 第 5 节）；
   测试名若跟命名空间，会退回拆目录之前的旧域。

4. **二级领域目录按需引入**：只在同一「边界 + 领域」累计 ≥3 个项目时才建，例如以后的
   `Test/Terraria.Components/Npc/`。现在不建。

5. **跨边界项目放在「主要断言对象」的权威归属域下。** `Terraria.EntitySpawn` 引用
   `NSSLC.Network` 只是为了末尾验证网络槽位分配，主体是刷怪规则，因此留在
   `Terraria.Components/`；`Terraria.Network.OutboundOrdering` 的主体是投递泵与背压，
   因此进 `NSSLC.Network/`。完整引用集以 csproj 为准，不靠目录表达。

6. **夹具放各自项目内**，生成物、验证日志和证据放 `Build/`。

## 类型与意图

验证类型和审计意图**不进路径**，写在各项目 `Program.cs` 的头注释里。描述类型时只留四档，
避免各自造词：

| type | 含义 |
| --- | --- |
| `rule` | 对照参考实现（Version4）的规则与布局 |
| `contract` | 单点 API / 框架契约 |
| `chain` | 跨系统链路（授予→拒绝→到期、生成→驱动等） |
| `transport` | 真实 socket、时序与背压（当前只有一项，也是最可能 flaky 的一项） |

## 运行

单个项目：

```powershell
dotnet build Test/Terraria.Components/Terraria.Chat/Terraria.Chat.csproj --nologo --verbosity minimal
dotnet run   --project Test/Terraria.Components/Terraria.Chat/Terraria.Chat.csproj --no-build --no-restore
```

全量：

```powershell
Get-ChildItem Test -Recurse -Filter *.csproj | ForEach-Object {
  dotnet build $_.FullName --nologo --verbosity minimal
  if ($LASTEXITCODE -eq 0) { dotnet run --project $_.FullName --no-build --no-restore }
}
```

只构建受影响项目，不加 `-t:Rebuild`，验证输出仍落在 `Build/bin/<项目名>/`——详见
[构建与验证约束](../Context/项目约束/构建与验证约束.md)。

## 新增验证项目检查清单

- [ ] 落在一级边界目录下，目录名与该边界生产项目一致。
- [ ] 项目名 = `Terraria.<生产领域目录>[.<目标>]`，与生产项目、其他验证项目都不重名。
- [ ] csproj 引用按 `Test/<边界>/<项目>` 深度写 `../../../src/...`。
- [ ] `Program.cs` 头注释写明「固定几件事」与验证类型，与下面的 `PASS` 检查逐条对应。
- [ ] 成功返回 0、失败返回 1，最后一行给出结论。
- [ ] 只构建受影响项目，并记录命令、退出码、告警/错误数和产物路径。

## 迁移对照（2026-11-22）

历史文档里出现的旧路径按此表换算。

| 旧路径 | 新路径 | 新项目名 |
| --- | --- | --- |
| `Test/Terraria.Attributes.Verification` | `Test/Terraria.Components/Terraria.Attributes` | `Terraria.Attributes` |
| `Test/Terraria.Chat.Verification` | `Test/Terraria.Components/Terraria.Chat` | `Terraria.Chat` |
| `Test/Terraria.Combat.ProtectionWindow.Verification` | `Test/Terraria.Components/Terraria.Combat.ProtectionWindow` | `Terraria.Combat.ProtectionWindow` |
| `Test/Terraria.EcsScheduling.Verification` | `Test/Terraria.Components/Terraria.Entity.Scheduling` | `Terraria.Entity.Scheduling` |
| `Test/Terraria.EntityDefinition.Verification` | `Test/Terraria.Components/Terraria.Entity.Definition` | `Terraria.Entity.Definition` |
| `Test/Terraria.Inventory.Verification` | `Test/Terraria.Components/Terraria.Inventory` | `Terraria.Inventory` |
| `Test/Terraria.PlayerItemUse.Verification` | `Test/Terraria.Components/Terraria.ItemUse` | `Terraria.ItemUse` |
| `Test/Terraria.NpcAi.MinimalVerification` | `Test/Terraria.Components/Terraria.Npc.Ai` | `Terraria.Npc.Ai` |
| `Test/Terraria.Npc.SpawnEligibility.Verification` | `Test/Terraria.Components/Terraria.EntitySpawn` | `Terraria.EntitySpawn` |
| `Test/Terraria.Projectile.Verification` | `Test/Terraria.Components/Terraria.Projectile` | `Terraria.Projectile` |
| `Test/Terraria.WorldSession.TileEntity.Verification` | `Test/Terraria.Components/Terraria.TileEntity` | `Terraria.TileEntity` |
| `Test/Terraria.WorldItem.Verification` | `Test/Terraria.Components/Terraria.WorldItem` | `Terraria.WorldItem` |
| `Test/Terraria.Network.OutboundOrdering.Verification` | `Test/NSSLC.Network/Terraria.Network.OutboundOrdering` | `Terraria.Network.OutboundOrdering` |

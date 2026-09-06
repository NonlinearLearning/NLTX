# WorldSession Component Code Implementation Plan

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** 将 WorldSession 代码草案中的 6 个 Component 以兼容性优先方式实现到 NLTX 的根 src 项目，并用 focused verification 验证字段、不变量和快照边界。

**Architecture:** 采用用户选择的方案 A：不迁移 dome、不重构全部 WorldSession 领域，保留现有 WorldSessionComponents.cs 中非目标的事件进度和刷怪压力类型。由于仓库要求一个核心公开类型对应一个同名文件，6 个实际 Component 和新增值类型落在 src/WorldSession/ 的同名文件中；WorldSessionComponents.cs 只保留兼容性 legacy 类型，移除会与 canonical 类型重复的目标定义。普通 Component 保持现有 ECS 风格的可变数据存储；WorldTickSnapshot 使用不可变快照值和显式提交工厂。

**Tech Stack:** C#、.NET SDK 10.0.400、net10.0、现有 Terraria.WorldSession.csproj、根 Test 下的 console-style focused verifier。

---

## 1. 记录实现边界

**Files:**

- Read: D:\TRbackup\NLTX\AGENTS.md
- Read: D:\TRbackup\NLTX\progress.md
- Read: D:\TRbackup\NLTX\docs\design\2026-09-06-version4-world-session-component-code-draft.md
- Modify: D:\TRbackup\NLTX\docs\plans\2026-09-06-version4-world-session-component-implementation.md

**Step 1: 确认当前用户选择**

记录方案 A 的约束：兼容性优先、保留非目标 legacy 类型、不修改 dome；但不得违反一核心公开类型一同名文件和根项目构建约束。

**Step 2: 确认工作区边界**

保留已有未提交改动；本任务只触碰 src/WorldSession、对应 focused verifier、实现计划文档和必要的项目文件。

## 2. 写入第一个失败测试

**Files:**

- Create: D:\TRbackup\NLTX\Test\Terraria.WorldSession.Components.Verification\Terraria.WorldSession.Components.Verification.csproj
- Create: D:\TRbackup\NLTX\Test\Terraria.WorldSession.Components.Verification\Program.cs

**Step 1: 建立 verifier 项目**

项目目标为 net10.0，引用 D:\TRbackup\NLTX\src\WorldSession\Terraria.WorldSession.csproj，使用 console-style verifier，与现有 WorldInteraction verifier 保持一致。

**Step 2: 写入红测试**

测试先通过反射查找 6 个目标类型，避免在生产类型尚不存在时把红阶段变成测试项目源码无法编译。第一条断言必须报告目标类型缺失；后续断言覆盖以下已确定契约：

- WorldDescriptorState 的默认名称、时间无关字段、Bounds 和 SectionCount 派生值；
- WorldRulesState 的 Journey、DualDungeons 和 Skyblock 派生值；
- WorldClockState 的 Version4 初始时间和月相范围判定；
- WorldWeatherState 的无尽降雨阈值；
- SessionReadinessState 在未 Ready、生成屏障或失败码存在时不得允许实体更新；
- WorldTickSnapshot 的 Uncommitted、负 revision 拒绝、非 Ready 不得提交；
- committed snapshot 的 5 个值非空且属性只读。

**Step 3: 运行红测试**

在执行前检查 active dotnet.exe/csc.exe；若有其他 owner，等待而不终止。

Run:

~~~powershell
pwsh -NoProfile -File .\Build\Tools\Invoke-SerialDotnet.ps1 `
  run .\Test\Terraria.WorldSession.Components.Verification\Terraria.WorldSession.Components.Verification.csproj `
  -m:1 -nr:false `
  -p:UseSharedCompilation=false `
  -p:MSBuildNodeReuse=false `
  -p:BuildInParallel=false
~~~

Expected: 非零退出；失败原因是至少一个目标类型尚不存在，而不是 SDK、项目路径或测试程序语法错误。记录退出码、错误数量和输出路径。

## 3. 整理兼容入口并创建支持值类型

**Files:**

- Modify: D:\TRbackup\NLTX\src\WorldSession\WorldSessionComponents.cs
- Create: D:\TRbackup\NLTX\src\WorldSession\WorldBounds.cs
- Create: D:\TRbackup\NLTX\src\WorldSession\WorldGameMode.cs
- Create: D:\TRbackup\NLTX\src\WorldSession\WorldSecretSeedFlags.cs
- Create: D:\TRbackup\NLTX\src\WorldSession\WorldEvilType.cs
- Create: D:\TRbackup\NLTX\src\WorldSession\OreTierState.cs
- Create: D:\TRbackup\NLTX\src\WorldSession\SessionReadinessPhase.cs

**Step 1: 移除重复目标定义**

从 WorldSessionComponents.cs 移除将由 canonical 文件承载的 WorldBounds、WorldDescriptorState、WorldSecretSeedFlags、WorldEvilType、OreTierState 和 WorldRulesState 定义；保留 WorldTimeWeatherState、WorldEventProgressState、WorldSpawnPressureState 及其仅属于 legacy/相邻领域的支持类型，除非编译引用证明需要进一步拆分。

**Step 2: 创建值类型**

使用代码草案中的精确字段和枚举成员：

- WorldBounds 为 readonly record struct；
- WorldGameMode 为 Classic/Expert/Master/Journey；
- WorldSecretSeedFlags 使用 ulong 和 Flags；
- WorldEvilType 为 Corruption/Crimson；
- OreTierState 为 readonly record struct，并提供 Uninitialized；
- SessionReadinessPhase 为 AwaitingData/ProcessingData/Ready/Failed/Unloading。

不得复制 dome 命名空间的类型，不得把 Version4 协议数值映射未经验证写入构造函数。

## 4. 实现 5 个核心状态 Component

**Files:**

- Create: D:\TRbackup\NLTX\src\WorldSession\WorldDescriptorState.cs
- Create: D:\TRbackup\NLTX\src\WorldSession\WorldRulesState.cs
- Create: D:\TRbackup\NLTX\src\WorldSession\WorldClockState.cs
- Create: D:\TRbackup\NLTX\src\WorldSession\WorldWeatherState.cs
- Create: D:\TRbackup\NLTX\src\WorldSession\SessionReadinessState.cs

**Step 1: 实现 WorldDescriptorState**

保留 WorldId、UniqueId、Name、SeedText、WorldGeneratorVersion、尺寸、边界、地层和出生/地牢锚点；SectionCountX、SectionCountY、HasSurface 和 Bounds 只能是派生属性。Section 宽高使用 200/150 具名常量。不得加入 Entity ID、Network ID、Section 内容或文件流。

**Step 2: 实现 WorldRulesState**

保留 GameMode、HardMode、SecretSeeds、WorldEvil 和 SavedOreTiers；只提供 IsJourneyMode、UsesDualDungeons、IsSkyblockWorld 派生属性。不加入 DifficultyOverride、EffectiveDifficulty、生态、事件进度、ShadowOrb 或 Altar 状态。

**Step 3: 实现 WorldClockState**

保留 DayTime=true、Time=13500.0、MoonPhase 和候选 ClockRevision；提供 HasValidMoonPhase。不得加入 UpdateTime、事件边界或调度逻辑。

**Step 4: 实现 WorldWeatherState**

保留雨/风事实和天气缓存候选；提供 IsRainingForever。不得加入 cloudAlpha、numClouds、粒子、音乐或表现逻辑；不额外强制未经证据确认的雨强范围。

**Step 5: 实现 SessionReadinessState**

保留 Phase、GenerationBarrierActive、FailureStatusCode；CanUpdateEntities 必须是只读派生值，且仅在 Ready、无屏障、无失败码时为 true。不得保存 Exception、Task、锁、文件流或连接。

## 5. 实现不可变快照值和 WorldTickSnapshot

**Files:**

- Create: D:\TRbackup\NLTX\src\WorldSession\WorldDescriptorSnapshotValue.cs
- Create: D:\TRbackup\NLTX\src\WorldSession\WorldRulesSnapshotValue.cs
- Create: D:\TRbackup\NLTX\src\WorldSession\WorldClockSnapshotValue.cs
- Create: D:\TRbackup\NLTX\src\WorldSession\WorldWeatherSnapshotValue.cs
- Create: D:\TRbackup\NLTX\src\WorldSession\SessionReadinessSnapshotValue.cs
- Create: D:\TRbackup\NLTX\src\WorldSession\WorldTickSnapshot.cs

**Step 1: 创建 5 个 readonly record struct**

快照值只复制描述、规则、时钟、天气事实和 readiness 值；不复制 Entity ID、Network ID、Section 位图、异常对象、协议 buffer 或可变集合。

**Step 2: 创建 WorldTickSnapshot**

提供：

- Uncommitted 静态工厂；
- CreateCommitted(revision, descriptor, rules, clock, weather, readiness)；
- 只读 Revision、IsCommitted、Descriptor、Rules、Clock、Weather、Readiness 属性。

CreateCommitted 拒绝负 revision 和不能更新实体的 readiness。构造过程只处理传入值，不读取全局状态、不执行 I/O、不修改源 Component。

## 6. 运行绿色测试并收敛 verifier

**Files:**

- Modify: D:\TRbackup\NLTX\Test\Terraria.WorldSession.Components.Verification\Program.cs

**Step 1: 运行 verifier**

Run:

~~~powershell
pwsh -NoProfile -File .\Build\Tools\Invoke-SerialDotnet.ps1 `
  run .\Test\Terraria.WorldSession.Components.Verification\Terraria.WorldSession.Components.Verification.csproj `
  -m:1 -nr:false `
  -p:UseSharedCompilation=false `
  -p:MSBuildNodeReuse=false `
  -p:BuildInParallel=false
~~~

Expected: 0 exit code，输出明确的 PASS 行，无编译错误或未处理异常。记录警告/错误数量和 Build/bin 下的输出路径。

**Step 2: 如有失败，按失败类型修复**

- 重复类型：只调整 WorldSessionComponents.cs 的目标定义，不修改 dome；
- namespace 不匹配：统一到 Terraria.WorldSession.Components；
- 快照不变量失败：先修正测试/实现契约，不放宽安全门控；
- 现有 legacy 类型引用失败：保留其兼容定义或调整引用，不把其字段塞入 6 个目标 Component。

**Step 3: 加强直接类型测试**

在生产类型已存在后，把关键反射断言替换为直接类型构造和属性访问，保持测试仍然只验证公开契约，不测试字段排列或实现细节。

## 7. 串行编译和项目级验证

**Files:**

- Verify: D:\TRbackup\NLTX\src\WorldSession\Terraria.WorldSession.csproj
- Verify: D:\TRbackup\NLTX\Test\Terraria.WorldSession.Components.Verification\Terraria.WorldSession.Components.Verification.csproj

**Step 1: 检查构建进程**

使用 AGENTS.md 中的 Win32_Process 查询，确认没有其他 compile-capable owner。

**Step 2: 构建受影响生产项目**

Run from repository root:

~~~powershell
pwsh -NoProfile -File .\Build\Tools\Invoke-SerialDotnet.ps1 `
  build .\src\WorldSession\Terraria.WorldSession.csproj `
  -m:1 -nr:false `
  -p:UseSharedCompilation=false `
  -p:MSBuildNodeReuse=false `
  -p:BuildInParallel=false
~~~

记录 exit code、warning/error 数量，并确认产物位于 Build/bin/ 而不是 src/ 目录。

**Step 3: 无构建运行 verifier**

Run:

~~~powershell
pwsh -NoProfile -File .\Build\Tools\Invoke-SerialDotnet.ps1 `
  run .\Test\Terraria.WorldSession.Components.Verification\Terraria.WorldSession.Components.Verification.csproj `
  --no-build --no-restore `
  -m:1 -nr:false `
  -p:UseSharedCompilation=false `
  -p:MSBuildNodeReuse=false `
  -p:BuildInParallel=false
~~~

记录退出码、PASS/FAIL 数量和输出路径。

## 8. 最终检查

**Files:**

- Verify: D:\TRbackup\NLTX\src\WorldSession\
- Verify: D:\TRbackup\NLTX\Test\Terraria.WorldSession.Components.Verification\
- Verify: D:\TRbackup\NLTX\docs\design\2026-09-06-version4-world-session-component-code-draft.md

**Step 1: 检查文件组织**

确认每个新增核心公开类型拥有同名文件，未创建通用 Components 子目录，未修改 dome。

**Step 2: 检查状态边界**

确认没有把事件/进度、云表现、保存 staging、网络包或异常对象加入 6 个目标 Component。

**Step 3: 检查 Git 差异**

确认本任务的目标文件清晰可识别；保留并报告开始前已存在的无关改动，不执行 reset、clean 或 broad cleanup。

**Step 4: 交付报告**

报告以下事实：新增/修改文件、测试命令及结果、生产构建命令及结果、warning/error 数量、Build/bin 产物路径，以及仍未解决的设计 gate。



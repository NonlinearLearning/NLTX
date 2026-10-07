# CR-2026-10-04：非通信模拟运行缺口整改

状态：实施中  
级别：Major（按影响范围评定）  
提出方式：用户对话  
提出日期：2026-10-04  
授权依据：用户 goal 明确要求根据完整审查报告补齐代码；goal continuation 要求保持原始完整范围，不缩减为较小子集。  
排期：沿用报告 P0 → P1 → P2 → P3 → P4 顺序，在同一持续 goal 内分批推进。

## 需求原文

> /goal 根据这份报告补齐代码[审查报告](D:/TRbackup/NLTX/docs/reviews/audit-report-NLTX-2026-10-04.md)。

## 变更范围

落实审查报告的 F01–F13：修复正式构建图；建立无通信 finite-run Host、可 Step 的模拟内核、固定 tick、内容发布与最小支持集；将加载会话发布为可运行 owner；接入运行期快照保存与退出；补齐报告定义的首个玩家/NPC/Projectile/物品玩法切片；确认 tile/time 状态唯一写入边界；用生产组合添加端到端 smoke 和失败/取消路径验收。

完整 Terraria 内容兼容不作为首批支持承诺。未实现的类型必须显式拒绝，不能以 no-op 表示成功。WorldFile 319 格式保持兼容，除非后续有独立证据及必要迁移。

## 影响评估

- **数据格式：**目标复用当前 WorldFile 319 DTO/codec，不改变文件 schema。
- **代码范围：**Projectile、NPC、Player、Content、WorldSession、WorldStorage、Application、WorldGeneration/WorldStorage Infrastructure、Host executable、solution 与集成验证。
- **架构影响：**补生产组合根与执行阶段，不重写 ECS，不引用 `分类参考` 作为生产实现。
- **风险：**NPC/Projectile slot 身份转换；加载成功后的实体 hydration 与发布；tick 阶段顺序；Main.tile 等 legacy 视图与持久化 owner 的唯一真值；保存快照与模拟线程的一致性。
- **现有工作树：**包含大量用户未提交改动。实施只做必要修改，保留这些改动，不做广泛清理或 reset。

## 分批验收

- **P0 / B0 构建基线：**修 F01 的身份类型/项目依赖；重建当前有效 root solution；验证项目路径、受影响项目构建及报告要求的加载/Projectile verifiers。
- **P1 / B1–B3 模拟宿主：**finite-run executable 与手动 Step 内核；有效内容目录启动；fresh load → publish → Ready；基础世界时钟；停止/取消语义。真实标准 319 世界 3600 ticks。
- **P2 / B4 运行存档：**在 tick 提交边界捕获一致 owner snapshot，经现有 coordinator 保存，重载后核对修改过的世界状态。
- **P3 / B5–B7 玩法切片：**玩家移动/地形碰撞/首个物品行为；一个敌怪及一个 town NPC 的恢复、spawn/tick；一个普通射弹的运动、碰撞和伤害；死亡、掉落、拾取。
- **P4 / B8–B9 加固：**0/1/多玩家、重复切换、加载失败/取消、保存失败与旧存档保持、600/3600-tick 生产组合 smoke。

每批结束记录 build/verifier 命令、退出码、警告/错误与产物路径。批次的验收证据需覆盖本批目标，不能用局部规则 PASS 推导全流程完成。

## 不在范围

- 客户端渲染、音频、聊天 UI、视觉粒子、成就；
- 所有 Terraria NPC/Projectile AI、所有 item/tile entity、全部世界事件；
- 非空 CreativePowers、所有旧 WorldFile 版本的完整运行兼容；
- 网络包设计与 TCP gateway 行为。

以上不是 F01–F13 核心最小运行切片的替代实现；不支持的能力必须明确记录。

## 决定

用户明确授权按报告完整范围补齐代码。本次选择 loop 方式：以 F01–F13 全部有代码落点和对应验收证据为退出条件，按 P0–P4 依赖推进；当一轮未完成时保留 goal 和批次状态继续执行。

## 实施进展（2026-10-05，持续进行）

- **F01：**运行 NPC slot 与 WorldStorage slot 在持久住房修复提交边界显式按值转换，避免把不同领域身份类型混用。
- **F02：**根 `Terraria.Dome.sln` 的 92 个项目路径均存在；修正一个 WorldSession 验证项目的过期 ProjectReference。新增 `NSSLC.NonCommunicationSimulation.sln`，只包含模拟宿主及传递生产依赖，供无通信链路单独构建。
- **F03–F05：**已有 finite-run Host、owner-thread `Step` 内核、load/recovery/publication、NPC 与本地玩家 hydration，以及经 `ContentCatalogBuildSystem` 校验的最小内容目录和支持清单。
- **F06–F08：**已有本地脚本输入、玩家 TileCollision 移动/跳跃、弓箭发射与普通箭 projectile 处理、Zombie/Slime 的有限 AI、NPC 接触伤害/死亡/重生和 Gel 掉落拾取。AI/Projectile/Item 类型仍由支持清单限界。
- **F09：**kernel 每个 committed tick 推进世界时钟、天气倒计时及已建模的部分事件计时；全量天气/事件规则尚未接入。
- **F10：**运行期掉落物有分配、物理、超时和库存拾取流程；训练假人与逻辑感应器按类型分派。当前未支持的 TileEntity 类型及逻辑检查会在宿主装配时显式拒绝。玩家压下已登记的 135/428 号压力板会遍历相连线色并切换致动器，按压状态由 session owner 持有、再投影到 legacy helper。
- **F11–F12：**已有从活动 LoadedWorldSession 捕获持久化快照并保存/自动保存的 Host 接线；runtime projection 每 tick 把 owner tile/time 状态发布到 legacy 视图，已实现的线路变更则先提交 tile owner 再投影。WorldGen/Liquid 运行阶段尚未接入，因此不存在其 legacy tile 写回调用点。
- **F13：**Host 在每次提交 tick 时检查 phase 顺序并输出运行报告。追加验收已覆盖 0/1/2 玩家 600 ticks、3600 ticks 战斗/掉落/拾取和接触死亡、保存重载、加载拒绝/取消、保存失败保留旧档及重复 world switch。

### 本轮构建证据

- `dotnet build src/NSSLC.Tools.Simulation/NSSLC.Tools.Simulation.csproj --no-restore --nologo -v:minimal -clp:NoSummary`：退出码 0，增量构建 0 警告 / 0 错误。
- `dotnet build NSSLC.NonCommunicationSimulation.sln --no-restore --nologo -v:minimal -m:1 -clp:NoSummary`：退出码 0，17 个既有 WorldGeneration 警告 / 0 错误。宿主程序集位于 `Build/bin/NSSLC.Tools.Simulation/Debug/net10.0/NSSLC.Tools.Simulation.dll`。
- `dotnet build Terraria.Dome.sln --no-restore --nologo -v:minimal -m:1 -clp:NoSummary`：还原 solution 后仍退出码 1，43 个编译错误 / 0 警告，错误分布在 NetworkCodecGenerator 和多个旧验证项目。solution 路径问题已消除，但完整根 solution 目前尚未达到绿色构建。
- 之前的根 solution 构建记录是路径修复前结果；92 个项目路径随后已确认存在，当前无通信入口以 `NSSLC.NonCommunicationSimulation.sln` 为准。根 solution 的完整编译诊断仍需重跑并分类。

---

## 追加实施与 P4 验收（2026-10-05）

- 修正投射物 owner 边界：`ProjectileHitImmunitySystem.RecordAcceptedNpcHit(ProjectileEntityState, int)` 在 Projectile 所属程序集内提交命中免疫；Simulation Host 不再赋值 internal setter。
- NPC 死亡掉落物报告加入类型、stack、位置、速度、剩余寿命；基础拾取范围定为 48 像素，并报告最近一次拾取到玩家碰撞框的距离。
- 取消在 tick 已提交且 kernel 已 Stopped 后结束循环，不再向 stopped kernel 排入下一条命令。Ctrl+C 和 `--cancel-after-ms` 都会在保存最新已提交状态后写 `Canceled: true` 报告。
- 新增重复 `--switch-world <world.wld>`：停止旧 kernel、清空 legacy runtime、创建新的 NPC/物品/玩家/Projectile owner、加载发布新 session，并在每个新世界实际提交首 tick。切换报告验证旧 session 不再 active。
- 新增可重跑验收脚本 [verify.ps1](D:/TRbackup/NLTX/Test/NSSLC.Tools.Simulation.Verification/verify.ps1) 与说明 [README.md](D:/TRbackup/NLTX/Test/NSSLC.Tools.Simulation.Verification/README.md)。脚本生成固定 3600 tick 输入并断言完整生产组合，不依赖临时手工输入文件。

### P4 运行证据

运行命令：

    pwsh -NoProfile -File Test/NSSLC.Tools.Simulation.Verification/verify.ps1 -WorldPath Build/diagnostics/WorldGenerationRoundTrip/12345-small-final/generated.wld -SecondWorldPath Build/diagnostics/WorldGenerationRoundTrip/67890-medium-final/generated.wld

脚本构建命令为 `dotnet build src/NSSLC.Tools.Simulation/NSSLC.Tools.Simulation.csproj --nologo -v:minimal -clp:NoSummary`；该次构建退出码 0，14 个既有 warning / 0 错误。宿主输出位于 `Build/bin/NSSLC.Tools.Simulation/Debug/net10.0/NSSLC.Tools.Simulation.dll`。

退出码 0，输出 `PASS: non-communication simulation verification`。最新摘要在 `Build/diagnostics/NonCommunicationSimulationAudit/verification-run/summary.json`：

- 0、1、2 名玩家分别完成 600 tick，三个报告均为 published，世界时间各前进 600。
- 3600 tick scripted bow 场景命中并击败 1 个 Zombie、生成 1 个 Gel、拾取 1 次；玩家 Gel=1、地面物品=0。拾取时物品到玩家碰撞框距离 46.62 像素，覆盖非重叠的拾取范围边界。运行后保存成功，输入世界 SHA-256 保持不变。
- 同场景保存后的新进程重载，保存时 FinalTime=17100 与重载 InitialTime=17100 一致，session 发布成功。
- 单独 Zombie 接触场景运行 3600 tick，PvE 死亡计数为 4，验证死亡后继续重生循环。
- 当前 `src/World/科研.wld` 格式 326 在 tick 0 被拒绝，失败为 UnsupportedFormatVersion；WorldFile 319 样本可运行。
- `--cancel-after-ms 1000` 在 WorldFile 319 加载尝试期间取消，LoadAttempts=1、Canceled=true、TickNumber=0，进程退出码 130。
- 对目标存档持有读共享文件句柄制造写失败，保存结果为 IoFailure；失败前后 SHA-256 相同，报告 `PreviousSavePreserved=true`。
- Small → Medium → Small 连续两次切换均创建新 session、使旧 session inactive，并在新 session 提交 tick 1。
- 生产组合 build 通过，0 错误；该项目依赖构建报告 14 个 WorldSession `ColorRgba` 类型冲突 warning。之前无通信 solution 构建曾报告 17 个 WorldGeneration warning / 0 错误。

### 追加实施与复验（2026-10-05）

- `ActiveLiquidTickPhase` 在已提交 tick 中调用液体更新，并通过 runtime tile 的 mutation observer 收集真实改动坐标，只把这些坐标提交回 `LoadedWorldSession`，不按 tick 扫描全图。探针容器底部封闭，避免夹具自身漏水。
- 长跑验收发现并修复一个真实状态投影缺口：tile map 发布时加载的 `maxTilesY` 已更新，但 `Main.UnderworldLayer` 仍取自空 headless host 的初始化值。液体算法因而把浅层水误判为地狱水，每 tick 蒸发 2 单位。`LegacyWorldTileMapProjection.PublishRuntimeTileMap` 现在按 WorldFile 持久化高度推导并发布 `UnderworldLayer = SizeY - 200`。
- `dotnet build src/NSSLC.Tools.Simulation/NSSLC.Tools.Simulation.csproj --no-restore --nologo -v:minimal -clp:NoSummary`：退出码 0，0 warning / 0 error；输出 `Build/bin/NSSLC.Tools.Simulation/Debug/net10.0/NSSLC.Tools.Simulation.dll`。
- `pwsh -NoProfile -File Test/NSSLC.Tools.Simulation.Verification/verify.ps1 -WorldPath Build/diagnostics/WorldGenerationRoundTrip/12345-small-final/generated.wld -SecondWorldPath Build/diagnostics/WorldGenerationRoundTrip/67890-medium-final/generated.wld`：退出码 0，输出 `PASS: non-communication simulation verification`。最新摘要 `Build/diagnostics/NonCommunicationSimulationAudit/verification-run/summary.json` 记录 11 个场景全部通过：0/1/2 玩家各 600 tick；压力板触发与致动器切换各 1 次；3600 tick 战斗、掉落、拾取和保存；液体探针保留 255 单位并保存重载一致；接触伤害导致 4 次死亡/重生；不支持格式拒绝；加载取消；保存失败保留旧文件；两次连续换世界。
- 按 F02 复查执行 `dotnet build Terraria.Dome.sln --no-restore --nologo -v:minimal -m:1 -clp:NoSummary`：退出码 1，0 warning / 43 distinct errors，日志 `Build/diagnostics/NonCommunicationSimulationAudit/root-solution-build-post-remediation.log`。92 个 solution 项目路径问题已消失；错误现分布于 NetworkCodecGenerator（1）、Player.InputControl 验证（6）、PlayerPresentationDerived 验证（3）、WorldGeneration.P20 验证（3）、WorldLoadApiGeneration 验证（28，含 27 个 WLA009）和 P16HousingRegistry 验证（2）。无通信模拟 solution 与生产 Host 均构建通过；这 43 项不是该 Host 的传递依赖。
- 审查 `WorldGen.UpdateWorld()` 后没有把整个 legacy 世界更新入口直接挂入每 tick：该方法还会运行全球地块生长/随机变更、TileEntity/Wiring 和 falling-object 逻辑，并在天气/事件路径直接创建 legacy NPC、Projectile、WorldItem 等实体。这些副作用没有全部经当前 runtime owner/支持清单绑定，整体接线会绕过本任务已建立的状态所有权边界。液体被作为有显式 tile 写回边界的独立阶段接入。

### 仍未闭合的范围

- 根 `Terraria.Dome.sln` 的 92 个项目路径均存在；最新构建仍以 1 个 distinct `CS1061` 失败，位于通信 `NetworkCodecGenerator` 与外部 PacketDesignCompiler 之间。Player InputControl、PlayerPresentationDerived、WorldGeneration.P20、P16 HousingRegistry 和 Simulation Host 在同次构建中均已生成。无通信宿主仍由独立的 `NSSLC.NonCommunicationSimulation.sln` 与生产脚本验证；通信 generator 错误不在本报告非通信运行链路范围。
- `WorldGen.UpdateWorld()` 的全球生态/随机地块更新尚未调度；天气、世界事件只推进支持的数据子集。此入口的 NPC、Projectile、WorldItem 和 TileEntity 全量副作用需要逐项接入 owner 与内容支持清单后再开放。
- NPC AI、Projectile AI、TileEntity 与 Wiring 均只支持 simulation manifest 列出的类型；Town NPC 已恢复并参与 tick，但完整 town AI、其他类型和表现效果仍未实现。
- 压力板/致动器验收用宿主内生成的布线夹具，并已覆盖触发、保存重载后的锚点注册和致动器状态；目前仅验证支持清单中的压力板类型，也未声明支持全部旧 WorldFile 版本。

Goal 保持 active；F01–F13 的核心无通信最小切片已通过当前生产组合验收，剩余世界更新类型、根 solution 的非模拟编译错误与兼容范围继续按原始授权推进，不据本轮验收提前标记完成。

### 复核更新（2026-10-05）

- 修复 WorldLoad API generator 对当前 assembly schema 的选择规则后，`Terraria.WorldLoadApiGenerationVerification` 已构建并运行通过。引用 assembly 的 API 仅由当前 assembly 声明的 schema 选择；当前 assembly 自身声明的 API 仍要求本地 schema。
- P16 HousingRegistry verifier 已改用公开的 `TownHousingRegistrySystem.TryGetRoom` 查询，并构建、运行通过。
- 此前根 solution 诊断位于 `Build/diagnostics/NonCommunicationSimulationAudit/root-solution-build-final.log`：退出码 1、0 warning、14 个 distinct errors。其后相关 Player 与 WorldGeneration verifiers 已独立修复；当前结果见下方 `root-solution-build-current.log`。
- `.learnings/ERRORS.md` 中 ERR-20261005-010 仍为 pending，记录上述根 solution 剩余错误；20 个 ERR 编号均唯一。ERR-20261005-012 另记 `Context/progress.md` 中失效的 Flowstate 文档链接。

### 持续复验（2026-10-05）

- 再运行 `pwsh -NoProfile -File Test/NSSLC.Tools.Simulation.Verification/verify.ps1 -WorldPath Build/diagnostics/WorldGenerationRoundTrip/12345-small-final/generated.wld -SecondWorldPath Build/diagnostics/WorldGenerationRoundTrip/67890-medium-final/generated.wld`：退出码 0，11 个场景全部通过。摘要位于 `Build/diagnostics/NonCommunicationSimulationAudit/verification-run/summary.json`；Host 构建退出码 0、0 warning / 0 error，产物为 `Build/bin/NSSLC.Tools.Simulation/Debug/net10.0/NSSLC.Tools.Simulation.dll`。
- 复核保存重载中的全图液体 checksum 差异：保存文件原始 Tile payload checksum 为 `B9B15FE3D89B8CCA`，重新加载后的活动 owner checksum 为 `59C0068A4F8FF9CE`；WorldFile Tile codec 对保存 payload 解码仍得到前者。差异来自 load recovery 在发布前再次运行液体 settling 与 `WorldGen.WaterCheck`，因为格式不持久化液体 work queue。验收逐格比较的密封探针状态仍完全一致；验证 README 已说明 checksum 只是恢复前后诊断值，不能作为本场景的相等断言。
- 将压力板 smoke 扩展为保存重载：玩家触发 production wiring actuator 后保存 WorldFile，新进程加载并检查锚点注册和 tile 的 actuator/inactive 位。tile area 采样提供通用 `--inspect-tile-area` 参数，旧 `--inspect-liquid-probe` 仍可用作兼容别名。完整脚本仍为 11 个场景，其中压力板场景现包含保存重载断言；最新运行退出码 0，重载后锚点数为 1、致动器状态为 true。
- 清除了 `Build/Tools/NetworkCodecGenerator/Program.cs` 上未完成的临时适配；该改动会使通信生成链路产生额外编译错误，且不属于本报告无通信模拟范围。随后复建当前根 solution：退出码 1、0 warning、1 个 distinct `CS1061`，发生在 NetworkCodecGenerator 使用外部 PacketDesignCompiler 不再提供的 `PacketProtocolDependency.Name`；Player InputControl、PlayerPresentationDerived、WorldGeneration.P20、P16 HousingRegistry verifier 和 Simulation Host 均在该次根 solution 构建中生成成功。诊断日志为 `Build/diagnostics/NonCommunicationSimulationAudit/root-solution-build-current.log`。

### F09 时钟修正与 F10 运行边界验收（2026-10-05）

- `WorldSimulationClockSystem.Advance` 现在先校验时钟时间/月相。`worldTimeRate=0` 时只增加 committed clock revision 并返回，不动时刻、天气、风、冷却和事件倒计时；速率大于 0 时，天气计时器、风变化、世界冷却及已建模事件倒计时按该速率推进。昼夜仍按剩余时间连续穿越边界，黎明只推进一次月相及日单位冷却。
- 新增 `NSSLC.Application.Simulation.Verification` 独立验证项目，覆盖暂停、rate 1、快进、雨/史莱姆雨/沙尘暴到期、风变化、倒计时、月相回绕、白天和夜晚最后一 tick，以及一次快进穿过多个边界。生产宿主另有 rate 0 场景，确认 kernel 仍提交 60 ticks 且世界时间未变化。
- 世界物品 store 把 NPC Gel 掉落和验证夹具共用同一分配路径，并报告部分拾取次数。生产夹具确认过期物品释放 owner slot 和 registry payload；部分拾取将库存补到 Gel 9999，世界物品仍保留 stack 4；满背包时 world item 的 stack 5 完整保留。
- 新增三 tick TileEntity 移除夹具：第 1 tick 更新已登记的逻辑感应器；随后使 owner tile anchor 失效；第 2 tick 从 owner 移除实体并取消 update schedule；第 3 tick 不再访问它。报告的 TileEntity update pass 数为 2，最终实体不存在且未登记。
- `dotnet build src/NSSLC.Tools.Simulation/NSSLC.Tools.Simulation.csproj --no-restore --nologo -v:minimal -clp:NoSummary`：退出码 0，0 warning / 0 error；产物 `Build/bin/NSSLC.Tools.Simulation/Debug/net10.0/NSSLC.Tools.Simulation.dll`，日志 `Build/diagnostics/NonCommunicationSimulationAudit/simulation-host-build-current.log`。
- `dotnet build Test/NSSLC.Application.Simulation.Verification/NSSLC.Application.Simulation.Verification.csproj --no-restore --nologo -v:minimal -clp:NoSummary`：退出码 0，0 warning / 0 error；产物 `Build/bin/NSSLC.Application.Simulation.Verification/Debug/net10.0/NSSLC.Application.Simulation.Verification.dll`，随后 verifier 退出码 0，输出 `PASS: world simulation clock verification`。脚本内 clock verifier build 同为退出码 0，日志 `Build/diagnostics/NonCommunicationSimulationAudit/verification-run/clock-verification-build.log`。
- 最新生产脚本命令仍为上文 Small/Medium 两个 WorldFile 319 输入：退出码 0，摘要记录 17 项全部通过，包含 0/1/2 玩家 600 tick、暂停时钟、四项 F10 边界、压力板保存重载、3600 tick 战斗/掉落/拾取/保存、死亡重生、保存重载、拒绝/取消/失败保存、重复切换。摘要 `Build/diagnostics/NonCommunicationSimulationAudit/verification-run/summary.json`；运行日志 `Build/diagnostics/NonCommunicationSimulationAudit/verification-run/clock-verification.log`。

Goal 仍保持 active。此批补上 F09 速率语义与 F10 指定的边界验收；完整天气/事件、全内容 AI、所有 TileEntity 类型和无通信范围外的根 solution 通信生成器依赖仍不据此宣称完成。

### F06–F08 生产行为断言增补（2026-10-05）

- 玩家报告现在包含初始位置、跳跃次数和着地次数；验证脚本增加 90 tick 输入场景，直接断言横向移动、执行一次跳跃、至少两次地面接触并回到原地面高度。Small 世界实测初始 `(33510, 3670)`、结束 `(33515.594, 3670)`、跳跃 1 次、着地 2 次。
- 3600 tick 战斗场景现在显式断言生产自然生成 pass 创建了 NPC、普通箭实际命中 NPC 并发生 tile collision，以及带输入的玩家位置发生移动。当前固定种子报告为自然生成 NetId 1、发射 99 箭、NPC 命中 45 次、tile collision 54 次。
- 这轮新增断言后重新运行同一 Small/Medium WorldFile 319 生产验证脚本：退出码 0，摘要记录 18 项全部通过。Host build 和 clock verifier build 均为 0 warning / 0 error；产物分别位于 `Build/bin/NSSLC.Tools.Simulation/Debug/net10.0/NSSLC.Tools.Simulation.dll` 与 `Build/bin/NSSLC.Application.Simulation.Verification/Debug/net10.0/NSSLC.Application.Simulation.Verification.dll`。证据摘要仍为 `Build/diagnostics/NonCommunicationSimulationAudit/verification-run/summary.json`。

### F07 NPC 内容映射与飞行 AI 扩展（2026-10-05）

- 校正模拟内容目录的 NPC 网络 ID：1 为 Blue Slime，2 为 Demon Eye，3 为 Zombie，16 为 Green Slime。此前把 ID 1/2 当成 Green/Blue Slime，会让加载、生成与 AI 行为对不上实际实体身份。
- 内容清单升至 `simulation-core-v3`，包含 Demon Eye 的 AI style 2 定义；自然天空生成选择 Demon Eye，白天地面与史莱姆雨选择 Green Slime。旧名解析器可恢复存档中的 Blue Slime、Demon Eye 和 Green Slime 记录。
- 在 runtime NPC owner 中加入有状态的 Demon Eye 行为：以最近存活玩家为目标，在悬停接近与俯冲阶段间切换；阶段计时、状态和悬停侧向保存在该实体的 AI 槽中。AI style 2 的重力按飞行 NPC 处理；无目标时速度衰减。
- 更新模拟 README 的支持类型与有限行为说明。
- 初次验证发现新增 NPC 漏配 persistent identity，且 Demon Eye 误用地面 TileCollision 后 160 tick 只移动约 12 像素；分别补齐身份目录和 style 2 飞行位移边界后复验通过。旧分场景 JSON 可能残留上一轮结果，验证入口现会先删除目标报告与 switch 报告。
- 复验命令 `pwsh -NoProfile -File Test/NSSLC.Tools.Simulation.Verification/verify.ps1 -WorldPath Build/diagnostics/WorldGenerationRoundTrip/12345-small-final/generated.wld -SecondWorldPath Build/diagnostics/WorldGenerationRoundTrip/67890-medium-final/generated.wld` 退出码 0；摘要 `Build/diagnostics/NonCommunicationSimulationAudit/verification-run/summary.json` 记录 19 项通过。Host 与时钟 verifier build 均为 0 warning / 0 error；Demon Eye 固定 160 tick 场景完成悬停/俯冲周期，末态 Action=0，位置移动约 72 像素。
- F07 仍未完整闭合：其他 NPC AI、town AI、寻路/攻击和全类型 spawn/despawn 规则仍是后续工作。

### F07 Guide 有限 Town NPC 行为（2026-10-05）

- Guide 现在按 owner tick 执行确定性白天巡逻周期：向右行走、停顿、向左行走、停顿；夜间朝已分配房屋的水平中心移动，未分配房屋时停止。地面碰撞受阻时会跳跃。Old Man 与训练假人仍保持静止。
- 生产报告新增每个 NPC 的 slot、初始位置/动作和最终位置/动作，以 slot 匹配同一实体；验证脚本增加固定玩家与中性输入的 Guide 巡逻场景。
- 完整脚本现记录 20 项通过；Guide 场景运行 200 ticks，末态巡逻动作 -1，位置移动 110.35 像素。恶魔之眼场景仍通过，160 ticks 位移 72.54 像素并结束在悬停阶段。
- `dotnet build NSSLC.NonCommunicationSimulation.sln --no-restore --nologo -v:minimal -clp:NoSummary`：退出码 0，0 warning / 0 error。完整生产验收脚本同样退出码 0，宿主与时钟 verifier 构建均为 0 warning / 0 error。
- 根 `Terraria.Dome.sln` 当前验证日志 `Build/diagnostics/NonCommunicationSimulationAudit/root-solution-build-current.log` 仍显示 1 个 distinct `CS1061`：本仓库 `NetworkCodecGenerator` 使用外部 PacketDesignCompiler 已移除的 `PacketProtocolDependency.Name`。外部 NetWork 工作树保持只读；直接替换为 model member 名会破坏 profile facts 注入与全局 `ProtocolInputs` 运行契约，因此该通信集成错误留待单独设计。
- 夜间 Guide 返回住所逻辑已编译，但本次生产场景只验证了白天巡逻；完整 town schedule、垂直寻路和其他 town NPC AI 仍未实现。

### NPC 生命周期与运行取消验收（2026-10-05）

- Simulation Host 新增 `--npc-slot-probe true`：填充 NPC owner 的 200 个槽位，断言第 201 个分配被拒绝；释放第一个探针 NPC 后再次分配到相同 slot，generation 从 1 增至 2，旧 handle 不能释放新实体；探针退出前清理临时实体并恢复原始 active count。
- 新增 `--npc-despawn-probe true`：以零玩家、300 ticks 运行一个自然生成敌怪，生产 NPC phase 到达离场宽限后释放其 runtime slot。参数限制为 `--players 0` 和至少 300 ticks。
- 新增 `--cancel-after-ticks <ticks>`：在指定已提交 tick 之后确定性停止；保存路径仍捕获最后已提交 owner snapshot，不能与毫秒取消选项合用。
- 验收脚本新增三个场景：NPC 容量/复用、自然敌怪 300 tick despawn、运行取消保存及 fresh reload。Small World 实测 slot 2 generation 1 → 2，despawn count 1；tick 120 取消时 snapshot revision 120，保存成功，reload 时间与保存时间均为 13620。
- 同一生产命令现覆盖 0/1/2 玩家各 600 ticks 与各 3600 ticks；3600 tick 场景逐项核对 phase、clock revision、最终 NPC update 和源存档未变。
- 最终命令 `pwsh -NoProfile -File Test/NSSLC.Tools.Simulation.Verification/verify.ps1 -WorldPath Build/diagnostics/WorldGenerationRoundTrip/12345-small-final/generated.wld -SecondWorldPath Build/diagnostics/WorldGenerationRoundTrip/67890-medium-final/generated.wld`：退出码 0，输出 `PASS: non-communication simulation verification`，25 项证据通过。摘要为 `Build/diagnostics/NonCommunicationSimulationAudit/verification-run/summary.json`；Host 与 clock verifier build 均为 0 warning / 0 error，clock verifier exit 0；Host 产物位于 `Build/bin/NSSLC.Tools.Simulation/Debug/net10.0/NSSLC.Tools.Simulation.dll`。
- F07 的其他 NPC 行为、其余类型 despawn 规则、完整天气/事件和 TileEntity/Wiring 兼容仍按支持边界待处理；根 solution 的通信 NetworkCodecGenerator 外部合同错误依旧不属于无通信宿主依赖图。goal 保持 active。

### F08 寿命终止证据修正（2026-10-05）

- 首次增加的 1200 tick 宿主探针把“handle 已失效”误作“寿命到期”；实际报告 `ProjectileTileCollisionCount=1`。普通箭从 y=32 静止生成后受重力，在 1200 tick 前先撞到地形，旧断言因此失败。
- 移除不能隔离终止原因的宿主探针及其 CLI/README 描述。`ProjectileTickResult` 现在报告本 tick 由协调器寿命尾段释放的数量；`ProjectileTickCoordinatorVerification` 对一 tick 生命周期断言 `LifetimeExpiredCount=1`、槽位释放，并核对过期后剩余额外更新停止。
- 定向构建 `dotnet build Test/Terraria.ProjectileCombatVerification/Terraria.ProjectileCombatVerification.csproj --no-restore --nologo -v:minimal -clp:NoSummary`：退出码 0，0 warning / 0 error；运行 `dotnet Build/bin/Terraria.ProjectileCombatVerification/Debug/net10.0/Terraria.ProjectileCombatVerification.dll --tick-coordinator` 退出码 0，输出 `PASS: projectile ordered tick coordinator and extra-update invariants`。
- 生产验收 `pwsh -NoProfile -File Test/NSSLC.Tools.Simulation.Verification/verify.ps1 -WorldPath Build/diagnostics/WorldGenerationRoundTrip/12345-small-final/generated.wld -SecondWorldPath Build/diagnostics/WorldGenerationRoundTrip/67890-medium-final/generated.wld` 退出码 0，摘要记录 25 项通过。Host 和 clock verifier 均构建为 0 warning / 0 error；真实箭矢生产场景继续断言 NPC 命中与地形碰撞。
- 1200 tick 无碰撞寿命到期不再被声称为生产验证；目标证据由组件协调器终止分支和真实宿主战斗/碰撞组合分别覆盖。F08 的其他 projectile AI 类型仍受支持清单限制。

### F10 世界掉落物物理验收补充（2026-10-05）

- `--world-item-probe physics` 现在只在零玩家、至少 300 ticks 时启用；测试物品生成在实际世界 spawn 点上方 160 像素，继续使用生产 `RuntimeWorldItemStore`、重力更新与加载世界 tile collision。
- Small World 319 的 300 tick 实测：物品从 y=3552 落到 y=3696，最终垂直速度 0，timeLeft 从 6000 递减至 5700，物品仍保留在 world-item owner 中。
- 宿主定向构建退出码 0、23 warnings / 0 errors（告警来自既有 WorldStorage/WorldGeneration 兼容代码）；完整生产脚本退出码 0，摘要现为 28 项证据，包含新增重力/碰撞/寿命场景。脚本内 Host 与 clock verifier 增量构建均 0 warnings / 0 errors。证据文件：`Build/diagnostics/NonCommunicationSimulationAudit/verification-run/summary.json` 与 `world-item-physics-probe.json`。
- 首次将探针放到世界顶部时，300 ticks 后物品仍以速度 10 下落；改为以存档实际 spawn tile 定位后通过。该修正只改变验收夹具，不改变物品物理规则。

### B1 / B4 生产输入与容器存档闭环（2026-10-05）

- `WorldContainerStore.TrySetChestItem` 仅允许在已登记锚点和有效 slot 内提交 item；拒绝越界/负值状态，将空物品规范化为 default，并在实际内容变更后递增 mutation revision。
- Host 新增 `--chest-item-probe set|inspect`。`set` 必须带 `--save`，将第一只 loaded chest 的 slot 0 改为 Gel ×7（若原值相同则写 ×8）；`inspect` 只读该 slot。Small World 实测保存后由全新进程重载，anchor `(1130,1119)`、slot 0、type 23、stack 7 一致。
- 验证脚本新增 60 tick 双玩家脚本输入：slot 0 从 x=33510 到 33635.234，slot 1 从 x=33534 到 33659.234；单玩家走停/跳跃/着陆、普通箭命中和其他原有场景仍通过。
- 最终完整命令 `pwsh -NoProfile -File Test/NSSLC.Tools.Simulation.Verification/verify.ps1 -WorldPath Build/diagnostics/WorldGenerationRoundTrip/12345-small-final/generated.wld -SecondWorldPath Build/diagnostics/WorldGenerationRoundTrip/67890-medium-final/generated.wld` 退出码 0，摘要为 28 项通过。Host 与 clock verifier 增量构建 0 warnings / 0 errors。

### F07 / B6 受支持 NPC despawn 策略闭环（2026-10-05）

- `SimulationContentSupportManifest` 为全部 7 种受支持 NPC 声明 natural despawn policy，并在 catalog validation 中核对 policy 集与 NPC 支持集完全一致。受支持范围以外的 NPC 仍在 catalog/hydration 边界拒绝。
- Blue Slime (1)、Demon Eye (2)、Zombie (3)、Green Slime (16) 使用统一的 3,000 像素、300 tick living-player-distance 策略；Guide (22)、Old Man (37)、Target Dummy (488) 使用 Persistent。未知 policy 不再隐式走默认 despawn 分支。
- `--npc-despawn-probe-net-id <id>` 让生产 Host 对每种支持类型分别报告 policy、slot 是否释放和总 despawn 数。Small World 300 tick、0 玩家验收中四种敌怪各释放 1 个 slot，三种 persistent NPC 均保留；旧 `INpcSpawnPassPort` 的 Unknown 兼容语义不变。

### B8 / F10 / F12 owner 与 wiring 边界补充（2026-10-05）

- Host 调度的 `Liquid.UpdateLiquid` 每 tick 使用 tile mutation observer 收集 legacy runtime 改动，只对变化坐标调用 `CommitLegacyTileMutations` 回写 owner，并检查 owner 与 legacy collision view 一致。Small World 密封液体探针 60 tick 记录 64 次 tile mutation、60 次液体状态变化；保存后由 fresh process reload 并逐格核对探针状态。3600 tick gameplay 场景另记录 7 次 tile mutation、2 次液体状态变化并成功保存。
- 当前 wiring 行为保持明确有限：pressure plate 135/428 和 logic sensor 通过已连接线色触发 actuator；其他 TileEntity kind 与未支持 logic check 在首 tick 前拒绝。运行报告 `RecognizedUnsupportedWiredDeviceTileTypes` 列出触发网络中已识别但没有 handler 的 device 类型。压力板验收夹具包含 Timer tile 144，实测报告 `{144}`，同时 pressure activation 和 actuator toggle 各 1 次，保存后的新进程仍恢复锚点和 actuated 状态。完整 Timer/door/pump/teleporter/logic-gate 等行为未声称实现。
- Host 未调用的通用 WorldGen mutation 算法仍不会被当作 runtime 更新；如果后续接入，必须沿 owner commit/projection 边界补充对应的碰撞与存档验收。

### F01–F13 最终验收矩阵（2026-10-05）

| Finding | 本轮处置与证据 | 明确边界 |
|---|---|---|
| F01 | 显式区分 NPC runtime slot 与 WorldStorage slot；Projectile/Application/Host 依赖图构建通过。 | 不允许调用方把两类 slot 当成同一身份。 |
| F02 | 根 solution 已清除报告指出的旧项目路径，当前登记路径可解析；非通信模拟 solution 和 Host 项目图构建通过。 | 完整根 solution 仍有通信 `NetworkCodecGenerator` 对外部 PacketDesignCompiler 合同 `PacketProtocolDependency.Name` 的 CS1061，详见前文；该项目不在无通信 Host 依赖图。 |
| F03 | 生产 Host、fixed-step kernel、phase order、有限运行和取消/保存路径由 34 项脚本实跑。 | 入口只提供离线有限 tick run，不包含渲染/UI。 |
| F04 | 真实 `.wld` load/recovery/publication 后 hydration 玩家及支持范围 NPC；重复 world switch 重新建 owner，失败/取消不开始 tick。 | 只恢复 manifest 支持的实体定义。 |
| F05 | Host 启动时构建并验证有限 ContentCatalog；重复、缺失、未知类型与 AI style 不匹配均拒绝。 | 不反射或默认接受完整游戏内容目录。 |
| F06 | 脚本输入驱动玩家 traversal、跳跃/地形碰撞、木弓/箭、NPC 命中、接触伤害、死亡与重生；单/双玩家输入均有生产场景。 | 坐骑、全物品行为与未列入支持表的角色能力未覆盖。 |
| F07 | 四种敌怪基础 AI 与三种 town/dummy 有限行为接入；7 种支持类型均由 300 tick 生产 probe 核对 despawn policy。 | 完整 AI、寻路、town schedule 和支持集外 NPC 明确不支持。 |
| F08 | 普通箭 production adapter 覆盖移动、NPC 命中、地形碰撞；Projectile coordinator verifier 断言寿命终止、slot 释放及额外更新停止。 | 仅 projectile type 1；其他 AI style、minion/sentry 和 owner-dependent 行为不支持。 |
| F09 | clock verifier 覆盖 pause/rate/day-night；Host 推进已建模天气与事件计时，并保存 reload。 | event wave、boss/event gameplay effects、完整季节及 invasion simulation 未实现。 |
| F10 | Gel 生产掉落物重力/碰撞/寿命/拾取通过；TileEntity type 0/2 和有限 pressure plate/actuator wiring 通过。 | 其他 TileEntity/logic check 在启动时拒绝；其他 wired device 有限识别并在报告列出，不模拟其效果。 |
| F11 | owner snapshot source 捕获运行期 tile/container/clock/NPC/TileEntity 等状态；宝箱、压力板、液体与世界时间均有 save → fresh reload 检查。 | 未由当前 owner 表示的只读/未支持 WorldFile sections 保留原内容；不推导为所有 section 可变。 |
| F12 | legacy liquid mutation observer 对实际调度算法作坐标级 owner commit；collision projection 对照和 fresh reload 探针通过。 | 未调度的 WorldGen 算法没有被伪装成已同步；未来接入需逐类补证。 |
| F13 | `verify.ps1` 在 Small/Medium WorldFile 319 上输出 PASS，摘要 34 项；Host/clock verifier 构建均 0 warnings / 0 errors，clock verifier 运行退出码 0。 | 该离线矩阵不代表完整 Terraria 玩法兼容或发布签收。 |

- 最终 Host 命令：`dotnet build src/NSSLC.Tools.Simulation/NSSLC.Tools.Simulation.csproj --no-restore --nologo -v:minimal -clp:NoSummary`，退出码 0、0 warnings / 0 errors；产物 `Build/bin/NSSLC.Tools.Simulation/Debug/net10.0/NSSLC.Tools.Simulation.dll`。
- 最终生产命令：`pwsh -NoProfile -File Test/NSSLC.Tools.Simulation.Verification/verify.ps1 -WorldPath Build/diagnostics/WorldGenerationRoundTrip/12345-small-final/generated.wld -SecondWorldPath Build/diagnostics/WorldGenerationRoundTrip/67890-medium-final/generated.wld`，退出码 0，输出 `PASS: non-communication simulation verification`。摘要 `Build/diagnostics/NonCommunicationSimulationAudit/verification-run/summary.json` 的 `Succeeded=true`、`Evidence.Count=34`、Host build 0/0、clock verifier build 0/0、clock run 0。
- 压力板 Timer 诊断报告 `Build/diagnostics/NonCommunicationSimulationAudit/verification-run/pressure-plate-probe-60.json`；NPC 七类策略报告为 `npc-natural-despawn-<net-id>-300.json`；液体 owner 保存重载证据为 `liquid-probe-smoke.json` / `liquid-probe-reload.json`。
- 本 goal 的 P0–P4 与 F01–F13 最小无通信模拟范围完成；上表中列出的 full-gameplay 能力仍为明确的后续兼容工作，不据此宣称完整 Terraria parity。

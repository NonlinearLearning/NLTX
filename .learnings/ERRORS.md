# Errors

## [ERR-20260906-008] serial_dotnet_p_argument_forwarding_world_interaction_components

**Logged**: 2026-09-06T00:00:00+08:00
**Priority**: low
**Status**: pending
**Area**: tests

### Summary
WorldInteraction 组件验证的首次串行 dotnet 调用因 PowerShell 未保护 `-p:` 参数而未启动 dotnet。

### Error
```text
Invoke-SerialDotnet.ps1: Parameter cannot be processed because the parameter name 'p' is ambiguous.
```

### Context
- 操作：执行 WorldInteraction 组件红阶段构建。
- 原因：直接把 `-p:UseSharedCompilation=false` 等参数写在 PowerShell 包装脚本调用中。
- 结果：包装脚本在参数绑定阶段退出，尚未产生编译结果。

### Suggested Fix
使用 PowerShell 数组保存完整 dotnet 参数，再以数组展开方式传给 `Invoke-SerialDotnet.ps1`。

### Metadata
- Reproducible: yes
- Related Files: Build/Tools/Invoke-SerialDotnet.ps1; Test/Terraria.WorldInteraction.Components.Verification/Terraria.WorldInteraction.Components.Verification.csproj
- See Also: ERR-20260906-003, ERR-20260906-004, ERR-20260906-006, ERR-20260906-007

---

## [ERR-20260906-010] invoke_serial_dotnet_parameter_binding

**Logged**: 2026-09-06T11:28:57+08:00
**Priority**: low
**Status**: resolved
**Area**: infra

### Summary
直接以脚本位置参数调用 `Invoke-SerialDotnet.ps1` 时，PowerShell 将 `-p:` MSBuild 属性误解析为 wrapper 的缩写参数。

### Error
```text
Invoke-SerialDotnet.ps1: Parameter cannot be processed because the parameter name 'p' is ambiguous. Possible matches include: -ProgressAction -PipelineVariable.
```

### Context
- 操作：通过仓库 wrapper 编译 `src/WorldSession/Terraria.WorldSession.csproj`。
- 原因：`[CmdletBinding()]` 脚本的 `ValueFromRemainingArguments` 参数仍会参与 PowerShell 参数绑定。
- 结果：命令在 dotnet 启动前退出，未产生编译结果。

### Suggested Fix
在当前 PowerShell 会话中先组装完整字符串数组，再使用 `-DotnetArguments $dotnetArguments` 调用 wrapper；不要直接把 `-p:*` 作为 wrapper 的位置参数传入。

### Resolution
- **Resolved**: 2026-09-06T11:28:57+08:00
- **Notes**: 使用 `-DotnetArguments` 数组后 WorldSession 串行构建成功，最终 0 warning、0 error。

### Metadata
- Reproducible: yes
- Related Files: Build/Tools/Invoke-SerialDotnet.ps1; docs/plans/2026-09-06-world-generation-ecology-code-components-implementation.md

---

## [ERR-20260906-007] serial_dotnet_m_argument_forwarding_spatial_components

**Logged**: 2026-09-06T11:15:00+08:00
**Priority**: low
**Status**: pending
**Area**: tests

### Summary
串行 dotnet 包装器的参数终止方式已使 -p: 参数正确转发，但未加引号的 -m:1 被 PowerShell 拆成了 -m: 和 1。

### Error
工具返回：MSBUILD error MSB1031，命令行中出现 -m: 1，最大 CPU 数参数无效。

### Context
- 操作：构建 src/SpatialSimulation/Terraria.SpatialSimulation.csproj。
- 原因：直接调用脚本时将 -m:1 作为未引用的参数传入。
- 结果：dotnet 已启动，但在 MSBuild 参数解析阶段退出，尚未编译源码。

### Suggested Fix
直接调用包装器时将每个带冒号的 dotnet 参数作为带引号的字符串传入，并继续使用参数终止方式。

### Metadata
- Reproducible: yes
- Related Files: Build/Tools/Invoke-SerialDotnet.ps1; src/SpatialSimulation/Terraria.SpatialSimulation.csproj
- See Also: ERR-20260906-006

---

## [ERR-20260906-007] serial_dotnet_parameter_binding_world_progression

**Logged**: 2026-09-06T10:00:00+08:00
**Priority**: low
**Status**: resolved
**Area**: tests

### Summary
第一次运行 WorldProgression focused verifier 时，PowerShell 将裸传的 `-p:` 属性参数解析为包装脚本参数。

### Error
```text
Invoke-SerialDotnet.ps1: Parameter cannot be processed because the parameter name 'p' is ambiguous.
```

### Context
- 操作：通过 `Build/Tools/Invoke-SerialDotnet.ps1` 启动新增 verifier。
- 原因：直接在 PowerShell 命令行上传递 `-p:UseSharedCompilation=false` 等参数。
- 结果：包装脚本退出码为 1，dotnet 未启动。

### Resolution
- 使用 `-DotnetArguments` 数组将完整 dotnet 参数转发给包装脚本。

### Metadata
- Reproducible: yes
- Related Files: Build/Tools/Invoke-SerialDotnet.ps1

---

## [ERR-20260906-008] dotnet_run_project_argument_world_progression

**Logged**: 2026-09-06T10:01:00+08:00
**Priority**: low
**Status**: resolved
**Area**: tests

### Summary
第二次运行 WorldProgression focused verifier 时，`dotnet run` 的位置项目参数未被识别。

### Error
```text
找不到要运行的项目。请确保 D:\TRbackup\NLTX 中存在项目，或使用 --project 传递项目路径。
```

### Context
- 操作：使用参数数组调用新增 verifier。
- 原因：使用 `run <project-path>` 形式经包装脚本转发后未识别目标项目。
- 结果：dotnet 启动但没有进入目标项目编译。

### Resolution
- 改用 `run --project <project-path>`，随后 RED 和 GREEN 验证均按预期执行。

### Metadata
- Reproducible: yes
- Related Files: Build/Tools/Invoke-SerialDotnet.ps1

---

## [ERR-20260906-007] cleanup_generated_test_output_policy_rejection

**Logged**: 2026-09-06T10:05:00+08:00
**Priority**: low
**Status**: pending
**Area**: tests

### Summary
清理本轮已删除测试项目的 Build/bin 与 Build/obj 输出时，本地 exec 安全策略拒绝了递归删除命令。

### Error
```text
exec_command failed: ... rejected by policy
```

### Context
- 操作：删除明确路径 Build/bin/Terraria.Teleportation.Components.Verification 和 Build/obj/Terraria.Teleportation.Components.Verification。
- 删除前已确认目标是本轮临时测试项目生成目录，且目标目录名称精确匹配。
- 测试项目源文件和项目文件已经通过 apply_patch 删除；未使用更宽泛的清理命令。

### Suggested Fix
保留生成输出在仓库约定的 Build 目录，或后续使用仓库允许的受控清理入口；不要为清理临时产物扩大删除范围。

### Metadata
- Reproducible: unknown
- Related Files: Build/bin/Terraria.Teleportation.Components.Verification; Build/obj/Terraria.Teleportation.Components.Verification

---

## [ERR-20260906-007] serial_dotnet_wrapper_parameter_binding_world_session

**Logged**: 2026-09-06T10:00:00+08:00
**Priority**: low
**Status**: pending
**Area**: config

### Summary
WorldSession 生产项目的首次串行构建调用在 PowerShell 参数绑定阶段失败，未启动 dotnet。

### Error
```text
Invoke-SerialDotnet.ps1: Parameter cannot be processed because the parameter name 'p' is ambiguous. Possible matches include: -ProgressAction -PipelineVariable.
```

### Context
- 操作：通过 Build/Tools/Invoke-SerialDotnet.ps1 构建 src/WorldSession/Terraria.WorldSession.csproj。
- 原因：将 -p:UseSharedCompilation=false 等 MSBuild 参数直接传给带 CmdletBinding 的 PowerShell 包装脚本。
- 结果：包装脚本退出码为 1；dotnet 未启动，没有编译结果。

### Suggested Fix
将 dotnet 参数先放入 string[]，再通过数组展开传入包装脚本，或使用 PowerShell 的参数转义方式，避免 -p: 被包装脚本自身解析。

### Metadata
- Reproducible: yes
- Related Files: Build/Tools/Invoke-SerialDotnet.ps1; src/WorldSession/Terraria.WorldSession.csproj

---

## [ERR-20260906-008] serial_dotnet_project_argument_forwarding

**Logged**: 2026-09-06T10:04:00+08:00
**Priority**: low
**Status**: pending
**Area**: tests

### Summary
串行包装脚本转发 `dotnet test --project` 时，当前 SDK 将 `--project` 传给 MSBuild 并报告未知开关。

### Error
```text
MSBUILD : error MSB1001: 未知开关。
开关:--project
```

### Context
- 操作：运行 `Test/Terraria.Npc.Components.Verification` 的 RED 验证。
- 原因：当前 `dotnet test` 版本/包装参数组合不接受该位置的 `--project`，导致项目路径没有按预期被识别。
- 结果：dotnet 启动但在项目解析阶段退出，未进入源代码编译；随后用户明确要求本次不写测试。

### Suggested Fix
本次按用户范围不再运行测试；若未来需要运行验证，应先确认当前 SDK 支持的项目参数形状，并保留串行包装器参数数组传递方式。

### Metadata
- Reproducible: yes
- Related Files: Build/Tools/Invoke-SerialDotnet.ps1; Test/Terraria.Npc.Components.Verification/Terraria.Npc.Components.Verification.csproj

---

## [ERR-20260906-006] serial_run_project_argument

**Logged**: 2026-09-06T00:00:00+08:00
**Priority**: low
**Status**: pending
**Area**: tests

### Summary
通过串行 wrapper 执行 focused verification 时，dotnet run 的位置项目参数未被识别。

### Error
~~~text
找不到要运行的项目。请确保 D:\\TRbackup\\NLTX 中存在项目，或使用 --project 传递项目路径。
~~~

### Context
- 操作：TDD RED 阶段运行 Test/Terraria.Projectile.Components.Verification。
- 命令形状：Invoke-SerialDotnet.ps1 run <project-path>，exit code 1。
- 结果：dotnet 未进入项目编译，不能作为“生产类型缺失”的有效 RED 证据。

### Suggested Fix
通过 DotnetArguments 数组显式传入 run、--project、项目路径，并保留仓库串行构建参数。

### Metadata
- Reproducible: yes
- Related Files: Build/Tools/Invoke-SerialDotnet.ps1; Test/Terraria.Projectile.Components.Verification/

---

## [ERR-20260906-007] serial_dotnet_parameter_forwarding_npc_components

**Logged**: 2026-09-06T10:00:00+08:00
**Priority**: low
**Status**: pending
**Area**: tests

### Summary
NPC 组件 RED 验证首次调用串行 dotnet 包装脚本时，PowerShell 将 `-p:` 项目属性误解析为包装脚本参数。

### Error
```text
Invoke-SerialDotnet.ps1: Parameter cannot be processed because the parameter name 'p' is ambiguous.
```

### Context
- 操作：运行 `Test/Terraria.Npc.Components.Verification` 的 RED 验证。
- 原因：通过 PowerShell 调用 `pwsh -File` 时裸传 `-p:UseSharedCompilation=false` 等参数，参数绑定抢先解析了 `-p`。
- 结果：包装脚本退出码为 1；dotnet 未启动，没有生成编译或测试结果。

### Suggested Fix
将 dotnet 参数放入 PowerShell 数组后展开，或对 `-p:` 参数进行字面量保护；继续通过仓库串行包装脚本执行。

### Metadata
- Reproducible: yes
- Related Files: Build/Tools/Invoke-SerialDotnet.ps1; Test/Terraria.Npc.Components.Verification/Terraria.Npc.Components.Verification.csproj

---

## [ERR-20260906-001] markdown_patch_string_escaping

**Logged**: 2026-09-06T01:30:00+08:00
**Priority**: medium
**Status**: pending
**Area**: docs

### Summary
通过 JavaScript 模板字符串构造包含 Markdown 内联反引号的 apply_patch 输入时，字符串被提前终止。

### Error
工具返回：SyntaxError: Unexpected identifier 'status'

### Context
- 操作：创建 SpatialSimulation Component Code Draft Markdown。
- 原因：补丁正文使用 String.raw 模板字符串，但正文内包含未转义的 Markdown 反引号。
- 结果：apply_patch 未执行，目标文档没有产生修改。

### Suggested Fix
构造长 Markdown 补丁时使用不包含反引号的正文写法，或使用不会解析模板字面量的输入构造方式；失败后先确认目标文件和工作区没有产生部分写入。

### Metadata
- Reproducible: yes
- Related Files: docs/design/2026-09-05-version4-spatial-simulation-component-code-draft.md

---

## [ERR-20260906-005] truncated_test_path_discovery

**Logged**: 2026-09-06T00:00:00+08:00
**Priority**: low
**Status**: pending
**Area**: tests

### Summary
根据截断的测试目录输出继续拼接不存在的 Content/Physics verification 文件路径。

### Error
~~~text
Get-Content: Cannot find path 'D:\\TRbackup\\NLTX\\Test\\Terraria.Content.Catalog.Verification\\Terraria.Content.Catalog.Verification.csproj'
~~~

### Context
- 操作：查找根 src 组件的 focused verification 入口。
- 原因：把目录名当作同名项目文件路径，且未先确认目录是否为空。
- 结果：读取操作返回非零；没有产生写入、构建或测试进程。

### Suggested Fix
先用完整的 Get-ChildItem 和 rg --files 确认目录内容，再选择真实存在的验证项目或新建最小入口。

### Metadata
- Reproducible: no
- Related Files: Test/

---

## [ERR-20260906-007] serial_dotnet_p_argument_forwarding_death_penalty_components

**Logged**: 2026-09-06T02:00:00+08:00
**Priority**: low
**Status**: pending
**Area**: tests

### Summary
首次运行 DeathPenaltyAndRevenge 组件 RED 验证时，PowerShell 将裸传的 `-p:` 参数误解析为串行包装脚本参数。

### Error
```text
Invoke-SerialDotnet.ps1: Parameter cannot be processed because the parameter name 'p' is ambiguous.
```

### Context
- 操作：通过 `Build/Tools/Invoke-SerialDotnet.ps1` 构建新建的组件验证项目。
- 原因：`-p:UseSharedCompilation=false` 等 MSBuild 参数未在 PowerShell 调用处显式保护。
- 结果：包装脚本在启动 dotnet 前退出，未产生编译结果。

### Suggested Fix
通过 PowerShell 数组或加引号的参数把 `-m:`、`-nr:` 和 `-p:` 选项明确转发给串行包装脚本。

### Metadata
- Reproducible: yes
- Related Files: Build/Tools/Invoke-SerialDotnet.ps1; Test/Terraria.DeathPenaltyAndRevenge.Components.Verification/Terraria.DeathPenaltyAndRevenge.Components.Verification.csproj
- See Also: ERR-20260906-005, ERR-20260906-006

---

## [ERR-20260906-006] serial_dotnet_p_argument_forwarding_spatial_components

**Logged**: 2026-09-06T10:55:00+08:00
**Priority**: low
**Status**: pending
**Area**: tests

### Summary
SpatialSimulation 组件 TDD RED 调用中，PowerShell 将未保护的 -p: 参数解析为包装脚本的缩写参数，导致 dotnet 没有启动。

### Error
工具返回：Invoke-SerialDotnet.ps1: Parameter cannot be processed because the parameter name 'p' is ambiguous.

### Context
- 操作：构建 Test/Terraria.SpatialSimulation.Components.Verification 项目以执行 RED 验证。
- 原因：直接在 PowerShell 调用串行包装脚本时传递了未引用的 -p:UseSharedCompilation=false 等参数。
- 结果：包装脚本在参数绑定阶段退出，尚未产生编译结果。

### Suggested Fix
使用 PowerShell 数组保存完整 dotnet 参数，再通过数组展开传给 Invoke-SerialDotnet.ps1；重新检查返回的退出码和 dotnet 输出。

### Metadata
- Reproducible: yes
- Related Files: Build/Tools/Invoke-SerialDotnet.ps1; Test/Terraria.SpatialSimulation.Components.Verification/Terraria.SpatialSimulation.Components.Verification.csproj
- See Also: ERR-20260906-003

---

## [ERR-20260906-006] serial_dotnet_p_argument_forwarding_teleportation_components

**Logged**: 2026-09-06T09:50:00+08:00
**Priority**: low
**Status**: pending
**Area**: tests

### Summary
Teleportation 组件验证的首次串行 dotnet 调用因 PowerShell 未保护 `-p:` 参数而未启动 dotnet。

### Error
```text
Invoke-SerialDotnet.ps1: Parameter cannot be processed because the parameter name 'p' is ambiguous.
```

### Context
- 操作：运行 `Test/Terraria.Teleportation.Components.Verification` 的 TDD RED 验证。
- 原因：直接把 `-p:UseSharedCompilation=false` 等参数写在 PowerShell 命令调用中，参数绑定抢先解析了 `-p`。
- 结果：串行包装脚本退出码为 1；dotnet 未启动，没有生成编译或测试结果。

### Suggested Fix
使用完整 `DotnetArguments` 数组或将每个 `-p:` 参数显式作为字符串传入串行包装器；不得把 `-p:` 参数裸传给 PowerShell 脚本。

### Metadata
- Reproducible: yes
- Related Files: Build/Tools/Invoke-SerialDotnet.ps1; Test/Terraria.Teleportation.Components.Verification/Terraria.Teleportation.Components.Verification.csproj
- See Also: ERR-20260906-003, ERR-20260906-004, ERR-20260906-005

---

## [ERR-20260906-004] test_path_assumption

**Logged**: 2026-09-06T00:00:00+08:00
**Priority**: low
**Status**: pending
**Area**: tests

### Summary
根据目录名拼接测试项目路径时，第一次读取了不存在的 Physics verification 路径。

### Error
```text
Get-Content: Cannot find path 'D:\\TRbackup\\NLTX\\Test\\Terraria.Physics.Components.Verification\\Terraria.Physics.Components.Verification.csproj'
```

### Context
- 操作：查找可复用的 focused verification 项目和测试入口。
- 原因：目录清单输出被截断，直接根据显示的目录名假设项目文件位于同名目录下。
- 结果：读取操作返回非零；没有产生部分写入、构建或测试进程。

### Suggested Fix
在读取测试文件前使用 `rg --files` 或 `Get-ChildItem -LiteralPath` 重新确认完整路径，不根据截断输出拼接路径。

### Metadata
- Reproducible: no
- Related Files: Test/

---

## [ERR-20260906-004] serial_dotnet_wrapper_parameter_binding

**Logged**: 2026-09-06T00:00:00+08:00
**Priority**: low
**Status**: pending
**Area**: tests

### Summary
通过 PowerShell 直接把未加引号的 `-p:` 属性参数传给仓库串行 dotnet 包装脚本时，参数被包装脚本自身的参数绑定解析。

### Error
```text
Invoke-SerialDotnet.ps1: Parameter cannot be processed because the parameter name 'p' is ambiguous.
```

### Context
- 操作：运行 Liquid 组件 TDD RED 阶段验证。
- 原因：直接传入 `-p:UseSharedCompilation=false`、`-p:MSBuildNodeReuse=false` 等参数。
- 结果：dotnet 未启动，尚未产生编译或测试结果。

### Suggested Fix
将带冒号的 dotnet 参数作为带引号的完整字符串传给包装脚本，或先放入 PowerShell 字符串数组后使用数组展开，避免包装脚本参数绑定抢先解析。

### Metadata
- Reproducible: yes
- Related Files: Build/Tools/Invoke-SerialDotnet.ps1; Test/Terraria.Liquid.Components.Verification/Terraria.Liquid.Components.Verification.csproj

---

## [ERR-20260906-003] stale_reference_path

**Logged**: 2026-09-06T00:00:00+08:00
**Priority**: low
**Status**: pending
**Area**: docs

### Summary
读取现有 Dome 类型时沿用了过时路径，第一次读取失败。

### Error
```text
Cannot find path 'D:\\TRbackup\\NLTX\\dome\\src\\Terraria.Dome.Simulation\\WorldObjects\\Sign\\SignIdentityComponent.cs'
```

### Context
- 操作：核对现有组件命名和 C# 文件组织方式。
- 原因：实际文件已位于 `dome/src/Terraria.Dome.Simulation/WorldObjects/SignIdentityComponent.cs`，而不是带有 `WorldObjects/Sign/` 子目录的旧路径。
- 结果：后续通过 `rg --files` 定位真实路径并完成只读核对；没有产生部分写入。

### Suggested Fix
读取历史引用路径前先用 `rg --files` 校验当前文件位置，再执行上下文读取。

### Metadata
- Reproducible: no
- Related Files: docs/design/2026-09-05-version4-world-calendar-and-event-orchestration-component-code-draft.md

---

## [ERR-20260906-002] apply_patch_hunk_prefix

**Logged**: 2026-09-06T02:00:00+08:00
**Priority**: low
**Status**: pending
**Area**: docs

### Summary
构造长 Markdown apply_patch 时，YAML 元数据行漏写 `+` 前缀，导致补丁在写入前被拒绝。

### Error
工具返回：`apply_patch verification failed: invalid hunk at line 11, 'taskNumber: 07' is not a valid hunk header.`

### Context
- 操作：创建 WorldProgressionAndUnlocks 实际代码组件草案 Markdown。
- 原因：Add File 补丁中的 `taskNumber` 行没有以 `+` 开头。
- 结果：apply_patch 未执行，目标草案文件没有产生部分写入。

### Suggested Fix
提交长 Add File 补丁前逐行检查新增内容是否全部带 `+`；失败后先确认目标文件不存在或未被部分写入，再重试。

### Metadata
- Reproducible: yes
- Related Files: docs/design/2026-09-06-version4-world-progression-and-unlocks-code-component-draft.md

---

## [ERR-20260906-003] serial_dotnet_p_argument_forwarding

**Logged**: 2026-09-06T02:15:00+08:00
**Priority**: low
**Status**: pending
**Area**: docs

### Summary
在 PowerShell 直接调用串行 dotnet 包装脚本时，未将 `-p:` 参数作为数组元素转发，导致参数被包装脚本自身解析。

### Error
工具返回：`Invoke-SerialDotnet.ps1: Parameter cannot be processed because the parameter name 'p' is ambiguous.`

### Context
- 操作：运行 WorldProgressionAndUnlocks 验证程序的 TDD RED 阶段。
- 原因：直接在 PowerShell 命令行传入 `-p:UseSharedCompilation=false` 等参数。
- 结果：dotnet 未启动，未产生编译或测试结果。

### Suggested Fix
使用 PowerShell 数组保存完整 `DotnetArguments`，再以 `@dotnetArgs` 传给 `Invoke-SerialDotnet.ps1`，确认参数不被包装器的参数绑定抢先解析。

### Metadata
- Reproducible: yes
- Related Files: Build/Tools/Invoke-SerialDotnet.ps1; Test/Terraria.WorldProgressionAndUnlocks.Verification/Terraria.WorldProgressionAndUnlocks.Verification.csproj

---

## [ERR-20260906-004] temporary_build_output_cleanup_rejected

**Logged**: 2026-09-06T02:30:00+08:00
**Priority**: low
**Status**: pending
**Area**: docs

### Summary
清理本次临时验证项目生成的明确 `Build/bin` 和 `Build/obj` 目录时，文件操作命令被执行策略拒绝。

### Error
工具返回：`exec_command failed: CreateProcess ... rejected by policy`

### Context
- 操作：删除已确认属于本次临时验证项目的测试目录、生成输出和中间输出。
- 目标：`Build/bin/Terraria.WorldProgressionAndUnlocks.Verification`、`Build/obj/Terraria.WorldProgressionAndUnlocks.Verification`。
- 结果：没有删除任何内容；测试源文件和项目文件已通过补丁删除，临时生成目录可能保留。

### Suggested Fix
不要绕过执行策略；生成物位于仓库约定的 Build 目录且不进入源码提交范围，保留并在后续受控清理时处理。

### Metadata
- Reproducible: unknown
- Related Files: Build/bin/Terraria.WorldProgressionAndUnlocks.Verification; Build/obj/Terraria.WorldProgressionAndUnlocks.Verification

追加尝试：对已确认为空的测试目录执行非递归删除同样被执行策略拒绝；未删除任何其他文件或目录。

---

## [ERR-20260906-004] serial_dotnet_p_argument_forwarding_player_components

**Logged**: 2026-09-06T02:30:00+08:00
**Priority**: low
**Status**: pending
**Area**: tests

### Summary
PlayerGameplay 组件验证的 TDD RED 调用同样因 PowerShell 未保护 `-p:` 参数而未启动 dotnet。

### Error
```text
Invoke-SerialDotnet.ps1: Parameter cannot be processed because the parameter name 'p' is ambiguous.
```

### Context
- 操作：运行 `Test/Terraria.Player.Components.Verification` 的 RED 验证。
- 原因：直接把 `-p:UseSharedCompilation=false` 等参数写在 PowerShell 命令调用中，参数绑定抢先解析了 `-p`。
- 结果：串行包装脚本退出码为 1；dotnet 未启动，没有生成编译或测试结果。

### Suggested Fix
后续调用使用完整 `DotnetArguments` 数组，再以数组展开方式传入串行包装器；不得把 `-p:` 参数裸传给 PowerShell 脚本。

### Metadata
- Reproducible: yes
- Related Files: Build/Tools/Invoke-SerialDotnet.ps1; Test/Terraria.Player.Components.Verification/Terraria.Player.Components.Verification.csproj
- See Also: ERR-20260906-003

---

## [ERR-20260906-005] dotnet_run_requires_explicit_project_player_components

**Logged**: 2026-09-06T02:35:00+08:00
**Priority**: low
**Status**: pending
**Area**: tests

### Summary
PlayerGameplay 组件验证的串行 `dotnet run` 需要显式 `--project`，裸项目路径未被当前 SDK 识别。

### Error
```text
找不到要运行的项目。请确保 D:\\TRbackup\\NLTX 中存在项目，或使用 --project 传递项目路径。
```

### Context
- 操作：在 `Build/Tools/Invoke-SerialDotnet.ps1` 中运行 Player 组件验证项目。
- 原因：按仓库旧计划传入了 `run <project-path>`；当前 SDK 将该参数解析为非项目参数。
- 结果：dotnet 启动但没有编译项目，退出码为 1。

### Suggested Fix
验证项目时使用 `run --project <project-path>`，同时继续通过参数数组转发 `-m:`、`-nr:` 和 `-p:` 参数。

### Metadata
- Reproducible: yes
- Related Files: Build/Tools/Invoke-SerialDotnet.ps1; Test/Terraria.Player.Components.Verification/Terraria.Player.Components.Verification.csproj

---

## [ERR-20260906-005] serial_dotnet_p_argument_forwarding_item_components

**Logged**: 2026-09-06T00:00:00+08:00
**Priority**: low
**Status**: pending
**Area**: tests

### Summary
ItemContainerAndEconomy 组件验证的首次串行 dotnet 调用因 PowerShell 未保护 `-p:` 参数而未启动 dotnet。

### Error
```text
Invoke-SerialDotnet.ps1: Parameter cannot be processed because the parameter name 'p' is ambiguous.
```

### Context
- 操作：运行 `Test/Terraria.Items.Components.Verification` 的 RED 验证。
- 原因：直接把 `-p:UseSharedCompilation=false` 等参数写在 PowerShell 命令调用中，参数绑定抢先解析了 `-p`。
- 结果：串行包装脚本退出码为 1；dotnet 未启动，没有生成编译或测试结果。

### Suggested Fix
使用完整 `DotnetArguments` 数组，再以数组展开方式传入串行包装器；不得把 `-p:` 参数裸传给 PowerShell 脚本。

### Metadata
- Reproducible: yes
- Related Files: Build/Tools/Invoke-SerialDotnet.ps1; Test/Terraria.Items.Components.Verification/Terraria.Items.Components.Verification.csproj
- See Also: ERR-20260906-003, ERR-20260906-004

---

## [ERR-20260906-006] verification_project_argument_forwarding_item_components

**Logged**: 2026-09-06T00:00:00+08:00
**Priority**: low
**Status**: pending
**Area**: tests

### Summary
通过参数数组调用串行 dotnet 包装脚本时，验证项目位置参数未被 `dotnet run` 识别。

### Error
```text
找不到要运行的项目。请确保 D:\TRbackup\NLTX 中存在项目，或使用 --project 传递项目路径。
```

### Context
- 操作：运行 `Test/Terraria.Items.Components.Verification` 的 RED 验证。
- 原因：使用 `run <project-path>` 形式经包装脚本转发后未进入指定项目。
- 结果：dotnet 启动但未编译验证项目或生产项目。

### Suggested Fix
使用 `run --project <project-path>` 明确传递验证项目，并保留 dotnet 属性参数数组转发方式。

### Metadata
- Reproducible: yes
- Related Files: Build/Tools/Invoke-SerialDotnet.ps1; Test/Terraria.Items.Components.Verification/Terraria.Items.Components.Verification.csproj

---

## [ERR-20260906-005] powershell_object_property_expression

**Logged**: 2026-09-06T09:40:00+08:00
**Priority**: low
**Status**: pending
**Area**: docs

### Summary
只读文档清单对比脚本在 PowerShell `PSCustomObject` 属性表达式中直接写入命令调用，导致脚本解析失败。

### Error
```text
ParserError: An empty pipe element is not allowed.
```

### Context
- 操作：按固定 `-public-decomposition.md` 后缀推导 `-component-design.md` 并检查目标文件是否存在。
- 原因：`StandardDesignExists=Test-Path $standard` 未使用子表达式或先赋值，触发 PowerShell 对象初始化语法解析错误。
- 结果：该次脚本未执行任何文档比较逻辑，也没有产生文件写入、构建或测试进程。

### Suggested Fix
在 `PSCustomObject` 初始化前先计算布尔值，或使用 `$(Test-Path $standard)`；长只读核对脚本优先拆分为更小的可验证表达式。

### Metadata
- Reproducible: yes
- Related Files: docs/第一轮审查/2026-09-06-version4-component-only-design-session-prompt.md; docs/design/

---

## [ERR-20260906-009] build_project_switch

**Logged**: 2026-09-06T00:00:00+08:00
**Priority**: low
**Status**: pending
**Area**: tests

### Summary
对 `dotnet build` 误用仅适用于 `dotnet run` 的 `--project` 选项，导致 MSBuild 拒绝未知开关。

### Error
```text
MSBUILD : error MSB1001: 未知开关。
开关:--project
```

### Context
- 操作：构建 Liquid 组件受影响项目以完成实现验证。
- 原因：将 `run --project` 的项目参数形态直接套用于 `build`。
- 结果：该次构建退出码为 1；随后改用 `build <project-path>` 成功完成构建。

### Suggested Fix
使用 `run --project <project-path>`，使用 `build <project-path>`；两者都通过 `Invoke-SerialDotnet.ps1` 串行执行。

### Metadata
- Reproducible: yes
- Related Files: Build/Tools/Invoke-SerialDotnet.ps1; src/WorldStorage/Terraria.WorldStorage.csproj

---

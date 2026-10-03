# 世界生成代码

本目录包含从 `D:/TRbackup/Version4` 复制的世界生成相关源码，按生成职责组织，并保留源码原有命名空间。

## 目录

- `WorldGen.cs`：世界生成入口和 Pass 编排。
- `WorldBuilding/`：生成器、Pass、形状、搜索、生成变量和结构占用等。
- `Generation/`：通用生成算法辅助类型。
- `Biomes/`：地形、生物群系、洞穴房屋和沙漠结构生成。
- `Dungeon/`：地牢生成、入口、特征、走廊、布局和房间。
- `EntitySources/`：世界生成实体来源上下文。
- `Snapshots/`：生成调试快照与 Tile 快照源码。
- `Configuration/`：世界生成配置。
- `Mocks/`：仅供诊断编译使用的最小外部 API mock，不属于运行时世界生成实现。

## 明确边界

- 未复制 `Terraria.IO/WorldFile.cs`、完整世界存档编解码、文件系统适配器或云存储适配器；这些持久化能力由仓库现有实现承接。
- `WorldGenSnapshot.cs` 和 `TileSnapshot.cs` 按单独的快照源码需求保留在 `Snapshots/`。
- 已按项目边界要求移除本目录 C# 文件中的 `using Terraria...;` 命名空间导入；源码中的旧游戏类型引用未在本次改动中重写。
- 复制的 Version4 源码保留其原有 `Terraria.*` 命名空间和旧运行时依赖。文件归位本身没有改变运行时所有权，也没有接入 ECS 调度或任何 `.csproj` 编译项。
- `Mocks/` 中的文件由 `Build/diagnostics/WorldGenerationCompile/WorldGeneration.Diagnostic.csproj` 显式引用；正式世界生成源码不会自动编译这些 mock。

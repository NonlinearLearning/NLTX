# Version4 世界生成参考副本

本目录复制自 `D:/TRbackup/Version4`，用于对照和拆分世界生成代码。

## 内容

- `Terraria/WorldGen.cs`
- `Terraria.WorldBuilding/`
- `Terraria.GameContent.Generation/`
- `Terraria.GameContent.Biomes/`
- `Terraria.GameContent.Biomes.CaveHouse/`
- `Terraria.GameContent.Biomes.Desert/`
- `Terraria.GameContent.Generation.Dungeon*/`
- `Configuration/Terraria.GameContent.WorldBuilding.Configuration.json`

源目录层级和命名空间保持不变，便于搜索、比对和后续拆分。

## 边界

- 本目录是参考材料，不是生产代码目录。
- 不加入任何生产项目的编译项，也不得被生产项目引用。
- 快照组件源码已移至正式的 `src/NSSLC.Infrastructure/WorldGeneration/Snapshots/` 目录。
- 未复制 `Terraria.IO/WorldFile.cs` 以及文件系统、云存储和持久化适配代码。
- Version4 参考目录没有实际生成的 `.gensnapshot` 二进制数据文件。
- 世界生成算法和权威世界状态仍属于权威领域；本目录不改变 NLTX 的运行时行为。

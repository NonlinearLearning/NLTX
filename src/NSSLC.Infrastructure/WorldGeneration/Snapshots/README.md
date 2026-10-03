# 世界生成快照组件

本目录存放从 `D:/TRbackup/Version4` 复制的世界生成快照组件源码：

- `WorldGenSnapshot.cs`
- `TileSnapshot.cs`

这些文件用于保留 Version4 快照机制的组件级参考，目录与 `WorldGeneration` 能力独立管理。

## 边界

- 该快照组件只描述生成阶段的快照捕获、加载和恢复模型。
- 文件路径、文件创建、删除、备份、云存储和字节流提交仍由持久化端口与适配器负责。
- 当前 Version4 源码仍依赖 `Main`、`GenVars`、`TileEntity`、`Chest` 等旧运行时类型；未加入 `NSSLC.Infrastructure.WorldStorage.csproj` 的编译项。
- 参考源码没有实际生成的 `.gensnapshot` 二进制数据文件。

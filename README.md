# NLTX 组件定义

活动项目为 [Terraria.Components.csproj](src/NSSLC/Terraria.Components.csproj)，目标框架为
`net10.0`。源码按 ECS 领域放在 `src/NSSLC/` 下：端别无关的共享能力域收在 `Share/`
（`AI`、`Attributes`、`Combat`、`Entity`、`Inventory`、`Item`、`Physics`），其余领域
（`Chat`、`Communication`、`ItemUse`、`Npc`、`NpcSpawn`、`Player`、`Projectile`、
`TileEntity`、`World`、`WorldItem`、`Session`）直接位于项目根目录，不经过 `Components/`
中间层。目录只表达领域归属，C# 命名
空间保持原样，移动目录不改变公共 API。项目引用 `Arch` 与 `Arch.System` 包，不引用其他
生产项目。服务器诊断日志不是 ECS 领域，位于
[NSSLC.Infrastructure/Diagnostics](src/NSSLC.Infrastructure/Diagnostics/README.md)。

```powershell
dotnet build src/NSSLC/Terraria.Components.csproj --nologo --verbosity minimal
```

编译输出位于 `Build/bin/Terraria.Components/Debug/net10.0/`。

领域清单、子目录边界和共享组件归属见 [src/NSSLC/README.md](src/NSSLC/README.md)。组织
规则见 [ECS 文件组织设计约束](Context/架构设计/ECS文件组织设计约束.md) 与
[项目文件组织设计约束](Context/架构设计/项目文件组织设计约束.md)。

用户明确保护 `src/NSSLC.Infrastructure/` 整棵目录：其中源码、项目、参考材料和已有修改
完整保留，不纳入当前组件库的编译。该目录的旧项目可能继续引用已经删除的项目；保留文件
并不代表其原有项目仍可编译或运行。唯一例外是 `Diagnostics/`：它是当前服务器与网络工程
正式引用的日志工程；该目录其余内容未因这次移入而被改动或修复。

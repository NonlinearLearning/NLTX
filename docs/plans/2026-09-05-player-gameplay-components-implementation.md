# 玩家玩法与能力组件实施记录

## 范围

本次将《[Version4 玩家玩法与能力系统组件字段设计](../Version4玩家玩法与能力系统组件字段设计.md)》
中的 13 个状态组件落入 `src/Player` 的 `Terraria.Player` 项目。实现只交付状态、兼容字段和
只读派生属性，不包含 System、Query、网络 Adapter、存档 Projection 或测试。

## 迁移映射

| 设计来源 | 目标路径 | 依赖影响 |
| --- | --- | --- |
| `PlayerIdentityState` 至 `PlayerSummonCapacityState` | `src/Player/Player*State.cs` | 仅依赖 .NET 基类库；没有项目引用变更。 |
| 设计中的领域值类型、内容 ID、固定容器、Buff 槽和 Loadout 值 | `src/Player/PlayerValueTypes.cs` | 使 Player 项目不依赖 Version4 的 `Player`、`Item`、`Mount` 或网络对象。 |

组件按玩家能力领域直接放置在 `src/Player`，未创建泛化 `Components/` 子目录。公开集合均为
只读视图；设计归类为 `Derived` 的值仅允许同程序集的未来 System 重建，不能通过公共 API
写入为独立权威源。

## 保留的边界

- 位置、速度、碰撞体和朝向继续由 `Entity`、`Movement`、`Physics` 领域拥有。
- 输入由既有 `InputIntentComponent` 表达；没有进入持久玩家玩法状态。
- Item、Chest、Mount、Player 与 socket 的对象引用均未进入新组件。
- 没有把坐骑定义、钓鱼尝试、浮标、掉落、投射物 owner 聚合或渲染状态伪装成玩家字段。

## 验证与回滚

执行了仓库串行构建：

```powershell
pwsh -NoProfile -Command "& .\Build\Tools\Invoke-SerialDotnet.ps1 'build' '.\src\Player\Terraria.Player.csproj' '-m:1' '-nr:false' '-p:UseSharedCompilation=false' '-p:MSBuildNodeReuse=false' '-p:BuildInParallel=false'"
```

结果：退出码 `0`，`0` 个警告，`0` 个错误；产物为
`Build/bin/Terraria.Player/Debug/net10.0/Terraria.Player.dll`。

本轮不包含行为 System，因此没有新增或运行测试。回滚单位是本记录所列的 14 个
`src/Player` 文件；删除它们不会影响已有 Player、Items、Combat、Projectile 或 Entity 的类型。

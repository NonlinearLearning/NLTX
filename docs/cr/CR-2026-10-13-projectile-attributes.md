# 射弹布尔属性组件合并

日期：2026-10-13

## 范围与决策

按用户要求实现 `ProjectileAttributeComponent`，合并射弹的附魔限制和已反射标记。
采用参考源码研究后建议的位标记方案：`ProjectileAttribute : byte` 使用 `[Flags]`，
组件只保存一个公开的 `Attributes` 字段，不承载射弹行为。

| 原组件及字段 | 新枚举成员 | 值 |
| --- | --- | --- |
| `ProjectileDamagePolicyComponent.NoEnchantments` | `NoEnchantments` | 1 |
| `ProjectileDamagePolicyComponent.NoEnchantmentVisuals` | `NoEnchantmentVisuals` | 2 |
| `ProjectileReflectionStateComponent.Reflected` | `Reflected` | 4 |

`None = 0` 对应原来三个字段全部为 `false`。三个标记可以独立组合；设置一个标记不隐式
设置另一个标记。设置、检查、清除分别使用位运算，保持原有的二值语义。枚举与组件都是
值类型，不引入可变集合或实例间共享的引用。反射引起的阵营、速度变化不属于组件数据。

## 文件与依赖

所有源码路径均位于 `src/NSSLC/Components/Projectile/`，命名空间保留
`Terraria.Projectile`。

- 删除 `ProjectileDamagePolicyComponent.cs` 和 `ProjectileReflectionStateComponent.cs`。
- 新增 `ProjectileAttribute.cs` 和 `ProjectileAttributeComponent.cs`。
- 活动项目 `src/NSSLC/Terraria.Components.csproj` 的现有通配符直接包含新文件。
- 当前生产源码中没有旧类型消费者、注册表、原型或序列化入口需要修改。
- 原组件的公开类型被替换，外部消费者需改用新类型和标记。历史审计清单保留为历史证据。
- `src/NSSLC.Infrastructure/` 和外部参考目录没有改动。

该决策仅适用于上述三个射弹属性，不将所有布尔字段统一转换为枚举。

## 回滚

恢复两个旧组件文件并移除两个新增源码文件即可回滚数据定义；若后续消费者使用了新 API，
需同步恢复对应读写方式。

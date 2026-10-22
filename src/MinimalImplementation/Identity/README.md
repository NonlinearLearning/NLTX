# Identity：实例身份、世界范围与代次

[返回总览](../README.md)。身份系统回答“这个引用现在还指向同一个有效实例吗”，为玩家、NPC、
弹幕和机关的生命周期提供共同基础。

## 状态与组件

| 部件 | 保存或负责的内容 |
| --- | --- |
| [EntityIdentityRegistry](EntityIdentityRegistry.cs) | 当前世界身份范围、已分配身份集合、有效句柄和下一代次 |
| [EntityHandle](EntityHandle.cs) | EntityUuid、WorldScopeId、EntityGeneration 的组合 |
| [WorldScopeId](WorldScopeId.cs) | 实体身份所在的世界范围 |
| [EntityGeneration](EntityGeneration.cs) | 区分实例生命周期的代次 |
| [IdentityContracts](../Contracts/IdentityContracts.cs) | 跨项目使用的身份值类型 |

Registry 只管理身份有效性。玩家生命、NPC 位置、弹幕伤害分别留在所属领域，
其 Manager/Owner 建立身份到领域状态及槽位的映射。

## 创建与销毁怎样协作

Create 生成新的身份根，并附上当前世界范围和代次，登记进有效表。
领域 Owner 在其创建流程中持有句柄并建立自己的索引。之后 IsActive 同时检查身份、
世界范围和完整句柄；只传一个网络槽位不足以证明实例还有效。

Retire 删除当前有效句柄。BeginWorldScope 会清空有效表、推进世界范围并重置范围内代次
分配；旧范围的引用因此失效。世界数据的 WorldEpoch 与这里的范围是不同概念，跨边界
映射由 Host 和协调器明确处理。

## 各领域怎样使用身份

- PlayerOwner 使用 EntityHandle，并通过 SessionBinding 与当前 SessionEpoch 关联。
- Combat 通过 [CombatIdentityAuthority](../Host/ServerIdentityAuthorities.cs) 实现领域身份端口。
- Mechanisms 通过同一文件中的 MechanismIdentityAuthority 使用 Registry。
- 世界物品有自己的 WorldItemHandle，携带世界范围、槽位和生成代次；不能把它当作网络槽位整数。
- NPC 的网络 byte 代次由 [NpcNetworkProjection](../Host/NpcNetworkProjection.cs) 维护，
  它是协议投影，不能直接当作 Registry 的 EntityGeneration。

## 推荐跟读

从 Create → PlayerOwner 创建 → SessionBinding → IsActive/Retire 阅读一次玩家生命周期。
再看 NPC 的 PrepareSpawn/Commit 与网络代次校验，理解“实例身份”“槽位”“包里的代次”
如何同时存在并分别校验。

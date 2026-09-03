# Player Blocked Core Evidence Reconciliation

**Oracle:** `D:\TRbackup\Version4物理删除了某些文件\Terraria\Player.cs`  
**SHA-256:** `AF4C868BE773599E271853A549791405AF8F9CD315191B1FE4357E92BD84D7C1`  
**Disposition:** `partial`; every archive row remains `Blocked`.

| PB | Current evidence | Evidence directory |
| --- | --- | --- |
| PB-001 | Deferred: mixed owners and byte-range-only anchor | `Build/diagnostics/player-blocked-core/PB-001/20260829-specialized-mechanics-boundary/` |
| PB-002 | Deferred: capability/intent contract missing | `Build/diagnostics/player-blocked-core/PB-002/20260829-special-movement-boundary/` |
| PB-003 | Deferred: source 58 slots vs runtime/persistence 40 slots | `Build/diagnostics/player-blocked-core/PB-003/20260829-inventory-mode-boundary/` |
| PB-004 | Deferred: environment snapshot and damage-event owner missing | `Build/diagnostics/player-blocked-core/PB-004/20260829-environment-boundary/` |
| PB-005 | Accepted policy slice: luck/counter equation; runtime integration deferred | `Build/diagnostics/player-blocked-core/PB-005/20260829-luck-counter-policy/` |
| PB-006 | Accepted policy slice: spawn-area validation; other spawn paths deferred | `Build/diagnostics/player-blocked-core/PB-006/20260829-spawn-area/` |
| PB-007 | Accepted slice: Shadow Dodge immunity; other dodge branches deferred | `Build/diagnostics/player-blocked-core/PB-007/20260829-shadow-dodge/` |
| PB-008 | Accepted policy slice: Paladin predicate; multiplayer protection deferred | `Build/diagnostics/player-blocked-core/PB-008/20260829-paladin-shield-policy/` |
| PB-009 | Deferred: deterministic death-drop contract missing | `Build/diagnostics/player-blocked-core/PB-009/20260829-tombstone-boundary/` |
| PB-010 | Accepted policy slice: melee scale; other item effects deferred | `Build/diagnostics/player-blocked-core/PB-010/20260829-melee-scale-policy/` |
| PB-011 | Accepted slice: attack cooldown; banner/NPC/jellyfish rules deferred | `Build/diagnostics/player-blocked-core/PB-011/20260829-attack-cooldown/` |

This records closure of the blocked-core execution inventory, not Player parity.
`Blocked` is preserved until the complete behavior family meets its archive
reopen condition.

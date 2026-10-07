# NPC AI Guide danger, attack configuration, and idle checkpoint — 2026-10-07

## Scope and source

This batch extends the exact `type=22 / netID=22 / aiStyle=7` Guide profile
with three source-bounded slices:

- danger scan and danger response from `NPC.cs:54241-54405`;
- Guide `ai[0] == 12` attack configuration, launch request, and cycle timer
  from `NPC.cs:55312-55582`;
- random NPC-to-NPC idle conversation entry from `NPC.cs:55989-56149`, plus
  Guide dialogue selection from `NPC.cs:96181-96193`.

Reference source:
`D:/TRbackup/无任何删减通过编译/Terraria/NPC.cs`

Reference SHA-256:
`ED8AA2302730A9E046EF310E543204FBA391BD35B1C9C0B213C8065DFBBE39F0`

Source golden comparison remains **not-run** because no source golden output
exists. New read-only excerpts are under
`Build/diagnostics/NpcAiRedesign/guide-source-profile-20261007/`.

## Implemented APIs

### Danger scan and response

- `NpcGuideDangerScan` consumes explicit NPC and player snapshots.
- The scan preserves the source distinction between the extended danger range
  (`flag15`) and the base range (`flag16`), nearest left/right offsets, chase
  entity IDs, and stinky state (`flag17`).
- `NpcGuideDangerProfile` preserves the Guide-specific response gates:
  server authority, the talking-player suppression, `ai[0] == 8` return to
  walk, specialized attack/conversation state preservation, `PrettySafe`,
  `ai[0] == 1` direction correction, and unsafe-walk transition.
- `EvaluateWithRandom` consumes `Next(300)` and `Next(120)` only on the source
  branches that use those values.
- Partner NPC state writes are returned through
  `NpcGuidePartnerStateResetRequest` and an explicit effect port. The Guide
  profile does not mutate a partner entity.

### Attack configuration

`NpcGuideAttackConfigurationProfile` covers the type-22 branch in `ai[0] ==
12`:

| mode | projectile | base damage | cooldown base | random range |
| --- | ---: | ---: | ---: | ---: |
| normal | 1 | 12 | 30 | `Next(20)` |
| hardmode | 2 | 18 | 15 | `Next(10)` |

Both modes preserve speed `10f`, spawn tick `1`, knockback `2.75f`, aim offset
`4`, spread `0.7f`, and source integer damage scaling. The result also reports
the frame-reset condition, the spawn timing window, and cycle completion. On
the server launch window, `EvaluateWithRandom` computes source target aim,
fallback direction, spread, spawn position, projectile flags, and cycle
cooldown randomization. `ApplyEffects` passes the projectile request and
network update through explicit owner ports; it does not access a projectile
store or network host.

### Random idle and dialogue

- `NpcGuideIdleProfile` preserves the `flag30` idle gate, the `Next(300)` NPC
  conversation branch, the `Next(1800)` second conversation branch, source
  duration randomization, state slots, facing, and partner request ordering.
- `NpcGuideDialogueProfile` preserves Guide special-event precedence, blood
  moon choices `170/171/172`, Lantern Night, eclipse, slime rain, night text
  `173`, hardmode chatter checks, and generic choices `174/175/176`.
- All random calls use explicit random ports. Localization lookup and entity
  writes remain caller-owned.

## Ownership and side effects

| Concern | Owner |
| --- | --- |
| danger NPC/player snapshot | caller-owned target/collision adapter |
| danger calculation | `NpcGuideDangerScan` |
| Guide danger state transition | `NpcGuideDangerProfile` |
| partner danger reset | existing NPC owner through `INpcGuideDangerEffectPort` |
| attack configuration and timer facts | `NpcGuideAttackConfigurationProfile` |
| projectile creation and network flags | existing projectile/effect owner through `INpcGuideAttackEffectPort` |
| idle candidate snapshot | caller-owned NPC/collision adapter |
| partner conversation reset | existing NPC owner through `INpcGuideIdleEffectPort` |
| random source | explicit Guide random ports |
| localized dialogue text | existing localization owner |
| task entry/completion/failure | existing `NpcTaskLifecycleSystem` and `NpcTaskReference` |

No runtime host, entity registry, identity store, Housing/Town owner, Blue,
Mother, Eye, ReferenceVerification, or canonical plan/ledger was modified by
this batch.

## Verifier coverage

`Test/Terraria.NpcAi.GuideProfileVerification/` now verifies:

1. extended versus base danger range, stinky flags, and nearest chase IDs;
2. danger state entry, random timer, direction selection, PrettySafe
   suppression, partner reset request, and effect order;
3. normal and hardmode Guide attack configuration, damage scaling, target aim,
   projectile spawn geometry/effect order, cycle cooldown randomization, timer
   reset, and spawn timing window;
4. Guide special-event, blood-moon, hardmode chatter, and generic dialogue
   random branches;
5. first NPC conversation, partner state, source duration randomization, and
   generic idle transition;
6. all previously recorded return-home, resting-spot, ForceSitting,
   walk-prediction, conversation-gate, and shared task-lifecycle scenarios.

## Verification status

The independent verifier run passed after correcting the PrettySafe fixture to
the source condition (`PrettySafe < nearest danger distance`) and after adding
the source launch and cycle checks. Final evidence files are:

- `build-npc-attack-cycle-final.txt`: `dotnet build src/NSSLC/Component/Npc/Terraria.Npc.csproj --no-restore`, exit `0`, zero warnings/errors;
- `build-guide-verifier-attack-cycle-final.txt`: verifier project build, exit `0`, zero warnings/errors;
- `run-guide-verifier-attack-cycle-final.txt`: verifier exit `0`, with the
  `PASS` line covering danger, attack configuration/launch/cycle, dialogue,
  idle conversation, and all earlier Guide slices;
- `final-fingerprint.txt`: profile and binary hashes with
  `sourcegolden=not-run`.

## Open slices and fallback

- `ai[0] == 10` has no Guide-specific projectile configuration in the source
  branch and remains represented by the surrounding town-NPC state owner.
- The ordinary movement/door state machine, attack animation details, and
  non-Guide generic idle branches remain outside this Guide profile.
- If a later source differential changes candidate filtering or random order,
  retain this checkpoint as the verified fallback and isolate the changed
  branch in a new source excerpt and verifier scenario.

# Upgrade WorldFile Writes to Version 326

Document ID: `CR-2026-10-21-WORLDFILE-326-WRITE`  
Domain: persistence-format  
Status: implemented; acceptance partially verified  
Date: 2026-10-21

## 1. Original request and authorization

Original request:

> 同时修改世界生成api生成符合新版的世界

User-selected scope:

> 改为升级 WorldFile 编码器至 326

User confirmation of change level and schedule:

> 中度：当前任务实施（推荐）

The scope was changed from integrating the full terrain generator to updating WorldFile output.
Implementation is authorized for the current task as a moderate change.

## 2. Findings

- The latest client source is Terraria `v1.4.5.8` and saves WorldFile version 326.
- In version 326, the client writes `DualDungeonsSeed`, `MoreLightningSeed`, and
  `NoLightningSeed` in that order, followed by the world manifest in the versioned header.
- The repository decoder already reads both lightning seed flags for version 323 and later, and
  `WorldFileSpawnSection` already retains both values.
- The encoder currently writes version 319, omits the two lightning seed booleans, and rejects
  non-default values. New-world document creation and the full generated-world projection also
  currently declare format version 319.
- The latest client source accepts WorldFile versions through 326, so existing version 319 files
  remain within its readable range. The generated file was not loaded in the client during this
  change.

## 3. Scope and acceptance criteria

- Make 326 the current writable version while allowing supported version 319 documents to be
  encoded as version 326.
- Write `MoreLightningSeed` and `NoLightningSeed` after `DualDungeonsSeed` and before the
  manifest, including when their values are false.
- Declare version 326 in new-world documents and the generated-world projection.
- Preserve decoder support for 319 and decode both flags for version 323 and later.
- A loaded 319 document can be saved as 326 without losing fields represented by the current
  persistence model.
- A newly generated 326 file loads in the latest client (not verified in this task).

## 4. Impact assessment

- Database or external schema: none.
- Persistence format: new writes move from 319 to 326; backward decoding remains supported.
- Code areas: WorldFile encoder/validator constants, minimal world creation coordinator/builder,
  generated-world projection, and format documentation.
- Runtime impact: negligible; two boolean fields are added to the serialized header.
- Compatibility risk: incorrect field order or version promotion could misalign the header.
- Verification commands and outcomes:
  - `dotnet build src/NSSLC.Infrastructure/WorldStorage/NSSLC.Infrastructure.WorldFile.Minimal.csproj --no-restore --verbosity quiet` — exit 0; 0 warnings, 0 errors; output under `Build/bin/NSSLC.Infrastructure.WorldFile.Minimal/Debug/net10.0/`.
  - `dotnet build src/MinimalImplementation/Persistence/MinimalImplementation.Persistence.csproj --no-restore --verbosity quiet` — exit 0; 0 warnings, 0 errors; output under `Build/bin/MinimalImplementation.Persistence/Debug/net10.0/`.
  - `dotnet build src/MinimalImplementation/MinimalImplementation.Server.csproj --no-restore --verbosity quiet -p:BaseOutputPath=Build/bin/MinimalImplementation.Server-WorldFile326Verify/` — exit 0; 10 existing nullable warnings, 0 errors; output under `Build/bin/MinimalImplementation.Server-WorldFile326Verify/`.
  - `dotnet build src/NSSLC.Infrastructure/WorldStorage/NSSLC.Infrastructure.WorldStorage.csproj --no-restore --verbosity quiet` — exit 1; 3 warnings, 139 errors from removed legacy ECS references and unavailable dependencies; no successful output.
  - No tests were added or run, and no latest-client load was attempted.
- Runtime creation used the isolated 326 server build with `--create-world` at
  `Build/runtime/WorldFile326-2026-10-21/world.wld`, listener `127.0.0.1:7778`. Logs reported
  `server.world.create.completed`, `server.world.load.validated`, `server.ready`, and on graceful
  stop `saveCommitted=True`. The final file is 2,545 bytes; its first Int32 is 326. The separate
  verification server was stopped; the existing server on port 7777 remained running.
- Full terrain-generation integration is outside this change.

## 5. Implementation limits

- The output version and header serialization are upgraded; this does not change the server's
  network protocol version or establish packet-layout compatibility with Terraria 1.4.5.8.
- The current minimal new-world API still creates its existing minimal/placeholder world content.
  Version 326 output does not make it a full terrain-generated world.
- The full generated-world projection now declares the current writable version, but its containing
  legacy WorldStorage project could not be built in the current source tree.
- Latest-client acceptance remains an unverified acceptance criterion.

## 6. Decision record

| Field | Decision |
| --- | --- |
| Level | Moderate |
| Decision | Accepted |
| Approved by | User |
| Schedule | Current task |
| Strategy | `spec` |

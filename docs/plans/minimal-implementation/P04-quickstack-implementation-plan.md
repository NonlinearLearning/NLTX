# Minimal Server P04 QuickStack Implementation Plan

Status: subplan for integration into the MinimalImplementation master plan. This document is
planning evidence only; it does not claim that QuickStack behavior is implemented or verified.

## Goal and boundary

Implement the authoritative Version4 packet 85 QuickStack path in
`D:\TRbackup\NLTX\src\MinimalImplementation`: decode an authenticated player's request, resolve
the requested item slots from the server-owned inventory, find eligible nearby chests from
server-owned world state, calculate a complete transfer, commit all inventory and chest changes
atomically, and publish the required authoritative projections and packet 85 response.

The only external code dependency permitted for this work is the reviewed public API of
`src/NSSLC.Infrastructure`. QuickStack business rules, DTOs, adapters, transaction coordination,
handlers, and verification sources stay under `src/MinimalImplementation`. Do not add ECS
dependencies. Do not trust client-provided item values, chest targets, transfer amounts, or item
eligibility decisions.

This subplan owns the behavior behind packet 85 only. It consumes chest/world and player
position/access facts through explicit server-side interfaces. It shares the inventory/chest
owners and atomic transaction seam with crafting and other item-transfer paths; those owners
must not be duplicated here.

## Verified baseline and evidence gaps

- Version4 `Terraria/MessageBuffer.cs`, packet 85, resolves `Main.player[whoAmI]`, calls
  `QuickStacking.ReadNetInventory(player, reader)`, reads a `bool smartStack`, and calls
  `QuickStacking.QuickStackToNearbyChests(...)`. The server-side player identity therefore comes
  from the authenticated connection, and the selected slot IDs must be resolved against the
  server's inventory.
- Version4 `Terraria/NetMessage.cs`, packet 85, calls
  `QuickStacking.WriteBlockedChestList(writer)`. The known S2C shape is
  `Int32 blockedCount + blockedCount × UInt16 chestId`.
- The known C2S shape is
  `Int32 count + count × Int16 slotId + bool smartStack`. The slot IDs refer to player inventory
  slots; they do not carry item instances.
- The inspected `D:\TRbackup\Version4\Terraria.GameContent\QuickStacking.cs` has empty bodies for
  `ReadNetInventory`, `QuickStackToNearbyChests`, and `WriteBlockedChestList`. It establishes
  method names and call flow, but not the algorithm. Exact range, candidate ordering, `smartStack`
  semantics, blocked-chest semantics, and response timing must therefore remain explicit evidence
  gaps until recovered from an authoritative Version4 artifact or frozen by the master plan.
- The current MinimalImplementation already contains packet 85 DTOs and
  `QuickStackPayloadCodec`, `ItemValidationPolicy`, inventory/chest snapshots and stores, chest
  access/lease types, and content bounds. `CanQuickStackSource` currently excludes favorited items
  and coins. These are reusable foundations, not a completed transfer path.
- The local destination provider captures active chest snapshots for one world scope, requires a
  server-owned player position and an explicitly configured maximum distance, applies the injected
  chest access policy, marks leases held by another session as blocked, and orders candidates by
  squared distance then chest index. Version4 does not establish those rules; they are explicit
  minimal-server compatibility choices, not proven parity.
- Inventory has a preparation seam, chest updates can validate and apply a batch across distinct
  chests, and `InventoryTransactionCoordinator` stages the inventory before the chest batch then
  commits both synchronously. Its atomicity depends on every write running on the same serialized
  server commit loop; it has not been compiled or verified. The root should review that assumption
  before QuickStack consumes it.

Before coding transfer rules, the implementation owner must resolve the evidence gaps above. If
the exact legacy rule cannot be recovered, record the chosen minimal-server rule in the master
plan as an intentional compatibility limit; do not silently present a guess as Version4 parity.

## Target flow

1. The packet dispatcher supplies a fully framed packet, authenticated player identity, session
   epoch, and current world scope. Decode packet 85 with the existing codec and the configured
   inventory-slot and chest-index bounds. Reject malformed counts, duplicate or out-of-range slot
   IDs, truncated payloads, and trailing bytes before touching state.
2. Resolve each requested slot from a fresh server inventory snapshot. Apply the item validity and
   source eligibility policy to server values. Empty, invalid, favorited, coin, or otherwise
   ineligible slots do not create transfer deltas. Never accept an item stack from the wire.
3. Query candidate chests from the server's active world scope and player position through a
   narrow eligibility policy. The local provider requires an injected server position source and
   explicit distance in world-tile units; it uses Euclidean distance, applies world access, marks
   another session's current lease as blocked, and sorts by distance then chest index. These choices
   fill the unrecoverable Version4 gaps; the client does not nominate destinations.
4. Build an immutable transfer plan from snapshots. For each source slot and destination slot,
   validate item stacking compatibility and maximum stack bounds, calculate partial or complete
   transfers, and retain expected inventory revision, source slot generations, chest generations,
   chest revisions, and destination slot generations.
5. Prepare every affected owner without mutation. Recheck all prepared tokens on the serialized
   server commit loop. Commit the prepared inventory and chest deltas as one transaction; if any
   revision, generation, access, or capacity check fails, commit none. Ensure no `await`, callbacks,
   or externally supplied code can interleave between final validation and the non-throwing commit
   section.
6. After commit, publish the existing authoritative inventory/chest projections through the
   shared outbound-intent path. Emit packet 85 with the authoritative blocked chest index set using
   the frozen Version4 response timing. Output failure must not roll back an already committed
   transfer; record the committed result and retry/connection handling according to the shared
   outbound contract.

## Dependency graph and batches

The subplan uses a graph strategy because behavior depends on independently owned protocol,
inventory, chest, world-access, and outbound seams. Batches below are sequential at their stated
dependency edges; independent source review can happen concurrently.

| Batch | Dependencies | Work | Completion gate |
| --- | --- | --- | --- |
| A. Evidence and contract freeze | None | Record packet 85 field order and signedness; recover or explicitly bound the range, candidate ordering, smart-stack rule, blocked chest meaning, and reply timing. Confirm which existing inventory/world/chest facts are authoritative. | Master plan records each frozen rule and citation, or explicitly lists the exact unresolved compatibility limit before implementation starts. |
| B. Eligibility and transfer plan | A | Add immutable server-owned candidate/transfer-plan types and a dedicated QuickStack eligibility seam. Re-read inventory slots by authenticated player identity. Keep the algorithm pure over snapshots and policy results. | Given the same snapshots and policies, preparation yields deterministic slot deltas and never mutates owners. |
| C. Cross-owner prepare/commit | B | Add chest prepared mutations and a shared inventory/chest transaction coordinator. Validate all source and destination revisions/generations before any mutation. Keep commit synchronous on the server commit loop. | Injected stale inventory or chest state rejects the whole operation with both owners unchanged; valid preparation commits all deltas once. |
| D. Packet and projection integration | C | Register packet 85 C2S handler with authenticated context, call the transaction coordinator, publish committed authoritative projections, and encode packet 85 S2C blocked chest IDs. Reuse approved Infrastructure wire primitives only. | Packet handling reads no client item data, selects no client-chosen chest, and emits only post-commit owner projections plus the specified response. |
| E. Core verification and evidence | D | Propose SC041 as P04's single core scenario; run it only if the root's frozen global 10% selection includes it. Run the affected project build only when the root session opens its serialized build window. Capture command, exit code, warning/error counts, output path, and scenario output. | If selected, SC041 passes with no partial transfer on its rejection assertions; do not run SC042–SC055 or broad verification. |

## Shared interfaces and ownership

- `InventoryStore` remains the sole writer for player inventory. QuickStack receives an inventory
  snapshot and prepares slot mutations; it does not keep a second item array.
- `ChestStore` remains the sole writer for chest slots. Add a prepare/commit API rather than
  bypassing it or mutating snapshots.
- A shared transaction coordinator owns the atomic boundary across inventory and all affected
  chests. Crafting that touches only chest sources can use `ChestStore.ApplyItemsBatch`; any future
  mixed inventory/chest operation should use the shared coordinator instead of adding a second
  partial commit path.
- A server-owned eligibility policy supplies player position, world scope, range, chest active
  state, lock/block status, and access decisions. It must not expose a broad mutable world API to
  the packet handler.
- Packet 85 DTOs/codecs stay in `Inventory/Protocol`; the handler emits `OutboundPacketIntent`
  values and does not perform transport I/O directly.
- The root session owns `MinimalImplementation.Server.csproj`, shared composition, and shared
  contracts. The implementation agent must provide source paths and required compile includes to
  the root session rather than editing those shared files concurrently.

## Fixed 10% core-test budget

The P04 core inventory contains fifteen scenarios, `SC041`–`SC055`. SC041 is the only P04 scenario
selected by the root's global budget freeze; the remaining scenarios are inventory entries only.
Only SC041 may run for P04. If selected, it drives one framed packet 85 request through the server
handler and asserts all of the following as one scenario:

- the slot list is re-resolved from the authenticated player's server inventory;
- one eligible nearby chest receives the expected bounded transfer, while a non-source or
  ineligible stack remains unchanged;
- source inventory and chest revisions/generations advance together exactly once;
- the packet 85 blocked-chest response has the exact `Int32 + UInt16[]` wire shape and contains
  server-derived indices;
- forced stale state at final preparation leaves both inventory and every chest unchanged and
  emits no success projection.

Do not run SC042–SC055, a full P04 matrix, broad network verification, or the full solution. A
project build is a separate compile gate and must use the root session's serialized build window.
This subplan does not claim SC041 has run or passed.

| Scenario | Core assertion | Status |
| --- | --- | --- |
| SC041 | Packet 85 re-resolves server inventory, transfers to an eligible nearby chest, returns the exact blocked-index response, and leaves all owners unchanged on a stale cross-owner commit. | Selected; run status recorded below. |
| SC042 | Malformed packet 85 count, duplicate/out-of-range slot, truncated body, or trailing bytes is rejected before state access. | Not selected. |
| SC043 | Stale inventory revision or slot generation rejects a transfer without changing any chest. | Not selected. |
| SC044 | A stale revision or generation in any destination chest rejects the whole multi-chest transaction without changing inventory or other chests. | Not selected. |
| SC045 | Empty, favorite, coin, invalid, and maximum-stack source cases preserve item and stack invariants. | Not selected. |
| SC046 | Smart-stack category choice, deterministic destination ordering, and blocked lease reporting match the explicitly configured compatibility policy. | Not selected. |
| SC047 | Chest open/close checks world access, enforces one current lease, and releases only the matching session token. | Not selected. |
| SC048 | Chest slot updates require the current lease and a newer packet sequence; stale or invalid updates preserve chest state. | Not selected. |
| SC049 | Inventory-to-chest and chest-to-inventory transfer paths preserve item counts and reject stale slot generations atomically. | Not selected. |
| SC050 | World-item create/update/remove validates item facts, world scope, and generation before publishing a snapshot. | Not selected. |
| SC051 | Concurrent pickup requests reserve one world item for one player and cannot duplicate the item into two inventories. | Not selected. |
| SC052 | Expired, released, or stale world-item reservations cannot be committed by an old session or generation. | Not selected. |
| SC053 | Module 11 wire decoding preserves the `Int32` material ID, signed chest ID representation, positive 7-bit stack, and bool-only response shape. | Not selected. |
| SC054 | An active authenticated module 11 request consumes only supported materials from server-captured accessible chests and commits once. | Not selected. |
| SC055 | Unsupported requirements, insufficient materials, inaccessible chests, and stale chest state reject without partial consumption and return the selected bool response. | Not selected. |

## Acceptance and handoff

The direction is ready for root acceptance when:

1. The source compiles only from `src/MinimalImplementation` plus approved
   `NSSLC.Infrastructure` APIs; no ECS or other project source references are added.
2. Packet 85 decode and encode preserve the frozen wire shape and reject malformed framing before
   state access.
3. Inventory values and chest destinations are server-derived, and all transfers obey the frozen
   source eligibility, range, stacking, ordering, and blocked-chest rules.
4. Every multi-owner transfer is prepared and committed atomically under the serialized commit
   contract; stale state produces no partial mutation.
5. If the global 10% selection includes P04, only SC041 is run for this direction, with reproducible verification evidence.
6. Handoff reports modified source files, required compile includes, protocol facts, unresolved
   compatibility differences, SC041 command/result, build command/result, and integration risks.

## Current compatibility limits

- The inspected Version4 QuickStack call flow establishes the packet shape and authenticated player
  source, but its selection and transfer methods are empty. The local range is mandatory constructor
  configuration; there is intentionally no guessed default. The host must provide the position
  adapter and choose a radius before enabling packet 85.
- Candidate ordering is nearest chest first, then ascending chest index. A lease held by another
  session marks that destination blocked; same-session leases remain eligible. World-access denial
  excludes a chest from the candidate snapshot. These are explicit local choices pending stronger
  Version4 evidence.
- When the position adapter cannot resolve a valid position for the authenticated player, the
  provider returns no destinations. It never falls back to client coordinates or an unrestricted
  chest scan.

## Source references

- `D:\TRbackup\Version4\Terraria\MessageBuffer.cs`, packet 85 handler.
- `D:\TRbackup\Version4\Terraria\NetMessage.cs`, packet 85 writer dispatch.
- `D:\TRbackup\Version4\Terraria.GameContent\QuickStacking.cs`, declarations and empty method
  bodies in the inspected source copy.
- `src/MinimalImplementation/Inventory/Protocol/QuickStackPayloads.cs` and
  `QuickStackPayloadCodec.cs`.
- `src/MinimalImplementation/Inventory/Rules/ItemValidationPolicy.cs`.
- `src/MinimalImplementation/Inventory/State/InventoryStore.cs`, `ChestStore.cs`,
  `ChestAccessService.cs`, `ChestLeaseStore.cs`, and `InventoryTransactionCoordinator.cs`.

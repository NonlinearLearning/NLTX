# TorchID Definition Boundary

Source oracle: `D:\TRbackup\Version4物理删除了某些文件\Terraria.ID\TorchID.cs`

Source SHA-256: `FFC5A6973D78C5207115746C08917EB6A228FF44010607147FAB39239A5ABEB3`

## Accepted child scope

`TorchID.Initialize` exposes a fixed server-visible definition subset:

- `Count = 24`;
- the 24-position `Dust` mapping;
- `Sets.IsABiomeTorch` membership for IDs `0, 7, 9, 13, 16, 18, 19, 20, 21, 22, 23`.

That subset is owned by `TorchDefinitionRegistry` under
`Terraria.Dome.Simulation.WorldGeneration.Definitions`. The registry is immutable, indexed by
torch ID, and rejects IDs outside `0..23`.

## Explicitly deferred

`TorchColor` and all light providers remain outside this child. The source providers include
client-owned `Main.mouseTextColor` and presentation-dependent behavior (`Demon`, `Disco` and
`Shimmer` providers), so they cannot be imported as server definition facts. Tile placement,
framing, lighting and client rendering remain separate responsibilities.

This card is a narrow M-001 child acceptance. It does not create or imply an aggregate
`Initialize_AlmostEverything` owner. Other unresolved initializer families remain planned or
deferred.

## Verification

- `Test/Terraria.Dome.WorldGeneration.Verification --reduced`: exit `0`, 32 of 81 sections
  executed (39.5%); fixed torch count, first/last definitions, biome flags and invalid IDs passed.
- `MainBoundary`: exit `0`, 769 Simulation source files, 0 violations.
- Scoped `git diff --check`: exit `0`; `progress.md` emitted the normal LF/CRLF warning.

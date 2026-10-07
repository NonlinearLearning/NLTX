# NPC AI coverage verification

Run from the repository root after building this project:

```powershell
dotnet build Test/Terraria.NpcAi.CoverageVerification/Terraria.NpcAi.CoverageVerification.csproj --no-restore --nologo -v:minimal
dotnet run --project Test/Terraria.NpcAi.CoverageVerification/Terraria.NpcAi.CoverageVerification.csproj --no-build --no-restore
```

The default inventory gate validates the exact ordered 0–127 style inventory, profile identity
registrations, finite handler identity gates, and rejection of invalid or premature registrations.
It can pass while every profile still has open work. The JSON `Completion.IsReady` value reports the
separate coverage closure result; inventory success does not establish source behavior equivalence.

For the required coverage completion gate:

```powershell
dotnet run --project Test/Terraria.NpcAi.CoverageVerification/Terraria.NpcAi.CoverageVerification.csproj --no-build --no-restore -- --require-complete --report Build/diagnostics/NpcAiCoverage/completion-report.json
```

`--require-complete` returns exit 2 when any style or registered concrete profile is incomplete and
writes the report before exiting. Every style must have a verified stage, `Profiles=verified`,
`Closure=closed`, and `Verification=passed`. Every runtime registration must be verified, have a
handler, and have no open work. Merely changing ledger text cannot promote runtime registrations.
`IncompleteStyles` and `IncompleteProfiles` identify the remaining entries. Invalid inventories or
registry assertions also fail; they are distinct from expected exit 2 for incomplete coverage.

These checks are necessary coverage evidence. Source-output differential, actual host behavior,
authority/replication, persistence, unload cleanup, and deletion still require their own evidence.
The current finite handler registrations include Blue Slime, Demon Eye, Zombie, Mother Slime, and
Eye of Cthulhu. All remain mapped/open; a finite handler does not imply a complete source profile.

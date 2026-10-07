# NPC finite AI verification

```text
dotnet build Test/Terraria.NpcAi.Verification/Terraria.NpcAi.Verification.csproj --nologo
dotnet run --project Test/Terraria.NpcAi.Verification/Terraria.NpcAi.Verification.csproj --no-build --no-restore
```

This checks the extracted finite simulation rules: center-based target selection, equal-distance
ordering, dead/inactive target gates, repeatable evaluation, the hover/dive boundary, nighttime
home movement and rejection of unsupported content sharing an AI style.

It does not verify the 128 reference AI styles. The full design and coverage boundary are in
[NPC AI redesign](../../docs/system-decomposition/2026-10-05-npc-ai-system-redesign.md).

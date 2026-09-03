using System;
using System.Collections.Generic;
using Terraria.Dome.Simulation.StatusEffects.Definitions;

IReadOnlySet<int> types = LegacyBuffNoSaveRegistry.RegisterDefaults();
if (types.Count != 99 ||
    !LegacyBuffNoSaveRegistry.IsBuffNoSave(20) ||
    !LegacyBuffNoSaveRegistry.IsBuffNoSave(181) ||
    LegacyBuffNoSaveRegistry.IsBuffNoSave(0) ||
    LegacyBuffNoSaveRegistry.IsBuffNoSave(-1) ||
    LegacyBuffNoSaveRegistry.IsBuffNoSave(LegacyBuffNoSaveRegistry.BuffTypeCount))
{
  throw new InvalidOperationException("Buff no-save registry contract diverged.");
}

Console.WriteLine($"PASS: legacy buff-no-save types={types.Count} domain=389");
Console.WriteLine("DEFERRED: complete buff persistence and runtime reset parity remain outside this slice");

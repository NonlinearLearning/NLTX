using System;
using Terraria.Dome.Protocol.V1456.Protocol;

if (LegacyTerrariaVersion.VersionString != "v1.4.5.6" ||
    LegacyTerrariaVersion.AssemblyVersion != "1.4.5.6" ||
    LegacyTerrariaVersion.Release != 319)
{
  throw new InvalidOperationException("Legacy Terraria version contract diverged.");
}

Console.WriteLine("PASS: legacy Terraria version=v1.4.5.6 release=319");
Console.WriteLine("DEFERRED: display strings are not Simulation state or wire-version authority");

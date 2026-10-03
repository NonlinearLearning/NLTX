using System;

namespace NSSLC.WorldGeneration.IO;

/// <summary>
/// Rejects legacy persistence calls. The generation entry point produces in-memory data only.
/// </summary>
internal static class WorldFile
{
  internal static readonly object IOLock = new object();

  internal static void SaveNewWorld()
  {
    throw new NotSupportedException("Saving generated worlds requires the world storage adapter.");
  }

  internal static void SaveWorld(bool resetTime = false, bool useTemps = false, bool canBeSkipped = false)
  {
    throw new NotSupportedException("Saving generated worlds requires the world storage adapter.");
  }

  internal static void SetTempToOngoing()
  {
    throw new NotSupportedException("Generation does not manage world files.");
  }

  internal static void LoadWorld()
  {
    throw new NotSupportedException("Loading worlds requires the world storage adapter.");
  }

  internal static void SetOngoingToTemps()
  {
    throw new NotSupportedException("Generation does not manage world files.");
  }

  internal static void ResetTemps()
  {
    throw new NotSupportedException("Generation does not manage world files.");
  }
}

using System;

namespace NSSLC.WorldGeneration.IO;

/// <summary>
/// Routes world loading through the configured storage host; save operations remain host-owned.
/// </summary>
internal static class WorldFile
{
  internal static readonly object IOLock = new object();
  internal static Exception LastThrownLoadException { get; private set; }

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
    LastThrownLoadException = null;
    WorldGen.loadFailed = false;
    WorldGen.worldBackup = false;
    WorldGen.worldCleared = false;
    try
    {
      if (Main.rand == null)
      {
        Main.rand = new NSSLC.WorldGeneration.Utilities.UnifiedRandom((int)DateTime.Now.Ticks);
      }

      Main.lockMenuBGChange = true;
      Main.checkXMas();
      Main.checkHalloween();
      Main.LoadWorld();
    }
    catch (Exception exception)
    {
      LastThrownLoadException = exception;
      NSSLC.WorldGeneration.WorldGen.loadFailed = true;
    }
  }

  internal static void SetOngoingToTemps()
  {
    throw new NotSupportedException("Generation does not manage world files.");
  }

  internal static void ResetTemps()
  {
    // These are the only WorldFile temporary values represented by the generated runtime.
    Main.anglerWhoFinishedToday.Clear();
    Main.anglerQuestFinished = false;
  }
}

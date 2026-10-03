using System;
using System.Globalization;

namespace NSSLC.WorldGeneration.IO;

/// <summary>Generation metadata. File access belongs to the host's storage adapter.</summary>
public sealed class WorldFileData {
  public int Seed { get; }
  public string SeedText { get; }
  public int WorldId { get; set; }
  public string Path { get; } = "";
  public bool IsCloudSave => false;

  public WorldFileData(string seed) {
    SeedText = seed;
    Seed = TranslateSeed(seed);
  }

  public static int TranslateSeed(string seed) {
    if (int.TryParse(seed, NumberStyles.Integer, CultureInfo.InvariantCulture, out int value)) {
      return value;
    }
    uint hash = 2166136261;
    foreach (char character in seed) {
      hash = unchecked((hash ^ character) * 16777619);
    }
    return (int)(hash & int.MaxValue);
  }

  public string GetFullSeedText(bool allowCropping = false) {
    return SeedText;
  }

  public void SetAsActive() {
    Main.ActiveWorldFileData = this;
  }
}

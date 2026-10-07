using System;
using System.Globalization;

namespace NSSLC.WorldGeneration.IO;

/// <summary>Generation metadata. File access belongs to the host's storage adapter.</summary>
public sealed class WorldFileData {
  public int Seed { get; private set; }
  public string SeedText { get; private set; }
  public int WorldId { get; set; }
  public Guid UniqueId { get; private set; }
  public ulong WorldGeneratorVersion { get; private set; }
  public uint MetadataRevision { get; private set; }
  public bool MetadataIsFavorite { get; private set; }
  public DateTime? CreationTime { get; private set; }
  public DateTime? LastPlayed { get; private set; }
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

  public void ApplyLoadedMetadata(uint revision, bool isFavorite) {
    MetadataRevision = revision;
    MetadataIsFavorite = isFavorite;
  }

  public void ApplyLoadedIdentity(string seedText, int worldId, Guid uniqueId,
      ulong worldGeneratorVersion, DateTime? creationTime, DateTime? lastPlayed) {
    ArgumentNullException.ThrowIfNull(seedText);
    SeedText = seedText;
    Seed = TranslateSeed(seedText);
    WorldId = worldId;
    UniqueId = uniqueId;
    WorldGeneratorVersion = worldGeneratorVersion;
    if (creationTime.HasValue) {
      CreationTime = creationTime;
    }
    if (lastPlayed.HasValue) {
      LastPlayed = lastPlayed;
    }
  }

  public void SetAsActive() {
    Main.ActiveWorldFileData = this;
  }
}

using System;

namespace Terraria.NonAuthoritative.Persistence;

/// <summary>
/// The completion marker written at the end of a pointer-based WorldFile.
/// </summary>
public sealed class WorldFileFooterSection
{
  public const string SectionId = "world.footer";

  public WorldFileFooterSection(bool isComplete, string worldName, int worldId)
  {
    ArgumentNullException.ThrowIfNull(worldName);
    IsComplete = isComplete;
    WorldName = worldName;
    WorldId = worldId;
  }

  public bool IsComplete { get; }

  public string WorldName { get; }

  public int WorldId { get; }
}

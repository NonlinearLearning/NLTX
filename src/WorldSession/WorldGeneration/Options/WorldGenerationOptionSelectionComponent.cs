namespace Terraria.WorldGeneration.Components;

/// <summary>
/// Stores the selected state of one world-generation option for a run.
/// </summary>
public sealed class WorldGenerationOptionSelectionComponent
{
  public WorldGenerationOptionSelectionComponent(
    bool enabled = false,
    bool autoGenEnabled = false)
  {
    Enabled = enabled;
    AutoGenEnabled = autoGenEnabled;
  }

  public bool Enabled { get; private set; }

  public bool AutoGenEnabled { get; private set; }
}

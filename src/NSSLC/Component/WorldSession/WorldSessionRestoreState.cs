using Terraria.WorldGeneration.Components;
using Terraria.WorldSession.Calendar;

namespace Terraria.WorldSession.Components;

public sealed class WorldSessionRestoreState {
  public WorldTileMetricsComponent TileMetrics { get; } = new();
  public TownHousingRegistryComponent TownHousing { get; } = new();
  public WorldDescriptorState Descriptor { get; } = new();
  public WorldRulesState Rules { get; } = new();
  public WorldTimeWeatherState TimeWeather { get; } = new();
  public WorldEventProgressState Progression { get; } = new();
  public WorldAppearanceStateComponent Appearance { get; } = new();
  public WorldNpcHistoryStateComponent History { get; } = new();
  public WorldMilestoneStateComponent Milestones { get; } = new();
  public WorldSeasonPolicyStateComponent SeasonPolicy { get; } = new();
}

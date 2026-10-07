namespace Terraria.WorldGeneration.Adapters;

public interface IDualDungeonDistanceControlPort : IDualDungeonDistanceQuery
{
  new double NormalizedDistanceSafeFromDither { get; set; }
}

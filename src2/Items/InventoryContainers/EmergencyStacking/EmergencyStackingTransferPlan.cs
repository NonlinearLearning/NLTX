namespace Terraria.Items.InventoryContainers;

public sealed class EmergencyStackingTransferPlan
{
  private EmergencyStackingTransferPlan(
    EmergencyStackingCandidateSnapshot source,
    EmergencyStackingCandidateSnapshot destination,
    int distanceOrder,
    int preservationOrder,
    int distance)
  {
    Source = source;
    Destination = destination;
    DistanceOrder = distanceOrder;
    PreservationOrder = preservationOrder;
    Distance = distance;
  }

  public EmergencyStackingCandidateSnapshot Source { get; }

  public EmergencyStackingCandidateSnapshot Destination { get; }

  public int DistanceOrder { get; }

  public int PreservationOrder { get; }

  public int Distance { get; }

  public static EmergencyStackingTransferPlan Create(
    EmergencyStackingCandidateSnapshot source,
    EmergencyStackingCandidateSnapshot destination,
    int distanceOrder,
    int preservationOrder,
    int distance)
  {
    ArgumentNullException.ThrowIfNull(source);
    ArgumentNullException.ThrowIfNull(destination);
    if (distanceOrder < 0 || preservationOrder < 0 || distance < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(distance));
    }

    return new EmergencyStackingTransferPlan(
      source,
      destination,
      distanceOrder,
      preservationOrder,
      distance);
  }
}

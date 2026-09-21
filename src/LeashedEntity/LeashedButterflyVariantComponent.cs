namespace Terraria.LeashedEntity;

/// <summary>
/// Stores the per-entity normal butterfly variant.
/// status: implemented
/// componentOwner: LeashedEntitySimulation
/// crossSubsystemOwner: integration-review
/// </summary>
public struct LeashedButterflyVariantComponent
{
  /// <summary>
  /// Item/style-selected butterfly variant. The accepted range is validated by the future
  /// content/network boundary and is not inferred by this passive component.
  /// </summary>
  public byte Variant;

  public LeashedButterflyVariantComponent(byte variant)
  {
    Variant = variant;
  }
}

namespace Terraria.Share.Liquid;

/// <summary>Declares which liquid contacts affect an entity.</summary>
public readonly record struct LiquidInteractionCapabilityComponent(
    bool Enabled,
    LiquidContactFlags AffectedLiquids)
{
  public bool Affects(LiquidContactFlags liquid)
  {
    return Enabled && (AffectedLiquids & liquid) != LiquidContactFlags.None;
  }
}

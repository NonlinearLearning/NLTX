namespace Terraria.Physics;

/// <summary>Supplies the current authoritative world revision to a commit adapter.</summary>
public interface ISpatialWorldRevisionSource
{
  SpatialWorldRevision Current { get; }
}

using System.Numerics;

namespace Terraria.Npc;

/// <summary>
/// Effect boundary for the source Guide return-home slice. The profile computes
/// the decision; the caller owns entity, network, sitting, and housing writes.
/// </summary>
public interface INpcGuideSourceEffectPort
{
  void ApplyHomeTeleport(Vector2 position, Vector2 velocity);

  void RequestNetworkSync();

  bool TryForceSitting(in NpcGuideForceSittingRequest request);

  void MarkHomeless();

  void RequestQuickFindHome();
}

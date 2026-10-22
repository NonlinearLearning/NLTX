using MinimalImplementation.Contracts;
using Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

namespace MinimalImplementation.Server.Modules.Effects;

internal static class Packet82PresentationValidation
{
  internal const int MaximumRecipientCount = byte.MaxValue;
  internal const int ParticleTypeCount = 69;
  private const float MaximumWorldCoordinateMagnitude = 1_000_000f;
  private const float MaximumMovementComponentMagnitude = 100_000f;

  internal static bool IsValidParticleData(Packet82ParticlesData? particleData)
  {
    if (particleData?.Settings is not Packet82ParticleSettings settings
      || particleData.ParticleType >= ParticleTypeCount)
    {
      return false;
    }

    return IsBoundedFinite(settings.PositionX, MaximumWorldCoordinateMagnitude)
      && IsBoundedFinite(settings.PositionY, MaximumWorldCoordinateMagnitude)
      && IsBoundedFinite(settings.MovementX, MaximumMovementComponentMagnitude)
      && IsBoundedFinite(settings.MovementY, MaximumMovementComponentMagnitude);
  }

  internal static void ValidateTargets(
    IReadOnlyCollection<SessionEpoch>? targets,
    CommitVersion commitVersion)
  {
    ArgumentNullException.ThrowIfNull(targets);
    if (targets.Count == 0)
    {
      throw new ArgumentException(
        "A presentation intent must have at least one target.",
        nameof(targets));
    }

    if (targets.Count > MaximumRecipientCount)
    {
      throw new ArgumentOutOfRangeException(
        nameof(targets),
        "A presentation intent cannot exceed the Version4 player-slot limit.");
    }

    if (!commitVersion.IsValid)
    {
      throw new ArgumentOutOfRangeException(nameof(commitVersion));
    }

    HashSet<SessionEpoch> uniqueTargets = new();
    foreach (SessionEpoch target in targets)
    {
      if (!target.IsValid || !uniqueTargets.Add(target))
      {
        throw new ArgumentException(
          "Presentation targets must be valid and unique session epochs.",
          nameof(targets));
      }
    }
  }

  private static bool IsBoundedFinite(float value, float magnitudeLimit)
  {
    return float.IsFinite(value) && MathF.Abs(value) <= magnitudeLimit;
  }
}

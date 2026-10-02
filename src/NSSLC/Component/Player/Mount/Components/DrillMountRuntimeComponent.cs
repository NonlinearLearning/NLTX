using System.Numerics;

namespace Terraria.Player.Mount;

public sealed class DrillMountRuntimeComponent
{
  public const int BeamCapacity = 8;

  private readonly MountDrillBeamState[] _beamStates;
  private readonly IReadOnlyList<MountDrillBeamState> _beamStateView;

  public DrillMountRuntimeComponent()
  {
    _beamStates = new MountDrillBeamState[BeamCapacity];
    _beamStateView = Array.AsReadOnly(_beamStates);
    for (int i = 0; i < _beamStates.Length; i++)
    {
      _beamStates[i] = MountDrillBeamState.Empty;
    }

    DiodeRotationTarget = 0f;
    DiodeRotation = 0f;
    OuterRingRotation = 0f;
    BeamCooldownTicks = 0;
    CrosshairPosition = Vector2.Zero;
  }

  public IReadOnlyList<MountDrillBeamState> BeamStates => _beamStateView;

  public float DiodeRotationTarget { get; internal set; }

  public float DiodeRotation { get; internal set; }

  public float OuterRingRotation { get; internal set; }

  public int BeamCooldownTicks { get; internal set; }

  public Vector2 CrosshairPosition { get; internal set; }

  internal MountDrillBeamState[] MutableBeamStates => _beamStates;

  internal void Reset()
  {
    DiodeRotationTarget = 0f;
    DiodeRotation = 0f;
    OuterRingRotation = 0f;
    BeamCooldownTicks = 0;
    CrosshairPosition = Vector2.Zero;
    for (int i = 0; i < _beamStates.Length; i++)
    {
      _beamStates[i] = MountDrillBeamState.Empty;
    }
  }

  public readonly record struct MountDrillBeamState(
    DrillTileTarget? Target,
    int CooldownTicks,
    DrillBeamPurpose Purpose)
  {
    public static MountDrillBeamState Empty => new(null, 0, DrillBeamPurpose.Block);
  }

  public readonly record struct DrillTileTarget(int X, int Y);

  public enum DrillBeamPurpose
  {
    Unknown = -1,
    Block = 0,
    Wall = 1,
  }
}

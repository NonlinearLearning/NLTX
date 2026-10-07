using System.Numerics;

namespace Terraria.Player.Mount;

/// <summary>
/// 保存钻机坐骑的光束、旋转、冷却和准星状态。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.Mount.DrillMountData。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Mount.cs。</para>
/// <para>
/// 主要源成员：diodeRotationTarget（第 38 行）； diodeRotation（第 40 行）； outerRingRotation（第 42 行）； beams（第
/// 44 行）； beamCooldown（第 46 行）； crosshairPosition（第 48 行）。
/// </para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>拆分依据文件：2026-09-11-version4-P03-mount-vehicle-component-design.md。</para>
/// <para>依据位置：第 95 行。</para>
/// </remarks>
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

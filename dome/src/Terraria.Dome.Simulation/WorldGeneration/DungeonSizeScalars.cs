using System;

namespace Terraria.Dome.Simulation.WorldGeneration;

public readonly struct DungeonSizeScalars
{
  public DungeonSizeScalars(
    double hallStrengthScalar,
    double hallStepScalar,
    double roomStrengthScalar,
    double roomStepScalar)
  {
    if (!double.IsFinite(hallStrengthScalar) ||
        !double.IsFinite(hallStepScalar) ||
        !double.IsFinite(roomStrengthScalar) ||
        !double.IsFinite(roomStepScalar))
    {
      throw new ArgumentOutOfRangeException("scalar");
    }

    HallStrengthScalar = hallStrengthScalar;
    HallStepScalar = hallStepScalar;
    RoomStrengthScalar = roomStrengthScalar;
    RoomStepScalar = roomStepScalar;
  }

  public double HallStrengthScalar { get; }

  public double HallStepScalar { get; }

  public double RoomStrengthScalar { get; }

  public double RoomStepScalar { get; }

  public double HallSizeScalar => (HallStrengthScalar + HallStepScalar) / 2.0;

  public double RoomSizeScalar => (RoomStrengthScalar + RoomStepScalar) / 2.0;
}

using System;

namespace Terraria.WorldStorage;

public readonly struct WorldLoadPrepareResult<TPrepared>
  where TPrepared : notnull
{
  private readonly TPrepared _preparedData;

  private WorldLoadPrepareResult(
    bool isPrepared,
    TPrepared preparedData,
    WorldLoadApiFailure failure)
  {
    IsPrepared = isPrepared;
    _preparedData = preparedData;
    Failure = failure;
  }

  public bool IsPrepared { get; }

  public TPrepared PreparedData => IsPrepared
    ? _preparedData
    : throw new InvalidOperationException("The load input was not prepared.");

  public WorldLoadApiFailure Failure { get; }

  public static WorldLoadPrepareResult<TPrepared> Prepared(TPrepared preparedData)
  {
    ArgumentNullException.ThrowIfNull(preparedData);
    return new WorldLoadPrepareResult<TPrepared>(
      isPrepared: true,
      preparedData,
      default);
  }

  public static WorldLoadPrepareResult<TPrepared> Rejected(
    WorldLoadApiFailure failure)
  {
    if (!failure.IsValid)
    {
      throw new ArgumentException("A rejected load result requires a valid failure.", nameof(failure));
    }

    return new WorldLoadPrepareResult<TPrepared>(
      isPrepared: false,
      default!,
      failure);
  }
}

using System;

namespace Terraria.WorldStorage;

public readonly record struct WorldLoadApiFailure
{
  private WorldLoadApiFailure(string code, string message)
  {
    Code = code;
    Message = message;
  }

  public string Code { get; }

  public string Message { get; }

  public bool IsValid =>
    !string.IsNullOrWhiteSpace(Code) && !string.IsNullOrWhiteSpace(Message);

  public static WorldLoadApiFailure Create(string code, string message)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(code);
    ArgumentException.ThrowIfNullOrWhiteSpace(message);
    return new WorldLoadApiFailure(code, message);
  }
}

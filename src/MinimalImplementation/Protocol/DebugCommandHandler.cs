using Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

namespace MinimalImplementation.Server.Protocol;

public sealed record DebugCommandState(bool ReportCommandUsage, int ServerPing);

public enum DebugCommandApplyResult
{
  Disabled,
  Applied,
  UnknownCommand,
  InvalidValue
}

/// <summary>Applies the two Version4 debug controls without touching domain state or sockets.</summary>
public sealed class DebugCommandHandler
{
  public DebugCommandHandler(bool enableDebugCommands, DebugCommandState? initialState = null)
  {
    State = initialState ?? new DebugCommandState(true, 0);
    EnableDebugCommands = enableDebugCommands;
  }

  public bool EnableDebugCommands { get; }

  public DebugCommandState State { get; private set; }

  public DebugCommandApplyResult Apply(DevCommandsPacket command)
  {
    ArgumentNullException.ThrowIfNull(command);
    if (!EnableDebugCommands)
    {
      return DebugCommandApplyResult.Disabled;
    }

    if (!float.IsFinite(command.Value)
        || !float.IsFinite(command.TrailingValue))
    {
      return DebugCommandApplyResult.InvalidValue;
    }

    switch (command.Command)
    {
      case "/showdebug" when IsSafeInt32Conversion(command.Value):
        State = State with { ReportCommandUsage = (int)command.Value == 1 };
        return DebugCommandApplyResult.Applied;
      case "/setserverping" when command.Value is >= 0 and < 2147483648f:
        State = State with { ServerPing = (int)command.Value };
        return DebugCommandApplyResult.Applied;
      case "/showdebug":
      case "/setserverping":
        return DebugCommandApplyResult.InvalidValue;
      default:
        return DebugCommandApplyResult.UnknownCommand;
    }
  }

  private static bool IsSafeInt32Conversion(float value)
  {
    return value >= int.MinValue && value < 2147483648f;
  }
}

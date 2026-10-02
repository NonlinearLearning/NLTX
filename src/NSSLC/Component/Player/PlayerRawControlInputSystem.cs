using System;
using System.Collections.Generic;

namespace Terraria.Player;

public static class PlayerRawControlInputSystem
{
  public static bool TryApply(
    in PlayerInputCommand command,
    bool isTargetActive,
    ref PlayerRawControlInputComponent component,
    out PlayerInputRejectionReason rejectionReason)
  {
    if (!TryValidate(command, isTargetActive, out rejectionReason))
    {
      return false;
    }

    component = CreateComponent(command);
    rejectionReason = PlayerInputRejectionReason.None;
    return true;
  }

  public static bool TryApplyBatch(
    IReadOnlyList<PlayerInputCommand> commands,
    IReadOnlyDictionary<LegacyPlayerSlot, bool> activePlayers,
    IDictionary<LegacyPlayerSlot, PlayerRawControlInputComponent> components,
    out PlayerInputRejectionReason rejectionReason)
  {
    ArgumentNullException.ThrowIfNull(commands);
    ArgumentNullException.ThrowIfNull(activePlayers);
    ArgumentNullException.ThrowIfNull(components);

    Dictionary<LegacyPlayerSlot, PlayerRawControlInputComponent> pending =
      new(commands.Count);
    HashSet<LegacyPlayerSlot> seenPlayers = new(commands.Count);

    foreach (PlayerInputCommand command in commands)
    {
      if (!seenPlayers.Add(command.Player))
      {
        rejectionReason = PlayerInputRejectionReason.DuplicatePlayer;
        return false;
      }

      if (command.Player.Value < 0)
      {
        rejectionReason = PlayerInputRejectionReason.InvalidPlayerSlot;
        return false;
      }

      if (!activePlayers.TryGetValue(command.Player, out bool isTargetActive))
      {
        rejectionReason = PlayerInputRejectionReason.UnknownPlayer;
        return false;
      }

      if (!components.ContainsKey(command.Player))
      {
        rejectionReason = PlayerInputRejectionReason.UnknownPlayer;
        return false;
      }

      if (!TryValidate(command, isTargetActive, out rejectionReason))
      {
        return false;
      }

      pending.Add(command.Player, CreateComponent(command));
    }

    foreach (KeyValuePair<LegacyPlayerSlot, PlayerRawControlInputComponent> update in pending)
    {
      components[update.Key] = update.Value;
    }

    rejectionReason = PlayerInputRejectionReason.None;
    return true;
  }

  private static bool TryValidate(
    in PlayerInputCommand command,
    bool isTargetActive,
    out PlayerInputRejectionReason rejectionReason)
  {
    if (command.Player.Value < 0)
    {
      rejectionReason = PlayerInputRejectionReason.InvalidPlayerSlot;
      return false;
    }

    if (!isTargetActive)
    {
      rejectionReason = PlayerInputRejectionReason.InactivePlayer;
      return false;
    }

    rejectionReason = PlayerInputRejectionReason.None;
    return true;
  }

  private static PlayerRawControlInputComponent CreateComponent(
    in PlayerInputCommand command)
  {
    return new PlayerRawControlInputComponent
    {
      ControlLeft = command.ControlLeft,
      ControlRight = command.ControlRight,
      ControlUp = command.ControlUp,
      ControlDown = command.ControlDown,
      ControlJump = command.ControlJump,
      ControlTorch = command.ControlTorch,
      ControlDash = command.ControlDash,
      ControlDownHold = command.ControlDownHold,
    };
  }
}

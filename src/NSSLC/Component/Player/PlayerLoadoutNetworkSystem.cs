using System.IO;

namespace Terraria.Player;

// Composes the packet-147 order without letting the adapter write loadout state.
public sealed class PlayerLoadoutNetworkSystem
{
  private readonly PlayerLoadoutSystem _loadoutSystem;
  private readonly PlayerAccessoryVisibilitySystem _visibilitySystem;

  public PlayerLoadoutNetworkSystem(
    PlayerLoadoutSystem loadoutSystem,
    PlayerAccessoryVisibilitySystem visibilitySystem)
  {
    ArgumentNullException.ThrowIfNull(loadoutSystem);
    ArgumentNullException.ThrowIfNull(visibilitySystem);

    _loadoutSystem = loadoutSystem;
    _visibilitySystem = visibilitySystem;
  }

  public PlayerLoadoutNetworkResult Process(
    in PlayerLoadoutNetworkRequest request)
  {
    PlayerLoadoutSwitchResult loadoutResult = _loadoutSystem.Switch(
      new PlayerLoadoutSwitchCommand(
        request.CommandId,
        request.TargetLoadoutIndex,
        request.AuthorityPlayerIndex,
        request.MainPlayerIndex,
        request.UsingOrReusingItem,
        request.CCed,
        request.Dead));

    // Version4 reads and applies the mask after the void-returning switch call.
    PlayerAccessoryVisibilityResult visibilityResult =
      _visibilitySystem.Apply(
        new PlayerAccessoryVisibilityApplyCommand(
          request.CommandId,
          request.VisibilityMask));

    return new PlayerLoadoutNetworkResult(
      request.PacketPlayerIndex,
      request.AuthorityPlayerIndex,
      loadoutResult,
      visibilityResult,
      VisibilityAttemptedAfterLoadout: true);
  }

  public PlayerLoadoutPacket147ProcessResult ProcessPacket147(
    BinaryReader reader,
    Guid commandId,
    int authorityPlayerIndex,
    int mainPlayerIndex,
    bool usingOrReusingItem,
    bool cced,
    bool dead)
  {
    if (!PlayerLoadoutPacket147Adapter.TryReadHeader(
      reader,
      commandId,
      authorityPlayerIndex,
      mainPlayerIndex,
      usingOrReusingItem,
      cced,
      dead,
      out PlayerLoadoutPacket147Header header))
    {
      return new PlayerLoadoutPacket147ProcessResult(
        PlayerLoadoutPacket147ProcessStatus.HeaderTruncated,
        null,
        default,
        default,
        VisibilityReadAfterLoadout: false);
    }

    PlayerLoadoutSwitchResult loadoutResult =
      _loadoutSystem.Switch(header.ToSwitchCommand());
    if (!PlayerLoadoutPacket147Adapter.TryReadVisibilityMask(
      reader,
      out ushort visibilityMask))
    {
      return new PlayerLoadoutPacket147ProcessResult(
        PlayerLoadoutPacket147ProcessStatus.VisibilityMaskTruncated,
        header,
        loadoutResult,
        default,
        VisibilityReadAfterLoadout: true);
    }

    PlayerAccessoryVisibilityResult visibilityResult =
      _visibilitySystem.Apply(
        new PlayerAccessoryVisibilityApplyCommand(
          commandId,
          visibilityMask));
    return new PlayerLoadoutPacket147ProcessResult(
      PlayerLoadoutPacket147ProcessStatus.Applied,
      header,
      loadoutResult,
      visibilityResult,
      VisibilityReadAfterLoadout: true);
  }

  public void ResetForLifecycle()
  {
    _loadoutSystem.ResetForLifecycle();
    _visibilitySystem.ResetForLifecycle();
  }
}

namespace Terraria.Teleportation;

public interface ITeleportCommitPort
{
  TeleportCommitPortResult Commit(
    in TeleportTransitionRequest request,
    in TeleportTransitionSnapshot snapshot);
}

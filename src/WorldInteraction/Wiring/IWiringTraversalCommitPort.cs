namespace Terraria.WorldInteraction.Wiring;

public interface IWiringTraversalCommitPort
{
  WiringTraversalCommitResult CommitPump(
    in PumpTransferCommand command);

  WiringTraversalCommitResult CommitTeleport(
    in WiringTeleportCommand command);
}

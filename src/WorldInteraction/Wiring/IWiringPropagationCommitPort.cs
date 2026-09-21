namespace Terraria.WorldInteraction.Wiring;

public interface IWiringPropagationCommitPort
{
  WiringPropagationCommitResult Commit(
    in WiringPropagationCommand command);

  WiringPropagationSnapshot Snapshot();
}

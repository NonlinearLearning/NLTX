namespace Terraria.Player.Progression;

public interface IPlayerProgressionPacketProjection
{
  PlayerProgressionPacketFlags Project(
    PlayerUnlockProgressionLedgerComponent unlockProgression,
    PlayerConsumedProgressionLedgerComponent consumedProgression);
}

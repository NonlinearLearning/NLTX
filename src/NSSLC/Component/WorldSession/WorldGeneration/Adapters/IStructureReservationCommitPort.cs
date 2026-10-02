namespace Terraria.WorldGeneration.Adapters;

public interface IStructureReservationCommitPort
{
  ReservationResult Reserve(StructureReservationIntent intent);

  bool CanPlace(StructureReservationIntent intent);

  StructureReservationSnapshot CreateSnapshot();
}

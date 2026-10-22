namespace NSSLC.WorldGeneration.Runtime.Housing;

public readonly record struct HousingRoomRequirementResult(
  bool HasTorch,
  bool HasDoor,
  bool HasChair,
  bool HasTable,
  bool CanSpawn);

namespace NSSLC.WorldGeneration.Runtime.Housing;

public readonly record struct HousingRoomScoreCandidate(
  int X,
  int Y,
  int Score,
  bool IsNewHighScore);

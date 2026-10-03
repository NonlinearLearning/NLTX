namespace NSSLC.WorldGeneration;

public enum GeneratedWorldSize {
  Small,
  Medium,
  Large
}

public enum GeneratedWorldEvil {
  Random = -1,
  Corruption = 0,
  Crimson = 1
}

public sealed record WorldCreationRequest(
  string Seed,
  GeneratedWorldSize Size = GeneratedWorldSize.Small,
  GeneratedWorldEvil Evil = GeneratedWorldEvil.Corruption,
  int Difficulty = 0);

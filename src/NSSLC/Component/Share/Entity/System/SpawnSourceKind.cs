namespace EntityEcs.Components;

public enum SpawnSourceKind : byte
{
  Unknown,
  Direct,
  Statue,
  Replacement,
  DespawnReplacement,
  External,
}

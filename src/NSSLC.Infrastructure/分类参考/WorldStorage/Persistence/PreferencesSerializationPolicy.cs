namespace Terraria.NonAuthoritative.Persistence;

public readonly record struct PreferencesSerializationPolicy(
  bool ParseAllTypes,
  PreferencesSerializationFormat Format);

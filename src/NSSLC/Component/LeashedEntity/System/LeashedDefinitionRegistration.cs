namespace Terraria.LeashedEntity;

public readonly record struct LeashedDefinitionRegistration(
  string Key,
  LeashedDefinitionKind Kind,
  int? ContentId);


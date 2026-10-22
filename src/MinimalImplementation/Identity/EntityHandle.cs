using MinimalImplementation.Contracts;

namespace MinimalImplementation.Server.Identity;

public readonly record struct EntityHandle(
  EntityUuid Id,
  WorldScopeId WorldScope,
  EntityGeneration Generation);

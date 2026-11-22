using ArchEntity = Arch.Core.Entity;

namespace Terraria.Relationships;

/// <summary>Resolves a generation-scoped runtime handle at a local commit boundary.</summary>
public interface IEntityRuntimeResolver
{
  bool TryResolve(EntityRuntimeHandle handle, out ArchEntity entity);
}

using ArchEntity = Arch.Core.Entity;

namespace Terraria.Share.Relationships;

/// <summary>Resolves a live Arch entity to its world-scoped runtime handle.</summary>
public interface IEntityRuntimeHandleResolver
{
  bool TryGetHandle(in ArchEntity entity, out EntityRuntimeHandle handle);
}

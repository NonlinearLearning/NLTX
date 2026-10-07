using Arch.Core;
using Arch.Relationships;

namespace Terraria.Arch.Verification;

internal struct ParentRelation {
  public int Weight;
}

internal static class RelationshipsApiCompileProbe {
  private static void CompileRelationshipSurface() {
    World world = World.Create();
    Entity source = world.Create();
    Entity target = world.Create();
    world.AddRelationship(source, target, new ParentRelation { Weight = 1 });
    world.SetRelationship(source, target, new ParentRelation { Weight = 2 });
    bool has = world.HasRelationship<ParentRelation>(source, target);
    bool hasAny = world.HasRelationship<ParentRelation>(source);
    ParentRelation value = world.GetRelationship<ParentRelation>(source, target);
    bool found = world.TryGetRelationship(source, target, out ParentRelation copied);
    ref Relationship<ParentRelation> relationships = ref world.GetRelationships<ParentRelation>(source);
    relationships.Set(target, value);
    world.RemoveRelationship<ParentRelation>(source, target);
    _ = has && hasAny && found;
    world.Destroy(source);
    world.Destroy(target);
    World.Destroy(world);
  }

  private static void Main() { }
}


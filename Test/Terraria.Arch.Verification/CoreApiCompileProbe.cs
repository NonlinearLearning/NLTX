using Arch.Buffer;
using Arch.Core;

namespace Terraria.Arch.Verification;

internal static class CoreApiCompileProbe {
  private sealed class ReferenceComponent {
    public int Value { get; set; }
  }

  private struct ValueComponent {
    public int Value;
  }

  private struct MarkerComponent {
  }

  private static void CompileCoreSurface() {
    World world = World.Create();
    Entity entity = world.Create(new[] {
      (ComponentType)typeof(ReferenceComponent),
      (ComponentType)typeof(ValueComponent),
    });

    int worldId = entity.WorldId;
    int entityId = entity.Id;
    int version = entity.Version;
    _ = worldId + entityId + version;

    bool alive = world.IsAlive(entity);
    bool hasReference = world.Has<ReferenceComponent>(entity);
    ReferenceComponent reference = world.Get<ReferenceComponent>(entity);
    world.Set(entity, new ReferenceComponent { Value = reference.Value + 1 });
    world.Add(entity, new MarkerComponent());
    world.Remove<MarkerComponent>(entity);

    bool hasValue = world.TryGet<ValueComponent>(entity, out ValueComponent value);
    ref ValueComponent valueRef = ref world.TryGetRef<ValueComponent>(entity, out bool valueExists);
    if (valueExists) {
      valueRef.Value = value.Value + 1;
    }

    QueryDescription query = new QueryDescription()
      .WithAll<ReferenceComponent>()
      .WithAny<ValueComponent>()
      .WithNone<MarkerComponent>()
      .WithExclusive<ReferenceComponent, ValueComponent>();
    query.Build();
    world.Query(query, (Entity current) => {
      ReferenceComponent currentReference = world.Get<ReferenceComponent>(current);
      _ = currentReference.Value;
    });

    _ = alive && hasReference && hasValue;
    world.Destroy(entity);
    World.Destroy(world);
  }
}

# PUA L2: CommandBuffer group-order failure review

## Exact failed run output
Command: dotnet run --project Test/Terraria.Arch.Verification/Terraria.Arch.Verification.csproj --no-build --no-restore -- --case world.id-reuse --case world.isalive-worldid-boundary --case component.struct-class-access-critical --case query.composition-critical --case command-buffer.create-and-groups-critical
Started: 2026-10-08T02:53:12.8957833+08:00
Finished: 2026-10-08T02:53:14.1185307+08:00
ExitCode: 1

PASS world.id-reuse
PASS world.isalive-worldid-boundary
PASS component.struct-class-access-critical
PASS query.composition-critical
FAIL command-buffer.create-and-groups-critical: System.InvalidOperationException: Grouped Add then Set uses the Set value.
   at Terraria.Arch.Verification.Program.Assert(Boolean condition, String message) in C:\Users\shan\.codex\worktrees\32f2\NLTX\Test\Terraria.Arch.Verification\Program.cs:line 659
   at Terraria.Arch.Verification.Program.VerifyCriticalCommandBufferBehavior() in C:\Users\shan\.codex\worktrees\32f2\NLTX\Test\Terraria.Arch.Verification\Program.cs:line 596
   at Terraria.Arch.Verification.Program.Main(String[] args) in C:\Users\shan\.codex\worktrees\32f2\NLTX\Test\Terraria.Arch.Verification\Program.cs:line 89

## Failing assertion source context (lines 551-611; 61 lines)
    Console.WriteLine(exception is null
      ? "OBSERVE destroyed target: Playback returned without an exception."
      : $"OBSERVE destroyed target: Playback threw {exception.GetType().FullName}.");
    Assert(!world.IsAlive(entity), "The destroyed target remains not alive.");
  }

  private static void VerifyPlaybackKeepsEarlierEffectsOnFailure()
  {
    using var world = World.Create();
    using var buffer = new CommandBuffer();
    var addTarget = world.Create();
    var missingComponentTarget = world.Create();
    buffer.Set(missingComponentTarget, new ProbePositionComponent(9));
    buffer.Add(addTarget, new ProbePositionComponent(5));

    var exception = CaptureException(() => buffer.Playback(world));
    Assert(exception is not null, "Set to a missing component causes a playback exception.");
    Assert(world.Has<ProbePositionComponent>(addTarget), "The earlier Add remains applied after failure.");
    Assert(world.Get<ProbePositionComponent>(addTarget).Value == 5, "The partial Add value remains visible.");
  }

  private static void VerifyBufferedSetDoesNotAdd()
  {
    using var world = World.Create();
    using var buffer = new CommandBuffer();
    var entity = world.Create();
    buffer.Set(entity, new ProbePositionComponent(1));
    var exception = CaptureException(() => buffer.Playback(world));
    Assert(exception is not null, "Set does not add an absent component.");
    Assert(!world.Has<ProbePositionComponent>(entity), "Set failure leaves the absent component absent.");
  }

  private static void VerifyCriticalCommandBufferBehavior()
  {
    using var world = World.Create();
    using var buffer = new CommandBuffer();
    var staged = buffer.Create(ComponentTypesForPositionAndVelocity());
    var existing = world.Create();
    buffer.Set(existing, new ProbePositionComponent(1));
    buffer.Add(existing, new ProbePositionComponent(2));

    Assert(staged.Id < 0, "Create returns a staged negative-id Entity.");
    Assert(world.CountEntities(QueryForPositionAndVelocity()) == 0, "Staged creation is not immediately visible.");
    buffer.Playback(world);
    Assert(world.CountEntities(QueryForPositionAndVelocity()) == 1, "Playback creates the staged component signature.");
    Assert(world.Get<ProbePositionComponent>(existing).Value == 1, "Grouped Add then Set uses the Set value.");
  }

  private static World CreateQueryWorld()
  {
    var world = World.Create();
    var positionAndVelocity = world.Create();
    world.Add(positionAndVelocity, new ProbePositionComponent(1));
    world.Add(positionAndVelocity, new ProbeVelocityComponent(1));

    var positionOnly = world.Create();
    world.Add(positionOnly, new ProbePositionComponent(2));

    var velocityAndMarker = world.Create();
    world.Add(velocityAndMarker, new ProbeVelocityComponent(3));
    world.Add(velocityAndMarker, new ProbeMarkerComponent());
## Pinned CommandBuffer source context (Create/Set/Add/Playback grouping)
    {
        var entityIndex = BufferedEntityInfo[entity.Id].Index;
        return Entities[entityIndex];
    }

    /// <summary>
    ///     Records a Create operation for an <see cref="Entity"/> based on its component structure.
    ///     Will be created during <see cref="Playback"/>.
    /// </summary>
    /// <param name="types">The <see cref="Entity"/>'s component structure/<see cref="Archetype"/>.</param>
    /// <returns>The buffered <see cref="Entity"/> with an index of <c>-1</c>.</returns>

    public Entity Create(ComponentType[] types)
    {
        lock (this)
        {
            var entity = new Entity(-(Size + 1), -1);
            Register(entity, out _);

            var command = new CreateCommand(Size - 1, types);
            Creates.Add(command);

            return entity;
        }
    }

    /// <summary>
    ///     Record a Destroy operation for an (buffered) <see cref="Entity"/>.
    ///     Will be destroyed during <see cref="Playback"/>.
    /// </summary>
    /// <param name="entity">The <see cref="Entity"/> to destroy.</param>

    public void Destroy(in Entity entity)
    {
        lock (this)
        {
            if (!BufferedEntityInfo.TryGetValue(entity.Id, out var info))
            {
                Register(entity, out info);
            }

            Destroys.Add(info.Index);
        }
    }

    /// <summary>
    ///     Records a set operation for an (buffered) <see cref="Entity"/>.
    ///     Overwrites previous values.
    ///     Will be set during <see cref="Playback"/>.
    /// </summary>
    /// <typeparam name="T">The component type.</typeparam>
    /// <param name="entity">The <see cref="Entity"/>.</param>
    /// <param name="component">The component value.</param>

    public void Set<T>(in Entity entity, in T? component = default)
    {
        BufferedEntityInfo info;
        lock (this)
        {
            if (!BufferedEntityInfo.TryGetValue(entity.Id, out info))
            {
                Register(entity, out info);
            }
        }

        Sets.Set(info.SetIndex, in component);
    }

    /// <summary>
    ///     Records a add operation for an (buffered) <see cref="Entity"/>.
    ///     Overwrites previous values.
    ///     Will be added during <see cref="Playback"/>.
    /// </summary>
    /// <typeparam name="T">The component type.</typeparam>
    /// <param name="entity">The <see cref="Entity"/>.</param>
    /// <param name="component">The component value.</param>

    public void Add<T>(in Entity entity, in T? component = default)
    {
        BufferedEntityInfo info;
        lock (this)
        {
            if (!BufferedEntityInfo.TryGetValue(entity.Id, out info))
            {
                Register(entity, out info);
            }
        }

        Adds.Set<T>(info.AddIndex);
        Sets.Set(info.SetIndex, in component);
    }

    /// <summary>
    ///     Records a remove operation for an (buffered) <see cref="Entity"/>.
    ///     Will be removed during <see cref="Playback"/>.
    /// </summary>
    /// <typeparam name="T">The component type.</typeparam>
    /// <param name="entity">The <see cref="Entity"/>.</param>

    public void Remove<T>(in Entity entity)
    {
        BufferedEntityInfo info;
        lock (this)
        {
            if (!BufferedEntityInfo.TryGetValue(entity.Id, out info))
            {
                Register(entity, out info);
            }
        }

        Removes.Set<T>(info.RemoveIndex);
    }

    /// <summary>
    ///     Plays back all recorded commands, modifying the world.
    /// </summary>
    /// <remarks>
    ///     This operation should only happen on the main thread.
    /// </remarks>
    /// <param name="world">The <see cref="World"/> where the commands will be playbacked too.</param>
    /// <param name="dispose">If true it will clear the recorded operations after they were playbacked, if not they will stay.</param>

    public void Playback(World world, bool dispose = true)
    {
        // Create recorded entities.
        foreach (var cmd in Creates)
        {
            var entity = world.Create(cmd.Types);
            Entities[cmd.Index] = entity;
        }

        // Play back additions.
        for (var index = 0; index < Adds.Count; index++)
        {
            var wrappedEntity = Adds.Entities[index];
            for (var i = 0; i < Adds.UsedSize; i++)
            {
                var usedIndex = Adds.Used[i];
                var sparseSet = Adds.Components[usedIndex];

                if (!sparseSet.Contains(wrappedEntity.Index))
                {
                    continue;
                }

                _addTypes.Add(sparseSet.Type);
            }

            if (_addTypes.Count <= 0)
            {
                continue;
            }

            // Resolves the entity to get the real one (e.g. for newly created negative entities and stuff).
            var entity = Resolve(wrappedEntity.Entity);
            Debug.Assert(world.IsAlive(entity), $"CommandBuffer can not to add components to the dead {wrappedEntity.Entity}");

            AddRange(world, entity, _addTypes.Span);
            _addTypes.Clear();
        }

        // Play back sets.
        for (var index = 0; index < Sets.Count; index++)
        {
            // Get wrapped entity
            var wrappedEntity = Sets.Entities[index];
            var entity = Resolve(wrappedEntity.Entity);
            var id = wrappedEntity.Index;

            Debug.Assert(world.IsAlive(entity), $"CommandBuffer can not to set components to the dead {wrappedEntity.Entity}");

            // Get entity chunk
            var entityInfo = world.EntityInfo.GetEntityData(entity.Id);
            var archetype = entityInfo.Archetype;
            ref readonly var chunk = ref archetype.GetChunk(entityInfo.Slot.ChunkIndex);
            var chunkIndex = entityInfo.Slot.Index;

            // Loop over all sparset component arrays and if our entity is in one, copy the set component to its chunk
            for (var i = 0; i < Sets.UsedSize; i++)
            {
                var used = Sets.Used[i];
                var sparseArray = Sets.Components[used];

                if (!sparseArray.Contains(id))
                {
                    continue;
                }

                var chunkArray = chunk.GetArray(sparseArray.Type);
                Array.Copy(sparseArray.Components, sparseArray.Entities[id], chunkArray, chunkIndex, 1);

#if EVENTS
                // Entity also exists in add and the set component was added recently
                if (Adds.Used.Length > i && Adds.Components[Adds.Used[i]].Contains(id))
                {
                    world.OnComponentAdded(entity, sparseArray.Type);
                }
                else
                {
                    world.OnComponentSet(entity, sparseArray.Type);
                }
#endif
            }
        }

        // Play back removals.
        for (var index = 0; index < Removes.Count; index++)
        {
            var wrappedEntity = Removes.Entities[index];
            for (var i = 0; i < Removes.UsedSize; i++)
            {
                var usedIndex = Removes.Used[i];
                var sparseSet = Removes.Components[usedIndex];
                if (!sparseSet.Contains(wrappedEntity.Index))
                {
                    continue;
                }

                _removeTypes.Add(sparseSet.Type);
            }

            if (_removeTypes.Count <= 0)
            {
                continue;
            }

            var entity = Resolve(wrappedEntity.Entity);
            Debug.Assert(world.IsAlive(entity), $"CommandBuffer can not to remove components from the dead {wrappedEntity.Entity}");

            world.RemoveRange(entity, _removeTypes.Span);
            _removeTypes.Clear();
        }

        // Play back destructions.
        foreach (var cmd in Destroys)
        {
            world.Destroy(Entities[cmd]);
        }

        // Reset values.
        if (!dispose)
        {
            return;
        }

        Size = 0;
        Entities.Clear();
        BufferedEntityInfo.Clear();
        Creates.Clear();
        Sets.Clear();
        Adds.Clear();
        Removes.Clear();
        Destroys.Clear();
        _addTypes.Clear();
        _removeTypes.Clear();
    }

    /// <summary>
    ///     Disposes the <see cref="CommandBuffer"/>.
    /// </summary>
    public void Dispose()
## Pinned World Set/Add source context
    /// </summary>
    /// <typeparam name="T">The component type.</typeparam>
    /// <param name="entity">The <see cref="Entity"/>.</param>
    /// <param name="component">The instance, optional.</param>
    public void Set<T>(Entity entity, in T? component = default)
    {
        var entitySlot = EntityInfo.GetEntityData(entity.Id);
        var slot = entitySlot.Slot;
        var archetype = entitySlot.Archetype;
        archetype.Set(ref slot, in component);
        OnComponentSet<T>(entity);
    }

    /// <summary>
    ///     Checks if an <see cref="Entity"/> has a certain component.
    /// </summary>
    /// <typeparam name="T">The component type.</typeparam>
    /// <param name="entity">The <see cref="Entity"/>.</param>
    /// <returns>True if it has the desired component, otherwise false.</returns>
    [Pure]
    public bool Has<T>(Entity entity)
    {
        var archetype = EntityInfo.GetArchetype(entity.Id);
        return archetype.Has<T>();
    }

    /// <summary>
    ///     Returns a reference to the <typeparamref name="T"/> component of an <see cref="Entity"/>.
    /// </summary>
    /// <typeparam name="T">The component type.</typeparam>
    /// <param name="entity">The <see cref="Entity"/>.</param>
    /// <returns>A reference to the <typeparamref name="T"/> component.</returns>
    [Pure]
    public ref T Get<T>(Entity entity)
    {
        var entitySlot = EntityInfo.GetEntityData(entity.Id);
        var slot = entitySlot.Slot;
        var archetype = entitySlot.Archetype;
        return ref archetype.Get<T>(ref slot);
    }

    /// <summary>
    ///     Tries to return a reference to the component of an <see cref="Entity"/>.
    ///     Will copy the component if its a struct.
    /// </summary>
    /// <typeparam name="T">The component type.</typeparam>
    /// <param name="entity">The <see cref="Entity"/>.</param>
    /// <param name="component">The found component.</param>
    /// <returns>True if it exists, otherwise false.</returns>
    [Pure]
    public bool TryGet<T>(Entity entity, out T? component)
    {
        var slot = EntityInfo.GetEntityData(entity.Id);
        if (!slot.Archetype.TryIndex<T>(out int compIndex))
        {
            component = default;
            return false;
        }

        ref var chunk = ref slot.Archetype.GetChunk(slot.Slot.ChunkIndex);
        Debug.Assert(compIndex != -1 && compIndex < chunk.Components.Length, $"Index is out of bounds, component {typeof(T)} with id {compIndex} does not exist in this archetype.");

        var array = Unsafe.As<T[]>(chunk.Components.DangerousGetReferenceAt(compIndex));
        component = array[slot.Slot.Index];
        return true;
    }

    /// <summary>
    ///     Tries to return a reference to the component of an <see cref="Entity"/>.
    /// </summary>
    /// <typeparam name="T">The component type.</typeparam>
    /// <param name="entity">The <see cref="Entity"/>.</param>
    /// <param name="exists">True if it exists, otherwise false.</param>
    /// <returns>A reference to the component.</returns>
    [Pure]
    public ref T TryGetRef<T>(Entity entity, out bool exists)
    {
        var slot = EntityInfo.GetEntityData(entity.Id);

        if (!slot.Archetype.TryIndex<T>(out int compIndex))
        {
            exists = false;
            return ref Unsafe.NullRef<T>();
        }

        exists = true;
        ref var chunk = ref slot.Archetype.GetChunk(slot.Slot.ChunkIndex);
        Debug.Assert(compIndex != -1 && compIndex < chunk.Components.Length, $"Index is out of bounds, component {typeof(T)} with id {compIndex} does not exist in this archetype.");

        var array = Unsafe.As<T[]>(chunk.Components.DangerousGetReferenceAt(compIndex));
        return ref array[slot.Slot.Index];
    }

    /// <summary>
    ///     Ensures the existence of an component on an <see cref="Entity"/>.
    /// </summary>
    /// <remarks>
    ///     Causes a structural change.
    /// </remarks>
    /// <typeparam name="T">The component type.</typeparam>
    /// <param name="entity">The <see cref="Entity"/>.</param>
    /// <param name="component">The component value used if its being added.</param>
    /// <returns>A reference to the component.</returns>
    [StructuralChange]
    public ref T AddOrGet<T>(Entity entity, T? component = default)
    {
        ref T cmp = ref TryGetRef<T>(entity, out var exists);
        if (exists)
        {
            return ref cmp;
        }

        Add(entity, component);
        return ref Get<T>(entity);
    }

    /// <summary>
    ///     Adds a new component to the <see cref="Entity"/> and moves it to the new <see cref="Archetype"/>.
    /// </summary>
    /// <remarks>
    ///     Causes a structural change.
    /// </remarks>
    /// <param name="entity">The <see cref="Entity"/>.</param>
    /// <param name="newArchetype">The <see cref="Entity"/>'s new <see cref="Archetype"/>.</param>
    /// <param name="slot">The new <see cref="Slot"/> in which the moved <see cref="Entity"/> will land.</param>
    /// <typeparam name="T">The component type.</typeparam>
    [SkipLocalsInit]
    [StructuralChange]
    internal void Add<T>(Entity entity, out Archetype newArchetype, out Slot slot)
    {
        ref var data = ref EntityInfo.EntityData[entity.Id];
        var oldArchetype = data.Archetype;
        var type = Component<T>.ComponentType;
        newArchetype = GetOrCreateArchetypeByAddEdge(in type, oldArchetype);

        Move(entity, ref data, oldArchetype, newArchetype, out slot);
    }

    /// <summary>
    ///     Adds a new component to the <see cref="Entity"/> and moves it to the new <see cref="Archetype"/>.
    /// </summary>
    /// <remarks>
    ///     Causes a structural change.
    /// </remarks>
    /// <param name="entity">The <see cref="Entity"/>.</param>
    /// <typeparam name="T">The component type.</typeparam>
    [SkipLocalsInit]
    [StructuralChange]
    public void Add<T>(Entity entity)
    {
        Add<T>(entity, out _, out _);
        OnComponentAdded<T>(entity);
    }

    /// <summary>
    ///     Adds a new component to the <see cref="Entity"/> and moves it to the new <see cref="Archetype"/>.
    /// </summary>
    /// <remarks>
    ///     Causes a structural change.
    /// </remarks>
    /// <param name="entity">The <see cref="Entity"/>.</param>
    /// <typeparam name="T">The component type.</typeparam>
    /// <param name="component">The component instance.</param>
    [SkipLocalsInit]
    [StructuralChange]
    public void Add<T>(Entity entity, in T component)
    {
        Add<T>(entity, out var newArchetype, out var slot);
        newArchetype.Set(ref slot, component);
        OnComponentAdded<T>(entity);
    }

    /// <summary>
    ///     Removes an component from an <see cref="Entity"/> and moves it to a different <see cref="Archetype"/>.
    /// </summary>
    /// <remarks>
    ///     Causes a structural change.
    /// </remarks>
    /// <typeparam name="T">The component type.</typeparam>
    /// <param name="entity">The <see cref="Entity"/>.</param>
    [SkipLocalsInit]
    [StructuralChange]
    public void Remove<T>(Entity entity)
    {
        ref var data = ref EntityInfo.EntityData[entity.Id];
        var oldArchetype = data.Archetype;

        var type = Component<T>.ComponentType;
        var newArchetype = GetOrCreateArchetypeByRemoveEdge(in type, oldArchetype);

        OnComponentRemoved<T>(entity);
        Move(entity, ref data, oldArchetype, newArchetype, out _);
    }
}

#endregion
## Exact assertion search
596:    Assert(world.Get<ProbePositionComponent>(existing).Value == 1, "Grouped Add then Set uses the Set value.");
## Pinned source hashes
[
  {
    "url": "https://raw.githubusercontent.com/genaray/Arch/04d52e7268eb6f376ca4841a0c204334120c5e9d/src/Arch/Core/World.cs",
    "path": ".\\Build\\diagnostics\\ArchMigration\\T1\\upstream-World.cs",
    "status": 200,
    "sha256": "41F7078B462B5C12B8FA2A997699CDEC12591E453A62EBE02FF2FB3B0F1FFBC1",
    "bytes": 67795
  },
  {
    "url": "https://raw.githubusercontent.com/genaray/Arch/04d52e7268eb6f376ca4841a0c204334120c5e9d/src/Arch/Core/Entity.cs",
    "path": ".\\Build\\diagnostics\\ArchMigration\\T1\\upstream-Entity.cs",
    "status": 200,
    "sha256": "18EE844D65A508B6BAAF140F7F9CF91DDE4AEAFF40642049743704285340CD6B",
    "bytes": 8893
  },
  {
    "url": "https://raw.githubusercontent.com/genaray/Arch/04d52e7268eb6f376ca4841a0c204334120c5e9d/src/Arch/Core/QueryDescription.cs",
    "path": null,
    "status": 200,
    "sha256": null,
    "bytes": 0
  },
  {
    "url": "https://raw.githubusercontent.com/genaray/Arch/04d52e7268eb6f376ca4841a0c204334120c5e9d/src/Arch/Buffer/CommandBuffer.cs",
    "path": ".\\Build\\diagnostics\\ArchMigration\\T1\\upstream-CommandBuffer.cs",
    "status": 200,
    "sha256": "F193CD817F7AF04050F47B02826E90E17AABCD3EDA492736A659E93056DD0D4B",
    "bytes": 17018
  }
]

## Preconditions confirmed
- Probe project is net10.0 and references Arch 2.1.0 only.
- Build command succeeded with exit 0, zero warnings, and zero errors before this run.
- The failing case records CommandBuffer.Create(types), then records Set(existing, value 1), then Add(existing, value 2); existing is a real empty entity.
- Four earlier selected cases passed; the failure is specifically the final grouped Add/Set result.

## L3 checklist addendum

A diagnostic summarization command exited 1 after the review file had already been saved. Exact signal: `Select-Object: A parameter cannot be found that matches parameter name 'join'.` This was a PowerShell output-formatting misuse; no source, package, or probe output changed. The cause is the invalid `Select-Object -join` form. Use `[string]::Join()` or an explicit `-join` expression outside the cmdlet.

### Seven-point checklist

1. Read failure: selected probe output shows four PASS cases, then `FAIL command-buffer.create-and-groups-critical` at the assertion `Grouped Add then Set uses the Set value.`; the later diagnostics command failed only after saving the file.
2. Search: exact assertion text occurs once, in `Program.cs` critical CommandBuffer case. The PowerShell error text identifies the invalid formatting parameter.
3. Original source: `pua-playback-order-review.md` contains the 61-line test context, the pinned CommandBuffer Create/Set/Add/Playback source excerpt, the pinned World Set/Add excerpt, and the complete selected-run output.
4. Preconditions: the project is net10.0, Arch 2.1.0 restored and signed; build succeeded with 0 warnings/0 errors; first four selected cases pass. The critical method first queues a staged Create, then records Set(value 1) and Add(value 2) for a separate live empty entity.
5. Reversed assumption: assume Arch's source phase order (Add before Set) is correct and the test's mixed staged/live entity setup is the cause; verify by running Add/Set on a fresh live target with no buffered Create and by reading the buffered entity-index mapping.
6. Minimal isolation: run the existing selected CommandBuffer case with only the minimal live target operations after removing the staged Create from that case, then compare Add and Set lists and final component value. This remains within the already selected risk case.
7. Direction change: use an isolated no-Create buffer experiment plus source inspection of `CommandBuffer` index mapping and `World` archetype movement. Do not change the expected value until this evidence distinguishes the alternatives.

### Three distinct hypotheses to verify

- H1: `CommandBuffer` groups operations as documented, but the mixed buffered-Create/live-entity setup changes the entity-to-buffer index mapping. Inspect `CommandBuffer.Entities`/index mapping and run the no-Create minimal case.
- H2: Add and Set records for one entity/component are coalesced into a single buffered value where the last recorded call wins. Inspect the actual `Adds` and `Sets` collections before playback and read their write paths in pinned source.
- H3: Arch `World.Add` moves the live entity but a buffered Entity copy is stale, so subsequent `Set` resolves the old slot or ignores the newer value. Compare direct `World.Add` followed by `World.Set`, then buffered Add/Set with a stable Entity and inspect `EntityInfo` update behavior.

Each hypothesis will be recorded as supported/rejected only after its check runs.

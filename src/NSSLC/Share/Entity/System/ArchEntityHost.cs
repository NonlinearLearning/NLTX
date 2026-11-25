using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Arch.Core;
using ArchEntity = Arch.Core.Entity;
using ArchWorld = Arch.Core.World;
using Terraria.Share.Combat;
using Terraria.Share.Relationships;

namespace Terraria.Share.Entities;

/// <summary>把不可变实体定义落到 Arch 世界与运行时句柄表上的宿主。</summary>
/// <remarks>
/// 这是 <see cref="IEntityDefinitionHost{TEntity}"/> 目前唯一的实现。在此之前该接口没有任何
/// 实现，<c>EntitySystem</c> 无法构造，整簇定义类型在目录外零引用。
/// </remarks>
public sealed class ArchEntityDefinitionHost :
    IEntityDefinitionHost<ArchEntity>,
    IDeferredEntityDefinitionHost<ArchEntity>
{
  // Arch 2.1.0 的 World.AddRange 对"新建实体 + 一批组件"会越界（BitSet.AsSpan），
  // 因此按类型走泛型 World.Add<T>：每种组件类型只反射一次，之后复用委托。
  private static readonly ConcurrentDictionary<Type, Action<ArchWorld, ArchEntity, object>> Adders = new();
  private static readonly ConcurrentDictionary<
      Type,
      Action<EcsServerCommandBuffer, ArchEntity, object>> Setters = new();

  private readonly ArchWorld _world;
  private readonly EntityRuntimeRegistry _registry;
  private readonly Action<ArchEntity>? _beforeDestroy;

  public ArchEntityDefinitionHost(
      ArchWorld world,
      EntityRuntimeRegistry registry,
      Action<ArchEntity>? beforeDestroy = null)
  {
    _world = world ?? throw new ArgumentNullException(nameof(world));
    _registry = registry ?? throw new ArgumentNullException(nameof(registry));
    _beforeDestroy = beforeDestroy;
  }

  /// <inheritdoc />
  public ArchEntity Create(EntityDefinition definition, EntityComponentSet components)
  {
    ArgumentNullException.ThrowIfNull(definition);
    ArgumentNullException.ThrowIfNull(components);

    ArchEntity entity = _world.Create();
    EntityRuntimeHandle? allocatedHandle = null;
    try
    {
      foreach (EntityComponentValue value in components.Values)
      {
        Adders.GetOrAdd(value.ComponentType, CreateAdder)(_world, entity, value.Value);
      }

      if (components.TryGet<EntityIdentityComponent>(out EntityIdentityComponent identity))
      {
        EntityRuntimeHandle handle = _registry.Allocate(identity.Id);
        allocatedHandle = handle;
        _registry.Register(handle, entity);
      }

      return entity;
    }
    catch
    {
      if (allocatedHandle is EntityRuntimeHandle handle)
      {
        _registry.Remove(handle, out _);
      }

      if (_world.IsAlive(entity))
      {
        _world.Destroy(entity);
      }

      throw;
    }
  }

  /// <inheritdoc />
  public EcsServerEntityCreationTicket CreateDeferred(
      EntityDefinition definition,
      EntityComponentSet components,
      EcsServerCommandBuffer commands)
  {
    ArgumentNullException.ThrowIfNull(definition);
    ArgumentNullException.ThrowIfNull(components);
    ArgumentNullException.ThrowIfNull(commands);

    ArchEntity buffered = commands.Create(components.ComponentTypes.ToArray().Select(
        static type => (ComponentType)type).ToArray());
    EntityRuntimeHandle? allocatedHandle = null;
    try
    {
      foreach (EntityComponentValue value in components.Values)
      {
        Setters.GetOrAdd(value.ComponentType, CreateSetter)(commands, buffered, value.Value);
      }

      if (components.TryGet<EntityIdentityComponent>(out EntityIdentityComponent identity))
      {
        allocatedHandle = _registry.Allocate(identity.Id);
      }

      EcsServerEntityCreationTicket ticket = new(
          commands,
          buffered,
          components,
          _registry,
          allocatedHandle);
      commands.RegisterAfterPlayback(ticket.Complete);
      commands.RegisterOnDiscard(ticket.Abort);
      return ticket;
    }
    catch
    {
      if (allocatedHandle is EntityRuntimeHandle handle)
      {
        _registry.Remove(handle, out _);
      }

      if (buffered != ArchEntity.Null)
      {
        commands.Destroy(buffered);
      }

      throw;
    }
  }

  /// <inheritdoc />
  public void Destroy(ArchEntity entity)
  {
    if (!_world.IsAlive(entity))
    {
      return;
    }

    _beforeDestroy?.Invoke(entity);

    if (_registry.TryGetHandle(entity, out EntityRuntimeHandle handle))
    {
      _registry.Remove(handle, out _);
    }

    if (_world.IsAlive(entity))
    {
      _world.Destroy(entity);
    }
  }

  private static Action<ArchWorld, ArchEntity, object> CreateAdder(Type componentType)
  {
    MethodInfo add = typeof(ArchEntityDefinitionHost)
        .GetMethod(nameof(AddBoxed), BindingFlags.NonPublic | BindingFlags.Static)!
        .MakeGenericMethod(componentType);
    return (Action<ArchWorld, ArchEntity, object>)add.CreateDelegate(
        typeof(Action<ArchWorld, ArchEntity, object>));
  }

  private static Action<EcsServerCommandBuffer, ArchEntity, object> CreateSetter(Type componentType)
  {
    MethodInfo set = typeof(ArchEntityDefinitionHost)
        .GetMethod(nameof(SetBoxed), BindingFlags.NonPublic | BindingFlags.Static)!
        .MakeGenericMethod(componentType);
    return (Action<EcsServerCommandBuffer, ArchEntity, object>)set.CreateDelegate(
        typeof(Action<EcsServerCommandBuffer, ArchEntity, object>));
  }

  private static void AddBoxed<T>(ArchWorld world, ArchEntity entity, object value)
  {
    world.Add(entity, (T)value);
  }

  private static void SetBoxed<T>(
      EcsServerCommandBuffer commands,
      ArchEntity entity,
      object value)
  {
    commands.Set(entity, (T)value);
  }
}

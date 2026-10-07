using System;

using EntityEcs;

using Terraria.Projectile;
using Terraria.WorldStorage;

internal static class ProjectileVerificationAccess
{
  public static bool HasHandle(
    ProjectileLifecycleSystem lifecycle,
    ProjectileHandle handle)
  {
    return lifecycle.TryGetRuntimeHandle(handle, out _);
  }

  public static bool TryRead<TComponent>(
    ProjectileLifecycleSystem lifecycle,
    ProjectileHandle handle,
    out TComponent component)
    where TComponent : notnull
  {
    TComponent value = default!;
    bool captured = false;
    bool found = lifecycle.TryInspect<TComponent>(
      handle,
      (in TComponent current) =>
      {
        value = current;
        captured = true;
      });
    component = captured ? value : default!;
    return found && captured;
  }

  public static TComponent Read<TComponent>(
    ProjectileLifecycleSystem lifecycle,
    ProjectileHandle handle)
    where TComponent : notnull
  {
    if (!TryRead(lifecycle, handle, out TComponent component))
    {
      throw new InvalidOperationException(
        $"Projectile {handle} does not expose {typeof(TComponent).Name}.");
    }

    return component;
  }

  public static void Edit<TComponent>(
    ProjectileLifecycleSystem lifecycle,
    ProjectileHandle handle,
    EntityComponentEditor<TComponent> editor)
    where TComponent : notnull
  {
    if (!lifecycle.TryEdit(handle, editor))
    {
      throw new InvalidOperationException(
        $"Projectile {handle} could not edit {typeof(TComponent).Name}.");
    }
  }
}

using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using Terraria.WorldGeneration.Actions;
using Terraria.WorldGeneration.Components;

namespace Terraria.WorldGeneration.Systems;

public static class WorldSecretSeedRuntimeRegistryCommitSystem
{
  public static WorldSecretSeedRuntimeRegistryCommitResult Commit(
    WorldSecretSeedRuntimeRegistryComponent component,
    EnableSecretSeedCommand command)
  {
    ArgumentNullException.ThrowIfNull(component);
    WorldSecretSeedRuntimeRegistryCommitStatus? rejection =
      ValidateCommand(component, command.IsWellFormed, command.GenerationId, command.RuntimeVersion);
    if (rejection.HasValue)
    {
      return Rejected(component, rejection.Value);
    }

    if (!component.IsKnownVariant(command.Variant))
    {
      return Rejected(
        component,
        WorldSecretSeedRuntimeRegistryCommitStatus.RejectedUnknownVariant);
    }

    const string operation = "enable:";
    WorldSecretSeedRuntimeRegistryCommitStatus? idempotency =
      ValidateIdempotency(
        component,
        command.IdempotencyKey,
        operation + command.Variant);
    if (idempotency.HasValue)
    {
      return idempotency.Value ==
        WorldSecretSeedRuntimeRegistryCommitStatus.Idempotent
        ? Idempotent(component)
        : Rejected(component, idempotency.Value);
    }

    if (component.Contains(command.Variant))
    {
      component.RecordProcessedCommand(
        command.IdempotencyKey,
        operation + command.Variant);
      return Idempotent(component);
    }

    HashSet<string> next = CopyEnabledVariants(component);
    next.Add(command.Variant);
    component.ReplaceEnabledVariants(next);
    component.RecordProcessedCommand(
      command.IdempotencyKey,
      operation + command.Variant);
    return Applied(component);
  }

  public static WorldSecretSeedRuntimeRegistryCommitResult Commit(
    WorldSecretSeedRuntimeRegistryComponent component,
    DisableSecretSeedCommand command)
  {
    ArgumentNullException.ThrowIfNull(component);
    WorldSecretSeedRuntimeRegistryCommitStatus? rejection =
      ValidateCommand(component, command.IsWellFormed, command.GenerationId, command.RuntimeVersion);
    if (rejection.HasValue)
    {
      return Rejected(component, rejection.Value);
    }

    if (!component.IsKnownVariant(command.Variant))
    {
      return Rejected(
        component,
        WorldSecretSeedRuntimeRegistryCommitStatus.RejectedUnknownVariant);
    }

    const string operation = "disable:";
    WorldSecretSeedRuntimeRegistryCommitStatus? idempotency =
      ValidateIdempotency(
        component,
        command.IdempotencyKey,
        operation + command.Variant);
    if (idempotency.HasValue)
    {
      return idempotency.Value ==
        WorldSecretSeedRuntimeRegistryCommitStatus.Idempotent
        ? Idempotent(component)
        : Rejected(component, idempotency.Value);
    }

    if (!component.Contains(command.Variant))
    {
      component.RecordProcessedCommand(
        command.IdempotencyKey,
        operation + command.Variant);
      return Idempotent(component);
    }

    HashSet<string> next = CopyEnabledVariants(component);
    next.Remove(command.Variant);
    component.ReplaceEnabledVariants(next);
    component.RecordProcessedCommand(
      command.IdempotencyKey,
      operation + command.Variant);
    return Applied(component);
  }

  public static WorldSecretSeedRuntimeRegistryCommitResult Commit(
    WorldSecretSeedRuntimeRegistryComponent component,
    ClearSecretSeedsCommand command)
  {
    ArgumentNullException.ThrowIfNull(component);
    WorldSecretSeedRuntimeRegistryCommitStatus? rejection =
      ValidateCommand(component, command.IsWellFormed, command.GenerationId, command.RuntimeVersion);
    if (rejection.HasValue)
    {
      return Rejected(component, rejection.Value);
    }

    const string operation = "clear";
    WorldSecretSeedRuntimeRegistryCommitStatus? idempotency =
      ValidateIdempotency(component, command.IdempotencyKey, operation);
    if (idempotency.HasValue)
    {
      return idempotency.Value ==
        WorldSecretSeedRuntimeRegistryCommitStatus.Idempotent
        ? Idempotent(component)
        : Rejected(component, idempotency.Value);
    }

    if (component.ActiveSecretSeedCount == 0)
    {
      component.RecordProcessedCommand(command.IdempotencyKey, operation);
      return Idempotent(component);
    }

    component.ReplaceEnabledVariants(
      Array.Empty<string>().ToFrozenSet(StringComparer.Ordinal));
    component.RecordProcessedCommand(command.IdempotencyKey, operation);
    return Applied(component);
  }

  public static WorldSecretSeedRuntimeRegistrySnapshot ResetForGeneration(
    WorldSecretSeedRuntimeRegistryComponent component,
    long generationId,
    ulong runtimeVersion)
  {
    ArgumentNullException.ThrowIfNull(component);
    component.ResetForGeneration(generationId, runtimeVersion);
    return component.CreateSnapshot();
  }

  public static WorldSecretSeedRuntimeRegistrySnapshot Close(
    WorldSecretSeedRuntimeRegistryComponent component)
  {
    ArgumentNullException.ThrowIfNull(component);
    component.Close();
    return component.CreateSnapshot();
  }

  private static WorldSecretSeedRuntimeRegistryCommitStatus? ValidateCommand(
    WorldSecretSeedRuntimeRegistryComponent component,
    bool wellFormed,
    long generationId,
    ulong runtimeVersion)
  {
    if (!wellFormed)
    {
      return WorldSecretSeedRuntimeRegistryCommitStatus.RejectedInvalidCommand;
    }

    if (component.Lifecycle != WorldSecretSeedRuntimeRegistryLifecycle.Active)
    {
      return WorldSecretSeedRuntimeRegistryCommitStatus.RejectedLifecycle;
    }

    if (component.GenerationId != generationId ||
        component.RuntimeVersion != runtimeVersion)
    {
      return WorldSecretSeedRuntimeRegistryCommitStatus.RejectedStaleVersion;
    }

    return null;
  }

  private static HashSet<string> CopyEnabledVariants(
    WorldSecretSeedRuntimeRegistryComponent component)
  {
    return new HashSet<string>(component.EnabledVariants, StringComparer.Ordinal);
  }

  private static WorldSecretSeedRuntimeRegistryCommitStatus? ValidateIdempotency(
    WorldSecretSeedRuntimeRegistryComponent component,
    string idempotencyKey,
    string fingerprint)
  {
    if (!component.TryGetProcessedCommand(
      idempotencyKey,
      fingerprint,
      out bool conflicts))
    {
      return null;
    }

    return conflicts
      ? WorldSecretSeedRuntimeRegistryCommitStatus.RejectedIdempotencyConflict
      : WorldSecretSeedRuntimeRegistryCommitStatus.Idempotent;
  }

  private static WorldSecretSeedRuntimeRegistryCommitResult Applied(
    WorldSecretSeedRuntimeRegistryComponent component)
  {
    return new WorldSecretSeedRuntimeRegistryCommitResult(
      WorldSecretSeedRuntimeRegistryCommitStatus.Applied,
      component.CreateSnapshot());
  }

  private static WorldSecretSeedRuntimeRegistryCommitResult Idempotent(
    WorldSecretSeedRuntimeRegistryComponent component)
  {
    return new WorldSecretSeedRuntimeRegistryCommitResult(
      WorldSecretSeedRuntimeRegistryCommitStatus.Idempotent,
      component.CreateSnapshot());
  }

  private static WorldSecretSeedRuntimeRegistryCommitResult Rejected(
    WorldSecretSeedRuntimeRegistryComponent component,
    WorldSecretSeedRuntimeRegistryCommitStatus status)
  {
    return new WorldSecretSeedRuntimeRegistryCommitResult(
      status,
      component.CreateSnapshot());
  }
}

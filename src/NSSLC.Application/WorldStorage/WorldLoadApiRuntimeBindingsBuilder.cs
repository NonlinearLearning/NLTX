using System;
using System.Collections.Generic;
using Terraria.WorldStorage;

namespace Terraria.NonAuthoritative.Persistence;

/// <summary>
/// Builds the runtime bindings supplied to the generated world-load catalog.
/// </summary>
/// <remarks>
/// The builder stores only the API instances and owner contexts required by the selected
/// composition root. It never stores a persistence document or discovers APIs at runtime.
/// Registration is exact by API type and owner id so an ambiguous binding cannot be selected.
/// </remarks>
public sealed class WorldLoadApiRuntimeBindingsBuilder
{
  private readonly Dictionary<Type, object> _apis = new();
  private readonly Dictionary<string, object> _ownerContexts =
    new(StringComparer.Ordinal);

  public WorldLoadApiRuntimeBindingsBuilder AddApi<TApi>(TApi api)
    where TApi : class
  {
    ArgumentNullException.ThrowIfNull(api);
    Type apiType = typeof(TApi);
    if (!_apis.TryAdd(apiType, api))
    {
      throw new InvalidOperationException(
        $"A world load API binding for '{apiType}' is already registered.");
    }

    return this;
  }

  public WorldLoadApiRuntimeBindingsBuilder AddOwnerContext<TOwnerContext>(
    string ownerId,
    TOwnerContext ownerContext)
    where TOwnerContext : notnull
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(ownerId);
    ArgumentNullException.ThrowIfNull(ownerContext);
    if (!_ownerContexts.TryAdd(ownerId, ownerContext))
    {
      throw new InvalidOperationException(
        $"A world load owner context for '{ownerId}' is already registered.");
    }

    return this;
  }

  public WorldLoadApiRuntimeBindings Build()
  {
    return new RegisteredBindings(
      new Dictionary<Type, object>(_apis),
      new Dictionary<string, object>(_ownerContexts, StringComparer.Ordinal));
  }

  private sealed class RegisteredBindings(
    IReadOnlyDictionary<Type, object> apis,
    IReadOnlyDictionary<string, object> ownerContexts) : WorldLoadApiRuntimeBindings
  {
    public override bool TryGetApi<TApi>(out TApi api)
    {
      if (apis.TryGetValue(typeof(TApi), out object? registeredApi) &&
          registeredApi is TApi typedApi)
      {
        api = typedApi;
        return true;
      }

      api = null!;
      return false;
    }

    public override bool TryGetOwnerContext<TOwnerContext>(
      string ownerId,
      out TOwnerContext ownerContext)
    {
      if (!string.IsNullOrWhiteSpace(ownerId) &&
          ownerContexts.TryGetValue(ownerId, out object? registeredContext) &&
          registeredContext is TOwnerContext typedContext)
      {
        ownerContext = typedContext;
        return true;
      }

      ownerContext = default!;
      return false;
    }
  }
}

using System.Collections.Concurrent;

namespace Terraria.NetWork.Prototype.PacketDesignCompiler;

public sealed record MemberDeclaration(Type DeclaringType, Type ValueType, MemberRef Member);

public sealed record ProtocolModelRef(
    bool IsClass, bool IsPublic, bool IsAbstract, bool IsSealed, Func<object?>? ReadInstance);

public static class CompilerMemberCatalog
{
    private static readonly ConcurrentDictionary<Type, ProtocolModelRef> s_models = new();
    private static readonly ConcurrentDictionary<string, ConcurrentDictionary<Type, byte>> s_scopes = new();

    public static void RegisterModel(Type model, ProtocolModelRef declaration)
        => s_models.TryAdd(model, declaration);

    public static void RegisterModel(string scope, Type model, ProtocolModelRef declaration)
    {
        RegisterModel(model, declaration);
        s_scopes.GetOrAdd(scope, static _ => new()).TryAdd(model, 0);
    }

    internal static Type[] FindModels(string scope)
        => s_scopes.TryGetValue(scope, out var models) ? models.Keys.ToArray() : [];

    internal static ProtocolModelRef? FindModel(Type model)
        => s_models.GetValueOrDefault(model);
}

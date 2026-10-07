using System.Collections.Concurrent;

namespace Terraria.NetWork.Prototype.PacketDesignCompiler;

// Generated declarations supply signatures; delegates identify named methods only.
public static class CompilerMethodCatalog
{
    private static readonly ConcurrentDictionary<Delegate, MethodRef> s_methods = new();

    internal static readonly MethodRef Unresolved = new(
        TypeRef.From(typeof(object)), "UnresolvedCodec", TypeRef.From(typeof(void)), [], false, false, false, false, false);

    public static void Register(Delegate method, MethodRef declaration)
    {
        s_methods.TryAdd(method, declaration);
    }

    internal static MethodRef? Find(Delegate method) =>
        s_methods.TryGetValue(method, out var declaration) ? declaration : null;
}

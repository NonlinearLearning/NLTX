using System.Collections.Immutable;

namespace Terraria.NetWork.Prototype.PacketDesignCompiler;

public sealed class FunctionDeclaration
{
    private readonly PacketGraph _graph;
    private readonly SourceLocation _location;
    private readonly ByteRange? _range;
    private readonly List<MethodRef> _methods = [];
    private readonly List<ImmutableArray<object>> _parameters = [];
    private readonly List<ImmutableArray<object>> _returns = [];
    private readonly List<ImmutableArray<object>> _sizes = [];

    internal FunctionDeclaration(PacketGraph graph, SourceLocation location, ByteRange? range)
    {
        _graph = graph;
        _location = location;
        _range = range;
    }

    public FunctionDeclaration FunctionName(Delegate function)
    {
        _graph.EnsureDependencyMutable();
        ArgumentNullException.ThrowIfNull(function);
        _methods.Add(CompilerMethodCatalog.Find(function) ?? CompilerMethodCatalog.Unresolved);
        return this;
    }

    public FunctionDeclaration ParameterList(params object[] parameters)
    {
        _graph.EnsureDependencyMutable();
        _parameters.Add(CopyMembers(parameters));
        return this;
    }

    public FunctionDeclaration ReturnValue(params object[] fields)
    {
        _graph.EnsureDependencyMutable();
        _returns.Add(CopyMembers(fields));
        return this;
    }

    public FunctionDeclaration Sizeof(params object[] fields)
    {
        _graph.EnsureDependencyMutable();
        _sizes.Add(CopyMembers(fields));
        return this;
    }

    private static ImmutableArray<object> CopyMembers(object[] members)
    {
        ArgumentNullException.ThrowIfNull(members);
        foreach (var member in members) ArgumentNullException.ThrowIfNull(member);
        return members.ToImmutableArray();
    }

    internal FunctionDeclarationSnapshot Snapshot(IReadOnlyList<FunctionDeclaration> functions)
    {
        FunctionArgumentDeclaration ConvertArgument(object argument) => argument switch
        {
            MemberDeclaration member => new(member, null, null),
            PacketGraphNode node => new(null, node, null),
            FunctionDeclaration function => new(null, null,
                Enumerable.Range(0, functions.Count).FirstOrDefault(index => ReferenceEquals(functions[index], function), -1)),
            _ => new(null, null, null)
        };
        ImmutableArray<ImmutableArray<FunctionArgumentDeclaration>> ConvertLists(List<ImmutableArray<object>> lists)
            => lists.Select(list => list.Select(ConvertArgument).ToImmutableArray()).ToImmutableArray();
        return new(_methods.ToImmutableArray(), ConvertLists(_parameters), ConvertLists(_returns),
            ConvertLists(_sizes), _location, _range);
    }
}

public sealed record FunctionArgumentDeclaration(
    MemberDeclaration? Member, PacketGraphNode? Node, int? FunctionOrdinal);

public sealed record FunctionDeclarationSnapshot(
    ImmutableArray<MethodRef> Methods,
    ImmutableArray<ImmutableArray<FunctionArgumentDeclaration>> ParameterLists,
    ImmutableArray<ImmutableArray<FunctionArgumentDeclaration>> ReturnValues,
    ImmutableArray<ImmutableArray<FunctionArgumentDeclaration>> SizeFields,
    SourceLocation SourceLocation,
    ByteRange? LengthRange);

public sealed record MemberPresenceBinding(
    PacketFieldNode Target, MemberDeclaration Source, int BitIndex, SourceLocation SourceLocation);

public partial class PacketGraph
{
    private readonly List<FunctionDeclaration> _functions = [];
    private readonly List<MemberPresenceBinding> _memberPresence = [];

    public FunctionDeclaration FunctionDeclaration(
        SourceLocation? sourceLocation = null, ByteRange? lengthRange = null)
    {
        EnsureMutable();
        var declaration = new FunctionDeclaration(this, sourceLocation ?? SourceLocation.None, lengthRange);
        _functions.Add(declaration);
        return declaration;
    }

    internal void BindMemberPresence(PacketFieldNode target, MemberDeclaration source,
        int bitIndex, SourceLocation? sourceLocation)
    {
        EnsureMutable();
        ArgumentNullException.ThrowIfNull(source);
        _memberPresence.Add(new(target, source, bitIndex, sourceLocation ?? target.SourceLocation));
    }
}

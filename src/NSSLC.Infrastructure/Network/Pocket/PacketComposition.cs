using System.Collections.Immutable;
using System.Linq.Expressions;
using System.Reflection;

namespace Terraria.NetWork.Prototype.PacketDesignCompiler;

public sealed record PacketConstructionBinding(ConstructorInfo Constructor,
    ImmutableArray<PacketFieldNode> Arguments);

public sealed class PacketWireLayout
{
    private readonly PacketGraph _graph;
    private ImmutableArray<PacketFieldNode> _fields;

    internal PacketWireLayout(PacketGraph graph) => _graph = graph;

    public void Sequence(params PacketFieldNode[] fields)
    {
        _graph.EnsureDependencyMutable();
        ArgumentNullException.ThrowIfNull(fields);
        var declared = _graph.DependencyNodes.OfType<PacketFieldNode>().ToArray();
        if (fields.Length != declared.Length || fields.Any(field => field is null || !declared.Contains(field))
            || fields.Distinct(ReferenceEqualityComparer.Instance).Count() != fields.Length)
            throw new ArgumentException("Wire order must contain every declared field exactly once.", nameof(fields));
        _fields = fields.ToImmutableArray();
    }

    internal ImmutableArray<PacketFieldNode> Snapshot()
    {
        var declared = _graph.DependencyNodes.OfType<PacketFieldNode>().ToImmutableArray();
        if (_fields.IsDefault) return declared;
        if (_fields.Length != declared.Length || declared.Any(field => !_fields.Contains(field)))
            throw new InvalidOperationException("Wire order must include fields created after the order was declared.");
        return _fields;
    }
}

public partial class PacketGraph
{
    private readonly Dictionary<PacketFieldNode, FieldFormatBinding> _fieldFormats =
        new(ReferenceEqualityComparer.Instance);
    private PacketConstructionBinding? _construction;

    public PacketWireLayout WireLayout { get; }

    internal ImmutableArray<FieldFormatBinding> DependencyFormats => _fieldFormats.Values.ToImmutableArray();

    public void BindConstructor(params PacketFieldNode[] arguments)
    {
        EnsureMutable();
        ArgumentNullException.ThrowIfNull(arguments);
        if (_construction is not null) throw new InvalidOperationException("Construction is already bound.");
        if (arguments.Any(field => field is null || !_declaredNodes.Contains(field))
            || arguments.Distinct(ReferenceEqualityComparer.Instance).Count() != arguments.Length)
            throw new ArgumentException("Constructor arguments must be distinct fields of this graph.", nameof(arguments));
        Type[] types = arguments.Select(static field => field.ValueType).ToArray();
        var constructor = _packetType.GetConstructors().SingleOrDefault(constructor =>
            constructor.GetParameters().Select(static parameter => parameter.ParameterType).SequenceEqual(types))
            ?? throw new ArgumentException("No public constructor matches the field argument types.", nameof(arguments));
        _construction = new(constructor, arguments.ToImmutableArray());
    }

    protected void BindFieldFormat(PacketFieldNode field, WireFormat format)
    {
        EnsureMutable();
        ArgumentNullException.ThrowIfNull(format);
        _fieldFormats.Add(field, new(field, format));
    }
}

public partial class PacketGraph<TPacket>
{
    public PacketFieldNode Field(MemberDeclaration member,
        WireFormat format, SourceLocation? sourceLocation = null)
    {
        ArgumentNullException.ThrowIfNull(format);
        var field = Field(member, sourceLocation);
        BindFieldFormat(field, format);
        return field;
    }

    public PacketFieldNode<T> Field<T>(Expression<Func<TPacket, T>> selector,
        WireFormat format, SourceLocation? sourceLocation = null)
    {
        ArgumentNullException.ThrowIfNull(format);
        var field = Field(selector, sourceLocation);
        BindFieldFormat(field, format);
        return field;
    }

}

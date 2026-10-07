using System.Collections.Immutable;
using System.Linq.Expressions;

namespace Terraria.NetWork.Prototype.PacketDesignCompiler;

public partial class PacketGraph
{
    private readonly byte _messageId;
    private readonly Type _packetType;
    private readonly PacketCodecDirections _codecDirections;
    private readonly WireDirection _wireDirection;
    private readonly SourceLocation _sourceLocation;
    private Type? _externalDependencyModel;
    private readonly List<ImmutableArray<MemberDeclaration>> _externalDependencies = [];
    private readonly List<PacketGraphNode> _nodes = [];
    private readonly List<PacketGraphEdge> _edges = [];
    private readonly HashSet<PacketGraphNode> _declaredNodes = new(ReferenceEqualityComparer.Instance);
    // Lets one redundant AddNode call remain harmless for factory-registered nodes.
    private readonly HashSet<PacketGraphNode> _implicitlyDeclaredNodes = new(ReferenceEqualityComparer.Instance);
    private PacketGraphManifest? _manifest;

    public PacketGraph(
        byte messageId,
        Type packetType,
        SourceLocation? sourceLocation = null,
        PacketCodecDirections codecDirections = PacketCodecDirections.Bidirectional,
        WireDirection wireDirection = WireDirection.Bidirectional)
    {
        ArgumentNullException.ThrowIfNull(packetType);
        if (codecDirections == PacketCodecDirections.None
            || (codecDirections & ~PacketCodecDirections.Bidirectional) != 0)
        {
            throw new ArgumentOutOfRangeException(nameof(codecDirections));
        }

        _messageId = messageId;
        if (wireDirection == WireDirection.None
            || (wireDirection & ~WireDirection.Bidirectional) != 0)
        {
            throw new ArgumentOutOfRangeException(nameof(wireDirection));
        }

        _wireDirection = wireDirection;
        _packetType = packetType;
        _codecDirections = codecDirections;
        _sourceLocation = sourceLocation ?? SourceLocation.None;
        Dependencies = new PacketDependencyModel(this);
        WireLayout = new PacketWireLayout(this);
    }

    public PacketDependencyModel Dependencies { get; }

    public PacketGraph ExternalDependencies(params MemberDeclaration[] members)
    {
        EnsureMutable();
        ArgumentNullException.ThrowIfNull(members);
        foreach (var member in members) ArgumentNullException.ThrowIfNull(member);
        _externalDependencies.Add(members.ToImmutableArray());
        foreach (var member in members) CreateProtocolInput(member);
        return this;
    }

    internal IReadOnlyList<PacketGraphNode> DependencyNodes => _nodes.AsReadOnly();
    internal IReadOnlyList<PacketGraphEdge> DependencyRelations => _edges.AsReadOnly();
    internal void EnsureDependencyMutable() => EnsureMutable();

    public void AddNode(PacketGraphNode node)
    {
        EnsureMutable();
        ArgumentNullException.ThrowIfNull(node);

        if (_implicitlyDeclaredNodes.Remove(node))
        {
            return;
        }

        if (!_declaredNodes.Add(node))
        {
            throw new InvalidOperationException("The same graph node object cannot be added twice.");
        }

        _nodes.Add(node);
    }

    // Protocol inputs are stable wire facts, bound when constructing a reader/writer.
    public GameSystemInputNode<TValue> CreateProtocolInput<TValue>(
        SourceLocation? sourceLocation = null)
    {
        return DeclareNode(new GameSystemInputNode<TValue>(sourceLocation));
    }

    public GameSystemInputNode CreateProtocolInput(
        MemberDeclaration member, SourceLocation? sourceLocation = null)
    {
        EnsureMutable();
        ArgumentNullException.ThrowIfNull(member);
        var input = GameSystemInputNode.FromDeclaration(member, sourceLocation);
        _externalDependencyModel ??= member.DeclaringType;
        var existing = _nodes.OfType<GameSystemInputNode>()
            .SingleOrDefault(node => node.ModelMember == input.ModelMember && node.ValueType == input.ValueType);
        return existing ?? DeclareNode(input);
    }

    public void AddEdge(
        PacketGraphNode source,
        PacketGraphNode target,
        SourceLocation? sourceLocation = null,
        int? bitIndex = null,
        int? ordinal = null)
    {
        EnsureMutable();
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(target);

        AddEdge(
            InferEdgeKind(source, target),
            source,
            target,
            sourceLocation,
            bitIndex,
            ordinal);
    }

    // Codec-first syntax; normalize the stored edge to its value-flow direction.
    public void AddEdge(
        CodecRefNode codec,
        PacketGraphNode connectedNode,
        SourceLocation? sourceLocation = null,
        int? bitIndex = null,
        int? ordinal = null)
    {
        EnsureMutable();
        ArgumentNullException.ThrowIfNull(codec);
        ArgumentNullException.ThrowIfNull(connectedNode);

        PacketGraphNode source;
        PacketGraphNode target;
        PacketGraphEdgeKind kind;
        if (codec.Direction == CodecDirection.Encode)
        {
            kind = PacketGraphEdgeKind.CodecInput;
            source = connectedNode;
            target = codec;
        }
        else if (connectedNode is PacketFieldNode)
        {
            kind = PacketGraphEdgeKind.CodecOutput;
            source = codec;
            target = connectedNode;
        }
        else if (connectedNode is GameSystemInputNode or PacketCalculationNode)
        {
            kind = PacketGraphEdgeKind.CodecInput;
            source = connectedNode;
            target = codec;
        }
        else
        {
            throw new ArgumentException(
                $"Cannot infer a codec edge for '{codec.Direction}' and '{connectedNode.GetType().Name}'. Use the overload with an explicit edge kind.",
                nameof(connectedNode));
        }

        AddEdge(kind, source, target, sourceLocation, bitIndex, ordinal);
    }

    public void AddEdge(
        PacketGraphEdgeKind kind,
        PacketGraphNode source,
        PacketGraphNode target,
        SourceLocation? sourceLocation = null,
        int? bitIndex = null,
        int? ordinal = null)
    {
        EnsureMutable();
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(target);

        if (!_declaredNodes.Contains(source) || !_declaredNodes.Contains(target))
        {
            throw new InvalidOperationException(
                "Both graph edge endpoints must be declared in the graph before AddEdge.");
        }

        if (kind is PacketGraphEdgeKind.CalculationInput
                or PacketGraphEdgeKind.CodecInput
                or PacketGraphEdgeKind.CodecOutput
            && ordinal is null)
        {
            throw new ArgumentException(
                $"{kind} edges require a parameter ordinal.",
                nameof(ordinal));
        }

        if (kind == PacketGraphEdgeKind.Presence && ordinal is not null)
        {
            throw new ArgumentException(
                "Presence edges do not accept a parameter ordinal.",
                nameof(ordinal));
        }

        _edges.Add(new PacketGraphEdge(
            kind,
            source,
            target,
            sourceLocation ?? SourceLocation.None,
            bitIndex,
            ordinal));
    }

    private static PacketGraphEdgeKind InferEdgeKind(
        PacketGraphNode source,
        PacketGraphNode target)
    {
        if (target is PacketCalculationNode)
        {
            return PacketGraphEdgeKind.CalculationInput;
        }

        if (target is CodecRefNode)
        {
            return PacketGraphEdgeKind.CodecInput;
        }

        if (source is CodecRefNode && target is PacketFieldNode)
        {
            return PacketGraphEdgeKind.CodecOutput;
        }

        if (source is PacketFieldNode && target is PacketFieldNode)
        {
            return PacketGraphEdgeKind.Presence;
        }

        throw new ArgumentException(
            $"Cannot infer an edge kind for '{source.GetType().Name}' -> '{target.GetType().Name}'. Use the overload with an explicit edge kind.",
            nameof(target));
    }

    public PacketGraphManifest Build()
    {
        if (_manifest is null)
        {
            var nodes = _nodes.ToImmutableArray();
            var edges = _edges.ToImmutableArray();
            _manifest = new PacketGraphManifest(
                _messageId,
                _packetType,
                _codecDirections,
                nodes,
                edges,
                _sourceLocation,
                _wireDirection,
                [],
                Dependencies.Snapshot(nodes, edges),
                WireLayout.Snapshot(),
                _fieldFormats.Values.ToImmutableArray(),
                _construction,
                _externalDependencyModel,
                _functions.Select(function => function.Snapshot(_functions)).ToImmutableArray(),
                _memberPresence.ToImmutableArray(),
                _externalDependencies.ToImmutableArray());
            foreach (var field in nodes.OfType<PacketFieldNode>()) field.DeclarationGraph = null;
        }

        return _manifest;
    }

    protected PacketFieldNode<TValue> DeclareField<TValue>(PacketFieldNode<TValue> field)
    {
        return DeclareNode(field);
    }

    protected TNode DeclareNode<TNode>(TNode node)
        where TNode : PacketGraphNode
    {
        EnsureMutable();
        if (node is PacketFieldNode field) field.DeclarationGraph = this;
        _declaredNodes.Add(node);
        _implicitlyDeclaredNodes.Add(node);
        _nodes.Add(node);
        return node;
    }

    private void EnsureMutable()
    {
        if (_manifest is not null)
        {
            throw new InvalidOperationException("A packet graph cannot be modified after Build().");
        }
    }
}

public partial class PacketGraph<TPacket> : PacketGraph
{
    public PacketGraph(
        byte messageId,
        SourceLocation? sourceLocation = null,
        PacketCodecDirections codecDirections = PacketCodecDirections.Bidirectional,
        WireDirection wireDirection = WireDirection.Bidirectional)
        : base(messageId, typeof(TPacket), sourceLocation, codecDirections, wireDirection)
    {
    }

    public new PacketGraph<TPacket> ExternalDependencies(params MemberDeclaration[] members)
    {
        base.ExternalDependencies(members);
        return this;
    }

    public PacketFieldNode Field(MemberDeclaration member,
        SourceLocation? sourceLocation = null)
        => DeclareNode(PacketFieldNode.FromDeclaration(member, sourceLocation));

    public PacketVariableFieldNode VariableField(MemberDeclaration member,
        SourceLocation? sourceLocation = null)
        => DeclareNode(PacketVariableFieldNode.FromDeclaration(member, sourceLocation));

    public PacketFieldNode<TValue> Field<TValue>(
        Expression<Func<TPacket, TValue>> selector,
        SourceLocation? sourceLocation = null)
    {
        return DeclareField(PacketFieldNode<TValue>.Create(selector, sourceLocation));
    }

    public PacketVariableFieldNode<TValue> VariableField<TValue>(
        Expression<Func<TPacket, TValue>> selector,
        SourceLocation? sourceLocation = null)
        => DeclareNode(PacketFieldNode<TValue>.CreateVariable(selector, null, sourceLocation));

}

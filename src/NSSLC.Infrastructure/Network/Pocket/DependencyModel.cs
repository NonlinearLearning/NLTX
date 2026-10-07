using System.Collections.Immutable;

namespace Terraria.NetWork.Prototype.PacketDesignCompiler;

public sealed record PacketProtocolDependency(
    GameSystemInputNode Node,
    int Ordinal,
    TypeRef ValueType);

public sealed record PacketDependencySnapshot(
    ImmutableArray<PacketGraphNode> Nodes,
    ImmutableArray<PacketGraphEdge> Relations,
    ImmutableArray<PacketProtocolDependency> Inputs,
    ImmutableArray<FieldFormatBinding> Formats = default);

public sealed class PacketDependencyModel
{
    private readonly PacketGraph _graph;

    internal PacketDependencyModel(PacketGraph graph) => _graph = graph;

    public IReadOnlyList<PacketGraphNode> Nodes => _graph.DependencyNodes;

    public IReadOnlyList<PacketGraphEdge> Relations => _graph.DependencyRelations;

    public ImmutableArray<FieldFormatBinding> Formats => _graph.DependencyFormats;

    public ImmutableArray<PacketProtocolDependency> Inputs => Nodes
        .OfType<GameSystemInputNode>()
        .Select((node, ordinal) => new PacketProtocolDependency(
            node, ordinal, TypeRef.From(node.ValueType)))
        .ToImmutableArray();

    internal PacketDependencySnapshot Snapshot(
        ImmutableArray<PacketGraphNode> nodes,
        ImmutableArray<PacketGraphEdge> relations) => new(nodes, relations, Inputs, Formats);
}

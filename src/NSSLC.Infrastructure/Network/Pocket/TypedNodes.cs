namespace Terraria.NetWork.Prototype.PacketDesignCompiler;

// Invariant T preserves exact wire value types, including nullable value types.
public interface IWireValueNode<TValue>
{
    PacketGraphNode Node { get; }
}

namespace Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

/// <summary>
/// Pure functions supplied by the packet toolchain for values carried by more
/// than one packet format. These methods can be passed directly to
/// FunctionDeclaration().FunctionName(...).
/// </summary>
[PacketSharedFunction]
public static class NetworkTextCalculations
{
    public static string ToDisplayText(NetworkText value)
    {
        ArgumentNullException.ThrowIfNull(value);
        return value.ToDisplayString();
    }
}

using System.Collections.Immutable;

namespace Terraria.NetWork.Prototype.PacketDesignCompiler;

public sealed class PacketDesignCompiler
{
    public CompilationResult Compile(ProtocolManifest protocol, CompilerOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(protocol);
        return Compile(new CompilationInputBundle(protocol), options, cancellationToken);
    }

    public CompilationResult Compile(
        CompilationInputBundle input,
        CompilerOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(input);
        cancellationToken.ThrowIfCancellationRequested();
        options ??= new CompilerOptions();
        return CompilerPipeline.Run(input, options, cancellationToken);
    }
}

using System.Collections.Immutable;
using System.Linq.Expressions;
using System.Reflection;

namespace Terraria.NetWork.Prototype.PacketDesignCompiler;

public static class CompilerProfile
{
    public const string FixedScalarPresenceV1 = "FixedScalarPresenceV1";
    public const string BoundedBytesV1 = "BoundedBytesV1";
    public const int Version = 17;
    public const int SchemaVersion = 17;
}

public sealed record TypeRef(
    string AssemblyName,
    string Namespace,
    string MetadataName,
    ImmutableArray<TypeRef> GenericArguments,
    bool IsValueType)
{
    public bool Matches(TypeRef other) => AssemblyName == other.AssemblyName
        && Namespace == other.Namespace && MetadataName == other.MetadataName
        && IsValueType == other.IsValueType
        && GenericArguments.Length == other.GenericArguments.Length
        && GenericArguments.Zip(other.GenericArguments).All(static pair => pair.First.Matches(pair.Second));

    public string Identity => string.IsNullOrEmpty(Namespace)
        ? MetadataName
        : Namespace + "." + MetadataName;

    public string CSharpName
    {
        get
        {
            var name = Identity.Replace('+', '.');
            if (!GenericArguments.IsDefaultOrEmpty)
            {
                name += "<" + string.Join(", ", GenericArguments.Select(static argument => argument.CSharpName)) + ">";
            }

            return "global::" + name;
        }
    }

    public static TypeRef From(Type type)
    {
        ArgumentNullException.ThrowIfNull(type);

        var definition = type.IsGenericType ? type.GetGenericTypeDefinition() : type;
        var names = new Stack<string>();
        for (var declaringType = definition; declaringType is not null; declaringType = declaringType.DeclaringType)
        {
            var name = declaringType.Name;
            var tick = name.IndexOf('`');
            names.Push(tick >= 0 ? name[..tick] : name);
        }

        var metadataName = string.Join('.', names);

        var arguments = type.IsGenericType
            ? type.GetGenericArguments().Select(From).ToImmutableArray()
            : ImmutableArray<TypeRef>.Empty;

        return new TypeRef(
            type.Assembly.GetName().Name ?? string.Empty,
            definition.Namespace ?? string.Empty,
            metadataName,
            arguments,
            type.IsValueType);
    }
}

public enum MemberKind : byte
{
    Field,
    Property
}

public sealed record MemberRef(
    TypeRef DeclaringType,
    string Name,
    MemberKind Kind,
    TypeRef ValueType,
    bool IsReadable,
    bool IsWritable,
    bool IsStatic = false,
    bool IsIndexed = false)
{
    public static MemberRef From(MemberInfo member)
    {
        ArgumentNullException.ThrowIfNull(member);

        return member switch
        {
            FieldInfo field => new MemberRef(
                TypeRef.From(field.DeclaringType ?? throw new ArgumentException("A field needs a declaring type.")),
                field.Name,
                MemberKind.Field,
                TypeRef.From(field.FieldType),
                field.IsPublic,
                field.IsPublic && !field.IsInitOnly,
                field.IsStatic),
            PropertyInfo property => new MemberRef(
                TypeRef.From(property.DeclaringType ?? throw new ArgumentException("A property needs a declaring type.")),
                property.Name,
                MemberKind.Property,
                TypeRef.From(property.PropertyType),
                property.GetMethod?.IsPublic == true,
                property.SetMethod?.IsPublic == true,
                (property.GetMethod ?? property.SetMethod)?.IsStatic == true,
                property.GetIndexParameters().Length != 0),
            _ => throw new ArgumentException("Only fields and properties can be packet members.", nameof(member))
        };
    }
}

public sealed record CalculationParameterRef(
    string Name,
    TypeRef Type,
    bool IsByRef,
    bool IsOut,
    bool IsIn,
    bool IsOptional,
    bool IsParamArray);

public sealed record MethodRef(
    TypeRef DeclaringType,
    string Name,
    TypeRef ReturnType,
    ImmutableArray<CalculationParameterRef> Parameters,
    bool IsStatic,
    bool IsPublic,
    bool IsGeneric,
    bool IsPartialContainingType,
    bool IsPacketSharedFunction);

[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public sealed class PacketSharedFunctionAttribute : Attribute
{
}

public enum ScalarKind : byte
{
    Byte,
    SByte,
    Int16,
    UInt16,
    Int32,
    UInt32,
    Int64,
    UInt64,
    Bool,
    Float32
}

// Common payload-byte upper bounds, not wire types or prefix formats.
// The current length model uses Int32, so larger bounds are not advertised.
public enum LengthLimit
{
    Int8 = sbyte.MaxValue,
    UInt8 = byte.MaxValue,
    Int16 = short.MaxValue,
    UInt16 = ushort.MaxValue,
    Int32 = int.MaxValue
}

public sealed record ByteRange(
    int MinimumLength,
    int MaximumLength);

public static class ScalarKinds
{
    public static bool IsInteger(ScalarKind kind) => kind is not ScalarKind.Bool and not ScalarKind.Float32;

    public static int Width(ScalarKind kind) => kind switch
    {
        ScalarKind.Byte or ScalarKind.SByte or ScalarKind.Bool => 1,
        ScalarKind.Int16 or ScalarKind.UInt16 => 2,
        ScalarKind.Int32 or ScalarKind.UInt32 or ScalarKind.Float32 => 4,
        ScalarKind.Int64 or ScalarKind.UInt64 => 8,
        _ => throw new ArgumentOutOfRangeException(nameof(kind))
    };

    public static TypeRef TypeRefFor(ScalarKind kind) => TypeRef.From(kind switch
    {
        ScalarKind.Byte => typeof(byte),
        ScalarKind.SByte => typeof(sbyte),
        ScalarKind.Int16 => typeof(short),
        ScalarKind.UInt16 => typeof(ushort),
        ScalarKind.Int32 => typeof(int),
        ScalarKind.UInt32 => typeof(uint),
        ScalarKind.Int64 => typeof(long),
        ScalarKind.UInt64 => typeof(ulong),
        ScalarKind.Bool => typeof(bool),
        ScalarKind.Float32 => typeof(float),
        _ => throw new ArgumentOutOfRangeException(nameof(kind))
    });

    public static ScalarKind FromType(Type type)
    {
        var underlying = Nullable.GetUnderlyingType(type) ?? type;
        return Type.GetTypeCode(underlying) switch
        {
            TypeCode.Byte => ScalarKind.Byte,
            TypeCode.SByte => ScalarKind.SByte,
            TypeCode.Int16 => ScalarKind.Int16,
            TypeCode.UInt16 => ScalarKind.UInt16,
            TypeCode.Int32 => ScalarKind.Int32,
            TypeCode.UInt32 => ScalarKind.UInt32,
            TypeCode.Int64 => ScalarKind.Int64,
            TypeCode.UInt64 => ScalarKind.UInt64,
            TypeCode.Boolean => ScalarKind.Bool,
            TypeCode.Single => ScalarKind.Float32,
            _ => throw new ArgumentException($"Type '{type}' is not a supported scalar.", nameof(type))
        };
    }

    // Non-throwing probe. Callers that must distinguish "this really is a scalar" from
    // "I have no wire image for this type" need this: folding the failure into a default
    // scalar silently classifies a nested structure as a 1-byte integer.
    public static bool TryFromType(Type type, out ScalarKind scalar)
    {
        var underlying = Nullable.GetUnderlyingType(type) ?? type;
        switch (Type.GetTypeCode(underlying))
        {
            case TypeCode.Byte: scalar = ScalarKind.Byte; return true;
            case TypeCode.SByte: scalar = ScalarKind.SByte; return true;
            case TypeCode.Int16: scalar = ScalarKind.Int16; return true;
            case TypeCode.UInt16: scalar = ScalarKind.UInt16; return true;
            case TypeCode.Int32: scalar = ScalarKind.Int32; return true;
            case TypeCode.UInt32: scalar = ScalarKind.UInt32; return true;
            case TypeCode.Int64: scalar = ScalarKind.Int64; return true;
            case TypeCode.UInt64: scalar = ScalarKind.UInt64; return true;
            case TypeCode.Boolean: scalar = ScalarKind.Bool; return true;
            case TypeCode.Single: scalar = ScalarKind.Float32; return true;
            default: scalar = default; return false;
        }
    }
}

public sealed record SourceLocation(string Path, int Line, int Column)
{
    public static SourceLocation None { get; } = new(string.Empty, 0, 0);

    public string Display => string.IsNullOrEmpty(Path)
        ? "<manifest>"
        : $"{Path}:{Line}:{Column}";
}

public enum PacketGraphNodeKind : byte
{
    PacketField,
    GameSystemInput,
    PacketCalculation,
    CodecRef
}

public enum PacketGraphEdgeKind : byte
{
    Presence,
    CalculationInput,

    // Field/system/calculation input -> codec. A calculation is legal only as a
    // single-use value consumed by a wire codec.
    CodecInput,

    // Codec -> field: the block produces the value. This is the decode side's outlet;
    // a field targeted by one is carried by the block and has no standalone wire bytes.
    CodecOutput
}

public abstract class PacketGraphNode
{
    protected PacketGraphNode(SourceLocation sourceLocation)
    {
        SourceLocation = sourceLocation ?? throw new ArgumentNullException(nameof(sourceLocation));
    }

    public SourceLocation SourceLocation { get; }

    public abstract PacketGraphNodeKind Kind { get; }
}

public abstract class PacketFieldNode : PacketGraphNode
{
    protected PacketFieldNode(
        MemberRef member,
        Type valueType,
        SourceLocation sourceLocation,
        ByteRange? lengthRange)
        : base(sourceLocation)
    {
        Member = member;
        ValueType = valueType;
        LengthRange = lengthRange;
    }

    public override PacketGraphNodeKind Kind => PacketGraphNodeKind.PacketField;

    public MemberRef Member { get; }

    public Type ValueType { get; }

    public ByteRange? LengthRange { get; }

    public bool IsVariableLength => LengthRange is not null || InfersLengthRange;

    internal virtual bool InfersLengthRange => false;

    internal PacketGraph? DeclarationGraph { get; set; }

    protected void BindMask(MemberDeclaration source, int bitIndex,
        SourceLocation? sourceLocation = null)
    {
        var graph = DeclarationGraph
            ?? throw new InvalidOperationException("BindMSK requires a field in a mutable graph.");
        graph.BindMemberPresence(this, source, bitIndex, sourceLocation);
    }

    internal static PacketFieldNode FromDeclaration(MemberDeclaration declaration,
        SourceLocation? sourceLocation, ByteRange? lengthRange = null)
    {
        ArgumentNullException.ThrowIfNull(declaration);
        return new DeclaredFieldNode(declaration, sourceLocation ?? SourceLocation.None, lengthRange);
    }

    private sealed class DeclaredFieldNode(MemberDeclaration declaration,
        SourceLocation sourceLocation, ByteRange? lengthRange)
        : PacketFieldNode(declaration.Member, declaration.ValueType, sourceLocation, lengthRange)
    {
    }
}

public sealed class PacketVariableFieldNode : PacketFieldNode
{
    internal override bool InfersLengthRange => ValueType == typeof(string)
        || ValueType == typeof(ReadOnlyMemory<byte>);
    private PacketVariableFieldNode(MemberDeclaration declaration,
        SourceLocation sourceLocation, ByteRange? lengthRange)
        : base(declaration.Member, declaration.ValueType, sourceLocation, lengthRange)
    {
    }

    internal new static PacketVariableFieldNode FromDeclaration(MemberDeclaration declaration,
        SourceLocation? sourceLocation, ByteRange? lengthRange = null)
    {
        ArgumentNullException.ThrowIfNull(declaration);
        return new(declaration, sourceLocation ?? SourceLocation.None, lengthRange);
    }

    public PacketVariableFieldNode BindMSK(MemberDeclaration source, int bitIndex,
        SourceLocation? sourceLocation = null)
    {
        BindMask(source, bitIndex, sourceLocation);
        return this;
    }
}

public sealed class PacketVariableFieldNode<TValue> : PacketFieldNode<TValue>
{
    internal override bool InfersLengthRange => ValueType == typeof(string)
        || ValueType == typeof(ReadOnlyMemory<byte>);
    internal PacketVariableFieldNode(MemberRef member, SourceLocation sourceLocation,
        ByteRange? lengthRange)
        : base(member, sourceLocation, lengthRange)
    {
    }

    public PacketVariableFieldNode<TValue> BindMSK(MemberDeclaration source, int bitIndex,
        SourceLocation? sourceLocation = null)
    {
        BindMask(source, bitIndex, sourceLocation);
        return this;
    }
}

public class PacketFieldNode<TValue> : PacketFieldNode, IWireValueNode<TValue>
{
    PacketGraphNode IWireValueNode<TValue>.Node => this;

    private protected PacketFieldNode(
        MemberRef member,
        SourceLocation sourceLocation,
        ByteRange? lengthRange)
        : base(member, typeof(TValue), sourceLocation, lengthRange)
    {
    }

    public static PacketFieldNode<TValue> Create(
        LambdaExpression selector,
        SourceLocation? sourceLocation = null)
    {
        return CreateCore(selector, null, sourceLocation);
    }

    internal static PacketVariableFieldNode<TValue> CreateVariable(
        LambdaExpression selector,
        ByteRange? lengthRange,
        SourceLocation? sourceLocation = null)
    {
        var field = CreateCore(selector, lengthRange, sourceLocation);
        return new(field.Member, field.SourceLocation, lengthRange);
    }

    private static PacketFieldNode<TValue> CreateCore(
        LambdaExpression selector,
        ByteRange? lengthRange,
        SourceLocation? sourceLocation)
    {
        ArgumentNullException.ThrowIfNull(selector);

        Expression expression = selector.Body;
        while (expression is UnaryExpression
               {
                   NodeType: ExpressionType.Convert or ExpressionType.ConvertChecked,
                   Operand: var operand
               })
        {
            expression = operand;
        }

        if (expression is not MemberExpression memberExpression
            || memberExpression.Member is not FieldInfo and not PropertyInfo
            || memberExpression.Expression is not ParameterExpression)
        {
            throw new ArgumentException(
                "Packet field selectors must directly select a field or property from the packet parameter.",
                nameof(selector));
        }

        var member = memberExpression.Member;
        var memberType = member switch
        {
            FieldInfo field => field.FieldType,
            PropertyInfo property => property.PropertyType,
            _ => throw new InvalidOperationException("The selected member is not a packet field.")
        };

        if (member.DeclaringType != selector.Parameters[0].Type)
        {
            throw new ArgumentException(
                "Packet field selectors must select a member declared by the selector parameter type.",
                nameof(selector));
        }

        if (memberType != typeof(TValue))
        {
            throw new ArgumentException(
                "The selector result type must match the selected member type.",
                nameof(selector));
        }

        return new PacketFieldNode<TValue>(
            MemberRef.From(member),
            sourceLocation ?? SourceLocation.None,
            lengthRange);
    }
}

public abstract class GameSystemInputNode : PacketGraphNode
{
    protected GameSystemInputNode(SourceLocation? sourceLocation)
        : base(sourceLocation ?? SourceLocation.None)
    {
    }

    public override PacketGraphNodeKind Kind => PacketGraphNodeKind.GameSystemInput;

    public abstract Type ValueType { get; }

    public MemberRef? ModelMember { get; internal init; }

    internal static GameSystemInputNode FromDeclaration(MemberDeclaration declaration,
        SourceLocation? sourceLocation) => new DeclaredInputNode(declaration, sourceLocation);

    private sealed class DeclaredInputNode : GameSystemInputNode
    {
        public DeclaredInputNode(MemberDeclaration declaration, SourceLocation? sourceLocation)
            : base(sourceLocation)
        {
            ValueType = declaration.ValueType;
            ModelMember = declaration.Member;
        }

        public override Type ValueType { get; }
    }
}

public sealed class GameSystemInputNode<TValue> : GameSystemInputNode, IWireValueNode<TValue>
{
    PacketGraphNode IWireValueNode<TValue>.Node => this;

    public GameSystemInputNode(SourceLocation? sourceLocation = null)
        : base(sourceLocation)
    {
    }

    public override Type ValueType => typeof(TValue);

}

public sealed class PacketCalculationNode : PacketGraphNode
{
    private PacketCalculationNode(MethodRef method, SourceLocation sourceLocation)
        : base(sourceLocation)
    {
        Method = method;
    }

    public override PacketGraphNodeKind Kind => PacketGraphNodeKind.PacketCalculation;

    public MethodRef Method { get; }

    internal static PacketCalculationNode Create(
        MethodRef? calculation,
        SourceLocation? sourceLocation = null)
    {
        return new PacketCalculationNode(
            calculation ?? CompilerMethodCatalog.Unresolved, sourceLocation ?? SourceLocation.None);
    }

}

public enum CodecDirection : byte
{
    Decode,
    Encode
}

[Flags]
public enum WireDirection : byte
{
    None = 0,
    ClientToServer = 1,
    ServerToClient = 2,
    Bidirectional = ClientToServer | ServerToClient
}

[Flags]
public enum PacketCodecDirections : byte
{
    None = 0,
    Decode = 1,
    Encode = 2,
    Bidirectional = Decode | Encode
}

// A whole-code-block codec: one named method owns an arbitrary run of wire bytes
// (loops, per-element scope, table lookups). The compiler records only its
// signature; compilation derives its position from carried fields.
public class CodecRefNode : PacketGraphNode
{
    private CodecRefNode(MethodRef? method,
        SourceLocation? sourceLocation, ByteRange? lengthRange)
        : base(sourceLocation ?? SourceLocation.None)
    {
        Method = method ?? CompilerMethodCatalog.Unresolved;
        Direction = !Method.Parameters.IsDefaultOrEmpty
            && Method.Parameters[0].Type.Matches(TypeRef.From(typeof(Wire.PacketWireReader)))
                ? CodecDirection.Decode : CodecDirection.Encode;
        LengthRange = lengthRange;
    }

    public override PacketGraphNodeKind Kind => PacketGraphNodeKind.CodecRef;

    public CodecDirection Direction { get; }

    public MethodRef Method { get; }

    // Optional bounds for the byte segment owned by this codec.
    public ByteRange? LengthRange { get; }

    // Decode results contain only values; transport positions measure byte consumption.
    public bool TryGetDecodeOutputs(out ImmutableArray<TypeRef> outputs)
    {
        if (Direction != CodecDirection.Decode
            || Method.ReturnType == TypeRef.From(typeof(void)))
        {
            outputs = ImmutableArray<TypeRef>.Empty;
            return false;
        }

        outputs = CodecTypes.IsValueTuple(Method.ReturnType, out var elements)
            ? elements
            : [Method.ReturnType];
        return true;
    }

    internal static CodecRefNode Create(
        MethodRef? codec,
        SourceLocation? sourceLocation,
        ByteRange? lengthRange)
        => new(codec, sourceLocation, lengthRange);

}

// Recognizes the value-tuple shapes a decode codec uses as its value outlet.
public static class CodecTypes
{
    public static bool IsValueTuple(TypeRef type, out ImmutableArray<TypeRef> elements)
    {
        if (type.Namespace == "System"
            && type.MetadataName.StartsWith("ValueTuple", StringComparison.Ordinal)
            && !type.GenericArguments.IsDefaultOrEmpty)
        {
            elements = type.GenericArguments.Length == 8
                && IsValueTuple(type.GenericArguments[7], out var rest)
                    ? [.. type.GenericArguments.Take(7), .. rest]
                    : type.GenericArguments;
            return true;
        }

        elements = ImmutableArray<TypeRef>.Empty;
        return false;
    }
}

public sealed class PacketGraphEdge
{
    internal PacketGraphEdge(
        PacketGraphEdgeKind kind,
        PacketGraphNode source,
        PacketGraphNode target,
        SourceLocation sourceLocation,
        int? presenceBit,
        int? ordinal)
    {
        Kind = kind;
        Source = source;
        Target = target;
        SourceLocation = sourceLocation;
        PresenceBit = presenceBit;
        Ordinal = ordinal;
    }

    public PacketGraphEdgeKind Kind { get; }

    public PacketGraphNode Source { get; }

    public PacketGraphNode Target { get; }

    public SourceLocation SourceLocation { get; }

    public int? PresenceBit { get; }

    public int? Ordinal { get; }
}

public sealed class PacketGraphManifest
{
    internal PacketGraphManifest(
        byte messageId,
        Type packetType,
        PacketCodecDirections codecDirections,
        ImmutableArray<PacketGraphNode> nodes,
        ImmutableArray<PacketGraphEdge> edges,
        SourceLocation sourceLocation,
        WireDirection wireDirection,
        ImmutableArray<CodecFieldOwnership> codecFieldOwners,
        PacketDependencySnapshot dependencyModel,
        ImmutableArray<PacketFieldNode> wireFields,
        ImmutableArray<FieldFormatBinding> fieldFormats,
        PacketConstructionBinding? construction,
        Type? externalDependencyModel = null,
        ImmutableArray<FunctionDeclarationSnapshot> functions = default,
        ImmutableArray<MemberPresenceBinding> memberPresence = default,
        ImmutableArray<ImmutableArray<MemberDeclaration>> externalDependencies = default)
    {
        MessageId = messageId;
        PacketType = packetType;
        CodecDirections = codecDirections;
        WireDirection = wireDirection;
        CodecFieldOwners = codecFieldOwners;
        DependencyModel = dependencyModel;
        Nodes = nodes;
        Edges = edges;
        SourceLocation = sourceLocation;
        WireFields = wireFields;
        FieldFormats = fieldFormats;
        Construction = construction;
        ExternalDependencyModel = externalDependencyModel;
        Functions = functions.IsDefault ? [] : functions;
        MemberPresence = memberPresence.IsDefault ? [] : memberPresence;
        ExternalDependencies = externalDependencies.IsDefault ? [] : externalDependencies;
    }

    public byte MessageId { get; }

    public Type PacketType { get; }

    public PacketCodecDirections CodecDirections { get; }

    public WireDirection WireDirection { get; }

    public ImmutableArray<CodecFieldOwnership> CodecFieldOwners { get; }

    public PacketDependencySnapshot DependencyModel { get; }

    public ImmutableArray<PacketFieldNode> WireFields { get; }
    public ImmutableArray<FieldFormatBinding> FieldFormats { get; }
    public PacketConstructionBinding? Construction { get; }
    public Type? ExternalDependencyModel { get; }

    public ImmutableArray<FunctionDeclarationSnapshot> Functions { get; }

    public ImmutableArray<MemberPresenceBinding> MemberPresence { get; }

    public ImmutableArray<ImmutableArray<MemberDeclaration>> ExternalDependencies { get; }

    public ImmutableArray<PacketGraphNode> Nodes { get; }

    public ImmutableArray<PacketGraphEdge> Edges { get; }

    public SourceLocation SourceLocation { get; }

    public string PacketId => $"{WireDirection}:{MessageId}:{PacketType.FullName ?? PacketType.Name}";
}

public sealed record CodecFieldOwnership(CodecRefNode Codec, PacketFieldNode Field);

public sealed record PresenceBindingIr(
    PacketFieldNode Source,
    PacketFieldNode Target,
    int? Bit,
    SourceLocation SourceLocation);

public sealed record FieldIr(
    PacketFieldNode Node,
    MemberRef Member,
    ScalarKind ScalarKind,
    bool IsNullable,
    int WireOrdinal,
    PresenceBindingIr? Presence,
    ByteRange? LengthRange,
    SourceLocation SourceLocation,
    ByteRange? DecodeLengthRange = null,
    ByteRange? EncodeLengthRange = null,
    int DecodeTrailingBytes = 0,
    int EncodeTrailingBytes = 0)
{
    public bool IsVariableLength => LengthRange is not null;

    public ByteRange? RangeFor(CodecDirection direction) => direction == CodecDirection.Decode
        ? DecodeLengthRange ?? LengthRange : EncodeLengthRange ?? LengthRange;

    public int? MinimumLength => LengthRange?.MinimumLength;

    public int? MaximumLength => LengthRange?.MaximumLength;
}

public sealed record PacketCalculationInputIr(
    PacketGraphNode Source,
    int Ordinal,
    TypeRef ParameterType,
    SourceLocation SourceLocation);

public sealed record PacketCalculationIr(
    PacketCalculationNode Node,
    MethodRef Method,
    ImmutableArray<PacketCalculationInputIr> Inputs,
    SourceLocation SourceLocation);

public sealed record DependencyIr(
    PacketGraphNode Source,
    PacketGraphNode Target,
    PacketGraphEdgeKind Kind,
    int? PresenceBit,
    int? Ordinal,
    SourceLocation SourceLocation);

public sealed record DecodeFieldOperation(PacketFieldNode Node, int WireOrdinal);

public sealed record EncodeFieldOperation(PacketFieldNode Node, int WireOrdinal);

public sealed record DecodeCodecOperation(CodecRefNode Node, int WireOrdinal);

public sealed record EncodeCodecOperation(CodecRefNode Node, int WireOrdinal);

public sealed record DecodeProjection(
    ImmutableArray<DecodeFieldOperation> Fields,
    ImmutableArray<DecodeCodecOperation> Codecs);

public sealed record EncodeProjection(
    ImmutableArray<EncodeFieldOperation> Fields,
    ImmutableArray<EncodeCodecOperation> Codecs);

public sealed record LoweredPacket(
    DecodeProjection Decode,
    EncodeProjection Encode);

// A bound whole-code-block codec. Inputs carry the caller-supplied arguments only:
// ordinal 0 of the codec method is the wire transport, which the compiler injects.
//
// Outputs are the decode side's value outlet. A field listed here has no standalone
// wire bytes: the block owns them. On decode the value arrives as a tuple element of
// the codec's return value; on encode the block is the only thing that writes it.
public sealed record CodecRefIr(
    CodecRefNode Node,
    MethodRef Method,
    ImmutableArray<PacketCalculationInputIr> Inputs,
    ImmutableArray<CodecOutputIr> Outputs,
    int StartOrdinal,
    int Sequence,
    SourceLocation SourceLocation,
    ImmutableArray<PacketFieldNode> OwnedFields = default,
    ByteRange? LengthRange = null,
    int MinimumTrailingBytes = 0)
{
    public ByteRange? EffectiveRange => LengthRange ?? Node.LengthRange;

    public bool IsHeadAnchored => StartOrdinal == 0;

    // The number of decoded values.
    public int OutputArity => Outputs.IsDefaultOrEmpty
        ? 0
        : Outputs.Max(static output => output.Ordinal);

    public bool Owns(PacketFieldNode field) =>
        Node.Direction == CodecDirection.Decode
            ? !Outputs.IsDefaultOrEmpty && Outputs.Any(output => ReferenceEquals(output.Target, field))
            : !OwnedFields.IsDefaultOrEmpty && OwnedFields.Any(owned => ReferenceEquals(owned, field));
}

// A field whose wire bytes are owned by a codec block rather than by the scalar plan.
public sealed record CodecOutputIr(
    PacketFieldNode Target,
    int Ordinal,
    TypeRef ValueType,
    SourceLocation SourceLocation);

public sealed record PacketIr(
    string PacketId,
    byte MessageId,
    Type PacketType,
    PacketCodecDirections CodecDirections,
    ImmutableArray<FieldIr> Fields,
    ImmutableArray<DependencyIr> Dependencies,
    ImmutableArray<PacketCalculationIr> Calculations,
    ImmutableArray<CodecRefIr> Codecs,
    LoweredPacket Lowered,
    PacketDependencySnapshot DependencyModel,
    WireDirection WireDirection = WireDirection.Bidirectional,
    ImmutableArray<FieldFormatIr> FieldFormats = default,
    PacketConstructionBinding? Construction = null,
    string? GeneratedName = null,
    Type? ExternalDependencyModel = null,
    int? BodyCapacity = null);

public sealed record GeneratedArtifact(
    string HintName,
    string PacketId,
    string Source,
    string ContentHash);

public sealed record CompilationResult(
    string InputFingerprint,
    ImmutableArray<PacketIr> Packets,
    ImmutableArray<CompilerDiagnostic> Diagnostics,
    ImmutableArray<GeneratedArtifact> Artifacts)
{
    public bool Success => Diagnostics.All(static diagnostic => !diagnostic.IsError);
}

public sealed class CompilationInputBundle
{
    public CompilationInputBundle(ImmutableArray<PacketGraphManifest> manifests, FrameLayout? frame = null)
    {
        if (manifests.IsDefault)
        {
            throw new ArgumentException("The manifest collection cannot be default.", nameof(manifests));
        }

        Manifests = manifests;
        Frame = frame;
        InputFingerprint = string.Empty;
    }

    public CompilationInputBundle(IEnumerable<PacketGraphManifest> manifests, FrameLayout? frame = null)
        : this(manifests?.ToImmutableArray() ?? throw new ArgumentNullException(nameof(manifests)), frame)
    {
    }

    public ImmutableArray<PacketGraphManifest> Manifests { get; }

    public ProtocolManifest? Protocol { get; }
    public FrameLayout? Frame { get; }

    internal CompilationInputBundle(ProtocolManifest protocol)
        : this(protocol.Packets.Select(static packet => packet.Packet), protocol.Frame) => Protocol = protocol;

    public string InputFingerprint { get; }
}

public sealed record CompilerOptions(
    string ProfileId = CompilerProfile.FixedScalarPresenceV1,
    int ProfileVersion = CompilerProfile.Version,
    string CompilerVersion = "packet-design-compiler-0.17",
    bool EmitSources = true);

public enum DiagnosticSeverity : byte
{
    Error,
    Warning
}

public enum DiagnosticPhase : byte
{
    Input,
    Aggregate,
    Bind,
    Validate,
    Lower,
    Emit
}

public sealed record CompilerDiagnostic(
    string Code,
    DiagnosticSeverity Severity,
    DiagnosticPhase Phase,
    string Message,
    string? PacketId,
    SourceLocation SourceLocation)
{
    public bool IsError => Severity == DiagnosticSeverity.Error;
}

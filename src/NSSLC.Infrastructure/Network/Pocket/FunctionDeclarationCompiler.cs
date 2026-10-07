using System.Collections.Immutable;
using System.Text;

namespace Terraria.NetWork.Prototype.PacketDesignCompiler;

internal static partial class CompilerPipeline
{
    private static PacketGraphManifest ResolveMemberDeclarations(
        PacketGraphManifest manifest, List<CompilerDiagnostic> diagnostics)
    {
        if (manifest.Functions.IsEmpty && manifest.MemberPresence.IsEmpty) return manifest;

        var nodes = manifest.Nodes.ToList();
        var edges = manifest.Edges.ToList();
        var owners = manifest.CodecFieldOwners.ToList();
        Type? externalModel = manifest.ExternalDependencyModel;

        PacketFieldNode? ResolveMemberField(MemberDeclaration member, SourceLocation location)
        {
            var fields = manifest.Nodes.OfType<PacketFieldNode>()
                .Where(field => member.DeclaringType == manifest.PacketType
                    && field.Member == member.Member && field.ValueType == member.ValueType).ToArray();
            if (fields.Length == 1) return fields[0];
            Add(diagnostics, "CPK195", DiagnosticPhase.Bind,
                $"Member '{member.DeclaringType.Name}.{member.Member.Name}' must identify exactly one declared packet field.",
                manifest, location);
            return null;
        }

        PacketFieldNode? ResolveField(FunctionArgumentDeclaration argument, SourceLocation location)
        {
            if (argument.Member is { } member) return ResolveMemberField(member, location);
            if (argument.Node is PacketFieldNode field && manifest.Nodes.Contains(field)) return field;
            Add(diagnostics, "CPK195", DiagnosticPhase.Bind,
                "ReturnValue and Sizeof require declared fields of this packet.", manifest, location);
            return null;
        }

        PacketGraphNode? ResolveMemberInput(MemberDeclaration member, SourceLocation location)
        {
            if (member.DeclaringType == manifest.PacketType) return ResolveMemberField(member, location);
            if (externalModel is not null && member.DeclaringType != externalModel)
            {
                Add(diagnostics, "CPK182", DiagnosticPhase.Bind,
                    $"Input member '{member.Member.Name}' must belong to external model '{externalModel}'.",
                    manifest, location);
                return null;
            }
            var inputs = manifest.Nodes.OfType<GameSystemInputNode>()
                .Where(input => input.ModelMember == member.Member && input.ValueType == member.ValueType).ToArray();
            if (inputs.Length == 1) return inputs[0];
            Add(diagnostics, "CPK198", DiagnosticPhase.Bind,
                $"Input '{member.Member.Name}' must be declared by ExternalDependencies or an explicit protocol input.",
                manifest, location);
            return null;
        }

        var functions = new Dictionary<int, PacketGraphNode>();
        for (int index = 0; index < manifest.Functions.Length; index++)
        {
            var function = manifest.Functions[index];
            if (function.Methods.Length != 1 || function.ParameterLists.Length > 1
                || function.ReturnValues.Length > 1 || function.SizeFields.Length > 1)
            {
                Add(diagnostics, "CPK194", DiagnosticPhase.Validate,
                    "A function declaration requires exactly one FunctionName and at most one ParameterList, ReturnValue and Sizeof.",
                    manifest, function.SourceLocation);
                continue;
            }
            var method = function.Methods[0];
            if (ReferenceEquals(method, CompilerMethodCatalog.Unresolved))
            {
                Add(diagnostics, "CPK184", DiagnosticPhase.Bind,
                    "FunctionName requires a named method with generated declaration metadata.", manifest, function.SourceLocation);
                continue;
            }
            TypeRef packetType = TypeRef.From(manifest.PacketType);
            bool isPacketMethod = method.DeclaringType.Matches(packetType);
            if ((isPacketMethod && !method.IsPartialContainingType)
                || (!isPacketMethod && !method.IsPacketSharedFunction))
            {
                Add(diagnostics, "CPK202", DiagnosticPhase.Bind,
                    $"Function '{method.Name}' must be declared on packet partial class '{manifest.PacketType.Name}' or on a type marked PacketSharedFunctionAttribute; declaring type is '{method.DeclaringType.Identity}'.",
                    manifest, function.SourceLocation);
                continue;
            }
            bool isCodec = !method.Parameters.IsEmpty
                && (method.Parameters[0].Type.Matches(TypeRef.From(typeof(Wire.PacketWireReader)))
                    || method.Parameters[0].Type.Matches(TypeRef.From(typeof(Wire.PacketWireWriter))));
            PacketGraphNode node = isCodec
                ? CodecRefNode.Create(method, function.SourceLocation, function.LengthRange)
                : PacketCalculationNode.Create(method, function.SourceLocation);
            nodes.Add(node);
            functions.Add(index, node);
        }

        for (int index = 0; index < manifest.Functions.Length; index++)
        {
            if (!functions.TryGetValue(index, out var node)) continue;
            var function = manifest.Functions[index];
            var location = function.SourceLocation;
            var method = function.Methods[0];
            var parameters = function.ParameterLists.IsEmpty ? [] : function.ParameterLists[0];
            bool isCalculation = node is PacketCalculationNode;

            for (int parameterIndex = 0; parameterIndex < parameters.Length; parameterIndex++)
            {
                var argument = parameters[parameterIndex];
                int ordinal = parameterIndex + (isCalculation ? 0 : 1);
                PacketGraphNode? input;
                if (argument.Member is { } member)
                {
                    if (ordinal < method.Parameters.Length && !string.Equals(
                        method.Parameters[ordinal].Name, member.Member.Name, StringComparison.OrdinalIgnoreCase))
                        Add(diagnostics, "CPK196", DiagnosticPhase.Validate,
                            $"Function '{method.Name}' parameter '{method.Parameters[ordinal].Name}' is bound to member '{member.Member.Name}'; parameter and member names must match ignoring case.",
                            manifest, location);
                    input = ResolveMemberInput(member, location);
                }
                else if (argument.Node is PacketFieldNode or GameSystemInputNode
                    && manifest.Nodes.Contains(argument.Node))
                    input = argument.Node;
                else if (argument.FunctionOrdinal is { } reference && functions.TryGetValue(reference, out var result)
                    && result is PacketCalculationNode)
                    input = result;
                else
                {
                    Add(diagnostics, "CPK199", DiagnosticPhase.Bind,
                        "A function parameter requires a declared member, input node or calculation declaration from this graph.",
                        manifest, location);
                    input = null;
                }
                if (input is not null)
                    edges.Add(new(isCalculation ? PacketGraphEdgeKind.CalculationInput : PacketGraphEdgeKind.CodecInput,
                        input, node, location, null, ordinal));
            }

            var sizeArguments = function.SizeFields.IsEmpty ? [] : function.SizeFields[0];
            var returnArguments = function.ReturnValues.IsEmpty ? [] : function.ReturnValues[0];
            if (isCalculation)
            {
                if (!returnArguments.IsEmpty || !function.SizeFields.IsEmpty || function.LengthRange is not null)
                    Add(diagnostics, "CPK199", DiagnosticPhase.Validate,
                        "A calculation produces an intermediate value and cannot assign packet fields, carry wire fields or declare a byte range.",
                        manifest, location);
                continue;
            }

            var codec = (CodecRefNode)node;
            var sizeFields = sizeArguments.Select(argument => ResolveField(argument, location)).OfType<PacketFieldNode>().ToArray();
            if (!function.SizeFields.IsEmpty && (sizeFields.Length == 0
                || sizeFields.Distinct(ReferenceEqualityComparer.Instance).Count() != sizeFields.Length))
                Add(diagnostics, "CPK197", DiagnosticPhase.Validate,
                    $"Function '{method.Name}' Sizeof requires distinct declared packet fields.", manifest, location);

            if (codec.Direction == CodecDirection.Encode)
            {
                if (!returnArguments.IsEmpty)
                    Add(diagnostics, "CPK133", DiagnosticPhase.Validate,
                        $"Void writer '{method.Name}' cannot bind ReturnValue fields; declare its carried fields with Sizeof.",
                        manifest, location);
                foreach (var field in sizeFields) owners.Add(new(codec, field));
            }
            else
            {
                var outputs = returnArguments.IsEmpty ? sizeFields
                    : returnArguments.Select(argument => ResolveField(argument, location)).OfType<PacketFieldNode>().ToArray();
                if (outputs.Distinct(ReferenceEqualityComparer.Instance).Count() != outputs.Length)
                    Add(diagnostics, "CPK197", DiagnosticPhase.Validate,
                        $"Reader '{method.Name}' requires distinct ReturnValue fields.", manifest, location);
                if (!function.SizeFields.IsEmpty && !returnArguments.IsEmpty
                    && (outputs.Length != sizeFields.Length || sizeFields.Any(field => !outputs.Contains(field))))
                    Add(diagnostics, "CPK197", DiagnosticPhase.Validate,
                        $"Reader '{method.Name}' ReturnValue and Sizeof must refer to the same carried fields.", manifest, location);
                for (int outputIndex = 0; outputIndex < outputs.Length; outputIndex++)
                    edges.Add(new(PacketGraphEdgeKind.CodecOutput, codec, outputs[outputIndex], location, null, outputIndex + 1));
            }
        }

        foreach (var presence in manifest.MemberPresence)
        {
            if (ResolveMemberField(presence.Source, presence.SourceLocation) is { } source)
                edges.Add(new(PacketGraphEdgeKind.Presence, source, presence.Target,
                    presence.SourceLocation, presence.BitIndex, null));
        }

        var boundNodes = nodes.ToImmutableArray();
        var boundEdges = edges.ToImmutableArray();
        var inputs = boundNodes.OfType<GameSystemInputNode>().Select((node, ordinal) =>
            new PacketProtocolDependency(node, ordinal, TypeRef.From(node.ValueType))).ToImmutableArray();
        return new PacketGraphManifest(manifest.MessageId, manifest.PacketType, manifest.CodecDirections,
            boundNodes, boundEdges, manifest.SourceLocation, manifest.WireDirection, owners.ToImmutableArray(),
            new PacketDependencySnapshot(boundNodes, boundEdges, inputs, manifest.FieldFormats),
            manifest.WireFields, manifest.FieldFormats, manifest.Construction, externalModel);
    }

    private static void AppendFunctionDeclarationFingerprint(StringBuilder builder, PacketGraphManifest manifest)
    {
        static void AppendMember(StringBuilder builder, MemberDeclaration member)
        {
            var shape = member.Member;
            builder.Append(TypeFingerprint(TypeRef.From(member.DeclaringType))).Append(':')
                .Append(TypeFingerprint(TypeRef.From(member.ValueType))).Append(':')
                .Append(TypeFingerprint(shape.DeclaringType)).Append(':').Append(shape.Name).Append(':')
                .Append(shape.Kind).Append(':').Append(TypeFingerprint(shape.ValueType)).Append(':')
                .Append(shape.IsReadable).Append(':').Append(shape.IsWritable).Append(':')
                .Append(shape.IsStatic).Append(':').Append(shape.IsIndexed).Append(';');
        }

        void AppendArgument(FunctionArgumentDeclaration argument)
        {
            if (argument.Member is { } member) AppendMember(builder, member);
            else if (argument.Node is { } node) builder.Append("node:").Append(manifest.Nodes.IndexOf(node)).Append(';');
            else builder.Append("function:").Append(argument.FunctionOrdinal).Append(';');
        }

        foreach (var list in manifest.ExternalDependencies)
        {
            builder.Append("external-dependencies:[");
            foreach (var member in list) AppendMember(builder, member);
            builder.Append(']');
        }
        foreach (var function in manifest.Functions)
        {
            builder.Append("function:");
            foreach (var method in function.Methods) AppendMethodFingerprint(builder, method);
            foreach (var (name, lists) in new[] { ("parameters", function.ParameterLists),
                ("returns", function.ReturnValues), ("size", function.SizeFields) })
            {
                builder.Append(name).Append(':');
                foreach (var list in lists)
                {
                    builder.Append('[');
                    foreach (var argument in list) AppendArgument(argument);
                    builder.Append(']');
                }
            }
            builder.Append("range:").Append(function.LengthRange?.MinimumLength).Append(':')
                .Append(function.LengthRange?.MaximumLength).Append(';');
        }
        foreach (var presence in manifest.MemberPresence)
        {
            builder.Append("member-presence:").Append(manifest.Nodes.IndexOf(presence.Target)).Append(':')
                .Append(presence.BitIndex).Append(':');
            AppendMember(builder, presence.Source);
        }
    }
}

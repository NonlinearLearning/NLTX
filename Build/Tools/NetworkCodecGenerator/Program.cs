using System.Text;
using Terraria.NetWork.Prototype.PacketDesignCompiler;
using Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

if (args.Length != 1) {
  throw new ArgumentException("Expected the generated output directory.");
}
string output = Path.GetFullPath(args[0]);
ProtocolManifest protocol = PacketAllDefinitions.CreateProtocol();
CompilationResult result = new PacketDesignCompiler().Compile(protocol);
if (!result.Success) {
  throw new InvalidOperationException(string.Join(Environment.NewLine, result.Diagnostics));
}
Directory.CreateDirectory(output);
var source = new StringBuilder();
source.AppendLine("// Generated from the frozen packet protocol. Do not edit.");
source.AppendLine("using Terraria.Network;");
source.AppendLine("namespace NSSLC.Infrastructure.Network;");
source.AppendLine("public static class TerrariaProtocolProfile {");
source.AppendLine($"  public const string Fingerprint = \"{result.InputFingerprint}\";");
source.AppendLine("  public static ProtocolProfile Create(ProtocolFacts facts) {");
source.AppendLine("    var bindings = new System.Collections.Generic.List<PacketBinding>();");
foreach (PacketIr packet in result.Packets) {
  string type = TypeRef.From(packet.PacketType).CSharpName;
  string baseName = packet.GeneratedName!;
  string artifact = result.Artifacts.Single(item => item.PacketId == packet.PacketId
      && item.Source.Contains("class " + baseName + "PacketCodecReader")).Source;
  foreach (PacketDirectionValue direction in Enum.GetValues<PacketDirectionValue>()) {
    if (((int)packet.WireDirection & (int)direction) == 0) {
      continue;
    }
    string dir = "PacketDirection." + direction;
    string dependencies = "";
    if (!packet.DependencyModel.Inputs.IsEmpty) {
      string values = string.Join(", ", packet.DependencyModel.Inputs
          .OrderBy(input => input.Name, StringComparer.Ordinal)
          .Select(input => $"facts.Get<{input.ValueType.CSharpName}>"
              + $"({packet.MessageId}, {dir}, \"{input.Name}\")"));
      dependencies = $"dependencies{packet.MessageId}_{direction}";
      source.AppendLine($"    var {dependencies} = new global::Terraria.NetWork.Generated.{baseName}Dependencies({values});");
    }
    bool readFacts = artifact.Contains("ReadOnlyMemory<byte> frame, " + baseName + "Dependencies")
        || artifact.Contains("ReadOnlyMemory<byte> buffer, " + baseName + "Dependencies");
    bool writeFacts = artifact.Contains(baseName + "PacketCodecWriter(" + baseName + "Dependencies");
    string readArguments = readFacts ? "body, " + dependencies : "body";
    string writeArguments = writeFacts ? dependencies : "";
    source.AppendLine($"    bindings.Add(new PacketBinding<{type}>({packet.MessageId}, {dir},");
    source.AppendLine($"        body => new global::Terraria.NetWork.Generated.{baseName}PacketCodecReader({readArguments}).ReadFrameDetailed(),");
    source.AppendLine($"        value => new global::Terraria.NetWork.Generated.{baseName}PacketCodecWriter({writeArguments}).TryWrite(value))); ");
  }
}
source.AppendLine("    return new ProtocolProfile(\"TerrariaV4:\" + Fingerprint + \":\" + facts.Version, \"Terraria319\", bindings);");
source.AppendLine("  }");
source.AppendLine("}");
foreach (GeneratedArtifact artifact in result.Artifacts) {
  WriteIfChanged(Path.Combine(output, artifact.HintName), artifact.Source);
}
WriteIfChanged(Path.Combine(output, "TerrariaProtocolProfile.g.cs"), source.ToString());
string manifestPath = Path.Combine(output, "network-codec-manifest.txt");
string[] generatedNames = result.Artifacts.Select(item => item.HintName)
    .Append("TerrariaProtocolProfile.g.cs").ToArray();
if (File.Exists(manifestPath)) {
  foreach (string oldName in File.ReadAllLines(manifestPath).Except(generatedNames)) {
    if (Path.GetFileName(oldName) != oldName || !oldName.EndsWith(".g.cs")) {
      throw new InvalidOperationException("Invalid generated-code manifest entry.");
    }
    File.Delete(Path.Combine(output, oldName));
  }
}
WriteIfChanged(manifestPath, string.Join(Environment.NewLine, generatedNames));
Console.WriteLine($"Network codecs: {result.Packets.Length} layouts, {result.InputFingerprint}.");

static void WriteIfChanged(string path, string source) {
  if (!File.Exists(path) || File.ReadAllText(path) != source) {
    File.WriteAllText(path, source, new UTF8Encoding(false));
  }
}

enum PacketDirectionValue {
  ClientToServer = 1,
  ServerToClient = 2
}

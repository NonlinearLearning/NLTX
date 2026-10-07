using System.Text;
using Terraria.NetWork.Prototype.PacketDesignCompiler;
using Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

if (args.Length != 1) {
  throw new ArgumentException("Expected the generated output directory.");
}
string output = Path.GetFullPath(args[0]);
// Compilation validates the global input model's shape. Values are never emitted;
// the gateway process initializes its own model with the host's real protocol tables.
_ = new ProtocolInputs(
    frameImportant: Array.Empty<bool>(),
    allowsSaveCompressionBatching: Array.Empty<bool>(),
    tileEntityCodecs: PacketTileEntityCodecsV4.Create(),
    isServer: true,
    catchableTypes: Array.Empty<bool>(),
    lifeWidthResolver: static (_, _, _) => 4,
    needsUuid: static _ => false,
    slotCount: 0,
    moduleCodecs: Packet82KnownModuleCodecsV4.Create(),
    tagEffectNpcSlotCount: 0,
    tagEffectUsesProcTimes: static _ => false);
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
source.AppendLine("    System.ArgumentNullException.ThrowIfNull(facts);");
source.AppendLine("    facts.FreezePacketInputs();");
source.AppendLine("    var bindings = new System.Collections.Generic.List<PacketBinding>();");
foreach (PacketIr packet in result.Packets) {
  string type = TypeRef.From(packet.PacketType).CSharpName;
  string registration = protocol.Packets.Single(item =>
      packet.GeneratedName == protocol.Name + "_" + item.Name).Name;
  string factory = $"global::Terraria.NetWork.Generated.{protocol.Name}Protocol.Packets.{registration}";
  foreach (PacketDirectionValue direction in Enum.GetValues<PacketDirectionValue>()) {
    if (((int)packet.WireDirection & (int)direction) == 0) {
      continue;
    }
    string dir = "PacketDirection." + direction;
    source.AppendLine($"    bindings.Add(new PacketBinding<{type}>({packet.MessageId}, {dir},");
    source.AppendLine($"        body => {factory}.CreateReader(body).ReadFrameDetailed(),");
    source.AppendLine($"        value => {factory}.CreateWriter().TryWrite(value)));");
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

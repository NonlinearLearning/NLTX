if (!TryReadArguments(args, out string scenario, out string outputRoot))
{
  Console.Error.WriteLine(
    "Usage: --scenario SC041 --output-root <Build/Verification/SC041 directory>");
  return 2;
}

Directory.CreateDirectory(outputRoot);
string result = scenario switch
{
  "SC041" => Sc041ChestConcurrencyScenario.Run(),
  _ => throw new InvalidOperationException($"Unknown scenario '{scenario}'.")
};

string resultPath = Path.Combine(outputRoot, $"{scenario}-result.md");
File.WriteAllText(resultPath, result);
Console.WriteLine($"{scenario} PASS");
Console.WriteLine($"Evidence: {resultPath}");
return 0;

static bool TryReadArguments(
  IReadOnlyList<string> args,
  out string scenario,
  out string outputRoot)
{
  scenario = string.Empty;
  outputRoot = string.Empty;
  if (args.Count != 4
    || args[0] != "--scenario"
    || args[2] != "--output-root"
    || string.IsNullOrWhiteSpace(args[1])
    || string.IsNullOrWhiteSpace(args[3]))
  {
    return false;
  }

  scenario = args[1];
  outputRoot = Path.GetFullPath(args[3]);
  return scenario == "SC041";
}

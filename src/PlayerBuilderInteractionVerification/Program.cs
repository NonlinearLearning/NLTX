using System.Reflection;

using Terraria.Player;

static void Require(bool condition, string message)
{
  if (!condition)
  {
    throw new InvalidOperationException(message);
  }
}

Assembly playerAssembly = typeof(PlayerInputCommand).Assembly;
Type catalogType = playerAssembly.GetType(
  "Terraria.Player.PlayerBuilderInteractionCatalog")
  ?? throw new InvalidOperationException("The builder interaction catalog is missing.");
Type queryType = playerAssembly.GetType(
  "Terraria.Player.BuilderInteractionDefinitionQuery")
  ?? throw new InvalidOperationException("The builder interaction definition query is missing.");

(string Name, int Value)[] expectedToggles =
[
  ("RulerLine", 0),
  ("RulerGrid", 1),
  ("AutoActuate", 2),
  ("AutoPaint", 3),
  ("WireVisibility_Red", 4),
  ("WireVisibility_Green", 5),
  ("WireVisibility_Blue", 6),
  ("WireVisibility_Yellow", 7),
  ("HideAllWires", 8),
  ("WireVisibility_Actuators", 9),
  ("BlockSwap", 10),
  ("TorchBiome", 11),
];

foreach ((string name, int value) in expectedToggles)
{
  FieldInfo field = catalogType.GetField(name, BindingFlags.Public | BindingFlags.Static)
    ?? throw new InvalidOperationException($"The catalog field '{name}' is missing.");
  Require(field.IsLiteral, $"The catalog field '{name}' must be a constant definition.");
  Require(
    (int)field.GetRawConstantValue()! == value,
    $"The catalog field '{name}' has an unexpected value.");
}

FieldInfo countField = catalogType.GetField(
    "Count",
    BindingFlags.Public | BindingFlags.Static)
  ?? throw new InvalidOperationException("The catalog Count field is missing.");
Require(!countField.IsLiteral, "Count must preserve the Version4 static readonly shape.");
Require((int)countField.GetValue(null)! == expectedToggles.Length, "Count must equal the toggle count.");

MethodInfo isKnownToggleId = queryType.GetMethods(BindingFlags.Public | BindingFlags.Static)
  .SingleOrDefault(method =>
    method.Name == "IsKnownToggleId" &&
    method.ReturnType == typeof(bool) &&
    method.GetParameters() is [{ ParameterType: var parameterType }] &&
    parameterType == typeof(int))
  ?? throw new InvalidOperationException("The known-toggle query is missing.");
Require((bool)isKnownToggleId.Invoke(null, [0])!, "The first toggle ID must be accepted.");
Require((bool)isKnownToggleId.Invoke(null, [11])!, "The last toggle ID must be accepted.");
Require(!(bool)isKnownToggleId.Invoke(null, [-1])!, "Negative toggle IDs must be rejected.");
Require(!(bool)isKnownToggleId.Invoke(null, [12])!, "IDs at Count must be rejected.");

Console.WriteLine("PASS: builder interaction catalog definitions and pure ID query");

using System;
using Terraria.Dome.Simulation.WorldGeneration;

AssertEqual(new TileFontMeasurement(0, 5), TileFontMeasurementQuery.Measure(string.Empty));
AssertEqual(new TileFontMeasurement(5, 5), TileFontMeasurementQuery.Measure("A"));
AssertEqual(new TileFontMeasurement(11, 5), TileFontMeasurementQuery.Measure("AB"));
AssertEqual(new TileFontMeasurement(5, 11), TileFontMeasurementQuery.Measure("A\nB"));
AssertEqual(new TileFontMeasurement(11, 5), TileFontMeasurementQuery.Measure("A?B"));
AssertEqual(new TileFontMeasurement(0, 11), TileFontMeasurementQuery.Measure("\n?"));

try
{
  _ = TileFontMeasurementQuery.Measure(null!);
  throw new InvalidOperationException("Null text was accepted.");
}
catch (ArgumentNullException)
{
}

Console.WriteLine("PASS: TileFont measurement contract");

static void AssertEqual(TileFontMeasurement expected, TileFontMeasurement actual)
{
  if (expected != actual)
  {
    throw new InvalidOperationException(
      $"Expected {expected}, received {actual}.");
  }
}

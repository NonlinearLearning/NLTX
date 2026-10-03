using System;
using System.Linq;
using NSSLC.WorldGeneration.Localization;

namespace NSSLC.WorldGeneration;

/// <summary>Text keys for generation progress in a host without localization services.</summary>
public static class Language {
  public static string GetTextValue(string key, params object[] arguments) => key;
  public static LocalizedText GetText(string key) => new LocalizedText { Key = key };
  public static LocalizedText RandomFromCategory(string category, Utilities.UnifiedRandom random = null) => GetText(category);
}

public static class Lang {
  public static LocalizedText[] gen = CreateTexts("Generation");
  public static LocalizedText[] misc = CreateTexts("Misc");
  public static LocalizedText[] inter = CreateTexts("Interaction");

  private static LocalizedText[] CreateTexts(string prefix) {
    return Enumerable.Range(0, 2000)
      .Select(index => new LocalizedText { Key = prefix + "." + index }).ToArray();
  }
}

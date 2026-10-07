namespace Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

public sealed record NetworkText(
    NetworkTextMode Mode,
    string Text,
    IReadOnlyList<NetworkText> Substitutions)
{
    public static NetworkText Literal(string text) => new(
        NetworkTextMode.Literal,
        text ?? throw new ArgumentNullException(nameof(text)),
        []);

    public static NetworkText Formattable(string text, params NetworkText[] substitutions) =>
        new(
            NetworkTextMode.Formattable,
            text ?? throw new ArgumentNullException(nameof(text)),
            CopySubstitutions(substitutions));

    public static NetworkText Key(string key, params NetworkText[] substitutions) =>
        new(
            NetworkTextMode.LocalizationKey,
            key ?? throw new ArgumentNullException(nameof(key)),
            CopySubstitutions(substitutions));

    /// <summary>
    /// Returns the display text that a compiler-owned pure calculation can consume.
    /// Localization keys remain keys because the packet compiler does not own an
    /// application localization catalog.
    /// </summary>
    public string ToDisplayString(
        Func<string, IReadOnlyList<string>, string>? resolveLocalizationKey = null)
    {
        string[] substitutions = Substitutions
            .Select(value => value.ToDisplayString(resolveLocalizationKey))
            .ToArray();
        return Mode switch
        {
            NetworkTextMode.Literal => Text,
            NetworkTextMode.Formattable => Format(Text, substitutions),
            NetworkTextMode.LocalizationKey => resolveLocalizationKey is null
                ? Text
                : resolveLocalizationKey(Text, substitutions),
            _ => Text
        };
    }

    public override string ToString() => ToDisplayString();

    private static NetworkText[] CopySubstitutions(NetworkText[] substitutions)
    {
        ArgumentNullException.ThrowIfNull(substitutions);
        if (substitutions.Any(static value => value is null))
            throw new ArgumentException("NetworkText substitutions cannot contain null values.", nameof(substitutions));
        return substitutions.ToArray();
    }

    private static string Format(string template, string[] substitutions)
    {
        try
        {
            return string.Format(template, substitutions.Cast<object>().ToArray());
        }
        catch (FormatException)
        {
            return template;
        }
    }
}

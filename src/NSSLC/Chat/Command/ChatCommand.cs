namespace Terraria.Unshared.Chat;

/// <summary>One chat command the server understands, as registered in the catalog.</summary>
/// <remarks>
/// Version4 declares commands with a <c>ChatCommandAttribute</c> name and resolves them through the
/// client's localized command table. The server keeps the same name/description pair plus the one
/// permission bit it can enforce: administrator-only commands.
/// </remarks>
public sealed record ChatCommand(string Name, string Description, bool RequiresOperator)
{
  public bool IsValid => Name.Length > 0 && Description.Length > 0;
}

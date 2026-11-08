namespace Terraria.Chat;

/// <summary>RGB color of one chat line, matching the reference's <c>WriteRGB</c> byte triple.</summary>
/// <remarks>
/// The presets are the exact values of Version4 <c>Terraria.Chat.ChatColors</c>. The client sends
/// color as three bytes inside the text module frame, so the server never needs a graphics type.
/// </remarks>
public readonly record struct ChatColor(byte R, byte G, byte B)
{
  public const byte DefaultR = 255;
  public const byte DefaultG = 255;
  public const byte DefaultB = 255;

  /// <summary>Version4 <c>ChatColors.BossOrEvent</c>.</summary>
  public static ChatColor BossOrEvent => new(175, 75, 255);

  /// <summary>Version4 <c>ChatColors.World</c>.</summary>
  public static ChatColor World => new(50, 255, 130);

  /// <summary>Version4 <c>ChatColors.NPCTravel</c>.</summary>
  public static ChatColor NpcTravel => new(50, 125, 255);

  /// <summary>Version4 <c>ChatColors.ServerMessage</c>.</summary>
  public static ChatColor ServerMessage => new(255, 240, 20);

  /// <summary>Version4 <c>ChatColors.Death</c>.</summary>
  public static ChatColor Death => new(255, 25, 25);

  /// <summary>Player chat renders in the client's default white.</summary>
  public static ChatColor PlayerText => new(DefaultR, DefaultG, DefaultB);
}

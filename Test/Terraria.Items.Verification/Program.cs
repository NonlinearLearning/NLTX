using Terraria.Items.Verification;

string mode = args.FirstOrDefault() ?? "items-wire";
switch (mode)
{
  case "items-wire":
    ItemPacketWireVerification.Run();
    break;
  case "items-domain":
    WorldItemReservationVerification.Run();
    WorldItemMotionVerification.Run();
    break;
  case "all":
    ItemPacketWireVerification.Run();
    WorldItemReservationVerification.Run();
    WorldItemMotionVerification.Run();
    break;
  default:
    throw new ArgumentException($"Unknown verification mode '{mode}'.", nameof(args));
}

Console.WriteLine($"Terraria.Items verification '{mode}' passed.");

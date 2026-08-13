using System;
using Terraria.Dome.Server;

int port = args.Length == 1 ? int.Parse(args[0]) : 0;
using DomeServer server = new();
server.Start(port);
Console.WriteLine($"Terraria Dome server listening on {server.Port}.");
Console.ReadLine();

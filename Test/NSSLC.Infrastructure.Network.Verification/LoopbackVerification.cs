using System.Net;
using NSSLC.Infrastructure.Network;
using Terraria.Network;

namespace NSSLC.NetworkVerification;

internal static class LoopbackVerification {
  public static async Task RunAsync() {
    var profile = new ProtocolProfile("loopback", "Terraria319", new[] {
      BindingVerification.CreateBinding(13, PacketDirection.ClientToServer),
      BindingVerification.CreateBinding(13, PacketDirection.ServerToClient)
    });
    var serverBudget = new PacketByteBudget(1024 * 1024);
    var clientBudget = new PacketByteBudget(1024 * 1024);
    var served = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
    await using var server = new PacketTcpServer(IPAddress.Loopback, 0, profile,
        async (connection, cancellation) => {
          try {
            for (int expected = 0; expected < 8; expected++) {
              PacketMessage message = (await connection.ReadPacketAsync(cancellation))!;
              Verify.That(message.Get<BindingVerification.BytePacket>().Value == expected,
                  "The server must receive the loopback stream in original frame order.");
              await connection.WritePacketAsync(new BindingVerification.BytePacket((byte)(expected + 1)),
                  cancellation);
            }
            served.TrySetResult();
          } catch (Exception error) {
            served.TrySetException(error);
            throw;
          }
        }, budget: serverBudget);
    server.Start();
    int port = ((IPEndPoint)server.EndPoint).Port;
    Verify.That(port != 0, "An ephemeral listener must expose its bound port.");
    var factory = new PacketConnectionFactory(profile, budget: clientBudget);
    await using (IPacketConnection client = await factory.ConnectAsync("127.0.0.1", port,
        TimeSpan.FromSeconds(5))) {
      for (int value = 0; value < 8; value++) {
        PacketWriteReceipt receipt = await client.WritePacketAsync(
            new BindingVerification.BytePacket((byte)value));
        PacketMessage reply = (await client.ReadPacketAsync())!;
        Verify.That(receipt.FrameBytes == 4 && receipt.Sequence == value + 1
            && reply.Get<BindingVerification.BytePacket>().Value == value + 1,
            "A real NetCoreServer connection must provide framed typed replies and sent receipts.");
      }
      await served.Task.WaitAsync(TimeSpan.FromSeconds(5));
    }
    await Verify.EventuallyAsync(() => serverBudget.Used == 0 && clientBudget.Used == 0,
        "Loopback shutdown must return every network byte reservation.");
    Verify.That(server.LastError is null, "The loopback server must not record an unexpected error.");
  }
}

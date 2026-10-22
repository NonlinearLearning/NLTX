using System;
using System.Threading;
using System.Threading.Tasks;
using MinimalImplementation.Server.Host;

namespace MinimalImplementation.Server;

internal static class Program
{
  public static async Task<int> Main(string[] args)
  {
    if (P08BServerConfiguration.IsHelpRequested(args))
    {
      P08BServerConfiguration.WriteUsage(Console.Out);
      return 0;
    }

    try
    {
      P08BServerConfiguration configuration = P08BServerConfiguration.Parse(args);
      ServerLog.Info(
        "server.starting",
        $"listen={configuration.ListenAddress}:{configuration.ListenPort} " +
        $"world={ServerLog.Quote(configuration.WorldFilePath)} " +
        $"createWorld={configuration.CreateWorld}");
      await using var runtime = new P08BServerRuntime(configuration);
      P08BStartResult start = await runtime.StartAsync().ConfigureAwait(false);
      if (!start.IsReady)
      {
        ServerLog.Warning(
          "server.start.blocked",
          $"blockers={ServerLog.Quote(string.Join("; ", start.Blockers))} " +
          $"worldLoaded={start.WorldLoaded} worldPublished={start.WorldPublished} " +
          $"listenerStarted={start.ListenerStarted} tickStarted={start.TickStarted} " +
          $"saveReady={start.SaveReady} exitCode={start.ExitCode}");
        if (start.WorldPublished && start.SaveReady)
        {
          P08BStopResult diagnosticStop = await runtime.StopAsync(
            configuration.SaveOnStop,
            configuration.StopDeadline).ConfigureAwait(false);
          ServerLog.Info(
            "server.diagnostic_stop",
            $"succeeded={diagnosticStop.Succeeded} stage={diagnosticStop.Stage} " +
            $"saveCommitted={diagnosticStop.SaveResult?.IsCommitted ?? false}");
          if (!diagnosticStop.Succeeded)
          {
            ServerLog.Error(
              "server.diagnostic_stop.failed",
              diagnosticStop.Detail ?? "Diagnostic stop failed without a detail message.");
            return 5;
          }
        }

        return start.ExitCode;
      }

      ServerLog.Info(
        "server.ready",
        $"listen={configuration.ListenAddress}:{configuration.ListenPort} " +
        $"world={ServerLog.Quote(configuration.WorldFilePath)}");
      using var shutdownSource = new CancellationTokenSource();
      ConsoleCancelEventHandler cancelHandler = (_, eventArgs) =>
      {
        eventArgs.Cancel = true;
        shutdownSource.Cancel();
      };
      Console.CancelKeyPress += cancelHandler;
      try
      {
        await runtime.WaitForShutdownAsync(shutdownSource.Token).ConfigureAwait(false);
      }
      catch (OperationCanceledException) when (shutdownSource.IsCancellationRequested)
      {
      }
      finally
      {
        Console.CancelKeyPress -= cancelHandler;
      }

      P08BStopResult stop = await runtime.StopAsync(
        configuration.SaveOnStop,
        configuration.StopDeadline).ConfigureAwait(false);
      if (!stop.Succeeded)
      {
        ServerLog.Error(
          "server.stop.failed",
          $"stage={stop.Stage} detail={ServerLog.Quote(stop.Detail ?? string.Empty)}");
        return 5;
      }

      ServerLog.Info(
        "server.stopped",
        $"stage={stop.Stage} saveCommitted={stop.SaveResult?.IsCommitted ?? false}");
      return 0;
    }
    catch (ArgumentException exception)
    {
      ServerLog.Error("server.configuration.invalid", exception);
      P08BServerConfiguration.WriteUsage(Console.Error);
      return 2;
    }
    catch (OperationCanceledException)
    {
      ServerLog.Warning("server.canceled", "Server startup or shutdown was canceled.");
      return 4;
    }
    catch (Exception exception)
    {
      ServerLog.Error("server.failed", exception);
      return 1;
    }
  }
}

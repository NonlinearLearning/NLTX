# V319 World Import Runbook

The local acceptance verifier is opt-in. It opens the configured `.wld` file with
`FileAccess.Read` and `FileShare.Read`, parses/projects it before creating a server,
then starts and disposes a server on a temporary loopback port. It never uses 7777 or
7778.

Run from the repository root:

```powershell
$env:TERRARIA_WLD_ACCEPTANCE_PATH = 'C:\Users\shan\Documents\My Games\Terraria\Worlds\test.wld'
dotnet run --project .\Test\Terraria.Dome.WorldImport.Verification\Terraria.Dome.WorldImport.Verification.csproj -p:UseSharedCompilation=false
```

The verifier must report a passing strict import-options and no-listener lifecycle.
Without `TERRARIA_WLD_ACCEPTANCE_PATH`, the local-world part is skipped.
For a configured v319 world it also compares parser and compatibility state at
`(0, 0)`, `(0, height - 1)`, `(width - 1, 0)`,
`(width - 1, height - 1)`, and `(width / 2, height / 2)`. The verifier checks
all normalized tile fields and logs neither coordinates nor world content.

For a manual server start, first check port 7777 without changing any process:

```powershell
Get-NetTCPConnection -LocalPort 7777 -ErrorAction SilentlyContinue
```

If a listener exists, obtain explicit authorization from its owner before taking any
action. This repository does not stop or replace an external listener automatically.

After that check confirms 7777 is available, start the import server:

```powershell
dotnet run --project .\src\Terraria.Dome.Server\Terraria.Dome.Server.csproj -- --world 'C:\Users\shan\Documents\My Games\Terraria\Worlds\test.wld' --port 7777
```

Startup rejects missing, duplicate, relative, malformed, and unsupported world input,
as well as port 7778, before constructing `DomeServer` or binding a listener.

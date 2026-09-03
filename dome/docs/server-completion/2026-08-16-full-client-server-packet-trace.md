# Full Client and Full Server Packet Trace

Date: 2026-08-16

## Runtime Provenance

The capture used the source-built Terraria client and the executable complete-server
baseline for the same 1.4.5.6 protocol version:

| Role | Artifact | Evidence |
| --- | --- | --- |
| Client | `D:\TRbackup\客户端\bin\Debug\net40\Terraria.exe` | SHA-256 `348FC8C35E18A47C5472DEBE6347DD065FCCFB05D989DD3527F71D81AC8171B` |
| Server | `D:\TRbackup\无任何删减通过编译\bin\Debug\net40\TerrariaServer.exe` | SHA-256 `5DF17C809726AF6EF72EDB893495BAA21DFA5650E29A4C2CC9B39DECD8069F42`, file version `1.4.5.6` |
| Protocol source index | `D:\TRbackup\Version4物理删除了某些文件\Terraria\NetMessage.cs` | SHA-256 `87B596BD8467B9C470DD1F2AD6963EBADE257689F87395163AB136C149BEBC9A`; source was read only |
| Protocol source index | `D:\TRbackup\Version4物理删除了某些文件\Terraria\MessageBuffer.cs` | SHA-256 `0F474225FA9D98C539F61249619273A44E73F69A4CF388B44258433A2D15D5EE`; source was read only |

`Version4物理删除了某些文件` is not treated as a runnable complete server. It is a
physically reduced source tree and has no complete server project or executable. Its two
retained network files are used only to map observed message IDs to `case` line numbers.

## Isolated Run

The complete server was started with:

```text
TerrariaServer.exe -config Build\diagnostics\full-client-server-20260816\serverconfig.txt -savedirectory Build\diagnostics\full-client-server-20260816\server-save -logerrors
```

The configuration generated a separate small world, disabled UPnP, and used backend port
`7797`. The lossless recorder listened on `127.0.0.1:7798` and forwarded bytes to `7797`.
The client was started from its own output directory, where `Content` and
`ReLogic.Native.dll` are present:

```text
Terraria.exe -savedirectory Build\diagnostics\full-client-server-20260816\client-save -join 127.0.0.1 -port 7798 -testautomation join-stable -testresult Build\diagnostics\full-client-server-20260816\client\join-stable.json -logerrors
```

The Dome runtime on `7777/7778` was left untouched. The temporary server and recorder
processes were stopped by their exact process IDs after the client finished.

## Captured Artifacts

All artifacts are under `Build\diagnostics\full-client-server-20260816`:

- `trace.jsonl`: 1,564 ordered JSONL transport/frame events. Every frame contains the full
  original frame hex and full payload hex; no preview or truncation field is written.
- `summary.json`: validated aggregate and Version4 source-case index for 168 message IDs.
- `summary.md`: human-readable direction/ID/count/length table.
- `client\join-stable.json`: successful complete-client stability result.
- `server.stdout.log` and `server.stderr.log`: complete-server startup and runtime output.
- `client.stdout.log` and `client.stderr.log`: client runtime output.
- `serverconfig.txt`: exact isolated world/port configuration.

The successful client result is:

```json
{
  "scenario": "join-stable",
  "success": true,
  "message": "Client remained connected with an active local player.",
  "elapsedMilliseconds": 25364,
  "playerSlot": 0,
  "netMode": 1
}
```

## Independent Frame Audit

The raw JSONL was independently parsed after both processes stopped:

| Check | Result |
| --- | ---: |
| Total events | 1,564 |
| Complete frames | 1,557 |
| C2S frames | 360 |
| S2C frames | 1,197 |
| Maximum frame length | 11,182 bytes |
| Payloads longer than 64 bytes | 18 |
| Invalid length prefix / frame length / message ID / payload mismatch | 0 |
| `preview` fields | 0 |
| Connections | 2 |

Connection 1 is the intentionally preserved preflight attempt that connected and then
closed before sending frames because the first automation launch omitted `-testresult`.
Connection 2 is the complete successful session: 1,557 frames, 360 C2S and 1,197 S2C,
followed by the client's normal close. No frame from connection 1 is silently mixed into
the successful-session counts.

The recorder also escaped Windows socket messages with JSON `ensure_ascii`, so the final
socket-close events remain readable and cannot terminate the recorder with an ASCII encoding
exception.

## Reproduction and Verification

From the repository root:

```text
python Build\diagnostics\test_full_session_trace.py
python Build\diagnostics\summarize_full_session_trace.py Build\diagnostics\full-client-server-20260816\trace.jsonl --source-root D:\TRbackup\Version4物理删除了某些文件 --json-out Build\diagnostics\full-client-server-20260816\summary.json --markdown-out Build\diagnostics\full-client-server-20260816\summary.md
```

The unit suite passed all six tests. The summarizer reported `1557` complete frames and
`0` invalid frames. The independent audit separately rechecked every frame's little-endian
length prefix, full hex length, message ID and payload bytes.

This is a complete transport transcript for the observed world-entry/stability session. It
does not claim that every possible gameplay action or every message ID was exercised in one
25-second run; the source index and raw trace preserve the exact evidence needed for later
message-specific sessions.

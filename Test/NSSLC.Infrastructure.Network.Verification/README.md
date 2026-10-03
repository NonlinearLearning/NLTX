# Network verification

Independent verification of `NSSLC.Infrastructure.Network`, using a console verifier
consistent with the repository's existing verification projects.

The implementation is verified against the network design's observable contracts:

- Framing: lengths 3/65535, every split boundary, multiple frames in one callback,
  invalid length, body truncation and trailing data, clean EOF and partial EOF.
- Ownership: callback input may be reused immediately; decoded values retain their data.
- Sending: serialized concurrent writes, partial and synchronous sent callbacks,
  accepted versus locally sent, failed submission, disconnect outcome certainty,
  cancellation before and after submission, item/byte capacity and timeout cleanup.
- Admission: phase/direction allowlists, opaque and module rejection before decoding,
  authoritative sender binding, application-confirmed transitions and old epoch events.
- Routing: current target epochs, sender inclusion/exclusion, selected targets,
  section subscriptions and failure isolation for a slow recipient.
- Cache: input/output ownership, profile/world/revision keys, TTL, bounded eviction,
  explicit invalidation and rejection of private or sensitive entries.
- Reconnect: finite attempts, jitter caps, total deadline, terminal failures,
  lifecycle cancellation, fresh epochs and no packet replay.
- Integration: generated packet bodies use one frame header, correct directional
  codecs, a real NetCoreServer loopback connection and controlled server shutdown.

Build only this project from the repository root, then run with `--no-build --no-restore`.
All build outputs inherit `Directory.Build.props` and belong under `Build/bin/`.
Command results and limitations are recorded in the implementation verification report.

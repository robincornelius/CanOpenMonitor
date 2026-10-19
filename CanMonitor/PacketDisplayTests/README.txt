Packet display regression tests
===============================

Run from PowerShell with Visual Studio MSBuild and the .NET Framework 4.8
 targeting pack installed:

    ./run-tests.ps1

This rebuilds and runs x64 and x86, Debug and Release. To additionally test
real CanMonitor documents, first build the application, then run one matching
variant, for example:

    ./run-tests.ps1 -Platform x64 -Configuration Release -ApplicationDirectory ../bin/x64/Release

The executable uses synthetic packets; it does not open a CAN adapter. Tests
cover ordered bursts, concurrent producers, blocked and failing decoders,
Clear during decoding, payload ownership, filtering, retention, XML round
trips, actual WinForms virtual-list retrieval and message-pump progress.
Application integration also checks event unsubscription and export during
incoming traffic. A stalled test process exits unsuccessfully after 90 seconds.

Display behaviour
-----------------
Capture callbacks copy packet metadata and payload into an unbounded queue.
A document-owned worker filters and formats entries. The UI drains bounded
batches into a virtual ListView; it does not create controls per captured packet.
Pending packets are never discarded because of load. Sustained overload can
therefore increase backlog and memory use. The status bar exposes pending
formatting/display counts and decoder failures. The existing retained-line
setting still controls visible history; unlimited retention remains unlimited.

The UI drain budget is cooperative: native control operations can exceed it.
A blocked plugin decoder stalls that document's formatting worker, but does
not block the capture callback or UI. Closing a document does not wait for a
blocked plugin; its background worker exits when the callback returns.
Plugin decoders must support calls from a worker thread and must not manipulate
WinForms controls directly. Decoder payload arrays are private copies.

Clear starts a new display session, so an old in-flight decoder cannot restore
cleared entries. File export snapshots retained rows at the time it starts;
pending packets are not included. Rotation retires only the exported prefix,
preserving new rows and pending traffic. Closing the application waits
asynchronously for an outstanding export.

Packet records retain typed COB-ID, kind, timestamp and payload metadata as a
seam for a future expression filter. Current type toggles filter before costly
decoding and apply to queued/live traffic. They do not retroactively restore
previously excluded traffic. A full capture catalogue and expression language
are separate work.

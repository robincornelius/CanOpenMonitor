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
incoming traffic. A stalled test process exits unsuccessfully after 180 seconds;
the suite includes two native WinForms message-pump stress runs.

Display behaviour
-----------------
Capture callbacks copy packet metadata and payload into an unbounded queue.
A document-owned worker formats entries. The UI drains bounded
batches into a virtual ListView; it does not create controls per captured packet.
Pending packets are never discarded because of load. Sustained overload can
therefore increase backlog and memory use. The status bar exposes pending
formatting/display counts and decoder failures. The existing retained-line
setting controls retained history, including entries hidden by display filters;
unlimited retention remains unlimited.

The UI drain budget is cooperative: native control operations can exceed it.
A blocked plugin decoder stalls that document's formatting worker, but does
not block the capture callback or UI. Closing a document does not wait for a
blocked plugin; its background worker exits when the callback returns.
Plugin decoders must support calls from a worker thread and must not manipulate
WinForms controls directly. Decoder payload arrays are private copies.

Clear starts a new display session, so an old in-flight decoder cannot restore
cleared entries. Manual file export snapshots matching retained rows at the
time it starts. Automatic rotation exports all retained rows independently of
display filters. Pending packets are not included. Rotation retires only the exported prefix,
preserving new rows and pending traffic. Closing the application waits
asynchronously for an outstanding export.

Display expressions
-------------------
Enter an expression in the log's filter bar and press Enter or Apply. Clear
filter removes the expression; protocol checkboxes still apply. Right-click a
packet anywhere across the row to include/exclude its type and COB-ID, or to
exclude its COB-ID across types. Exclusions append to the applied expression
with parentheses preserving its meaning. Show only replaces the expression.
Edits are inactive until applied. Invalid syntax retains the last valid filter
and displays an error position. Expressions are per-document and not persisted.

Examples:

    not (type == PDO and cobid == 0x182)
    type = PDO and (node == 2 or node == 3)
    length >= 4 and data[0] == 0x80
    type == SDO and sdo.index == 0x2000 and sdo.subindex == 1
    emcy.code != 0
    info contains "abort"
    payload contains "8012"

Fields: type; cobid/cob (11-bit CAN-ID); node/nodeid (standard CANopen COB-ID
low seven bits, or the NMT target byte); length/len; data[0] through data[7];
sdo.index and sdo.subindex (standard expedited/segmented initiate and abort
packets, never segment payload); emcy.code; payload (continuous hex); info
(decoded description). Types: PDO, SDO, NMT, NMTEC/HEARTBEAT, EMCY. NMTEC
includes boot-up packets. PDO node matching assumes standard CANopen mapping;
use cobid for custom PDO mappings. Short/absent fields do not match even !=;
use not (...) to also include packets lacking that field.

Numbers accept decimal and 0x hexadecimal; numeric operators are =, ==, !=,
<, <=, >, >=. Type supports =, ==, !=. Text supports =, ==, !=, contains;
info/payload values must be double quoted. Text comparisons ignore case;
backslashes and quotes can be escaped with a backslash. Boolean operators are
not, and, or (precedence in that order), also !, &&, ||, with parentheses.
Expressions are limited to 4096 characters and nesting to 64 levels.

Expressions and protocol toggles apply to retained history and incoming rows.
A sequence-index projection is rebuilt in bounded UI batches without copying
or re-decoding the entire history. Status shows shown/retained counts and
filtering progress. Save during rebuilding asks the user to retry after it
finishes, avoiding an incomplete projection export. Filters never drop pending
entries or affect library/plugin packet delivery. To permit reversible display
filtering, protocol-hidden traffic is now decoded and retained too. Existing
plugin decoders that return null still suppress rows permanently; changing a
display expression does not rerun those plugins or recover their suppressed rows.

Filter regressions cover grammar/precedence, numeric bounds, missing fields,
SDO direction and segment handling, context composition, retained-ring wrap,
Clear, live filtering and exported-prefix retirement. Actual application tests
also exercise Apply, invalid input, checkboxes, the context exclusion handler,
and filtered export. The synthetic --demo mode opens the actual docked log at
about 20,000 PDO packets/second, with no CAN adapter access.

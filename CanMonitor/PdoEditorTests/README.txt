PDO editor window regressions
=============================

Run ./run-tests.ps1 to rebuild and test x64/x86 Debug/Release, or select
-Platform and -Configuration. Prerequisites match the application: Visual Studio
MSBuild, .NET Framework 4.8.1 targeting pack and restored libEDSsharp dependencies.
The tests link the production dialog/model sources and use a fake device. No
CAN driver is opened. A stalled test process exits after 60 seconds.

Coverage: TX/RX catalog, PDO direction eligibility, decoded EDS object names,
unknown existing mappings, numeric widths, 64-bit payload validation, typed
communication/mapping writes, mapping count, actual native WinForms controls,
Save/Revert, read-only mappings, failed saves preserving drafts, stale device
values, fresh reads and prevention of closing during a configuration operation.

For a visual preview without hardware:
    ./build/x64/Debug/PdoEditorTests.exe --demo

Device OD Editor now has an Edit PDOs... menu item. It requires an open CAN
connection, selected node and loaded EDS/XDD/DCF dictionary. PDO choices come
from its enabled communication records. Communication current/draft values
are shown together. Read-only, unsupported and reserved parameters cannot be
edited. Numeric drafts accept decimal or 0x hexadecimal. Inhibit time is in
100 microsecond units and event time in milliseconds.

The mapping grid shows current raw descriptors and decoded index/subindex/name,
plus draft object dropdowns, generated raw descriptors and bit lengths. Choices
respect the dictionary's transmit/receive PDO capability and datatype size.
Unknown existing mappings remain visible/preservable but are not offered as
new targets. Add/Remove/Move controls update the final mapping count; the draft
must fit the 64-bit classic-CAN PDO payload. Fixed mappings are displayed read-only.

Save to device checks current values again, applies only changed parameters
through PdoConfiguration.ApplyAsync, and rereads the device after success.
Mapping changes include all draft mapping entries plus subindex-zero count.
The library performs pre-op/disable/remap/enable/operational sequencing. Opening
or reading the window does not change NMT or write parameters. The parent OD
editor pauses auto-refresh while this modal window is open.

Revert restores the last successful read without hardware I/O. Read device
refreshes that baseline after confirming discard of any edits. Closing or
switching PDO also asks before discarding a draft. Save failures retain the
draft and show the error; they are never retried automatically. Save does not
store configuration to nonvolatile memory. Disabled heartbeat produces an
explicit unconfirmed-NMT status, as in the underlying configuration API.

The x86 test executable builds the reusable library references as Any CPU;
x64 selects their x64 configurations.

OD integer display tests cover signed/unsigned 8/16/32-bit boundaries, fixed-width
hexadecimal two's-complement values, decimal round trips and preserved expressions.
The OD Editor has a Decimal/Hexadecimal page selector and row context overrides;
changing the page selector resets overrides. Actual Default/Current/DCF column
checks run through PacketDisplayTests/run-tests.ps1 with -OdEditorOnly and a
matching -ApplicationDirectory, -Platform and -Configuration.

Three-way OD comparisons list every active Current/Default, Current/DCF and
DCF/Default mismatch in a Differences column. Current cells are pale yellow
when either Current comparison differs; DCF cells are lavender when they differ
from Default. Tooltips give the result of each pair, including unavailable pairs.
Missing data, read failures, unresolved expressions and unsupported datatypes
do not create mismatches. Selection uses the normal Windows selection colors.
Regressions cover all equality combinations and removal of stale highlights.

Recent device files are saved immediately after a successful open, with the
newest ten unique paths retained in the existing user SDOMRU.txt location.
The menu refreshes from disk when opened and marks missing files unavailable.
MRU tests use isolated temporary storage and simulated EDS files, covering
reload, exact menu clicks, concurrent editor histories and storage failures.

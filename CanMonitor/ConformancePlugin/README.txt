CANopen Conformance plugin
==========================

Tools > CANopen Conformance opens a docked document using CanMonitor's current
connection. Load an XDD/EDS/DCF, select the hexadecimal node ID, and run checks.
Results can be saved as a self-contained HTML report, including the description
filename and SHA-256, node, timestamps, individual evidence and coverage gaps.

This is a development pre-conformance tool, not official certification and not
a full implementation of the CiA 310-1 test plan. A passing check describes only
the service and conditions actually exercised. CiA's official tool and laboratory
remain the reference for certification:
https://www.can-cia.org/services/canopen-conformance-test-tool
https://www.can-cia.org/services/cia-test-center

Default checks use SDO uploads only: required object widths, declared readable
objects, fixed-width types and boolean encoding, current PDO mapping targets,
mapping directions and bit widths, classic-CAN payload limits, standard COB-ID
bits, reserved transmission types, SYNC start values, nonexistent-object/subindex
aborts and recovery, write-only upload rejection, and observed heartbeat/EMCY format.
SAM/DAM MPDO selectors (254/255) are recognized without interpreting them as
mapping counts. Their complete scanner/dispatch services are explicitly skipped.
Byte-sized BOOLEAN and partial mappings are checked against OD storage capacity.
Strings and domains have variable lengths; their defaults are not live limits.
No comparison assumes that a configurable default must equal the current value.
An undescribed object that successfully answers the abort probe is inconclusive,
not automatically a failure. Error-history entries above the live count are skipped.

Optional NMT tests send only node-addressed pre-operational, start and stop
commands. They require an observed initial heartbeat state and attempt to restore
that state even after cancellation. They interrupt the target's normal operation.
Heartbeat producer configuration is never changed. With heartbeat disabled the
NMT tests are skipped because state transitions/restoration cannot be verified.
Pause other node configuration tools during NMT testing. Cancellation affects
only this tool's active SDO; it does not flush other plugins' queued transfers.

Not implemented: destructive/invalid downloads, reset/boot-up, remapping writes,
block SDO, stimulated SYNC/TIME/EMCY and heartbeat-consumer errors, LSS or device
profiles. Generic uploads can exercise segmented transfers when the device
exposes long objects, but the tool does not claim all segmented protocol cases.
The normal CANUSB API cannot certify physical-layer voltage, termination, bit
timing, error injection, bus-off recovery or precise wire-level timings. The
managed stack may reject malformed frames before the observer receives them.

Regression and optional hardware runner: ../ConformanceTests/run-tests.ps1.
Device descriptions and hardware reports belong in the user's/private workspace,
not in this public plugin or its generic regression fixtures.

Additional implementation references:
https://github.com/CANopenNode/CANopenDemo/blob/master/tutorial/PDO.md
https://www.renesas.com/en/document/mas/canopen-library-user-manual

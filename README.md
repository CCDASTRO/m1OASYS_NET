m1OASYS ASCOM Roof Driver

ASCOM dome/roof driver for observatories using the ELK M1 Gold and M1OASYS architecture.

This driver provides reliable roll-off roof control, live telemetry, watchdog protection, optional Hall-effect pulse tracking, and optional hardware-based mount safety interlocks using Elk M1 automation rules.

Features
ASCOM Dome Interface
Open roof
Close roof
Abort roof motion
Real-time shutter status
Compatible with ASCOM dome-capable applications including:
NINA
Voyager
SGP
CCD Commander
custom automation systems
Native Elk M1 Integration

Communicates directly with the Elk M1/XEP over TCP/IP using the Elk ASCII protocol.

Supports:

Elk task activation
status polling
counter polling
custom telemetry messages
CRC framed packet handling
Roof Telemetry

Optional Hall-effect pulse telemetry provides:

live pulse counting
percent-open roof tracking
motion monitoring
calibration-based positioning

Telemetry is fully optional and the driver also supports traditional limit-switch-only operation.

Automatic Calibration

Built-in calibration mode:

automatically learns full-open pulse count
saves calibration persistently
survives reconnects and reboots
supports automatic percent-open calculations
Safety Features
Movement Timeout Watchdog

Detects:

stalled motion
failed relays
missing limit transitions
roof jams

Automatically:

aborts roof motion
enters ASCOM error state
reports fault condition
Pulse Watchdog Protection

When Hall telemetry is enabled:

verifies pulse movement during roof motion
detects motor stalls/slipping couplers
faults safely if pulse motion stops unexpectedly
Optional Mount Safety Interlock

Supports hardware-based mount safety sensors through Elk rules.

When enabled:

roof movement is blocked unless Elk reports the mount/scope is safe
no telescope control is performed by the driver
client automation software remains responsible for mount parking

This architecture keeps:

hardware safety in Elk
observatory sequencing in client software
roof enforcement in the driver
Live Status UI

Integrated lightweight telemetry window displays:

roof state
percent open
pulse count
fault status
reconnect status
watchdog events
calibration status

The UI automatically opens/closes with ASCOM connection state.

Supported Operating Modes
Basic Mode
limit switches only
no pulse telemetry
no scope safety sensors required
Advanced Mode
Hall pulse telemetry
percent-open tracking
watchdog protection
mount safety interlocks
Example Elk Features

Typical Elk rule support includes:

Open Roof task
Close Roof task
Stop Roof task
Hall pulse counter
Scope-safe sensor evaluation
roof open/closed status reporting
Architecture Philosophy

The system intentionally separates responsibilities:

Component	Responsibility
Elk M1	Hardware authority and sensor evaluation
ASCOM Driver	Roof control, telemetry, safety enforcement
Client Software	Observatory automation and mount control

This creates a robust and maintainable observatory control system.

Requirements
Windows
ASCOM Platform
Elk M1 with XEP Ethernet interface
ElkRP configured automation rules
Optional Hall-effect pulse sensor
Optional mount-safe sensors

Current project status:

Core development complete
Operational testing in progress
Observatory deployment ready

License

Personal / observatory use project.

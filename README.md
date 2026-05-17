The M1OASYS Dome Driver is an ASCOM Dome driver for Windows that provides standardized control of observatories equipped with the Elk m1 Automation System. It allows ASCOM-compatible software to open and close a roll-off roof or dome shutter, monitor motion status, and integrate roof control into fully automated imaging and observatory workflows.

Capabilities

The driver implements the ASCOM IDomeV2 interface and supports:

Connect and disconnect to the M1OASYS controller
Open shutter / roof
Close shutter / roof
Abort motion
Report shutter state (Open, Closed, Opening, Closing, Error)
Report Slewing while the roof is moving
Optional safety checks prior to motion commands
ASCOM trace logging for troubleshooting
Setup Dialog for communication and driver configuration
Supported Communication Methods

Depending on the M1OASYS configuration, the driver can communicate with the controller via:

Ethernet/TCP-IP
Serial COM port
Virtual serial-over-network connections
ASCOM-Compatible Software

The driver can be used with:

N.I.N.A.
ASCOM Device Hub
ACP Observatory Control Software
Sequence Generator Pro
Any application that supports the ASCOM Dome interface
Intended Use

The M1OASYS Dome Driver enables imaging and observatory software to control an M1OASYS-managed roll-off roof or dome using the standard ASCOM Dome interface. This allows automated opening and closing of the observatory as part of unattended imaging sequences while preserving compatibility with a wide range of astronomy applications.

# m1OASYS Observatory Roof Driver

## Overview

The m1OASYS Observatory Automation System is an ASCOM-compatible roof control platform designed for observatories utilizing the Elk M1 Gold security and automation controller.

The system combines the reliability of the Elk M1 hardware platform with a custom ASCOM Dome Driver to provide robust roll-off roof automation, telemetry monitoring, hardware-based safety interlocks, and unattended observatory operation.

m1OASYS was designed around a layered architecture that separates observatory automation, roof control, and hardware safety enforcement into independent systems for maximum reliability.

---

# Key Features

## ASCOM Dome Driver

Full ASCOM IDomeV2 compatibility including:

* OpenShutter()
* CloseShutter()
* AbortSlew()
* ShutterStatus
* Connected state management

Compatible with:

* NINA
* Voyager
* Sequence Generator Pro (SGP)
* CCD Commander
* ASCOM Device Hub
* Custom automation software

---

## Native Elk M1 Integration

The driver communicates directly with the Elk M1 through the Elk XEP Ethernet Interface using the Elk ASCII protocol.

Supported functions include:

* Task activation
* Status polling
* Counter polling
* Custom telemetry messages
* CRC packet validation
* Automatic reconnect handling

The Elk M1 remains the authoritative hardware controller while the ASCOM driver provides roof automation and safety enforcement.

---

# Roof Position Tracking

## Hall Pulse Telemetry (Optional)

Optional Hall-effect pulse telemetry provides:

* Real-time pulse counting
* Motion verification
* Stall detection

Pulse telemetry may be enabled or disabled from the ASCOM Setup Dialog.

---

## Limit Switch Verification

Physical Open and Closed roof sensors provide positive confirmation of roof position.

The driver uses:

* Open limit status
* Closed limit status
* Controller-reported roof state
* Optional pulse telemetry

to determine roof position and movement status.

When Open or Closed limit sensors are active, they are treated as the authoritative indication of roof position.

---

## Calibration System

m1OASYS includes automatic pulse calibration.

### Calibration Process

1. Ensure the roof is fully closed.
2. Start calibration from the Status Window.
3. The roof opens automatically.
4. Total travel pulses are measured.
5. The full-open pulse count is stored.

Calibration values are automatically saved and restored across:

* Driver restarts
* Observatory software restarts
* Computer reboots

---

## Reconnect Recovery

Following disconnects, network interruptions, or software restarts, the driver automatically restores roof position based on:

* Open limit status
* Closed limit status
* Previously calibrated pulse values

This allows accurate roof position reporting even if pulse counters have been reset by the controller.

---

# Safety Systems

## Movement Timeout Watchdog

The driver continuously monitors roof motion duration.

If expected motion completion does not occur within the configured timeout period, the driver:

* Aborts roof motion
* Enters a fault state
* Reports an error to ASCOM clients

Protects against:

* Roof jams
* Failed relays
* Motor failures
* Missing limit transitions
* Communication failures

---

## Pulse Motion Watchdog

When Hall telemetry is enabled, the driver verifies pulse activity during roof movement.

If roof motion is commanded but pulse activity stops unexpectedly, the driver:

* Stops roof movement
* Reports a fault condition
* Protects observatory equipment

Detects:

* Mechanical stalls
* Slipping couplers
* Failed Hall sensors
* Drive system failures

---

## Mount Safety Interlock (Optional)

m1OASYS supports hardware-based mount safety validation through Elk M1 automation rules.

When enabled:

* Roof motion is blocked unless the mount is reported safe.
* Safety status is evaluated by the Elk controller.
* The ASCOM driver enforces roof protection.

The driver does not control telescope parking.

Responsibility remains:

* Telescope parking → Automation software
* Mount safety verification → Elk M1
* Roof movement enforcement → ASCOM driver

This separation provides maximum flexibility and safety.

---

# Telemetry System

The telemetry layer continuously tracks:

* Roof state
* Open limit status
* Closed limit status
* Mount-safe status
* Hall pulse count
* Percent open
* Motion status
* Fault conditions
* Reconnect events
* Watchdog activity

Telemetry is synchronized across:

* Elk M1 Controller
* ASCOM Driver
* Live Diagnostics Window

---

# Live Diagnostics Window

A dedicated diagnostics window provides real-time monitoring of observatory roof operations.

## Displays

* Roof state
* Percent open
* Hall pulse count
* Fault status
* Reconnect status
* Watchdog activity
* Mount-safe status
* Calibration status

## Behavior

* Automatically opens when connected
* Automatically closes when disconnected
* Runs independently of driver operations
* Remains responsive during calibration
* Designed as a lightweight monitoring utility

---

# Operating Modes

## Basic Mode

Traditional limit-switch roof operation.

Features:

* Open/Closed sensor monitoring
* Roof control through Elk tasks
* No Hall-effect sensor required
* No mount-safe sensor required

---

## Advanced Mode

Enhanced telemetry and safety monitoring.

Features:

* Hall pulse telemetry
* Percent-open tracking
* Pulse watchdog protection
* Automatic calibration
* Mount safety interlocks
* Enhanced diagnostics

---

# Example Elk Integration

Typical Elk automation support includes:

* Open Roof Task
* Close Roof Task
* Stop Roof Task
* Hall Pulse Counter
* Roof Open Status
* Roof Closed Status
* Mount Safe Evaluation
* Observatory Safety Rules

The exact implementation is configurable through ElkRP automation rules.

---

# System Architecture

```text
Observatory Automation Software
(NINA, Voyager, SGP, CCD Commander)
                 ↓
         ASCOM Dome Driver
                 ↓
      Telemetry & Safety Layer
                 ↓
         TCP/IP Communications
                 ↓
          Elk XEP Interface
                 ↓
          Elk M1 Controller
                 ↓
      Tasks, Rules & Counters
                 ↓
       Relays, Sensors, I/O
                 ↓
         Observatory Roof
```

---

# Design Philosophy

m1OASYS intentionally separates responsibilities between system components.

| Component           | Responsibility                               |
| ------------------- | -------------------------------------------- |
| Elk M1              | Hardware authority and sensor evaluation     |
| ASCOM Driver        | Roof control, telemetry, safety enforcement  |
| Automation Software | Observatory sequencing and telescope control |

This layered architecture provides:

* Improved reliability
* Easier troubleshooting
* Hardware independence
* Safe unattended operation

---

# Requirements

## Software

* Windows
* ASCOM Platform
* Observatory automation software (optional)

## Hardware

* Elk M1 Gold
* Elk XEP Ethernet Interface
* ElkRP Configuration Software
* Roof motor controller
* Open limit sensor
* Closed limit sensor

Optional:

* Hall-effect pulse sensor
* Mount-safe sensors
* Additional Elk automation rules

---

# Project Status

| Feature                 | Status              |
| ----------------------- | ------------------- |
| ASCOM Dome Driver       | Complete            |
| Elk M1 Integration      | Complete            |
| Automatic Calibration   | Complete            |
| Hall Pulse Telemetry    | Complete            |
| Watchdog Protection     | Complete            |
| Mount Safety Interlock  | Complete            |
| Live Diagnostics Window | Complete            |
| Observatory Deployment  | Operational Testing |

---

# License

Personal and educational observatory automation project.

---

# Author

Chuck Faranda

CCD Astro Observatory Automation

https://ccdastro.net

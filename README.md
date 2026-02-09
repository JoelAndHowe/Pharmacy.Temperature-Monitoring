# NhsWales.TemperatureMonitor

A .NET Worker Service proof-of-concept for monitoring device temperatures via MQTT and forwarding readings to multiple downstream interfaces.

---

## Overview

This service:

1. Subscribes to temperature messages over **MQTT**
2. Converts incoming payloads into a domain model (`TemperatureReading`)
3. Executes an application use case (`ITemperatureIngestUseCase`)
4. Forwards each reading to multiple configured sinks via a **fan-out pattern**

Example sinks:

- Primary HTTP ingest API
- WIS interface
- (Extensible to file / DB / message bus)

---

## Architecture

Clean Architecture principles are applied:

Domain  
└─ TemperatureReading  

Application  
├─ UseCases  
│  └─ ITemperatureIngestUseCase  
└─ Ports  
   └─ ITemperatureSink  

Infrastructure  
├─ MQTT Subscriber  
├─ HTTP Temperature Sinks  
└─ Options / Configuration  

Worker  
└─ Composition Root + Background Service  

---

## Prerequisites

- .NET SDK (target: net9.0)
- MQTT broker (local or remote)

Optional:
- Docker Desktop (for local Mosquitto broker)

---

## Configuration

Edit:

src/NhsWales.TemperatureMonitor.Worker/appsettings.json

Example:

{
  "Mqtt": {
    "Host": "localhost",
    "Port": 1883,
    "Topic": "poc/device1/temperature"
  },
  "HttpEndpoints": {
    "Primary": {
      "BaseUrl": "https://httpbin.org",
      "IngestPath": "/post"
    },
    "Wis": {
      "BaseUrl": "https://httpbin.org",
      "IngestPath": "/post"
    }
  }
}

Notes:
- BaseUrl must be an absolute URL
- IngestPath must start with /

---

## Secrets (Tokens)

Tokens are not stored in config files.

### Local development — User Secrets

dotnet user-secrets init --project .\src\NhsWales.TemperatureMonitor.Worker

dotnet user-secrets set "HttpEndpoints:Primary:Token" "PRIMARY_TOKEN" --project .\src\NhsWales.TemperatureMonitor.Worker
dotnet user-secrets set "HttpEndpoints:Wis:Token" "WIS_TOKEN" --project .\src\NhsWales.TemperatureMonitor.Worker

---

## Running an MQTT Broker (Docker)

docker run -it --rm -p 1883:1883 eclipse-mosquitto mosquitto -c /mosquitto-no-auth.conf

---

## Running the Service

dotnet run --project .\src\NhsWales.TemperatureMonitor.Worker

---

## MQTT Payload Format

{
  "deviceId": "device1",
  "celsius": 4.2,
  "timestampUtc": "2026-02-09T12:00:00Z"
}

timestampUtc is optional.

---

## Troubleshooting

Options validation errors:
- Ensure BaseUrl present
- Ensure IngestPath present
- Ensure Token set via secrets/env

No MQTT messages received:
- Confirm topic matches publisher
- Confirm broker running
- Confirm host/port correct

---

## Extending the Service

Add new sinks by implementing:

ITemperatureLeafSink

Register via DI:

builder.Services.AddSingleton<ITemperatureLeafSink, NewSink>();

Fan-out will include it automatically.

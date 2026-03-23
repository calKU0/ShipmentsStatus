# ShipmentsStatus

> 💼 **Commercial Project** — part of a client-facing initiative.

`ShipmentsStatus` is a `.NET 10` worker service that periodically synchronizes shipment statuses from multiple courier providers and updates them in a SQL database.

## What the service does

- Runs as a background worker (`BackgroundService`) with a configurable interval.
- Reads recent shipments from the database.
- Chooses the correct courier integration based on shipment courier type.
- Requests current tracking status from courier APIs.
- Maps external status text to internal `ShipmentStatus` values.
- Updates shipment status in the database.
- Writes operational logs to console and rolling log files.

## Solution structure

### `ShipmentsStatus.Service`
Host application and orchestration layer:

- Configures dependency injection, logging (`Serilog`), HTTP clients, and hosted worker.
- Contains `Worker` (execution loop) and `ShipmentStatusSyncService` (sync pipeline).
- Applies throttling for API and DB calls using `SemaphoreSlim`.
- Handles retry logic for HTTP `429 Too Many Requests` responses.

### `ShipmentsStatus.Contracts`
Shared contracts and domain primitives:

- DTOs for courier request/response models.
- Domain model: `Shipment`.
- Enums: `Courier`, `ShipmentStatus`.
- Settings models used by options binding.
- Interfaces for repository and courier services.

### `ShipmentsStatus.Infrastructure`
Infrastructure and external integration layer:

- Data access with `Dapper` (`IDbExecutor`, `DapperDbExecutor`).
- Repository implementation: `ShipmentRepository`.
- Courier integrations:
  - `DpdService`
  - `GlsService`
  - `FedexService` (SOAP/REST strategy selection)
  - `DpdRomaniaService` (currently not implemented)
- Status normalization via `ShipmentStatusMapper`.

## Processing flow

1. Worker starts sync cycle.
2. Recent shipments are loaded via stored procedure `kp.GetShipmentsFromDate`.
3. Each shipment is processed asynchronously.
4. Courier-specific client fetches tracking status.
5. External status text is translated to internal status enum.
6. Database update is executed via stored procedure `kp.UpdateShipmentStatus`.

## Key technical points

- Target framework: `net10.0`.
- Hosting model: Worker Service + Windows Service integration.
- Resilience:
  - HTTP transient retry policy.
  - Explicit `429` retry with incremental delay.
  - Separate concurrency limits for API and DB operations.
- Logging:
  - Console sink.
  - Daily rolling file sink (`logs/log-*.txt`).

## Current limitations

- `DpdRomaniaService.GetShipmentStatus` is not implemented.
- `FedexRestStrategy.GetShipmentStatus` is not implemented.
- Error handling is centralized in processing loop (failed shipments are logged and skipped for that cycle).

## Notes

The service is designed for scheduled synchronization workloads where shipment statuses need to be refreshed in bulk with controlled concurrency and reliable retry behavior.

## License

This project is **proprietary and confidential**.

It was developed for a client and is **not permitted to be shared, redistributed, or used** without explicit written permission from the owner.

See [LICENSE](LICENSE) for details.

---

© 2026-present [calKU0](https://github.com/calKU0)
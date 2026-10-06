using Microsoft.Extensions.Options;
using ShipmentsStatus.Contracts.Data.Enums;
using ShipmentsStatus.Contracts.DTOs;
using ShipmentsStatus.Contracts.Models;
using ShipmentsStatus.Contracts.Repositories;
using ShipmentsStatus.Contracts.Settings;
using ShipmentsStatus.Infrastructure.Mapping;
using ShipmentsStatus.Infrastructure.Services;
using System.Net;

namespace ShipmentsStatus.Service.Services
{
    public class ShipmentStatusSyncService : IShipmentStatusSyncService
    {
        private readonly IShipmentRepository _shipmentRepository;
        private readonly CourierFactory _courierFactory;
        private readonly AppSettings _appSettings;
        private readonly ILogger<ShipmentStatusSyncService> _logger;
        private readonly SemaphoreSlim _apiSemaphore;
        private readonly SemaphoreSlim _dbSemaphore;
        private readonly int _retryCount;
        private readonly int _retryBaseDelaySeconds;

        public ShipmentStatusSyncService(IShipmentRepository shipmentRepository, CourierFactory courierFactory, IOptions<AppSettings> options, ILogger<ShipmentStatusSyncService> logger)
        {
            _shipmentRepository = shipmentRepository;
            _appSettings = options.Value;
            _courierFactory = courierFactory;
            _logger = logger;

            var apiConcurrencyLimit = _appSettings.ApiConcurrencyLimit > 0 ? _appSettings.ApiConcurrencyLimit : 1;
            var dbConcurrencyLimit = _appSettings.DbConcurrencyLimit > 0 ? _appSettings.DbConcurrencyLimit : 1;
            _apiSemaphore = new SemaphoreSlim(apiConcurrencyLimit, apiConcurrencyLimit);
            _dbSemaphore = new SemaphoreSlim(dbConcurrencyLimit, dbConcurrencyLimit);
            _retryCount = _appSettings.TooManyRequestsRetryCount > 0 ? _appSettings.TooManyRequestsRetryCount : 3;
            _retryBaseDelaySeconds = _appSettings.TooManyRequestsBaseDelaySeconds > 0 ? _appSettings.TooManyRequestsBaseDelaySeconds : 5;
        }

        public async Task SyncRecentShipmentsAsync(CancellationToken cancellationToken)
        {
            var fromDate = DateTime.Now.AddDays(-_appSettings.ShipmentsBackDays);
            var shipments = (await _shipmentRepository.GetShipmentsFromDate(fromDate, cancellationToken)).ToList();
            _logger.LogInformation("Found {ShipmentCount} shipments to sync from date {FromDate}.", shipments.Count, fromDate);

            var tasks = shipments.Select(shipment => ProcessShipmentAsync(shipment, cancellationToken));
            await Task.WhenAll(tasks);
        }

        private async Task ProcessShipmentAsync(Shipment shipment, CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(shipment.TrackingNumber))
            {
                _logger.LogWarning("Skipping shipment {ShipmentId} because tracking number is empty.", shipment.Id);
                return;
            }

            try
            {
                var response = await GetShipmentStatusWithThrottleAndRetryAsync(shipment, cancellationToken);

                shipment.Status = ShipmentStatusMapper.MapToContractEnum(response.Status);
                shipment.StatusDate = response.Date;

                if (shipment.Status != ShipmentStatus.NotFound)
                {
                    await UpdateShipmentStatusWithThrottleAsync(shipment, cancellationToken);
                    _logger.LogInformation("Updated shipment {TrackingNumber} for courier {Courier} with status {Status}. Status from API: {ResponseStatus} ({ResponseDescription})", shipment.TrackingNumber, shipment.Courier, shipment.Status, response.Status, response.Description);
                }
                else
                {
                    _logger.LogWarning("Shipment status {TrackingNumber} not found for Courier {Courier}", shipment.TrackingNumber, shipment.Courier);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update shipment {TrackingNumber} for courier {Courier}", shipment.TrackingNumber, shipment.Courier);
            }
        }

        private async Task<ShipmentStatusResponse> GetShipmentStatusWithThrottleAndRetryAsync(Shipment shipment, CancellationToken cancellationToken)
        {
            for (var attempt = 1; ; attempt++)
            {
                await _apiSemaphore.WaitAsync(cancellationToken);
                try
                {
                    var courierService = _courierFactory.GetCourier(shipment.Courier);
                    return await courierService.GetShipmentStatus(new ShipmentStatusRequest
                    {
                        TrackingNumber = shipment.TrackingNumber,
                        IsDropshipping = shipment.IsDropshipping,
                        Country = shipment.Country
                    });
                }
                catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.TooManyRequests && attempt <= _retryCount)
                {
                    var delay = TimeSpan.FromSeconds(_retryBaseDelaySeconds * attempt);
                    _logger.LogWarning(
                        ex,
                        "429 received for shipment {TrackingNumber}. Retry {Attempt}/{RetryCount} after {DelaySeconds}s.",
                        shipment.TrackingNumber,
                        attempt,
                        _retryCount,
                        delay.TotalSeconds);

                    await Task.Delay(delay, cancellationToken);
                }
                finally
                {
                    _apiSemaphore.Release();
                }
            }
        }

        private async Task UpdateShipmentStatusWithThrottleAsync(Shipment shipment, CancellationToken cancellationToken)
        {
            await _dbSemaphore.WaitAsync(cancellationToken);
            try
            {
                await _shipmentRepository.UpdateShipmentStatus(shipment, cancellationToken);
            }
            finally
            {
                _dbSemaphore.Release();
            }
        }
    }
}

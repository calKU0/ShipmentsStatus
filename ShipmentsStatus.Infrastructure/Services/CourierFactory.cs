using Microsoft.Extensions.DependencyInjection;
using ShipmentsStatus.Contracts.Data.Enums;
using ShipmentsStatus.Contracts.Services;
using ShipmentsStatus.Infrastructure.Helpers;

namespace ShipmentsStatus.Infrastructure.Services
{
    public class CourierFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public CourierFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public ICourierService GetCourier(Courier courier) =>
            courier switch
            {
                Courier.DPD => _serviceProvider.GetRequiredService<DpdService>(),
                Courier.GLS => _serviceProvider.GetRequiredService<GlsService>(),
                Courier.Fedex => _serviceProvider.GetRequiredService<FedexService>(),
                Courier.DPD_Romania => _serviceProvider.GetRequiredService<DpdRomaniaService>(),
                _ => throw new NotSupportedException($"Kurier {courier.GetDescription()} nie jest wspierany")
            };
    }
}

using MedLink.Application.Interfaces.Common;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MedLink.Presentation.BackgroundServices
{
    public class BookingExpirationBackgroundService : BackgroundService
    {
        private readonly IServiceProvider serviceProvider;
        private readonly ILogger<BookingExpirationBackgroundService> logger;

        public BookingExpirationBackgroundService(
            IServiceProvider serviceProvider,
            ILogger<BookingExpirationBackgroundService> logger)
        {
            this.serviceProvider = serviceProvider;
            this.logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = serviceProvider.CreateScope();
                    var bookingService = scope.ServiceProvider.GetRequiredService<IBookingService>();
                    await bookingService.ExpireOverdueBookingsAsync();
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Failed to expire overdue bookings.");
                }

                try
                {
                    await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
                }
                catch (TaskCanceledException)
                {
                    break;
                }
            }
        }
    }
}
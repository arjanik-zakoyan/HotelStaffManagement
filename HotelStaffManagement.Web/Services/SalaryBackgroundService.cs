using Microsoft.EntityFrameworkCore;
using HotelStaffManagement.DataAccess;

namespace HotelStaffManagement.Web.Services
{
    public class SalaryBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<SalaryBackgroundService> _logger;

        public SalaryBackgroundService(IServiceProvider serviceProvider, ILogger<SalaryBackgroundService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var now = DateTime.Now;
                var previousMonthKey = now.AddMonths(-1).ToString("yyyy-MM");

                using var scope = _serviceProvider.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                bool alreadyProcessed = await context.Salaries.AnyAsync(s => s.Month == previousMonthKey);

                if (now.Day == 1 && !alreadyProcessed)
                {
                    var salaryService = scope.ServiceProvider.GetRequiredService<SalaryCalculationService>();

                    try
                    {
                        _logger.LogInformation("Starting monthly salary calculation for {Month}...", previousMonthKey);
                        await salaryService.CalculateSalariesForPreviousMonthAsync();
                        _logger.LogInformation("Salary calculation completed successfully for {Month}.", previousMonthKey);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error occurred while calculating salaries for {Month}.", previousMonthKey);
                    }
                }
                await Task.Delay(TimeSpan.FromHours(12), stoppingToken);
            }
        }
    }
}

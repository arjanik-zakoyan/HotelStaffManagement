using HotelStaffManagement.DataAccess;
using HotelStaffManagement.DataAccess.Models;
using HotelStaffManagement.Web.Helpers;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace HotelStaffManagement.Web.Services
{
    public class SalaryCalculationService
    {
        private readonly ApplicationDbContext _context;
        private readonly NotificationService _notificationService;

        // Գիշերային հերթափոխների ցանկը
        private readonly List<string> NightShifts = new() { "20:00 - 08:00", "00:00 - 08:00" };

        public SalaryCalculationService(ApplicationDbContext context, NotificationService notificationService)
        {
            _context = context;
            _notificationService = notificationService;
        }

        public async Task CalculateSalariesForPreviousMonthAsync()
        {
            var today = DateTime.Today;
            var targetMonth = new DateTime(today.Year, today.Month, 1).AddMonths(-1);
            var startDate = targetMonth;
            var endDate = startDate.AddMonths(1).AddDays(-1);
            var monthKey = targetMonth.ToString("yyyy-MM", CultureInfo.InvariantCulture);

            var schedules = await _context.Schedules
                .Include(s => s.Employee)
                .Where(s => s.StartDate.Date >= startDate && s.StartDate.Date <= endDate)
                .ToListAsync();

            var grouped = schedules.GroupBy(s => s.EmployeeID);

            foreach (var group in grouped)
            {
                var employee = group.First().Employee;
                if (employee == null || employee.SalaryPerHour == null) continue;

                decimal regularHours = 0;
                decimal nightHours = 0;

                foreach (var sched in group)
                {
                    var shift = ScheduleHelper.GetShiftString(sched.StartDate, sched.EndDate);
                    var hours = (decimal)(sched.EndDate - sched.StartDate).TotalHours;

                    if (NightShifts.Contains(shift))
                        nightHours += hours;
                    else
                        regularHours += hours;
                }
                decimal rate = employee.SalaryPerHour;

                decimal total = (regularHours * rate) + (nightHours * rate * 1.5m);

                var salary = new Salary
                {
                    EmployeeID = employee.EmployeeID,
                    Month = monthKey,
                    TotalSalaryAmount = total,
                    RegularHours = regularHours,
                    NightHours = nightHours
                };

                _context.Salaries.Add(salary);
                await _context.SaveChangesAsync();

                // Send Notification
                string message = $"{targetMonth.ToString("MMMM", new CultureInfo("hy-AM"))}-ի համար Ձեր աշխատավարձը կազմում է {total:N0} դրամ։";
                await _notificationService.AddNotificationAsync(employee.UserID, "Աշխատավարձի Հաշվարկ", message, "Salary");
            }
        }
    }

}

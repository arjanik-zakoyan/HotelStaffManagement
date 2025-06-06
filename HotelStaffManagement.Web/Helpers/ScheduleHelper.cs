using System.Collections.Generic;
using HotelStaffManagement.DataAccess;
using HotelStaffManagement.DataAccess.Enums;
using HotelStaffManagement.Web.Models.Manager;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace HotelStaffManagement.Web.Helpers
{
    public static class ScheduleHelper
    {
        public static string GetShiftString(DateTime start, DateTime end)
        {
            var startStr = start.ToString("HH:mm");
            var endStr = end.ToString("HH:mm");

            return $"{startStr} - {endStr}";
        }

        public static List<SelectListItem> GetPositionList()
        {
            return Enum.GetValues(typeof(EmployeePosition))
                .Cast<EmployeePosition>()
                .Select(p => new SelectListItem
                {
                    Value = p.ToString(),
                    Text = PositionHelper.GetPositionDisplayName(p)
                }).ToList();
        }
        public static List<string> GetShiftsForPosition(EmployeePosition position)
        {
            return position switch
            {
                EmployeePosition.Receptionist => new List<string> { "08:00 - 20:00", "20:00 - 08:00" },
                EmployeePosition.Housekeeper => new List<string> { "00:00 - 08:00", "08:00 - 16:00", "16:00 - 00:00" },
                EmployeePosition.Chef => new List<string> { "06:00 - 14:00", "14:00 - 22:00" },
                EmployeePosition.Security => new List<string> { "08:00 - 20:00", "20:00 - 08:00" },
                EmployeePosition.Waiter => new List<string> {"00:00 - 08:00", "08:00 - 16:00", "16:00 - 00:00" },
                EmployeePosition.Bartender => new List<string> { "00:00 - 08:00", "08:00 - 16:00", "16:00 - 00:00" },
                EmployeePosition.Technician => new List<string> { "09:00 - 17:00" },
                _ => new List<string>()
            };
        }
        public static (DateTime Start, DateTime End) ParseShiftTimes(DateTime date, string? shift)
        {
            if (string.IsNullOrWhiteSpace(shift))
                throw new ArgumentException("Հերթափոխը նշված չէ։", nameof(shift));

            var times = shift.Split(" - ");
            if (times.Length != 2)
                throw new FormatException("Հերթափոխի ձևաչափը սխալ է։");

            var start = DateTime.Parse($"{date:yyyy-MM-dd} {times[0]}");
            var end = DateTime.Parse($"{date:yyyy-MM-dd} {times[1]}");

            if (end <= start)
                end = end.AddDays(1);

            return (start, end);
        }
        public static async Task LoadDynamicOptions(CreateScheduleViewModel model, ApplicationDbContext _context)
        {
            if (!Enum.TryParse(model.SelectedPosition, out EmployeePosition positionEnum))
                return;

            model.Shifts = GetShiftsForPosition(positionEnum)
                .Select(s => new SelectListItem
                {
                    Value = s,
                    Text = s
                }).ToList();

            model.Employees = await _context.Employees
                .Where(e => e.Position == model.SelectedPosition)
                .Select(e => new SelectListItem
                {
                    Value = e.EmployeeID.ToString(),
                    Text = e.FullName
                }).ToListAsync();
        }
    }
}

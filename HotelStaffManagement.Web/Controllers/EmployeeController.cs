using HotelStaffManagement.DataAccess;
using HotelStaffManagement.DataAccess.Enums;
using HotelStaffManagement.Web.Models.Employee;
using HotelStaffManagement.Web.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using HotelStaffManagement.DataAccess.Models;

namespace HotelStaffManagement.Web.Controllers
{
    [Authorize(Roles = "Employee")]
    public class EmployeeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public EmployeeController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Employee Personal Page
        public async Task<IActionResult> Index(string? month)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

                var user = await _context.Users.FindAsync(userId);
                var employee = await _context.Employees.FirstOrDefaultAsync(e => e.UserID == userId);

                if (user == null || employee == null)
                    return RedirectToAction("GeneralError", "Error");

                // Month handling
                var currentMonth = string.IsNullOrWhiteSpace(month)
                    ? DateTime.Today
                    : DateTime.Parse(month);

                var startOfMonth = new DateTime(currentMonth.Year, currentMonth.Month, 1);
                var endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);

                var schedules = await _context.Schedules
                    .Where(s => s.EmployeeID == employee.EmployeeID &&
                                s.StartDate.Date >= startOfMonth &&
                                s.StartDate.Date <= endOfMonth)
                    .ToListAsync();

                var grouped = schedules
                    .GroupBy(s => s.StartDate.Date)
                    .Select(g => new EmployeeScheduleDayViewModel
                    {
                        Date = g.Key,
                        ShiftTimes = g.Select(s => ScheduleHelper.GetShiftString(s.StartDate, s.EndDate)).ToList()
                    })
                    .ToList();

                var model = new EmployeePersonalViewModel
                {
                    FullName = employee.FullName,
                    Username = user.Username,
                    PhoneNumber = employee.PhoneNumber,
                    Position = employee.Position,
                    SalaryPerHour = employee.SalaryPerHour,
                    CurrentMonth = startOfMonth,
                    CalendarDays = grouped
                };
                var unreadCount = await _context.Notifications
                    .CountAsync(n => n.UserID == userId && !n.IsRead);
                ViewBag.UnreadNotificationCount = unreadCount;

                ViewBag.FullName = employee?.FullName;

                return View(model);
            }
            catch (Exception ex)
            {
                return RedirectToAction("GeneralError", "Error");
            }
        }

        [HttpGet]
        public async Task<IActionResult> AllSchedulesAsync(DateTime? start, DateTime? end, string? position)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
                var employee = await _context.Employees.FirstOrDefaultAsync(e => e.UserID == userId);
                if (employee == null) return NotFound();

                var startDate = start ?? DateTime.Today.StartOfWeek(DayOfWeek.Monday);
                var endDate = end ?? startDate.AddDays(6);
                var selectedPosition = position ?? employee.Position;

                if (endDate < startDate)
                {
                    TempData["Error"] = "Սկզբի ամսաթիվը չի կարող լինել վերջի ամսաթվից մեծ։";
                    return RedirectToAction("AllSchedules", new { position });
                }

                var dates = Enumerable.Range(0, (endDate - startDate).Days + 1)
                    .Select(offset => startDate.AddDays(offset))
                    .ToList();

                var allPositions = Enum.GetValues(typeof(EmployeePosition))
                    .Cast<EmployeePosition>()
                    .Select(p => p.ToString())
                    .ToList();

                var scheduleData = _context.Schedules
                    .Include(s => s.Employee)
                    .Where(s => s.Employee != null &&
                                s.StartDate.Date <= endDate.Date &&
                                s.EndDate.Date >= startDate.Date)
                    .ToList();

                var schedule = new Dictionary<(string Shift, DateTime Date, string Position), List<string>>();

                foreach (var sched in scheduleData)
                {
                    if (!Enum.TryParse<EmployeePosition>(sched.Employee.Position, out var empPos))
                        continue;

                    // Restore Shifts
                    var shift = ScheduleHelper.GetShiftString(sched.StartDate, sched.EndDate);
                    if (string.IsNullOrWhiteSpace(shift)) continue;

                    var date = sched.StartDate.Date;
                    var key = (shift, date, sched.Employee.Position);

                    if (!schedule.ContainsKey(key))
                        schedule[key] = new List<string>();

                    schedule[key].Add(sched.Employee.FullName);
                }

                var model = new ScheduleDisplayViewModel
                {
                    StartDate = startDate,
                    EndDate = endDate,
                    Dates = dates,
                    SelectedPosition = selectedPosition,
                    AllPositions = allPositions,
                    Shifts = allPositions
                                .SelectMany(pos =>
                                    ScheduleHelper.GetShiftsForPosition(Enum.Parse<EmployeePosition>(pos)))
                                .Distinct()
                                .ToList(),
                    Schedule = schedule
                };

                var unreadCount = await _context.Notifications
                    .CountAsync(n => n.UserID == userId && !n.IsRead);
                ViewBag.UnreadNotificationCount = unreadCount;

                ViewBag.FullName = employee?.FullName;
                ViewBag.CurrentEmployeeName = employee.FullName;

                return View(model);
            }
            catch (Exception ex)
            {
                return RedirectToAction("GeneralError", "Error");
            }
        }


        [HttpGet]
        public async Task<IActionResult> Notifications()
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
                var employee = await _context.Employees.FirstOrDefaultAsync(e => e.UserID == userId);

                var notifications = await _context.Notifications
                    .Where(n => n.UserID == userId)
                    .OrderByDescending(n => n.CreatedAt)
                    .ToListAsync();

                ViewBag.UnreadNotificationCount = notifications.Count(n => !n.IsRead);
                ViewBag.FullName = employee?.FullName;
                return View(notifications);
            }
            catch (Exception ex)
            {
                return RedirectToAction("GeneralError", "Error");
            }
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAllAsRead()
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

                var unreadNotifications = await _context.Notifications
                    .Where(n => n.UserID == userId && !n.IsRead)
                    .ToListAsync();

                foreach (var note in unreadNotifications)
                    note.IsRead = true;

                await _context.SaveChangesAsync();

                return RedirectToAction("Notifications");
            }
            catch (Exception ex)
            {
                return RedirectToAction("GeneralError", "Error");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

                var notification = await _context.Notifications
                    .FirstOrDefaultAsync(n => n.NotificationID == id && n.UserID == userId);

                if (notification == null)
                    return NotFound();

                notification.IsRead = true;
                await _context.SaveChangesAsync();

                return RedirectToAction("Notifications");
            }
            catch (Exception ex)
            {
                return RedirectToAction("GeneralError", "Error");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteNotification(int id)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

                var notification = await _context.Notifications
                    .FirstOrDefaultAsync(n => n.NotificationID == id && n.UserID == userId);

                if (notification == null)
                    return NotFound();

                _context.Notifications.Remove(notification);
                await _context.SaveChangesAsync();

                return RedirectToAction("Notifications");
            }
            catch (Exception ex)
            {
                return RedirectToAction("GeneralError", "Error");
            }  
        }

    }

}

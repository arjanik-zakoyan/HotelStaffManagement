using HotelStaffManagement.DataAccess.Models;
using HotelStaffManagement.DataAccess;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using HotelStaffManagement.Web.Models.Manager;
using Microsoft.EntityFrameworkCore;
using HotelStaffManagement.Web.Helpers;
using HotelStaffManagement.DataAccess.Enums;
using Microsoft.AspNetCore.Mvc.Rendering;
using HotelStaffManagement.Web.Services;
using System.Globalization;

[Authorize(Roles = "Manager")]
public class ManagerController : Controller
{
    private readonly ApplicationDbContext _context;

    public ManagerController(ApplicationDbContext context)
    {
        _context = context;
    }

    // Manager Main Page
    [HttpGet]
    public async Task<IActionResult> Index(string? searchTerm, string? position, int page = 1)
    {
        try
        {
            int pageSize = 12;
            var query = _context.Users
                .Where(u => u.Role == "Employee")
                .Join(_context.Employees,
                      user => user.UserID,
                      employee => employee.UserID,
                      (user, employee) => new EmployeeViewModel
                      {
                          UserID = user.UserID,
                          FullName = employee.FullName,
                          PhoneNumber = employee.PhoneNumber,
                          Username = user.Username,
                          Position = employee.Position,
                          SalaryPerHour = employee.SalaryPerHour
                      });

            if (!string.IsNullOrWhiteSpace(searchTerm))
                query = query.Where(e => e.FullName.Contains(searchTerm));

            if (!string.IsNullOrWhiteSpace(position))
                query = query.Where(e => e.Position == position);

            var totalCount = await query.CountAsync();

            var employees = await query
                .OrderBy(e => e.FullName)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var model = new EmployeeListViewModel
            {
                Employees = employees,
                CurrentPage = page,
                TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
                SearchTerm = searchTerm,
                Position = position
            };

            return View(model);
        }
        catch (Exception ex)
        {
            return RedirectToAction("GeneralError", "Error");
        }
    }

    // Schedules Page
    [HttpGet]
    public IActionResult AllSchedules(DateTime? start, DateTime? end, string? position)
    {
        try
        {
            var startDate = start ?? DateTime.Today.StartOfWeek(DayOfWeek.Monday);
            var endDate = end ?? startDate.AddDays(6);
            var selectedPosition = position ?? "All";

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

            return View(model);
        }
        catch (Exception ex)
        {
            return RedirectToAction("GeneralError", "Error");
        }  
    }

    // Creating Employee Page
    [HttpGet]
    public IActionResult CreateEmployee()
    {
        try {
            var model = new CreateEmployeeViewModel
            {
                Positions = Enum.GetValues(typeof(EmployeePosition))
                .Cast<EmployeePosition>()
                .Select(p => new SelectListItem
                {
                    Value = p.ToString(),
                    Text = PositionHelper.GetPositionDisplayName(p)
                }).ToList()
            };

            return View(model);
        }
        catch (Exception ex)
        {
            return RedirectToAction("GeneralError", "Error");
        }

    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateEmployee(CreateEmployeeViewModel model)
    {
        try
        {
            if (ModelState.IsValid)
            {
                if (await _context.Users.AnyAsync(u => u.Username == model.Username))
                {
                    ModelState.AddModelError("Username", "Այս մուտքանունը արդեն օգտագործվում է։");
                    return View(model);
                }
                var salt = PasswordHelper.GenerateSalt();
                var hashedPassword = PasswordHelper.HashPassword(model.Password, salt);

                var user = new User
                {
                    Username = model.Username,
                    PasswordHash = hashedPassword,
                    Salt = salt,
                    Role = "Employee"
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                var employee = new Employee
                {
                    UserID = user.UserID,
                    FullName = model.FullName,
                    PhoneNumber = model.PhoneNumber,
                    Position = model.Position?.ToString(),
                    SalaryPerHour = model.SalaryPerHour
                };

                _context.Employees.Add(employee);
                await _context.SaveChangesAsync();

                return RedirectToAction("Index", "Manager");
            }

            model.Positions = Enum.GetValues(typeof(EmployeePosition))
                .Cast<EmployeePosition>()
                .Select(p => new SelectListItem
                {
                    Value = p.ToString(),
                    Text = PositionHelper.GetPositionDisplayName(p)
                }).ToList();

            return View(model);
        }
        catch (Exception ex)
        {
            return RedirectToAction("GeneralError", "Error");
        }
    }

    // Employee Details Page
    [HttpGet]
    public async Task<IActionResult> EmployeeDetails(int id, string? month)
    {
        try
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserID == id && u.Role == "Employee");
            if (user == null) return NotFound();

            var employee = await _context.Employees.FirstOrDefaultAsync(e => e.UserID == id);
            if (employee == null) return NotFound();

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

            var model = new EmployeeDetailsViewModel
            {
                UserID = user.UserID,
                FullName = employee.FullName,
                PhoneNumber = employee.PhoneNumber,
                Username = user.Username,
                Position = employee.Position,
                SalaryPerHour = employee.SalaryPerHour,
                CurrentMonth = startOfMonth,
                CalendarDays = grouped,
            };
            ViewBag.FullName = employee?.FullName;

            return View(model);
        }
        catch (Exception ex)
        {
            return RedirectToAction("GeneralError", "Error");
        }
    }

    // Editing Employee Page
    [HttpGet]
    public async Task<IActionResult> EditEmployee(int id)
    {
        try
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null || user.Role != "Employee")
            {
                return NotFound();
            }

            var employee = await _context.Employees.FirstOrDefaultAsync(e => e.UserID == user.UserID);
            if (employee == null)
            {
                return NotFound();
            }

            var model = new EditEmployeeViewModel
            {
                UserID = user.UserID,
                FullName = employee.FullName,
                PhoneNumber = employee.PhoneNumber,
                Position = employee.Position,
                SalaryPerHour = employee.SalaryPerHour,
                Positions = Enum.GetValues(typeof(EmployeePosition))
                    .Cast<EmployeePosition>()
                    .Select(p => new SelectListItem
                    {
                        Value = p.ToString(),
                        Text = PositionHelper.GetPositionDisplayName(p.ToString())
                    }).ToList()
            };

            return View(model);
        }
        catch (Exception ex)
        {
            return RedirectToAction("GeneralError", "Error");
        } 
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditEmployee(EditEmployeeViewModel model)
    {
        try
        {
            if (ModelState.IsValid)
            {
                var user = await _context.Users.FindAsync(model.UserID);
                var employee = await _context.Employees.FirstOrDefaultAsync(e => e.UserID == user.UserID);

                if (user == null || employee == null)
                {
                    return NotFound();
                }

                employee.FullName = model.FullName;
                employee.PhoneNumber = model.PhoneNumber;
                employee.Position = model.Position;
                employee.SalaryPerHour = model.SalaryPerHour;

                _context.Employees.Update(employee);
                await _context.SaveChangesAsync();

                return RedirectToAction("EmployeeDetails", new { id = model.UserID });
            }

            model.Positions = Enum.GetValues(typeof(EmployeePosition))
                .Cast<EmployeePosition>()
                .Select(p => new SelectListItem
                {
                    Value = p.ToString(),
                    Text = PositionHelper.GetPositionDisplayName(p.ToString())
                }).ToList();

            return View(model);
        }
        catch (Exception ex)
        {
            return RedirectToAction("GeneralError", "Error");
        }
    }

    // Deleting Employee Page
    [HttpGet]
    public async Task<IActionResult> DeleteEmployee(int id)
    {
        try
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null || user.Role != "Employee")
            {
                return NotFound();
            }

            var employee = await _context.Employees.FirstOrDefaultAsync(e => e.UserID == id);
            if (employee == null)
            {
                return NotFound();
            }

            var model = new DeleteEmployeeViewModel
            {
                UserID = user.UserID,
                FullName = employee.FullName
            };

            return View(model);
        }
        catch (Exception ex)
        {
            return RedirectToAction("GeneralError", "Error");
        }

    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteEmployeeConfirmed(DeleteEmployeeViewModel model)
    {
        try
        {
            var user = await _context.Users.FindAsync(model.UserID);
            if (user == null || user.Role != "Employee")
            {
                return NotFound();
            }

            var employee = await _context.Employees.FirstOrDefaultAsync(e => e.UserID == user.UserID);


            var sentMessages = await _context.Messages
                .Where(m => m.SenderID == user.UserID).ToListAsync();
            var receivedMessages = await _context.Messages
                .Where(m => m.ReceiverID == user.UserID).ToListAsync();

            _context.Messages.RemoveRange(sentMessages);
            _context.Messages.RemoveRange(receivedMessages);


            if (employee != null)
            {
                _context.Employees.Remove(employee);
            }
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            return RedirectToAction("GeneralError", "Error");
        }
    }

    // Create Schedule Page
    [HttpGet]
    public IActionResult CreateSchedule()
    {
        var model = new CreateScheduleViewModel
        {
            SelectedDate = DateTime.Today,
            Positions = ScheduleHelper.GetPositionList()
        };

        return View(model);
    }

    // Get Employees and Shifts by Position(AJAX)
    [HttpGet]
    public IActionResult GetEmployeesAndShifts(string position)
    {
        try
        {
            if (!Enum.TryParse(position, out EmployeePosition selectedPosition))
                return BadRequest("Invalid position.");

            var employees = _context.Employees
                .Include(e => e.User)
                .Where(e => e.Position == selectedPosition.ToString())
                .Select(e => new
                {
                    Value = e.EmployeeID.ToString(),
                    Text = e.FullName
                })
                .ToList();

            var shifts = ScheduleHelper.GetShiftsForPosition(selectedPosition)
                .Select(s => new { Value = s, Text = s })
                .ToList();

            return Json(new { employees, shifts });
        }
        catch (Exception ex)
        {
            return RedirectToAction("GeneralError", "Error");
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateSchedule(CreateScheduleViewModel model)
    {
        try
        {
            model.Positions = ScheduleHelper.GetPositionList();
            await ScheduleHelper.LoadDynamicOptions(model, _context);

            if (model.SelectedEmployeeIds == null || !model.SelectedEmployeeIds.Any())
            {
                ModelState.AddModelError(nameof(model.SelectedEmployeeIds), "Խնդրում ենք ընտրել գոնե մեկ աշխատակից։");
            }

            DateTime startTime = default, endTime = default;
            try
            {
                (startTime, endTime) = ScheduleHelper.ParseShiftTimes(model.SelectedDate, model.SelectedShift);
                if (startTime < DateTime.Now)
                {
                    ModelState.AddModelError(nameof(model.SelectedShift), "Խնդրում ենք ընտրել վավեր հերթափոխ։");
                }
            }
            catch
            {
                ModelState.AddModelError(nameof(model.SelectedShift), "Խնդրում ենք ընտրել վավեր հերթափոխ։");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var existingSchedules = _context.Schedules
                .Include(s => s.Employee)
                .Where(s => model.SelectedEmployeeIds.Contains(s.EmployeeID)
                            && s.StartDate == startTime
                            && s.EndDate == endTime)
                .ToList();

            if (existingSchedules.Any())
            {
                var names = existingSchedules.Select(s => s.Employee.FullName).ToList();
                TempData["Error"] = $"Հետևյալ աշխատակից(ներ)ը արդեն գրանցված են՝ {string.Join(", ", names)}";
                return View(model);
            }

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
                return Unauthorized();

            int managerId = int.Parse(userIdClaim.Value);

            foreach (var employeeId in model.SelectedEmployeeIds)
            {
                _context.Schedules.Add(new Schedule
                {
                    ManagerID = managerId,
                    EmployeeID = employeeId,
                    StartDate = startTime,
                    EndDate = endTime
                });

                var userId = _context.Employees
                    .Where(e => e.EmployeeID == employeeId)
                    .Select(e => e.UserID)
                    .FirstOrDefault();

                if (userId != 0)
                {
                    var shiftText = ScheduleHelper.GetShiftString(startTime, endTime);
                    var message = $"{model.SelectedDate.ToString("MMMM dd", new CultureInfo("hy-AM"))}-ին Դուք ունեք հերթափոխ՝ {shiftText}։";
                    var notificationService = HttpContext.RequestServices.GetRequiredService<NotificationService>();
                    await notificationService.AddNotificationAsync(userId, "Նոր Գրաֆիկ", message, "Schedule");
                }
            }
            await _context.SaveChangesAsync();

            TempData["Success"] = "Գրաֆիկը հաջողությամբ ավելացվեց։";

            model.SelectedEmployeeIds.Clear();
            ScheduleHelper.LoadDynamicOptions(model, _context);
            return View(model);
        }
        catch (Exception ex)
        {
            return RedirectToAction("GeneralError", "Error");
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CancelSchedule(string EmployeeName, string Date, string Shift, string? StartFilter, string? EndFilter, string? PositionFilter)
    {
        try
        {
            IActionResult RedirectWithFilters(string errorMessage)
            {
                TempData["Error"] = errorMessage;
                return RedirectToAction("AllSchedules", new
                {
                    start = StartFilter,
                    end = EndFilter,
                    position = PositionFilter
                });
            }

            if (string.IsNullOrWhiteSpace(EmployeeName) || string.IsNullOrWhiteSpace(Date) || string.IsNullOrWhiteSpace(Shift))
                return RedirectWithFilters("Տվյալները սխալ են։");

            if (!DateTime.TryParse(Date, out var parsedDate))
                return RedirectWithFilters("Ամսաթիվը սխալ է։");

            var times = Shift.Split(" - ");
            if (times.Length != 2)
                return RedirectWithFilters("Հերթափոխի ձևաչափը սխալ է։");

            DateTime start = DateTime.Parse($"{Date} {times[0]}");
            DateTime end = DateTime.Parse($"{Date} {times[1]}");
            if (end <= start)
                end = end.AddDays(1);

            if (start <= DateTime.Now)
                return RedirectWithFilters("Արդեն սկսված գրաֆիկ հնարավոր չէ չեղարկել։");

            var schedule = await _context.Schedules
                .Include(s => s.Employee)
                .FirstOrDefaultAsync(s => s.Employee.FullName == EmployeeName
                                        && s.StartDate == start
                                        && s.EndDate == end);

            if (schedule != null)
            {
                _context.Schedules.Remove(schedule);
                await _context.SaveChangesAsync();

                var shiftText = ScheduleHelper.GetShiftString(schedule.StartDate, schedule.EndDate);
                var message = $"{schedule.StartDate.ToString("MMMM dd", new CultureInfo("hy-AM"))}-ի {shiftText} հերթափոխը չեղարկվել է։";

                var notificationService = HttpContext.RequestServices.GetRequiredService<NotificationService>();
                await notificationService.AddNotificationAsync(schedule.Employee.UserID, "Գրաֆիկի Չեղարկում", message, "Schedule");

                TempData["Success"] = "Գրաֆիկը հաջողությամբ չեղարկվեց։";
            }
            else
            {
                TempData["Error"] = "Գրաֆիկը չի գտնվել։";
            }

            return RedirectToAction("AllSchedules", new
            {
                start = StartFilter,
                end = EndFilter,
                position = PositionFilter
            });
        }
        catch (Exception ex)
        {
            return RedirectToAction("GeneralError", "Error");
        }
    }
    [HttpGet]
    public async Task<IActionResult> Salaries(string? month)
    {
        try {
            var targetMonth = string.IsNullOrWhiteSpace(month)
                ? DateTime.Today.AddMonths(-1).ToString("yyyy-MM")
                : month;

            var salaries = await _context.Salaries
                .Include(s => s.Employee)
                .Where(s => s.Month == targetMonth)
                .Select(s => new SalaryEntryViewModel
                {
                    EmployeeID = s.EmployeeID,
                    FullName = s.Employee.FullName,
                    Position = s.Employee.Position ?? "",
                    RegularHours = s.RegularHours,
                    NightHours = s.NightHours,
                    TotalSalaryAmount = s.TotalSalaryAmount
                })
                .OrderBy(s => s.FullName)
                .ToListAsync();

            var model = new SalaryOverviewViewModel
            {
                Month = targetMonth,
                Salaries = salaries
            };

            return View(model);
        }
        catch (Exception ex)
        {
            return RedirectToAction("GeneralError", "Error");
        }
    }
}

using HotelStaffManagement.DataAccess;
using HotelStaffManagement.DataAccess.Models;
using HotelStaffManagement.Web.Models.Admin;
using HotelStaffManagement.Web.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HotelStaffManagement.Web.Controllers
{
    [Authorize(Roles = "Admin")] 
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Admin Main Page
        public async Task<IActionResult> Index()
        {
            try 
            {
                var users = await _context.Users
                    .Where(u => u.Role == "Manager" || u.Role == "Employee")
                    .Join(_context.Employees,
                          user => user.UserID,
                          employee => employee.UserID,
                          (user, employee) => new UserViewModel
                          {
                              UserID = user.UserID,
                              FullName = employee.FullName,
                              Username = user.Username,
                              Role = user.Role,
                          })
                    .ToListAsync();

                return View(users);

            }
            catch (Exception ex)
            {
                return RedirectToAction("GeneralError", "Error");
            }

        }

        // Creating Manager 
        [HttpGet]
        public IActionResult CreateManager()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateManager(CreateManagerViewModel model)
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
                        Role = "Manager"
                    };

                    _context.Users.Add(user);
                    await _context.SaveChangesAsync();

                    var employee = new Employee
                    {
                        UserID = user.UserID,
                        FullName = model.FullName,
                        Position = "Manager",
                        SalaryPerHour = model.SalaryPerHour
                    };
                    _context.Employees.Add(employee);
                    await _context.SaveChangesAsync();

                    return RedirectToAction(nameof(Index));
                }
                return View(model);
            }
            catch (Exception ex)
            {
                return RedirectToAction("GeneralError", "Error");
            }
           
        }

        // Editing User
        [HttpGet]
        public async Task<IActionResult> EditUser(int id)
        {
            try
            {
                var user = await _context.Users.FindAsync(id);
                if (user == null)
                {
                    return NotFound();
                }
                if (user.Role == "Employee" || user.Role == "Manager")
                {
                    var employee = await _context.Employees.FirstOrDefaultAsync(e => e.UserID == user.UserID);
                    if (employee == null)
                    {
                        return NotFound();
                    }

                    var model = new EditUserViewModel
                    {
                        UserID = user.UserID,
                        FullName = employee.FullName,
                        Username = user.Username,
                    };
                    return View(model);
                }
                return NotFound();
            }
            catch (Exception ex)
            {
                return RedirectToAction("GeneralError", "Error");
            }

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditUser(EditUserViewModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var user = await _context.Users.FindAsync(model.UserID);
                    if (user == null)
                    {
                        return NotFound();
                    }
                    if (user.Role == "Employee" || user.Role == "Manager")
                    {
                        if (await _context.Users.AnyAsync(u => u.Username == model.Username && u.UserID != model.UserID))
                        {
                            ModelState.AddModelError("Username", "Այս մուտքանունը արդեն օգտագործվում է։");
                            return View(model);
                        }
                        var employee = await _context.Employees.FirstOrDefaultAsync(e => e.UserID == user.UserID);
                        if (employee == null)
                        {
                            return NotFound();
                        }

                        if (user.Username != model.Username)
                        {
                            user.Username = model.Username;
                            _context.Users.Update(user);
                        }

                        if (employee.FullName != model.FullName)
                        {
                            employee.FullName = model.FullName;
                            _context.Employees.Update(employee);
                        }

                        await _context.SaveChangesAsync();

                        return RedirectToAction(nameof(Index));
                    }
                    return NotFound();
                }
                return View(model);
            }
            catch (Exception ex)
            {
                return RedirectToAction("GeneralError", "Error");
            }
        }

        // Change User Password
        [HttpGet]
        public async Task<IActionResult> ChangePassword(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            if (user.Role == "Employee" || user.Role == "Manager")
            {
                var model = new ChangePasswordViewModel
                {
                    UserID = user.UserID
                };

                return View(model);
            }
            return NotFound();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _context.Users.FindAsync(model.UserID);
            if (user == null)
            {
                return NotFound();
            }
            if (user.Role == "Employee" || user.Role == "Manager")
            {
                var newSalt = PasswordHelper.GenerateSalt();
                var newHash = PasswordHelper.HashPassword(model.NewPassword, newSalt);

                user.Salt = newSalt;
                user.PasswordHash = newHash;

                _context.Users.Update(user);
                await _context.SaveChangesAsync();

                return RedirectToAction("Index");
            }
            return NotFound();
        }

        // Deleting User
        [HttpGet]
        public async Task<IActionResult> DeleteUser(int id)
        {
            try
            {
                var user = await _context.Users.FindAsync(id);
                if (user == null)
                {
                    return NotFound();
                }
                if (user.Role == "Employee" || user.Role == "Manager")
                {
                    var employee = await _context.Employees.FirstOrDefaultAsync(e => e.UserID == user.UserID);
                    if (employee == null)
                    {
                        return NotFound();
                    }

                    var model = new DeleteUserViewModel
                    {
                        UserID = user.UserID,
                        FullName = employee.FullName
                    };

                    return View(model);
                }
                return NotFound();
            }
            catch (Exception ex)
            {
                return RedirectToAction("GeneralError", "Error");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(DeleteUserViewModel model)
        {
            try
            {
                var user = await _context.Users.FindAsync(model.UserID);

                if (user == null)
                    return NotFound();

                if (user.Role == "Employee" || user.Role == "Manager")
                {
                    var employee = await _context.Employees
                        .FirstOrDefaultAsync(e => e.UserID == user.UserID);

                    if (employee != null)
                        _context.Employees.Remove(employee);

                    if (user.Role == "Manager")
                    {
                        var schedules = await _context.Schedules
                            .Where(s => s.ManagerID == user.UserID)
                            .ToListAsync();

                        foreach (var schedule in schedules)
                        {
                            schedule.ManagerID = null;
                        }

                        _context.Schedules.UpdateRange(schedules);
                    }

                    var sentMessages = await _context.Messages
                        .Where(m => m.SenderID == user.UserID).ToListAsync();
                    var receivedMessages = await _context.Messages
                        .Where(m => m.ReceiverID == user.UserID).ToListAsync();

                    _context.Messages.RemoveRange(sentMessages);
                    _context.Messages.RemoveRange(receivedMessages);

                    _context.Users.Remove(user);
                    await _context.SaveChangesAsync();

                    return RedirectToAction(nameof(Index));
                }

                return NotFound();
            }
            catch (Exception ex)
            {
                return RedirectToAction("GeneralError", "Error");
            }
        }
    }
}

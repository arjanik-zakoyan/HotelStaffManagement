using HotelStaffManagement.DataAccess;
using HotelStaffManagement.Web.Hubs;
using HotelStaffManagement.Web.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

namespace HotelStaffManagement.Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Connection With DB
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            // HttpContext
            builder.Services.AddHttpContextAccessor();

            // Cookie Authentication
            builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.LoginPath = "/Account/Login";
                    options.LogoutPath = "/Account/Logout";
                    options.AccessDeniedPath = "/Account/AccessDenied";
                    options.ExpireTimeSpan = TimeSpan.FromDays(7);
                });

            // Authorization By Role
            builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
                options.AddPolicy("ManagerOnly", policy => policy.RequireRole("Manager"));
                options.AddPolicy("EmployeeOnly", policy => policy.RequireRole("Employee"));
            });

            // Notification Service
            builder.Services.AddScoped<NotificationService>();

            // Salary Calculation Service
            builder.Services.AddScoped<SalaryCalculationService>();

            // Salary Calculation Background
            builder.Services.AddHostedService<SalaryBackgroundService>();

            // SignalR (Chat)
            builder.Services.AddSignalR();

            // MVC
            builder.Services.AddControllersWithViews();

            var app = builder.Build();

            // Error Handling Pipeline
            app.UseExceptionHandler("/Error/GeneralError");
            app.UseStatusCodePagesWithReExecute("/Error/{0}");

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            // SignalR Hub Mapping
            app.MapHub<ChatHub>("/chathub");

            // Routing
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Account}/{action=Login}/{id?}");

            app.Run();
        }
    }
}

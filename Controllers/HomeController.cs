using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MaterniTrack.Data;
using MaterniTrack.Models;
using MaterniTrack.Models.ViewModels;

namespace MaterniTrack.Controllers;

[Authorize]
public class HomeController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public HomeController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        var currentUser = await _userManager.GetUserAsync(User);
        var fullName = currentUser?.FullName ?? User.Identity?.Name ?? "User";
        var isDoctor = User.IsInRole("Doctor");

        var today = DateOnly.FromDateTime(DateTime.Today);

        var todayAppointments = await _context.Appointments
            .Where(a => a.AppointmentDate == today && a.Status != "cancelled")
            .OrderBy(a => a.AppointmentTime)
            .ToListAsync();

        var totalPatients = await _context.Patients.CountAsync();
        var totalSupplies = await _context.InventoryItems.CountAsync();
        var displayInventory = await _context.InventoryItems.Take(4).ToListAsync();
        var lowStockAlerts = await _context.InventoryItems
            .Where(i => i.Quantity <= i.ReorderLevel || i.Quantity == 0)
            .ToListAsync();

        var initials = "LM";
        if (!string.IsNullOrWhiteSpace(fullName))
        {
            var parts = fullName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            initials = parts.Length > 1 
                ? $"{parts[0][0]}{parts[^1][0]}".ToUpper() 
                : fullName.Length >= 2 ? fullName[..2].ToUpper() : fullName.ToUpper();
        }

        var viewModel = new DashboardViewModel
        {
            UserName = fullName,
            UserRole = isDoctor ? "Doctor" : "Staff",
            UserInitials = initials,
            TodayAppointmentsCount = todayAppointments.Count,
            TotalPatientsCount = totalPatients,
            TotalSuppliesCount = totalSupplies,
            TodayAppointments = todayAppointments,
            DisplayInventory = displayInventory,
            LowStockAlertItems = lowStockAlerts
        };

        return View(viewModel);
    }
}

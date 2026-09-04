using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MaterniTrack.Data;
using MaterniTrack.Models;
using MaterniTrack.Models.ViewModels;

namespace MaterniTrack.Controllers;

[Authorize(Roles = "Doctor")]
public class SettingsController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly ApplicationDbContext _context;

    public SettingsController(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        ApplicationDbContext context)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _context = context;
    }

    // GET: Settings/Users or Settings
    [HttpGet]
    [Route("Settings")]
    [Route("Settings/Users")]
    public async Task<IActionResult> Users(string? search, string? role, string? status)
    {
        var allUsers = await _userManager.Users.ToListAsync();
        var userItemList = new List<UserItemViewModel>();

        foreach (var u in allUsers)
        {
            var userRoles = await _userManager.GetRolesAsync(u);
            var primaryRole = userRoles.FirstOrDefault() ?? u.ClinicRole ?? "Staff";
            userItemList.Add(new UserItemViewModel
            {
                Id = u.Id,
                FullName = u.FullName,
                Email = u.Email ?? "",
                Role = primaryRole,
                Status = u.Status ?? "Active"
            });
        }

        // Stats before filter
        var totalCount = userItemList.Count;
        var adminCount = userItemList.Count(u => u.Role.Equals("Doctor", StringComparison.OrdinalIgnoreCase));
        var staffCount = userItemList.Count(u => u.Role.Equals("Staff", StringComparison.OrdinalIgnoreCase));

        // Filtering
        var filteredList = userItemList.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim().ToLower();
            filteredList = filteredList.Where(u =>
                u.FullName.ToLower().Contains(s) ||
                u.Email.ToLower().Contains(s));
        }

        if (!string.IsNullOrWhiteSpace(role) && !role.Equals("all", StringComparison.OrdinalIgnoreCase))
        {
            filteredList = filteredList.Where(u =>
                u.Role.Equals(role, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(status) && !status.Equals("all", StringComparison.OrdinalIgnoreCase))
        {
            filteredList = filteredList.Where(u =>
                u.Status.Equals(status, StringComparison.OrdinalIgnoreCase));
        }

        var viewModel = new UserManagementViewModel
        {
            Users = filteredList.OrderBy(u => u.FullName).ToList(),
            TotalUsersCount = totalCount,
            AdminUsersCount = adminCount,
            StaffUsersCount = staffCount,
            CurrentSearch = search ?? "",
            CurrentRole = role ?? "all",
            CurrentStatus = status ?? "all"
        };

        if (TempData["SuccessMessage"] != null)
        {
            ViewBag.SuccessMessage = TempData["SuccessMessage"];
        }

        if (TempData["ErrorMessage"] != null)
        {
            ViewBag.ErrorMessage = TempData["ErrorMessage"];
        }

        return View(viewModel);
    }

    // POST: Settings/CreateUser
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateUser(CreateUserViewModel model)
    {
        if (ModelState.IsValid)
        {
            var existingUser = await _userManager.FindByEmailAsync(model.Email);
            if (existingUser != null)
            {
                TempData["ErrorMessage"] = $"A user with email \"{model.Email}\" already exists.";
                return RedirectToAction(nameof(Users));
            }

            var newUser = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                FullName = model.FullName,
                ClinicRole = model.Role,
                Status = model.Status,
                EmailConfirmed = true
            };

            var createResult = await _userManager.CreateAsync(newUser, model.Password);
            if (createResult.Succeeded)
            {
                if (!await _roleManager.RoleExistsAsync(model.Role))
                {
                    await _roleManager.CreateAsync(new IdentityRole(model.Role));
                }

                await _userManager.AddToRoleAsync(newUser, model.Role);
                TempData["SuccessMessage"] = $"Staff account \"{model.FullName}\" created successfully.";
            }
            else
            {
                var errors = string.Join("; ", createResult.Errors.Select(e => e.Description));
                TempData["ErrorMessage"] = $"Failed to create user: {errors}";
            }
        }
        else
        {
            TempData["ErrorMessage"] = "Please fill out all required fields correctly.";
        }

        return RedirectToAction(nameof(Users));
    }

    // POST: Settings/EditUser
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditUser(EditUserViewModel model)
    {
        if (ModelState.IsValid)
        {
            var user = await _userManager.FindByIdAsync(model.Id);
            if (user == null)
            {
                TempData["ErrorMessage"] = "User not found.";
                return RedirectToAction(nameof(Users));
            }

            user.FullName = model.FullName;
            user.Email = model.Email;
            user.UserName = model.Email;
            user.ClinicRole = model.Role;
            user.Status = model.Status;

            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                var errors = string.Join("; ", updateResult.Errors.Select(e => e.Description));
                TempData["ErrorMessage"] = $"Failed to update user: {errors}";
                return RedirectToAction(nameof(Users));
            }

            // Update role
            var currentRoles = await _userManager.GetRolesAsync(user);
            await _userManager.RemoveFromRolesAsync(user, currentRoles);
            if (!await _roleManager.RoleExistsAsync(model.Role))
            {
                await _roleManager.CreateAsync(new IdentityRole(model.Role));
            }
            await _userManager.AddToRoleAsync(user, model.Role);

            // Optional password update
            if (!string.IsNullOrWhiteSpace(model.NewPassword))
            {
                var removePassResult = await _userManager.RemovePasswordAsync(user);
                if (removePassResult.Succeeded)
                {
                    await _userManager.AddPasswordAsync(user, model.NewPassword);
                }
            }

            TempData["SuccessMessage"] = $"User \"{model.FullName}\" updated successfully.";
        }
        else
        {
            TempData["ErrorMessage"] = "Please check your inputs and try again.";
        }

        return RedirectToAction(nameof(Users));
    }

    // POST: Settings/DeleteUser
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteUser(string id)
    {
        var currentUserId = _userManager.GetUserId(User);
        if (id == currentUserId)
        {
            TempData["ErrorMessage"] = "You cannot delete your own account while logged in.";
            return RedirectToAction(nameof(Users));
        }

        var user = await _userManager.FindByIdAsync(id);
        if (user != null)
        {
            var result = await _userManager.DeleteAsync(user);
            if (result.Succeeded)
            {
                await LogActivityAsync("Settings", "Staff Account Deleted", $"Deleted staff account for {user.FullName} ({user.Email}).", "Warning");
                TempData["SuccessMessage"] = $"Account \"{user.FullName}\" was deleted.";
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to delete the user account.";
            }
        }

        return RedirectToAction(nameof(Users));
    }

    // ==========================================
    // CLINIC PROFILE
    // ==========================================
    [HttpGet]
    [Route("Settings/ClinicProfile")]
    public async Task<IActionResult> ClinicProfile()
    {
        var setting = await _context.ClinicSettings.FirstOrDefaultAsync() 
                      ?? new ClinicProfileSetting();

        if (TempData["SuccessMessage"] != null)
        {
            ViewBag.SuccessMessage = TempData["SuccessMessage"];
        }

        return View(setting);
    }

    [HttpPost]
    [Route("Settings/ClinicProfile")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ClinicProfile(ClinicProfileSetting model)
    {
        if (ModelState.IsValid)
        {
            var existing = await _context.ClinicSettings.FirstOrDefaultAsync();
            if (existing == null)
            {
                _context.ClinicSettings.Add(model);
            }
            else
            {
                existing.ClinicName = model.ClinicName;
                existing.Specialization = model.Specialization;
                existing.MedicalDirector = model.MedicalDirector;
                existing.DohLicense = model.DohLicense;
                existing.PhilHealthAccreditation = model.PhilHealthAccreditation;
                existing.ContactPhone = model.ContactPhone;
                existing.ContactEmail = model.ContactEmail;
                existing.Address = model.Address;
                existing.OperatingHours = model.OperatingHours;
                existing.EmergencyHotline = model.EmergencyHotline;
                _context.ClinicSettings.Update(existing);
            }

            await _context.SaveChangesAsync();
            await LogActivityAsync("Settings", "Clinic Profile Updated", "Updated clinic profile, contact details, and accreditation numbers.", "Success");
            TempData["SuccessMessage"] = "Clinic profile settings saved successfully.";
            return RedirectToAction(nameof(ClinicProfile));
        }

        TempData["ErrorMessage"] = "Please check all required fields.";
        return View(model);
    }

    // ==========================================
    // NOTIFICATIONS
    // ==========================================
    [HttpGet]
    [Route("Settings/Notifications")]
    public async Task<IActionResult> Notifications()
    {
        var setting = await _context.ClinicSettings.FirstOrDefaultAsync() 
                      ?? new ClinicProfileSetting();

        if (TempData["SuccessMessage"] != null)
        {
            ViewBag.SuccessMessage = TempData["SuccessMessage"];
        }

        return View(setting);
    }

    [HttpPost]
    [Route("Settings/Notifications")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Notifications(ClinicProfileSetting model)
    {
        var existing = await _context.ClinicSettings.FirstOrDefaultAsync();
        if (existing == null)
        {
            _context.ClinicSettings.Add(model);
        }
        else
        {
            existing.SmsRemindersEnabled = model.SmsRemindersEnabled;
            existing.EmailRemindersEnabled = model.EmailRemindersEnabled;
            existing.ConflictAlertsEnabled = model.ConflictAlertsEnabled;
            existing.HighRiskFlagsEnabled = model.HighRiskFlagsEnabled;
            existing.LowStockAlertsEnabled = model.LowStockAlertsEnabled;
            existing.DailyDigestEnabled = model.DailyDigestEnabled;
            _context.ClinicSettings.Update(existing);
        }

        await _context.SaveChangesAsync();
        await LogActivityAsync("Settings", "Notification Preferences Updated", "Updated clinic reminder and alert triggers.", "Success");
        TempData["SuccessMessage"] = "Notification preferences saved successfully.";
        return RedirectToAction(nameof(Notifications));
    }

    // ==========================================
    // ACTIVITY LOGS
    // ==========================================
    [HttpGet]
    [Route("Settings/ActivityLogs")]
    public async Task<IActionResult> ActivityLogs(string? category, string? search)
    {
        var query = _context.ActivityLogs.AsNoTracking().OrderByDescending(l => l.Timestamp).AsQueryable();

        var total = await _context.ActivityLogs.CountAsync();
        var authCount = await _context.ActivityLogs.CountAsync(l => l.Category == "Authentication");
        var patientCount = await _context.ActivityLogs.CountAsync(l => l.Category == "Patients");
        var appointmentCount = await _context.ActivityLogs.CountAsync(l => l.Category == "Appointments");
        var inventoryCount = await _context.ActivityLogs.CountAsync(l => l.Category == "Inventory");
        var settingsCount = await _context.ActivityLogs.CountAsync(l => l.Category == "Settings");

        if (!string.IsNullOrWhiteSpace(category) && !category.Equals("all", StringComparison.OrdinalIgnoreCase))
        {
            query = query.Where(l => l.Category.ToLower() == category.ToLower());
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim().ToLower();
            query = query.Where(l => 
                l.Action.ToLower().Contains(s) || 
                l.PerformedBy.ToLower().Contains(s) || 
                l.Details.ToLower().Contains(s));
        }

        var list = await query.Take(100).ToListAsync();

        var vm = new ActivityLogViewModel
        {
            Logs = list,
            CurrentCategory = category ?? "all",
            CurrentSearch = search ?? "",
            TotalLogsCount = total,
            AuthCount = authCount,
            PatientCount = patientCount,
            AppointmentCount = appointmentCount,
            InventoryCount = inventoryCount,
            SettingsCount = settingsCount
        };

        return View(vm);
    }

    // GET: Settings/ExportActivityLogsCsv
    [HttpGet]
    [Route("Settings/ExportActivityLogsCsv")]
    public async Task<IActionResult> ExportActivityLogsCsv()
    {
        var logs = await _context.ActivityLogs
            .AsNoTracking()
            .OrderByDescending(l => l.Timestamp)
            .ToListAsync();

        var sb = new StringBuilder();
        sb.AppendLine("LogId,Timestamp,Category,Action,PerformedBy,Severity,Details");

        foreach (var l in logs)
        {
            var cleanDetails = l.Details.Replace("\"", "\"\"");
            var cleanAction = l.Action.Replace("\"", "\"\"");
            sb.AppendLine($"{l.Id},\"{l.Timestamp:yyyy-MM-dd HH:mm:ss}\",\"{l.Category}\",\"{cleanAction}\",\"{l.PerformedBy}\",\"{l.Severity}\",\"{cleanDetails}\"");
        }

        var bytes = Encoding.UTF8.GetBytes(sb.ToString());
        var fileName = $"MaterniTrack_ActivityLogs_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
        return File(bytes, "text/csv", fileName);
    }

    // ==========================================
    // ABOUT SYSTEM
    // ==========================================
    [HttpGet]
    [Route("Settings/About")]
    public async Task<IActionResult> About()
    {
        var patientsCount = await _context.Patients.CountAsync();
        var appointmentsCount = await _context.Appointments.CountAsync();
        var itemsCount = await _context.InventoryItems.CountAsync();
        var usersCount = await _userManager.Users.CountAsync();

        var vm = new AboutSystemViewModel
        {
            TotalPatientsCount = patientsCount,
            TotalAppointmentsCount = appointmentsCount,
            TotalInventoryItemsCount = itemsCount,
            TotalUsersCount = usersCount,
            DatabaseConnected = await _context.Database.CanConnectAsync()
        };

        return View(vm);
    }

    private async Task LogActivityAsync(string category, string action, string details, string severity = "Info")
    {
        try
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var performer = currentUser?.FullName ?? User.Identity?.Name ?? "Admin";

            var log = new ActivityLog
            {
                Timestamp = DateTime.Now,
                Category = category,
                Action = action,
                PerformedBy = performer,
                Details = details,
                Severity = severity
            };

            await _context.ActivityLogs.AddAsync(log);
            await _context.SaveChangesAsync();
        }
        catch
        {
            // Logging failure should never crash the user workflow
        }
    }
}

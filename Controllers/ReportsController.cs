using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MaterniTrack.Data;
using MaterniTrack.Models.ViewModels;

namespace MaterniTrack.Controllers;

[Authorize]
public class ReportsController : Controller
{
    private readonly ApplicationDbContext _context;

    public ReportsController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: Reports
    public async Task<IActionResult> Index()
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        var currentYear = today.Year;
        var currentMonth = today.Month;

        var allAppointments = await _context.Appointments.ToListAsync();
        var allPatients = await _context.Patients.ToListAsync();

        var todayAppointments = allAppointments
            .Where(a => a.AppointmentDate == today)
            .OrderBy(a => a.AppointmentTime)
            .ToList();

        var todayPatients = allPatients
            .Where(p => p.DateAdded == today)
            .ToList();

        var monthlyAppointments = allAppointments
            .Where(a => a.AppointmentDate.Year == currentYear && a.AppointmentDate.Month == currentMonth)
            .ToList();

        var monthlyPatients = allPatients
            .Where(p => p.DateAdded.Year == currentYear && p.DateAdded.Month == currentMonth)
            .ToList();

        var activePatients = allPatients
            .Where(p => string.Equals(p.Status, "Active", StringComparison.OrdinalIgnoreCase))
            .ToList();

        var recentPatients = allPatients
            .OrderByDescending(p => p.DateAdded)
            .Take(10)
            .ToList();

        var viewModel = new ReportsViewModel
        {
            DailyAppointmentsCount = todayAppointments.Count,
            DailyPatientsCount = todayPatients.Count,
            TodayDateFormatted = DateTime.Today.ToString("MMMM dd, yyyy"),
            MonthlyAppointmentsCount = monthlyAppointments.Count,
            MonthlyPatientsCount = monthlyPatients.Count,
            ActivePatientsCount = activePatients.Count,
            TodayAppointments = todayAppointments,
            RecentPatients = recentPatients
        };

        return View(viewModel);
    }

    // GET: Reports/ExportCsv
    public async Task<IActionResult> ExportCsv()
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        var currentYear = today.Year;
        var currentMonth = today.Month;

        var allAppointments = await _context.Appointments.ToListAsync();
        var allPatients = await _context.Patients.ToListAsync();

        var todayAppointments = allAppointments
            .Where(a => a.AppointmentDate == today)
            .OrderBy(a => a.AppointmentTime)
            .ToList();

        var todayPatientsCount = allPatients.Count(p => p.DateAdded == today);
        var monthlyAppointmentsCount = allAppointments.Count(a => a.AppointmentDate.Year == currentYear && a.AppointmentDate.Month == currentMonth);
        var monthlyPatientsCount = allPatients.Count(p => p.DateAdded.Year == currentYear && p.DateAdded.Month == currentMonth);
        var activePatientsCount = allPatients.Count(p => string.Equals(p.Status, "Active", StringComparison.OrdinalIgnoreCase));

        var sb = new StringBuilder();

        // Title
        sb.AppendLine($"\"MaterniTrack Clinic Report - {DateTime.Today:MMMM dd, yyyy}\"");
        sb.AppendLine();

        // Daily Summary
        sb.AppendLine("\"Daily Summary\"");
        sb.AppendLine($"\"Appointments Today\",\"{todayAppointments.Count}\"");
        sb.AppendLine($"\"Patients Added Today\",\"{todayPatientsCount}\"");
        sb.AppendLine();

        // Monthly Summary
        sb.AppendLine("\"Monthly Summary\"");
        sb.AppendLine($"\"Total Appointments This Month\",\"{monthlyAppointmentsCount}\"");
        sb.AppendLine($"\"New Patients This Month\",\"{monthlyPatientsCount}\"");
        sb.AppendLine($"\"Active Patients\",\"{activePatientsCount}\"");
        sb.AppendLine();

        // Today's Appointments
        sb.AppendLine("\"Today's Appointments\"");
        sb.AppendLine("\"Time\",\"Patient\",\"Type\",\"Staff\",\"Status\"");

        foreach (var a in todayAppointments)
        {
            sb.AppendLine($"\"{a.AppointmentTime}\",\"{a.PatientName}\",\"{a.AppointmentType}\",\"{a.AssignedStaff}\",\"{a.Status}\"");
        }

        var csvBytes = Encoding.UTF8.GetBytes(sb.ToString());
        var fileName = $"clinic-report-{DateTime.Today:yyyy-MM-dd}.csv";

        return File(csvBytes, "text/csv", fileName);
    }
}
